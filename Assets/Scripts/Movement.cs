using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets;
using Assets.Generation;

public class Movement : MonoBehaviour
{
    public float Speed = 16;
    public float TurnSpeed = 3f;
    public TrailRenderer LeftTrail, RightTrail;
    public Color TrailColor;
    public Vector3 LeftPosition, RightPosition;
    public Material TrailMaterial;
    public AudioSource LeftSource, RightSource;
    public AudioClip SwooshClip;
    private GameObject Debris;
    private bool _lock;
    private float _leftTargetVolume = 1, _rightTargetVolume = 1;
    private float _originalVolume;
    private float _speed = 0;
    public Joystick joystick;
    public GameObject JoystickContainer;
    public Vector3 baseAcceleration;

    float hAxis;
    float vAxis;

    public bool IsInSpawn
    {
        get { return (transform.parent.position - WorldGenerator.SpawnPosition).sqrMagnitude < WorldGenerator.SpawnRadius * WorldGenerator.SpawnRadius; }
    }

    void Start()
    {
        // Inicialización de variables y componentes al comienzo del juego
        Debris = GameObject.FindGameObjectWithTag("Debris");
        LeftSource = GameObject.FindGameObjectWithTag("LeftSource").GetComponent<AudioSource>();
        RightSource = GameObject.FindGameObjectWithTag("RightSource").GetComponent<AudioSource>();
        _originalVolume = (RightSource.volume + LeftSource.volume) * .5f;
        baseAcceleration = Input.acceleration;
    }

    public void Lock()
    {
        // Bloquear el movimiento del jugador
        _lock = true;
    }

    public void Unlock()
    {
        // Desbloquear el movimiento del jugador
        _lock = false;
    }

    void Update()
    {
        // Actualización del juego en cada frame

        // Ajustar el volumen de los efectos de sonido de los trails
        LeftSource.volume = Mathf.Lerp(LeftSource.volume, _originalVolume * _leftTargetVolume, Time.deltaTime * 2f);
        RightSource.volume = Mathf.Lerp(RightSource.volume, _originalVolume * _rightTargetVolume, Time.deltaTime * 2f);

        // Detener el sonido si el volumen es muy bajo
        if (LeftSource.volume < 0.05f)
            LeftSource.Stop();

        if (RightSource.volume < 0.05f)
            RightSource.Stop();

        if (_lock)
            return;

        // Lerp para ajustar la velocidad gradualmente
        _speed = Mathf.Lerp(_speed, Speed, Time.deltaTime * .25f);
        transform.parent.position += transform.forward * Time.deltaTime * 4 * _speed;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 2.5f);

        // Ángulos de rotación para determinar si se activan los trails
        float zAngle = transform.localRotation.eulerAngles.z;
        float xAngle = transform.localRotation.eulerAngles.x;

        // Activar el trail izquierdo si se cumplen ciertas condiciones
        if (zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315)
        {
            StartTrail(ref LeftTrail, LeftPosition);
            LeftSource.transform.position = LeftPosition;
            LeftSource.clip = SwooshClip;
            _leftTargetVolume = 1;
            if (!LeftSource.isPlaying)
                LeftSource.Play();
        }
        else
        {
            StopTrail(ref LeftTrail);
            _leftTargetVolume = 0;
        }

        // Activar el trail derecho si se cumplen ciertas condiciones
        if (zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315)
        {
            StartTrail(ref RightTrail, RightPosition);
            RightSource.transform.position = RightPosition;
            RightSource.clip = SwooshClip;
            _rightTargetVolume = 1;
            if (!RightSource.isPlaying)
                RightSource.Play();
        }
        else
        {
            StopTrail(ref RightTrail);
            _rightTargetVolume = 0;
        }

        // Escala para ajustar el movimiento en función de la escala de tiempo
        float scale = (Time.timeScale != 1) ? (1 / Time.timeScale) * .5f : 1;
        int Controles = PlayerPrefs.GetInt("Control");

        // Obtener entrada del jugador en función del tipo de control seleccionado
        if (Controles == 1)
        {
            JoystickContainer = GameObject.FindGameObjectWithTag("Joystick");
            JoystickContainer.GetComponent<FixedJoystick>();
            hAxis = joystick.Horizontal;
            vAxis = joystick.Vertical;
        }
        else if (Controles == 2)
        {
            JoystickContainer = GameObject.FindGameObjectWithTag("Joystick");
            JoystickContainer.GetComponent<FixedJoystick>();
            hAxis = joystick.Horizontal;
            vAxis = joystick.Vertical;
        }
        else if (Controles == 3)
        {
            JoystickContainer = null;
            hAxis = Input.acceleration.x - baseAcceleration.x;
            vAxis = Input.acceleration.y - baseAcceleration.y;
        }

        // Rotación del jugador en función de la entrada del jugador
        if (Options.Invert)
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale * vAxis);
        else
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale * -vAxis);

        // Rotación y movimiento lateral del jugador en función de la entrada del jugador
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.forward * Time.deltaTime * 64f * TurnSpeed * scale * -hAxis);
        transform.parent.Rotate(-Vector3.up * Time.deltaTime * 64f * TurnSpeed * scale * -hAxis);
    }

    void StartTrail(ref TrailRenderer Trail, Vector3 Position)
    {
        // Iniciar el trail en una posición específica
        if (Trail != null)
            return;
        GameObject go = new GameObject("Trail");
        go.transform.parent = this.gameObject.transform;
        Trail = go.AddComponent<TrailRenderer>();
        Trail.widthMultiplier = .25f;
        Trail.endColor = new Color(0, 0, 0, 0);
        Trail.startColor = TrailColor;
        Trail.transform.localPosition = Position;
        Trail.material = TrailMaterial;
        Trail.time = 1.5f;
    }

    void StopTrail(ref TrailRenderer Trail)
    {
        // Detener el trail
        if (Trail == null)
            return;

        Trail.transform.parent = (Debris != null) ? Debris.transform : null;
        Destroy(Trail.gameObject, Trail.time + 1);
        Trail = null;
    }
}
