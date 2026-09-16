using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Generation;
using Assets;
using TMPro;

public class TimeControl : MonoBehaviour
{
    public RectTransform timeBar;

    public float energyLeft = 100;

    public float energyUsage = 8;

    public bool isUsing;

    private bool wasPressed;

    public Camera view;

    public TextMeshProUGUI scoreText, scoreCenterText;

    public TextMeshProUGUI highScoreText;

    public float score;

    public bool isLost = true;
	
    public TextMeshProUGUI gameOverText;

    public TextMeshProUGUI gameTitle;

    public TextMeshProUGUI restartButton, startButton;

    public GameObject playerPrefab;



    public GameObject joystick;
    public GameObject TutorialGO;
    private GameObject TutorialClone;

    public AudioSource sound;

    public AudioClip gameOverClip;

    public Text invertText;

    public Image invertCheck;

    public Button optionsButton;

    public Toggle invertToggle;

    private Movement movement;

    private float targetGameOver;

    private float targetRestart;

    private float targetScore;

    private float targetStart;

    private float targetTitle;

    private float targetPitch = 1;
    
    private float targetInvert;

    public GameObject cameraObject;

    public Cinemachine.CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        isLost = true;
        
        Time.timeScale = 0.25f;
        
        targetStart = 1;
        targetTitle = 1;
        
        StartCoroutine(PlayAnim());
        
        Cursor.visible = true;
        
        highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        
        Options.Invert = PlayerPrefs.GetInt("Invertir") == 0 ? true : false;
        
        invertToggle.isOn = PlayerPrefs.GetInt("Invertir") == 0 ? true : false;
        
