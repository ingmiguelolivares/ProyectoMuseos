using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorOpciones : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pantallaOpciones;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private GameObject panelGlobal;

    [Header("Sliders")]
    [SerializeField] private Slider sliderBrillo;
    [SerializeField] private Slider sliderVolumen;

    [Header("Brillo")]
    [SerializeField] private Image panelBrillo;

    [Header("Teclado")]
    [SerializeField] private bool permitirTeclaP = true;

    [Header("Login")]
    [SerializeField] private GameObject panelLogin;

    private bool opcionesAbiertasDesdeMenu;

    private const string CLAVE_BRILLO = "brillo";
    private const string CLAVE_VOLUMEN = "volumen";

    private string NombreEscenaActual =>
        SceneManager.GetActiveScene().name;

    private bool EsMenuPrincipal =>
        NombreEscenaActual == "menu principal";

    private void Start()
    {
        CargarConfiguracion();

        // Detectar si las opciones se encuentran dentro del menú.
        opcionesAbiertasDesdeMenu = EsMenuPrincipal;

        // El panel de opciones debe comenzar cerrado.
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(false);
    }


    private void Update()
    {
        if (!permitirTeclaP)
            return;

        // Si el panel de login está activo, ignoramos la tecla P.
        if (panelLogin != null && panelLogin.activeInHierarchy)
            return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            AlternarPausa();
        }
    }


    // La tecla P abre o cierra las opciones según la escena.
    public void AlternarPausa()
    {
        if (pantallaOpciones == null)
        {
            Debug.LogWarning(
                "ControladorOpciones: asigna Pantalla Opciones en el Inspector."
            );
            return;
        }

        if (pantallaOpciones.activeSelf)
        {
            Volver();
        }
        else
        {
            if (EsMenuPrincipal)
                AbrirOpcionesMenu();
            else
                AbrirPausaMuseo();
        }
    }

    // Botón Opciones del menú principal.
    public void AbrirOpcionesMenu()
    {
        opcionesAbiertasDesdeMenu = true;

        if (panelGlobal == null)
            panelGlobal = GameObject.Find("Panel Global");

        if (panelGlobal != null)
            panelGlobal.SetActive(false);

        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(true);

        // En el menú no necesitamos pausar la escena.
        Time.timeScale = 1f;
    }

    // Abrir pausa durante el recorrido 3D.
    public void AbrirPausaMuseo()
    {
        opcionesAbiertasDesdeMenu = false;

        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(true);

        Time.timeScale = 0f;
    }

    // Botón Volver: regresa al menú o reanuda el museo.
    public void Volver()
    {
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(false);

        if (panelControles != null)
            panelControles.SetActive(false);

        Time.timeScale = 1f;

        if (EsMenuPrincipal && opcionesAbiertasDesdeMenu)
        {
            if (panelGlobal != null)
                panelGlobal.SetActive(true);
        }
    }

    // Botón Ver controles.
    public void MostrarControles()
    {
        if (panelControles == null)
        {
            Debug.LogWarning(
                "ControladorOpciones: asigna Panel Controles en el Inspector."
            );
            return;
        }

        panelControles.SetActive(!panelControles.activeSelf);
    }

    // Slider de brillo.
    public void CambiarBrillo(float valor)
    {
        valor = Mathf.Clamp01(valor);

        PlayerPrefs.SetFloat(CLAVE_BRILLO, valor);
        AplicarBrillo(valor);
    }

    private void AplicarBrillo(float valor)
    {
        if (panelBrillo == null)
            return;

        Color color = panelBrillo.color;
        color.a = valor;
        panelBrillo.color = color;
    }

    // Slider de volumen.
    public void CambiarVolumen(float valor)
    {
        valor = Mathf.Clamp01(valor);

        AudioListener.volume = valor;
        PlayerPrefs.SetFloat(CLAVE_VOLUMEN, valor);
    }

    // Cargar la configuración guardada.
    private void CargarConfiguracion()
    {
        float brilloGuardado =
            PlayerPrefs.GetFloat(CLAVE_BRILLO, 0.5f);

        float volumenGuardado =
            PlayerPrefs.GetFloat(CLAVE_VOLUMEN, 1f);

        if (sliderBrillo != null)
            sliderBrillo.SetValueWithoutNotify(brilloGuardado);

        if (sliderVolumen != null)
            sliderVolumen.SetValueWithoutNotify(volumenGuardado);

        AplicarBrillo(brilloGuardado);
        AudioListener.volume = volumenGuardado;
    }

    // Botón Salir.
    public void Salir()
    {
        Time.timeScale = 1f;

        Application.Quit();

        Debug.Log(
            "Solicitud de salida. En Unity Editor, detén Play manualmente."
        );
    }
}
