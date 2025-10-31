using UnityEngine;
using UnityEngine.UI;

public class ReconectarOpciones : MonoBehaviour
{
    void Start()
    {
        // Buscar el botón de opciones en la escena por nombre
        Button botonOpciones = GameObject.Find("Boton opciones")?.GetComponent<Button>();

        if (botonOpciones != null)
        {
            // Limpiar eventos anteriores (por si hay Missing Object)
            botonOpciones.onClick.RemoveAllListeners();

            // Buscar panel global (menu principal)
            GameObject panelGlobal = GameObject.Find("Panel Global");

            // Buscar el objeto que no se destruye entre escenas
            GameObject objetosEntreEscenas = GameObject.Find("ObjetosEntreEscenas");

            if (objetosEntreEscenas != null)
            {
                // Ajusta la ruta si en tu jerarquía es distinta
                Transform tPanelOpciones = objetosEntreEscenas.transform.Find("Canvas/Panel Opciones");

                if (tPanelOpciones != null)
                {
                    GameObject panelOpciones = tPanelOpciones.gameObject;

                    // Agregar el listener para mostrar opciones
                    botonOpciones.onClick.AddListener(() =>
                    {
                        if (panelGlobal != null) panelGlobal.SetActive(false);
                        panelOpciones.SetActive(true);
                    });

                    Debug.Log("✅ Botón Opciones reconectado correctamente.");
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
            Debug.LogWarning("⚠ No se encontró el GameObject 'Boton opciones' con componente Button. Revisa el nombre exacto.");
        }
    }
}