        PlayerPrefs.Save();
    }

    public void Lose()
    {
        isLost = true;
        
        Time.timeScale = 0.25f;
        
        targetGameOver = 1f;
        
        targetScore = 1f;
        
        TutorialGO = GameObject.FindGameObjectWithTag("OwO");
        
        StartCoroutine(LostCoroutine());
        
        GetComponent<AudioSource>().clip = gameOverClip;
        
        GetComponent<AudioSource>().Play();
        
        optionsButton.gameObject.SetActive(true);
    }

    
    IEnumerator LostCoroutine()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
        
        if (GameObject.FindWithTag("Joystick") != null)
        {
            joystick = GameObject.FindGameObjectWithTag("Joystick");
            joystick.gameObject.SetActive(false);
        }
        
        yield return new WaitForSecondsRealtime(3f);
        
        targetGameOver = 0;

        while (isLost)
        {
            yield return new WaitForSecondsRealtime(1f);
            targetRestart = 1;
            yield return new WaitForSecondsRealtime(1f);
            targetRestart = 0;
        }
    }

    IEnumerator PlayAnim()
    {
        while (isLost)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            targetStart = 1;
            yield return new WaitForSecondsRealtime(0.5f);
            targetStart = 0;
        }
    }

    public void StartGame()
    {
        Restart();
    }

    public void Restart()
    {
        if (!isLost)
            return;
        StartCoroutine(RestartCoroutine());
    }

    IEnumerator RestartCoroutine()
    {
        targetStart = 0;
        targetRestart = 0;
        targetTitle = 0;
        
        isLost = false;
        
        Time.timeScale = 1f;
        
        score = 0;
        targetScore = 0;
        
        energyLeft = 100;
        
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        Destroy(GameObject.FindGameObjectWithTag("Debris"));
        
        optionsButton.gameObject.SetActive(false);
        
        GameObject debris = new GameObject("Debris");
        debris.tag = "Debris";
        
        OpenSimplexNoise.Load(Random.Range(int.MinValue, int.MaxValue));

        World world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        Chunk[] chunks = null;
        
        lock (world.Chunks)
        {
            chunks = world.Chunks.Values.ToList().ToArray();
        }
        
        for (int i = 0; i < chunks.Length; i++)
        {
            world.RemoveChunk(chunks[i]);
        }
        
        GameObject go = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);

        world.Player = go;

        UpdateControlUI();
        
        movement = go.GetComponentInChildren<Movement>();
        
        go.GetComponent<ShipCollision>().Control = this.GetComponent<TimeControl>();
        
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowShip>().TargetShip = go;
        
        cameraObject.GetComponent<ChunkLoader>().Player = go;
        
        virtualCamera.m_LookAt = go.transform;
        virtualCamera.m_Follow = go.transform;
        
        GetComponent<AudioSource>().clip = gameOverClip;
        
        GetComponent<AudioSource>().Play();
        
        yield return null;
    }


    void Update()
    {
        if (isLost && Input.GetKeyDown(KeyCode.Space))
            Restart();

        scoreText.text = ((int)score).ToString();
        scoreCenterText.text = scoreText.text;

        targetInvert = Mathf.Min(1, targetScore + targetTitle);
        
        sound.pitch = Mathf.Lerp(sound.pitch, targetPitch, Time.deltaTime * 8f);
        
        gameTitle.color = new Color(gameTitle.color.r, gameTitle.color.g, gameTitle.color.b, Mathf.Lerp(gameTitle.color.a, targetTitle, Time.unscaledDeltaTime * 4f));
        startButton.color = new Color(startButton.color.r, startButton.color.g, startButton.color.b, Mathf.Lerp(startButton.color.a, targetStart, Time.unscaledDeltaTime * 4f));
        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, Mathf.Lerp(gameOverText.color.a, targetGameOver, Time.unscaledDeltaTime * 4f));
        restartButton.color = new Color(restartButton.color.r, restartButton.color.g, restartButton.color.b, Mathf.Lerp(restartButton.color.a, targetRestart, Time.unscaledDeltaTime * 4f));
        invertToggle.targetGraphic.color = new Color(invertToggle.targetGraphic.color.r, invertToggle.targetGraphic.color.g, invertToggle.targetGraphic.color.b, Mathf.Lerp(invertToggle.targetGraphic.color.a, targetInvert, Time.unscaledDeltaTime * 4f));
        invertText.color = new Color(invertText.color.r, invertText.color.g, invertText.color.b, Mathf.Lerp(invertText.color.a, targetInvert, Time.unscaledDeltaTime * 4f));
        optionsButton.image.color = new Color(optionsButton.image.color.r, optionsButton.image.color.g, optionsButton.image.color.b, Mathf.Lerp(optionsButton.image.color.a, targetInvert, Time.unscaledDeltaTime * 4f));
        invertCheck.color = new Color(invertCheck.color.r, invertCheck.color.g, invertCheck.color.b, Mathf.Lerp(invertCheck.color.a, targetInvert, Time.unscaledDeltaTime * 4f));
        
        if (targetTitle != 1)
        {
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, Mathf.Lerp(scoreText.color.a, 1 - targetScore, Time.unscaledDeltaTime * 2f));
            scoreCenterText.color = new Color(scoreCenterText.color.r, scoreCenterText.color.g, scoreCenterText.color.b, Mathf.Lerp(scoreCenterText.color.a, targetScore, Time.unscaledDeltaTime * 2f));
        }
        
        if (isLost)
            return;

        if (Input.touchCount > 1 && energyLeft > 0 && !wasPressed)
        {
            energyLeft -= Time.unscaledDeltaTime * energyUsage;
            energyLeft = Mathf.Clamp(energyLeft, 0, 100);
            isUsing = true;
        }
        else
        {
            energyLeft += Time.deltaTime * energyUsage * 0.5f;
            energyLeft = Mathf.Clamp(energyLeft, 0, 100);
            isUsing = false;
        }
        
        timeBar.sizeDelta = Lerp(timeBar.sizeDelta, new Vector2(energyLeft - 0.5f, timeBar.sizeDelta.y), Time.deltaTime * 6f);
        
        if (isUsing)
        {
            Time.timeScale = 0.35f;
            targetPitch = 0.5f;
        }
        else
        {
            Time.timeScale = 1f;
            targetPitch = 1f;
        }
        
        if (!isUsing)
            wasPressed = Input.GetKey(KeyCode.Space);

        if (!movement.IsInSpawn)
            score += Time.deltaTime * 8;
        
        if (score < 125)
            movement.Speed = 12;
        else if (score < 275)
            movement.Speed = 14;
        else if (score < 500)
            movement.Speed = 16;
        else if (score < 1000)
            movement.Speed = 18;

        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {
            int intScore = (int)score;
            PlayerPrefs.SetInt("HighScore", intScore);
            highScoreText.text = intScore.ToString();
            PlayerPrefs.Save();
        }

        PlayerPrefs.Save();
        
        if (TutorialGO == null)
        {
            TutorialGO = GameObject.FindGameObjectWithTag("OwO");
            if (TutorialGO != null) UpdateControlUI();
        }

        if (TutorialGO != null)
        {
            bool shouldShow = !isLost && 
                              !movement.IsInSpawn && 
                              !isUsing && 
                              energyLeft > 20 && 
                              score < 200;

            if (shouldShow)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.15f;

                if (!TutorialGO.activeSelf) TutorialGO.SetActive(true);
                TutorialGO.transform.localScale = Vector3.one * pulse;

                if (TutorialClone != null)
                {
                    if (!TutorialClone.activeSelf) TutorialClone.SetActive(true);
                    TutorialClone.transform.localScale = Vector3.one * pulse;
                }
            }
            else
            {
                if (TutorialGO.activeSelf) TutorialGO.SetActive(false);
                if (TutorialClone != null && TutorialClone.activeSelf) TutorialClone.SetActive(false);
            }
        }
    }

    public void InvertControls()
    {
        Options.Invert = !Options.Invert;
    }

    Vector2 Lerp(Vector2 a, Vector2 b, float d)
    {
        return new Vector2(Mathf.Lerp(a.x, b.x, d), Mathf.Lerp(b.x, b.y, d));
    }

    public void UpdateControlUI()
    {
        joystick = GameObject.FindGameObjectWithTag("Joystick");
        
        if (TutorialGO == null)
            TutorialGO = GameObject.FindGameObjectWithTag("OwO");

        if (joystick == null) return;

        int controlType = PlayerPrefs.GetInt("Control");

        RectTransform rect = joystick.GetComponent<RectTransform>();
        RectTransform tutRect = TutorialGO != null ? TutorialGO.GetComponent<RectTransform>() : null;

        if (tutRect != null)
        {
            if (tutRect.parent == joystick.transform)
                tutRect.SetParent(joystick.transform.parent, false);
        }

        if (controlType == 1 || controlType == 0)
        {
            joystick.SetActive(true);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(100, 100);

            if (tutRect != null)
            {
                tutRect.gameObject.SetActive(true);
                tutRect.anchorMin = new Vector2(0, 0);
                tutRect.anchorMax = new Vector2(0, 0);
                tutRect.pivot = new Vector2(0, 0);
                tutRect.anchoredPosition = new Vector2(1766, 150);
                
                tutRect.anchorMin = new Vector2(1, 0);
                tutRect.anchorMax = new Vector2(1, 0);
                tutRect.pivot = new Vector2(1, 0);
                tutRect.anchoredPosition = new Vector2(-766, 150);
            }
            if (TutorialClone != null) TutorialClone.SetActive(false);
        }
        else if (controlType == 2)
        {
            joystick.SetActive(true);
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-100, 100);

            if (tutRect != null)
            {
                tutRect.gameObject.SetActive(true);
                tutRect.anchorMin = new Vector2(0, 0);
                tutRect.anchorMax = new Vector2(0, 0);
                tutRect.pivot = new Vector2(0, 0);
                tutRect.anchoredPosition = new Vector2(766, 150);
            }
            if (TutorialClone != null) TutorialClone.SetActive(false);
        }
        else if (controlType == 3)
        {
            joystick.SetActive(false);
            if (tutRect != null)
            {
                tutRect.gameObject.SetActive(true);
                tutRect.anchorMin = new Vector2(1, 0);
                tutRect.anchorMax = new Vector2(1, 0);
                tutRect.pivot = new Vector2(1, 0);
                tutRect.anchoredPosition = new Vector2(-766, 150);
            }

            if (TutorialGO != null)
            {
                if (TutorialClone == null)
                {
                    TutorialClone = Instantiate(TutorialGO, TutorialGO.transform.parent);
                }
                
                TutorialClone.SetActive(true);
                RectTransform cloneRect = TutorialClone.GetComponent<RectTransform>();
                cloneRect.anchorMin = new Vector2(0, 0);
                cloneRect.anchorMax = new Vector2(0, 0);
                cloneRect.pivot = new Vector2(0, 0);
                cloneRect.anchoredPosition = new Vector2(766, 150);
            }
        }
    }
}
