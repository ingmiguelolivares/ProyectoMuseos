
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlujoInicioMuseo : MonoBehaviour
{
    [Header("Paneles del menú")]
    [SerializeField] private GameObject panelGlobal;
    [SerializeField] private GameObject panelControles;

    [Header("Configuración del recorrido")]
    [SerializeField] private string nombreEscenaMuseo = "Museo SISIColombia";

    [Header("Tiempo de carga")]
    [SerializeField] private float tiempoPantallaCarga = 10f;

    // Evita que el recorrido se inicie varias veces simultáneamente.
    private Coroutine rutinaInicio;

    private void Awake()
    {
        // Conserva el Canvas de carga cuando se cambia de escena.
        DontDestroyOnLoad(gameObject);
    }

    public void IniciarRecorrido()
    {
        // Evita iniciar varias cargas con clics repetidos.
        if (rutinaInicio != null)
            return;

        // Verifica que los paneles estén asignados.
        if (panelGlobal == null || panelControles == null)
        {
            Debug.LogError(
                "FlujoInicioMuseo: asigna Panel Global y Panel_controles."
            );
            return;
        }

        // Comprueba que Unity pueda cargar la escena configurada.
        if (!Application.CanStreamedLevelBeLoaded(nombreEscenaMuseo))
        {
            Debug.LogError(
                "No se encuentra la escena: " + nombreEscenaMuseo +
                ". Comprueba el nombre en Build Profiles."
            );
            return;
        }

        // Oculta el menú y muestra la pantalla de carga.
        panelGlobal.SetActive(false);
        panelControles.SetActive(true);

        // Inicia el cambio de escena y mantiene el panel durante la espera.
        rutinaInicio = StartCoroutine(CargarMuseo());
    }

    private IEnumerator CargarMuseo()
    {
        // Evita que una pausa previa congele la carga.
        Time.timeScale = 1f;

        // Carga el museo sin bloquear la ejecución de la pantalla de carga.
        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(
            nombreEscenaMuseo,
            LoadSceneMode.Single
        );

        if (operacionCarga == null)
        {
            Debug.LogError("No fue posible cargar la escena del museo.");
            rutinaInicio = null;
            yield break;
        }

        // Espera a que Unity termine de cargar la escena.
        while (!operacionCarga.isDone)
            yield return null;

        Debug.Log(
            "Escena del museo cargada. Se mantendrá la pantalla de carga " +
            tiempoPantallaCarga + " segundos."
        );

        // Espera el tiempo configurado después de cargar la escena.
        // El Canvas persistente mantiene visible el panel durante este periodo.
        yield return new WaitForSecondsRealtime(tiempoPantallaCarga);

        // Oculta la pantalla de carga cuando termina la espera.
        if (panelControles != null)
            panelControles.SetActive(false);

        Debug.Log("Pantalla de carga cerrada. Comienza el recorrido.");

        // Permite volver a iniciar el recorrido si se regresa al menú.
        rutinaInicio = null;
    }
}

