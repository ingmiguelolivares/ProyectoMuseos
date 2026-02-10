using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReconectarRegresar : MonoBehaviour
{
    void Start()
    {
        // Buscar el botón "Regresar" en la escena
        Button botonRegresar = GameObject.Find("Regresar")?.GetComponent<Button>();

        if (botonRegresar != null)
        {
            // Limpiar eventos anteriores (por si hay Missing Object)
            botonRegresar.onClick.RemoveAllListeners();

            // Buscar el objeto persistente entre escenas
            GameObject objetosEntreEscenas = GameObject.Find("ObjetosEntreEscenas");

            // Buscar panel global (del menú principal)
            GameObject panelGlobal = GameObject.Find("Panel Global");

            if (objetosEntreEscenas != null)
            {
                // Buscar el panel de opciones dentro del objeto persistente
                Transform tPanelOpciones = objetosEntreEscenas.transform.Find("Canvas/Panel Opciones");

                if (tPanelOpciones != null)
                {
                    GameObject panelOpciones = tPanelOpciones.gameObject;

                    // Agregar acción al botón
                    botonRegresar.onClick.AddListener(() =>
                    {
                        Debug.Log("🔁 Clic en botón Regresar.");

                        // Cerrar panel de opciones
                        panelOpciones.SetActive(false);

                        // Si estamos en el menú principal, mostrar panel global
                        if (panelGlobal != null)
                        {
                            panelGlobal.SetActive(true);
                            Debug.Log("✅ Panel Global activado.");
                        }
                        else
                        {
                            // Si no está, volver al menú principal
                            Debug.Log("↩ No se encontró Panel Global, cargando escena 'menu principal'...");
                            SceneManager.LoadScene("menu principal");
                            GameObject.DontDestroyOnLoad(gameObject);
                            gameObject.AddComponent<EsperarPanelGlobal>();
                        }
                    });

                    Debug.Log("✅ Botón Regresar reconectado correctamente.");
                }
                else
                {
                    Debug.LogWarning("⚠ No se encontró 'Canvas/Panel Opciones' dentro de ObjetosEntreEscenas. Revisa nombres y rutas.");
                }
            }
            else
            {
                Debug.LogWarning("⚠ No se encontró el objeto 'ObjetosEntreEscenas' en la escena.");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No se encontró el GameObject 'Regresar' con componente Button. Revisa el nombre exacto.");
        }
    }
}

// Clase auxiliar para esperar el Panel Global después de cargar el menú
public class EsperarPanelGlobal : MonoBehaviour
{
    IEnumerator Start()
    {
        // Esperar un momento hasta que el menú principal esté cargado
        yield return new WaitForSeconds(0.5f);

        GameObject panelGlobal = null;
        while (panelGlobal == null)
        {
            panelGlobal = GameObject.Find("Panel Global");
            yield return null;
        }

        panelGlobal.SetActive(true);
        Debug.Log("✅ Panel Global encontrado y activado tras volver al menú.");

        Destroy(this); // eliminar el componente auxiliar
    }
}
