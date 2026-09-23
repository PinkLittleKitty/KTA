using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Generation;

public class RiskPoints : MonoBehaviour
{
    public int RiskPointsNumber = 5;

    public float ComboDuration = 3.0f;

    public int MaxMultiplier = 10;

    public float MinTimeBetweenRiskyEvents = 0.5f;

    public float ContinuousSkimInterval = 0.8f;

    [Header("Audio")]
    public AudioClip RiskyAudioClip;
    public AudioSource RiskyAudioSource;

    [Header("UI Feedback")]
    public TextMeshProUGUI ComboText;

    [Header("References")]
    public Collider Colision;
    public GameObject World;

    public int CurrentCombo { get; private set; } = 0;
    public int CurrentMultiplier => Mathf.Max(1, CurrentCombo);

    private TimeControl _timeControl;
    private Movement _movement;
    private float _comboTimer = 0f;
    private float _lastRiskyTime = -10f;
    private float _skimTimer = 0f;
    private bool _isSkimming = false;
    private float _comboTextScale = 1f;
    private Color _currentColor = Color.cyan;

    void Awake()
    {
        if (RiskPointsNumber <= 0)
            RiskPointsNumber = 25;
    }

    void Start()
    {
        World = GameObject.FindGameObjectWithTag("World");

        if (_timeControl == null)
            _timeControl = FindFirstObjectByType<TimeControl>() ?? FindObjectOfType<TimeControl>();

        if (_movement == null)
            _movement = GetComponentInParent<Movement>();

        EnsureUI();
    }

    public void Init(TimeControl tc, Movement mov = null)
    {
        _timeControl = tc;
        if (mov != null)
            _movement = mov;
        else
            _movement = GetComponentInParent<Movement>();

        EnsureUI();
        ResetCombo();
    }

    private void EnsureUI()
    {
        if (ComboText != null)
            return;

        if (_timeControl != null && _timeControl.scoreText != null)
        {
            Transform canvasTransform = _timeControl.scoreText.transform.parent;
            Transform existing = canvasTransform.Find("ComboText");
            if (existing != null)
            {
                ComboText = existing.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject comboObj = new GameObject("ComboText");
                comboObj.transform.SetParent(canvasTransform, false);

                RectTransform rt = comboObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-20, -175);
                rt.sizeDelta = new Vector2(350, 100);

                ComboText = comboObj.AddComponent<TextMeshProUGUI>();
                ComboText.font = _timeControl.scoreText.font;
                ComboText.fontSharedMaterial = _timeControl.scoreText.fontSharedMaterial;
                ComboText.alignment = TextAlignmentOptions.TopRight;
                ComboText.fontSize = 36;
                ComboText.text = "";
                ComboText.raycastTarget = false;
            }
        }
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray1 = new Ray(transform.position, Vector3.left);
        if (Physics.Raycast(ray1, out hit))
        {
            Debug.DrawLine(ray1.origin, hit.point, Color.magenta);
        }

        if (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }
            else if (ComboText != null)
            {
                _comboTextScale = Mathf.Lerp(_comboTextScale, 1.0f, Time.deltaTime * 8f);
                ComboText.transform.localScale = Vector3.one * _comboTextScale;

                float alpha = Mathf.Clamp01(_comboTimer / 0.8f);
                Color c = _currentColor;
                c.a = alpha;
                ComboText.color = c;
            }
        }
    }

    private bool IsTerrain(Collider col)
    {
        if (col == null) return false;
        if (col.GetComponent<Chunk>() != null) return true;
        if (col.transform.parent != null && col.transform.parent.CompareTag("World")) return true;
        if (col.gameObject.name.StartsWith("Chunk")) return true;
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsTerrain(other)) return;
        if (_timeControl != null && _timeControl.isLost) return;
        if (_movement != null && _movement.IsInSpawn) return;

        if (Time.time - _lastRiskyTime >= MinTimeBetweenRiskyEvents)
        {
            RegisterRiskyEvent();
            _lastRiskyTime = Time.time;
            _isSkimming = true;
            _skimTimer = 0f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsTerrain(other)) return;
        if (_timeControl != null && _timeControl.isLost) return;
        if (_movement != null && _movement.IsInSpawn) return;

        if (_isSkimming)
        {
            _skimTimer += Time.deltaTime;
            if (_skimTimer >= ContinuousSkimInterval)
            {
                _skimTimer = 0f;
                _lastRiskyTime = Time.time;
                RegisterRiskyEvent();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsTerrain(other))
        {
            _isSkimming = false;
            _skimTimer = 0f;
        }
    }

    private void RegisterRiskyEvent()
    {
        if (_timeControl == null || _timeControl.isLost) return;
        if (_movement != null && _movement.IsInSpawn) return;

        if (_comboTimer > 0f)
        {
            CurrentCombo = Mathf.Min(CurrentCombo + 1, MaxMultiplier);
        }
        else
        {
            CurrentCombo = 1;
        }

        _comboTimer = ComboDuration;

        int bonus = RiskPointsNumber * CurrentMultiplier;
        _timeControl.AddRiskyScore(bonus);

        PlayRiskySound();
        UpdateComboUI();
    }

    private void PlayRiskySound()
    {
        if (RiskyAudioSource != null && RiskyAudioClip != null)
        {
            float targetPitch = Mathf.Clamp(1.0f + (CurrentMultiplier - 1) * 0.12f, 0.8f, 2.2f);
            RiskyAudioSource.pitch = targetPitch;
            RiskyAudioSource.PlayOneShot(RiskyAudioClip, 0.75f);
        }
    }

    private void UpdateComboUI()
    {
        if (ComboText == null) EnsureUI();
        if (ComboText == null) return;

        if (CurrentCombo <= 0)
        {
            ComboText.text = "";
            return;
        }

        if (CurrentMultiplier <= 2)
            _currentColor = new Color(0.2f, 1f, 1f); 
        else if (CurrentMultiplier <= 4)
            _currentColor = new Color(1f, 0.9f, 0.2f); 
        else if (CurrentMultiplier <= 6)
            _currentColor = new Color(1f, 0.5f, 0.1f); 
        else
            _currentColor = new Color(1f, 0.1f, 0.6f); 

        ComboText.color = _currentColor;

        if (CurrentMultiplier == 1)
        {
            ComboText.text = $"<size=75%>RIESGO!</size>\n<size=110%>+{RiskPointsNumber}</size>";
        }
        else
        {
            ComboText.text = $"<size=75%>RIESGO x{CurrentMultiplier}!</size>\n<size=110%>+{RiskPointsNumber * CurrentMultiplier}</size>";
        }

        _comboTextScale = 1.35f;
        ComboText.transform.localScale = Vector3.one * _comboTextScale;
    }

    public void ResetCombo()
    {
        CurrentCombo = 0;
        _comboTimer = 0f;
        _isSkimming = false;
        _skimTimer = 0f;

        if (ComboText != null)
        {
            ComboText.text = "";
            ComboText.transform.localScale = Vector3.one;
        }
    }

    void OnDisable()
    {
        ResetCombo();
    }
}
