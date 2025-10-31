using UnityEngine;
using UnityEngine.UI;

public class ReconectarRegresarManager : MonoBehaviour
{
    void Start()
    {
        // Intentamos encontrar el botón "Regresar" dentro de este objeto persistente
        Transform tCanvas = transform.Find("Canvas");
        if (tCanvas == null)
        {
            Debug.LogWarning("Canvas no encontrado dentro de ObjetosEntreEscenas.");
            return;
        }

        Transform tRegresar = tCanvas.Find("Panel Opciones/Regresar");
        if (tRegresar == null)
        {
            Debug.LogWarning("Regresar no encontrado dentro de Canvas/Panel Opciones.");
            return;
        }

        Button botonRegresar = tRegresar.GetComponent<Button>();
        if (botonRegresar == null)
        {
            Debug.LogWarning("El GameObject 'Regresar' no tiene componente Button.");
            return;
        }

        // Removemos listeners previos (por si hay Missing)
        botonRegresar.onClick.RemoveAllListeners();

        // Buscar el panel global en la escena actual (menu principal)
        GameObject panelGlobal = GameObject.Find("Panel Global");

        botonRegresar.onClick.AddListener(() =>
        {
            // Ocultar panel de opciones (está dentro de este objeto)
            GameObject panelOpciones = tCanvas.Find("Panel Opciones")?.gameObject;
            if (panelOpciones != null) panelOpciones.SetActive(false);

            // Mostrar panel global de la escena principal
            if (panelGlobal != null) panelGlobal.SetActive(true);
            else Debug.LogWarning("Panel Global no encontrado en la escena actual.");
        });

        Debug.Log("✅ Botón Regresar reconectado desde ObjetosEntreEscenas.");
    }
}
