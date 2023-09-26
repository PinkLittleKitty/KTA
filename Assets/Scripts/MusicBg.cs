using UnityEngine;
using System.Collections;

public class MusicBg : MonoBehaviour
{
    // Instancia única de MusicBg (singleton)
    private static MusicBg instance = null;

    // Arreglo de clips de música de fondo
    public AudioClip[] musicbg;

    // Índice actual en la lista de reproducción
    private int i;

    // Propiedad estática para acceder a la instancia
    public static MusicBg Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        // Verifica si ya existe una instancia y la destruye si es necesario
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            // Establece esta instancia como la instancia única
            instance = this;
        }

        // Evita que este objeto se destruya al cambiar de escena
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // Inicializa el índice de la lista de reproducción con un valor aleatorio
        i = Random.Range(0, musicbg.Length);

        // Inicia la reproducción de la lista de reproducción
        StartCoroutine("Playlist");
    }

    // Corrutina para reproducir la lista de reproducción de música de fondo
    IEnumerator Playlist()
    {
        while (true)
        {
            // Espera un segundo antes de verificar la reproducción
            yield return new WaitForSeconds(1.0f);

            // Verifica si el AudioSource no está reproduciendo un clip
            if (!GetComponent<AudioSource>().isPlaying)
            {
                if (i != (musicbg.Length - 1))
                {
                    // Si no es el último clip, avanza al siguiente
                    i++;
                    GetComponent<AudioSource>().clip = musicbg[i];
                    GetComponent<AudioSource>().Play();
                }
                else
                {
                    // Si es el último clip, vuelve al primero
                    i = 0;
                    GetComponent<AudioSource>().clip = musicbg[i];
                    GetComponent<AudioSource>().Play();
                }
            }
        }
    }
}
