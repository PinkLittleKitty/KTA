using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Generation;
using Assets;

public class TimeControl : MonoBehaviour
{
	// Referencia al elemento UI que muestra la barra de tiempo.
    public RectTransform timeBar;

	// Referencia al elemento UI que muestra la barra de tiempo.
    public float energyLeft = 100;

	// Referencia al elemento UI que muestra la barra de tiempo.
    public float energyUsage = 8;

	// Indica si el jugador está utilizando energía.
    public bool isUsing;

	// Indica si el botón de reiniciar fue presionado previamente.
    private bool wasPressed;

	// Referencia a la cámara en el juego.
    public Camera view;

	// Referencia al texto que muestra la puntuación.
    public Text scoreText, scoreCenterText;

	// Referencia al texto que muestra la puntuación más alta.
    public Text highScoreText;

	// Puntuación actual del jugador.
    public float score;

	// Indica si el jugador perdió el juego.
    public bool isLost = true;
	
    // Referencia al texto que muestra "Game Over".
    public Text gameOverText;
	    
    // Referencia al texto que muestra el título del juego.
    public Text titleText;

	// Referencia al botón de reinicio.
    public Text restartButton, startButton;

	// Prefab del jugador.
    public GameObject playerPrefab;

	// Prefab del jugador con controles a la izquierda.
    public GameObject playerPrefabLeft;

	// Prefab del jugador con controles a la derecha.
    public GameObject playerPrefabRight;

	// Prefab del jugador con controles giroscópicos.
    public GameObject playerPrefabGyro;

	// Referencia al joystick del juego.
    public GameObject joystick;
    public GameObject TutorialGO;

	// Referencia al componente de audio.
    public AudioSource sound;

	// Clip de sonido para el juego.
    public AudioClip gameOverClip;

	// Texto de invertir controles.
    public Text invertText;

	// Imagen de verificación de invertir controles.
    public Image invertCheck;

	// Botón de opciones.
    public Button optionsButton;

	// Toggle para invertir controles.
    public Toggle invertToggle;

	// Referencia al componente de movimiento.
    private Movement movement;

	// Objetivo de la animación para el estado de "Game Over".
    private float targetGameOver;

	// Objetivo de la animación para el botón de reinicio.
    private float targetRestart;

	// Objetivo de la animación para la puntuación.
    private float targetScore;

	// Objetivo de la animación para el botón de inicio.
    private float targetStart;

	// Objetivo de la animación para el título del juego.
    private float targetTitle;

	// Objetivo de la animación para el tono de audio.
    private float targetPitch = 1;
    
    // Objetivo de la animación para la inversión de controles.
    private float targetInvert;

	// Referencia al objeto de la cámara.
    public GameObject cameraObject;

	// Referencia a la cámara virtual.
    public Cinemachine.CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        // Inicializar el estado del juego como perdido.
        isLost = true;
        
        // Establecer la escala de tiempo inicial.
        Time.timeScale = 0.25f;
        
        // Configurar objetivos iniciales para las animaciones.
        targetStart = 1;
        targetTitle = 1;
        
        // Iniciar la animación de inicio.
        StartCoroutine(PlayAnim());
        
        // Hacer el cursor visible.
        Cursor.visible = true;
        
        // Obtener la puntuación más alta almacenada en PlayerPrefs.
        highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        
        // Configurar la inversión de controles basada en PlayerPrefs.
        Options.Invert = PlayerPrefs.GetInt("Invertir") == 0 ? true : false;
        
        // Configurar el estado del toggle de inversión de controles.
        invertToggle.isOn = PlayerPrefs.GetInt("Invertir") == 0 ? true : false;
        
