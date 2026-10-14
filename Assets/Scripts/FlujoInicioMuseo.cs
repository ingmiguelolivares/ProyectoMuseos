
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
    [SerializeField] private float tiempoControles = 10f;

    private Coroutine rutinaInicio;

    public void IniciarRecorrido()
    {
        if (rutinaInicio != null)
            return;

        if (panelGlobal == null || panelControles == null)
        {
            Debug.LogError("Asigna Panel Global y Panel_Controles en el Inspector.");
            return;
        }

        panelGlobal.SetActive(false);
        panelControles.SetActive(true);

        rutinaInicio = StartCoroutine(CargarMuseo());
    }

    private IEnumerator CargarMuseo()
    {
        yield return new WaitForSecondsRealtime(tiempoControles);

        if (!Application.CanStreamedLevelBeLoaded(nombreEscenaMuseo))
        {
            Debug.LogError(
                "No se encuentra la escena: " + nombreEscenaMuseo +
                ". Comprueba Build Profiles."
            );

            panelControles.SetActive(false);
            panelGlobal.SetActive(true);
            rutinaInicio = null;
            yield break;
        }

        SceneManager.LoadScene(nombreEscenaMuseo);
    }
}

