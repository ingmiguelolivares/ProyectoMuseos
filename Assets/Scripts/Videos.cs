using UnityEngine;
using UnityEngine.Video;

public class Videos : MonoBehaviour
{
    [Header("Componentes")]
    public VideoPlayer video;
    public AudioSource audioSource3D;

    [Header("Configuración")]
    public bool reiniciarAlSalir = true;

    private bool jugadorDentro = false;

    void Start()
    {
        // Obtener componentes
        if (video == null)
            video = GetComponent<VideoPlayer>();

        if (audioSource3D == null)
            audioSource3D = GetComponent<AudioSource>();

        // Configurar VideoPlayer
        video.playOnAwake = false;
        video.skipOnDrop = true;

        #if UNITY_WEBGL
        // En WebGL: Usar Audio Source separado para audio 3D
        video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        video.SetTargetAudioSource(0, audioSource3D);
        
        // Configurar Audio Source para sonido 3D
        if (audioSource3D != null)
        {
            audioSource3D.spatialBlend = 1.0f; // 100% 3D
            audioSource3D.playOnAwake = false;
        }
        #else
        // En Editor: Direct mode funciona bien
        video.audioOutputMode = VideoAudioOutputMode.Direct;
        #endif

        video.Stop();

        Debug.Log("VideoPlayer configurado para: " +
        #if UNITY_WEBGL
            "WebGL con Audio 3D"
        #else
            "Editor"
        #endif
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EsJugador(other))
        {
            jugadorDentro = true;
            video.Play();
            Debug.Log("Video iniciado");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (EsJugador(other))
        {
            jugadorDentro = false;

            if (reiniciarAlSalir)
            {
                video.Stop();
                Debug.Log("Video detenido");
            }
            else
            {
                video.Pause();
                Debug.Log("Video pausado");
            }
        }
    }

    bool EsJugador(Collider other)
    {
        return other.CompareTag("Player") ||
               other.GetComponent<CharacterController>() != null;
    }
}