        // Guardar la configuración.
        PlayerPrefs.Save();
    }

    public void Lose()
    {
        // Indicar que el jugador ha perdido.
        isLost = true;
        
        // Ajustar la escala de tiempo.
        Time.timeScale = 0.25f;
        
        // Establecer el objetivo para la animación de "Game Over".
        targetGameOver = 1f;
        
        // Establecer el objetivo para la animación de puntuación.
        targetScore = 1f;
        
        // Encontrar el objeto Tutorial en la jerarquía.
        TutorialGO = GameObject.FindGameObjectWithTag("OwO");
        
        // Iniciar la rutina para la animación de pérdida.
        StartCoroutine(LostCoroutine());
        
        // Establecer el clip de sonido para el juego perdido.
        GetComponent<AudioSource>().clip = gameOverClip;
        
        // Reproducir el sonido de juego perdido.
        GetComponent<AudioSource>().Play();
        
        // Mostrar el botón de opciones.
        optionsButton.gameObject.SetActive(true);
    }

    
    IEnumerator LostCoroutine()
    {
        // Hacer vibrar el dispositivo móvil.
        Handheld.Vibrate();
        
        // Si existe un objeto "Joystick", desactivarlo.
        if (GameObject.FindWithTag("Joystick") != null)
        {
            joystick = GameObject.FindGameObjectWithTag("Joystick");
            joystick.gameObject.SetActive(false);
        }
        
        // Esperar un tiempo antes de ocultar "Game Over".
        yield return new WaitForSeconds(3 / (1 / Time.timeScale));
        
        // Restablecer el objetivo para la animación de "Game Over".
        targetGameOver = 0;

        while (isLost)
        {
            // Esperar un breve tiempo antes de mostrar y ocultar el botón de reinicio.
            yield return new WaitForSeconds(1 / (1 / Time.timeScale));
            targetRestart = 1;
            yield return new WaitForSeconds(1 / (1 / Time.timeScale));
            targetRestart = 0;
        }
    }

    IEnumerator PlayAnim()
    {
		// Esperar un breve tiempo antes de mostrar y ocultar el botón de inicio.
        while (isLost)
        {
            yield return new WaitForSeconds(0.5f / (1 / Time.timeScale));
            targetStart = 1;
            yield return new WaitForSeconds(0.5f / (1 / Time.timeScale));
            targetStart = 0;
        }
    }

    public void StartGame()
    {
		// Iniciar el juego nuevamente.
        Restart();
    }

    public void Restart()
    {
        if (!isLost)
            return;
		// Iniciar la rutina para reiniciar el juego.
        StartCoroutine(RestartCoroutine());
    }

    IEnumerator RestartCoroutine()
    {
        // Restablecer los objetivos de animación.
        targetStart = 0;
        targetRestart = 0;
        targetTitle = 0;
        
        // Indicar que el juego no está perdido.
        isLost = false;
        
        // Restablecer la escala de tiempo.
        Time.timeScale = 1f;
        
        // Restablecer la puntuación.
        score = 0;
        targetScore = 0;
        
        // Restablecer la energía del jugador.
        energyLeft = 100;
        
        // Destruir al jugador y los escombros existentes.
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        Destroy(GameObject.FindGameObjectWithTag("Debris"));
        
        // Ocultar el botón de opciones.
        optionsButton.gameObject.SetActive(false);
        
        // Determinar qué tipo de jugador instanciar.
        wichGameobject();
        // Crear un objeto "Debris" para los escombros.
        GameObject debris = new GameObject("Debris");
        debris.tag = "Debris";
        
        // Cargar un nuevo ruido de terreno.
        OpenSimplexNoise.Load(Random.Range(int.MinValue, int.MaxValue));

        // Obtener una referencia al objeto "World".
        World world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        Chunk[] chunks = null;
        
        // Bloquear la lista de fragmentos para su manipulación.
        lock (world.Chunks)
        {
            chunks = world.Chunks.Values.ToList().ToArray();
        }
        
        // Eliminar los fragmentos existentes.
        for (int i = 0; i < chunks.Length; i++)
        {
            world.RemoveChunk(chunks[i]);
        }
        
        // Instanciar un nuevo objeto de jugador.
        GameObject go = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);
        
        // Establecer el jugador como el objeto de jugador en el mundo.
        world.Player = go;
        
        // Obtener el componente de movimiento del jugador.
        movement = go.GetComponentInChildren<Movement>();
        
        // Configurar el control de colisión del jugador.
        go.GetComponent<ShipCollision>().Control = this.GetComponent<TimeControl>();
        
        // Obtener la cámara principal y configurarla para seguir al jugador.
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowShip>().TargetShip = go;
        
        // Configurar la cámara para cargar fragmentos en el área cercana al jugador.
        cameraObject.GetComponent<ChunkLoader>().Player = go;
        
        // Configurar la cámara virtual para seguir al jugador.
        virtualCamera.m_LookAt = go.transform;
        virtualCamera.m_Follow = go.transform;
        
        // Establecer el clip de sonido para iniciar el juego nuevamente.
        GetComponent<AudioSource>().clip = gameOverClip;
        
        // Reproducir el sonido de inicio del juego.
        GetComponent<AudioSource>().Play();
        
        yield return null;
    }


    void Update()
    {
        // Verificar si el juego está perdido y se presionó la tecla Espacio para reiniciar.
        if (isLost && Input.GetKeyDown(KeyCode.Space))
            Restart();

        // Actualizar el texto de la puntuación.
        scoreText.text = ((int)score).ToString();
        scoreCenterText.text = scoreText.text;

        // Calcular el objetivo de la animación para la inversión de controles.
        targetInvert = Mathf.Min(1, targetScore + targetTitle);
        
        // Realizar una interpolación lineal para el tono del audio.
        sound.pitch = Mathf.Lerp(sound.pitch, targetPitch, Time.deltaTime * 8f);
        
        // Ajustar la visibilidad y transparencia de los elementos UI mediante interpolación.
        titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, Mathf.Lerp(titleText.color.a, targetTitle, Time.deltaTime * 4f * (1 / Time.timeScale)));
        startButton.color = new Color(startButton.color.r, startButton.color.g, startButton.color.b, Mathf.Lerp(startButton.color.a, targetStart, Time.deltaTime * 4f * (1 / Time.timeScale)));
        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, Mathf.Lerp(gameOverText.color.a, targetGameOver, Time.deltaTime * 4f * (1 / Time.timeScale)));
        restartButton.color = new Color(restartButton.color.r, restartButton.color.g, restartButton.color.b, Mathf.Lerp(restartButton.color.a, targetRestart, Time.deltaTime * 4f * (1 / Time.timeScale)));
        invertToggle.targetGraphic.color = new Color(invertToggle.targetGraphic.color.r, invertToggle.targetGraphic.color.g, invertToggle.targetGraphic.color.b, Mathf.Lerp(invertToggle.targetGraphic.color.a, targetInvert, Time.deltaTime * 4f * (1 / Time.timeScale)));
        invertText.color = new Color(invertText.color.r, invertText.color.g, invertText.color.b, Mathf.Lerp(invertText.color.a, targetInvert, Time.deltaTime * 4f * (1 / Time.timeScale)));
        optionsButton.image.color = new Color(optionsButton.image.color.r, optionsButton.image.color.g, optionsButton.image.color.b, Mathf.Lerp(optionsButton.image.color.a, targetInvert, Time.deltaTime * 4f * (1 / Time.timeScale)));
        invertCheck.color = new Color(invertCheck.color.r, invertCheck.color.g, invertCheck.color.b, Mathf.Lerp(invertCheck.color.a, targetInvert, Time.deltaTime * 4f * (1 / Time.timeScale)));
        
        // Si el título no está completamente visible, ajustar la transparencia del texto de puntuación.
        if (targetTitle != 1)
        {
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, Mathf.Lerp(scoreText.color.a, 1 - targetScore, Time.deltaTime * 2f * (1 / Time.timeScale)));
            scoreCenterText.color = new Color(scoreCenterText.color.r, scoreCenterText.color.g, scoreCenterText.color.b, Mathf.Lerp(scoreCenterText.color.a, targetScore, Time.deltaTime * 2f * (1 / Time.timeScale)));
        }
        
        // Si el juego está perdido, detener la actualización.
        if (isLost)
            return;

        // Detectar si se tocan más de una pantalla y se está utilizando energía.
        if (Input.touchCount > 1 && energyLeft > 0 && !wasPressed)
        {
            energyLeft -= Time.deltaTime * energyUsage * (1 / Time.timeScale);
            energyLeft = Mathf.Clamp(energyLeft, 0, 100);
            isUsing = true;
        }
        else
        {
            energyLeft += Time.deltaTime * energyUsage * 0.5f;
            energyLeft = Mathf.Clamp(energyLeft, 0, 100);
            isUsing = false;
        }
        
        // Actualizar el tamaño de la barra de tiempo.
        timeBar.sizeDelta = Lerp(timeBar.sizeDelta, new Vector2(energyLeft - 0.5f, timeBar.sizeDelta.y), Time.deltaTime * 6f);
        
        // Ajustar la escala de tiempo y el tono de audio según si se está utilizando energía.
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
        
        // Registrar si se presiona la tecla Espacio para no gastar energía.
        if (!isUsing)
            wasPressed = Input.GetKey(KeyCode.Space);

        // Si el jugador no está en el proceso de generación de terreno, aumentar la puntuación con el tiempo.
        if (!movement.IsInSpawn)
            score += Time.deltaTime * 8;
        
        // Ajustar la velocidad del jugador según la puntuación.
        if (score < 125)
            movement.Speed = 12;
        else if (score < 275)
            movement.Speed = 14;
        else if (score < 500)
            movement.Speed = 16;
        else if (score < 1000)
            movement.Speed = 18;

        // Actualizar la puntuación más alta en PlayerPrefs si se supera.
        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {
            int intScore = (int)score;
            PlayerPrefs.SetInt("HighScore", intScore);
            highScoreText.text = intScore.ToString();
            PlayerPrefs.Save();
        }

        // Guardar la configuración en PlayerPrefs.
        PlayerPrefs.Save();
    }

    public void InvertControls()
    {
        // Cambiar la configuración de inversión de controles.
        Options.Invert = !Options.Invert;
    }

    Vector2 Lerp(Vector2 a, Vector2 b, float d)
    {
        // Realizar una interpolación lineal entre dos vectores.
        return new Vector2(Mathf.Lerp(a.x, b.x, d), Mathf.Lerp(b.x, b.y, d));
    }

     public void wichGameobject()
    {
        // Determinar qué objeto de jugador instanciar según la configuración.
        int controlType = PlayerPrefs.GetInt("Control");
        if (controlType == 1)
        {
            playerPrefab = playerPrefabLeft;
        }
        else if (controlType == 2)
        {
            playerPrefab = playerPrefabRight;
        }
        else if (controlType == 3)
        {
            playerPrefab = playerPrefabGyro;
        }
    }
}
