using UnityEngine;
using UnityEngine.Video;

public class Videos : MonoBehaviour
{
    // Referencia al componente encargado de reproducir el video.
    // Se puede asignar manualmente desde el Inspector de Unity.
    [Header("Configuración del video")]
    [SerializeField] private VideoPlayer video;


    private void Awake()
    {
        // Si no se asignó un VideoPlayer desde el Inspector,
        // intenta encontrarlo en el mismo objeto que contiene este script.
        if (video == null)
        {
            video = GetComponent<VideoPlayer>();
        }

        // Comprueba si se encontró el reproductor.
        // Si no existe, muestra un mensaje de error y desactiva
        // el script para evitar errores al intentar reproducir el video.
        if (video == null)
        {
            Debug.LogError(
                "Videos: no se encontró el componente VideoPlayer. " +
                "Añádelo al objeto o asígnalo en el Inspector.",
                this
            );

            enabled = false;
            return;
        }

        // Detiene la reproducción al iniciar para evitar
        // que el video comience automáticamente.
        video.Stop();
    }


    private void OnTriggerEnter(Collider other)
    {
        // Se ejecuta cuando otro objeto entra en el área
        // delimitada por el Collider configurado como Trigger.
        // Si el reproductor existe, inicia la reproducción del video.
        if (video != null)
        {
            video.Play();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        // Se ejecuta cuando otro objeto sale del área de detección.
        // Si el reproductor existe, detiene la reproducción del video.
        if (video != null)
        {
            video.Stop();
        }
    }
}
