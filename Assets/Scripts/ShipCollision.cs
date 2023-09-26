using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Generation;

public class ShipCollision : MonoBehaviour
{
    // Referencia al control del tiempo
    public TimeControl Control;

    // Referencia al modelo de la nave
    public GameObject Model;

    // Referencia al control de movimiento
    public Movement Controls;

    // Referencia al audio de choque
    public AudioSource CrashAudio;

    // Referencia al mundo del juego
    public World World;

    // Variable que indica si la nave está bloqueada (destruida)
    private bool _lock = false;

    void Start()
    {
        // Busca y asigna el objeto con la etiqueta "World" como el mundo del juego
        World = GameObject.FindGameObjectWithTag("World").GetComponent<World>();

        // Busca y asigna el objeto con la etiqueta "Modelo" como el modelo de la nave
        Model = GameObject.FindGameObjectWithTag("Modelo");
    }

    public void Reset()
    {
        // Reinicia el estado de bloqueo de la nave
        _lock = false;
    }

    void Update()
    {
        /*Vector3 BlockSpace = World.ToBlockSpace(transform.position);
        Chunk C = World.GetChunkAt(transform.position);
        if (C != null && C.GetBlockAt(BlockSpace) > 0)
        {
            DestroyShip();
        }*/
    }

    void DestroyShip()
    {
        if (_lock)
            return;

        // Bloquea los controles de la nave
        Controls.Lock();

        // Indica que el jugador ha perdido el juego
        Control.Lose();

        // Agrega un componente Explode al modelo de la nave para efecto de explosión
        Explode e = Model.AddComponent<Explode>();
        e.ExplosionAudio = CrashAudio;

        // Establece el estado de bloqueo como verdadero
        _lock = true;
    }

    // Se ejecuta cuando la nave colisiona con otro objeto
    void OnCollisionEnter(Collision col)
    {
        // Destruye la nave en caso de colisión
        DestroyShip();
    }
}
