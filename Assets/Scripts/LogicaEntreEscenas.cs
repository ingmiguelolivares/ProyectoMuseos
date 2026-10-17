
using UnityEngine;

public class LogicaEntreEscenas : MonoBehaviour
{
    private void Awake()
    {
        // Buscar otras instancias sin ordenar los resultados.
        LogicaEntreEscenas[] instancias =
            Object.FindObjectsByType<LogicaEntreEscenas>(
                FindObjectsSortMode.None
            );

        // Si ya existe otra instancia, destruir esta.
        if (instancias.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Mantener este objeto al cambiar de escena.
        DontDestroyOnLoad(gameObject);
    }
}

