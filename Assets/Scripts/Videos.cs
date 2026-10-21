
using UnityEngine;
using UnityEngine.Video;

public class Videos : MonoBehaviour
{
    // Referencia al componente encargado de reproducir el video.
    [Header("Configuración del video")]
    [SerializeField] private VideoPlayer video;

    // Referencia al componente que reproduce el audio del video.
    [SerializeField] private AudioSource audioVideo;

    // Permite ajustar el volumen desde el Inspector.
    // 0 = sin sonido y 1 = volumen máximo.
    [Header("Volumen del video")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenVideo = 1f;


    private void Awake()
    {
        // Busca el VideoPlayer en este objeto si no fue asignado manualmente.
        if (video == null)
        {
            video = GetComponent<VideoPlayer>();
        }

        // Si no encuentra el reproductor, muestra un error y desactiva el script.
        if (video == null)
        {
            Debug.LogError(
                "Videos: no se encontró el componente VideoPlayer.",
                this
            );

            enabled = false;
            return;
        }

        // Busca el AudioSource si no fue asignado manualmente.
        if (audioVideo == null)
        {
            audioVideo = GetComponent<AudioSource>();
        }

        // Configura el volumen inicial si existe un AudioSource.
        if (audioVideo != null)
        {
            audioVideo.volume = volumenVideo;
        }
        else
        {
            Debug.LogWarning(
                "Videos: no se encontró un AudioSource. " +
                "Comprueba la configuración de audio del VideoPlayer.",
                this
            );
        }

        // Evita que el video comience a reproducirse automáticamente.
        video.Stop();
    }


    private void OnTriggerEnter(Collider other)
    {
        // Reproduce el video cuando un objeto entra en el área de detección.
        video.Play();
    }


    private void OnTriggerExit(Collider other)
    {
        // Detiene el video cuando un objeto sale del área de detección.
        video.Stop();
    }
}
