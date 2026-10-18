
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Configuración de escenas")]
    [SerializeField] private string nombreEscenaMenu = "menu principal";

    // Indica si las opciones se abrieron desde el menú o desde el museo.
    private bool opcionesAbiertasDesdeMenu;

    // Evita iniciar varias cargas de la escena del menú simultáneamente.
    private bool regresandoAlLogin;

    // Nombres usados para guardar las preferencias del usuario.
    private const string CLAVE_BRILLO = "brillo";
    private const string CLAVE_VOLUMEN = "volumen";

    // Obtiene el nombre de la escena que está activa actualmente.
    private string NombreEscenaActual =>
        SceneManager.GetActiveScene().name;

    // Comprueba si el usuario está en la escena del menú principal.
    private bool EsMenuPrincipal =>
        NombreEscenaActual == nombreEscenaMenu;

    private void Start()
    {
        // Recupera y aplica el brillo y el volumen guardados.
        CargarConfiguracion();

        // Determina desde dónde se abrieron las opciones.
        opcionesAbiertasDesdeMenu = EsMenuPrincipal;

        // El panel de opciones comienza oculto.
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(false);
    }

    private void Update()
    {
        // Permite desactivar el atajo de teclado desde el Inspector.
        if (!permitirTeclaP)
            return;

        // Evita abrir la pausa mientras se escribe en el Login.
        if (panelLogin != null && panelLogin.activeInHierarchy)
            return;

        // La tecla P abre o cierra las opciones.
        if (Input.GetKeyDown(KeyCode.P))
            AlternarPausa();
    }

    // Abre o cierra las opciones mediante la tecla P.
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

    // Se conecta al botón Opciones del menú principal.
    public void AbrirOpcionesMenu()
    {
        opcionesAbiertasDesdeMenu = true;

        // Busca el panel global si no se asignó desde el Inspector.
        if (panelGlobal == null)
            panelGlobal = GameObject.Find("Panel Global");

        // Oculta el menú mientras se muestran las opciones.
        if (panelGlobal != null)
            panelGlobal.SetActive(false);

        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(true);

        // Mantiene el tiempo normal en el menú.
        Time.timeScale = 1f;
    }

    // Abre el panel de pausa durante el recorrido del museo.
    public void AbrirPausaMuseo()
    {
        opcionesAbiertasDesdeMenu = false;

        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(true);

        // Pausa el movimiento y el resto de sistemas que usan el tiempo del juego.
        Time.timeScale = 0f;
    }

    // Botón Volver: cierra las opciones y reanuda el museo o el menú.
    public void Volver()
    {
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(false);

        // Cierra el panel de controles si estaba abierto.
        if (panelControles != null)
            panelControles.SetActive(false);

        // Restablece el tiempo normal.
        Time.timeScale = 1f;

        // Si las opciones se abrieron desde el menú, vuelve al panel global.
        if (EsMenuPrincipal && opcionesAbiertasDesdeMenu)
        {
            if (panelGlobal != null)
                panelGlobal.SetActive(true);
        }
    }

    // Botón Ver controles: muestra u oculta el panel de controles.
    public void MostrarControles()
    {
        if (panelControles == null)
        {
            Debug.LogWarning(
                "ControladorOpciones: asigna Panel Controles en el Inspector."
            );
            return;
        }

        // Alterna la visibilidad del panel de controles.
        bool mostrar = !panelControles.activeSelf;
        panelControles.SetActive(mostrar);

        // Si los controles están abiertos, oculta las opciones.
        // Al cerrar los controles, vuelve a mostrar las opciones.
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(!mostrar);

        // Evita que el museo quede pausado al consultar los controles.
        Time.timeScale = 1f;
    }

    // Se ejecuta cuando cambia el valor del slider de brillo.
    public void CambiarBrillo(float valor)
    {
        valor = Mathf.Clamp01(valor);

        // Guarda el valor para futuras sesiones.
        PlayerPrefs.SetFloat(CLAVE_BRILLO, valor);
        PlayerPrefs.Save();

        // Aplica el brillo visual.
        AplicarBrillo(valor);
    }

    // Modifica la transparencia del panel que controla el brillo.
    private void AplicarBrillo(float valor)
    {
        if (panelBrillo == null)
            return;

        Color color = panelBrillo.color;
        color.a = valor;
        panelBrillo.color = color;
    }

    // Se ejecuta cuando cambia el valor del slider de volumen.
    public void CambiarVolumen(float valor)
    {
        valor = Mathf.Clamp01(valor);

        // Modifica el volumen general del audio.
        AudioListener.volume = valor;

        // Guarda la configuración para la siguiente sesión.
        PlayerPrefs.SetFloat(CLAVE_VOLUMEN, valor);
        PlayerPrefs.Save();
    }

    // Recupera la configuración guardada al iniciar el controlador.
    private void CargarConfiguracion()
    {
        float brilloGuardado =
            PlayerPrefs.GetFloat(CLAVE_BRILLO, 0.5f);

        float volumenGuardado =
            PlayerPrefs.GetFloat(CLAVE_VOLUMEN, 1f);

        // Actualiza los sliders sin disparar sus eventos.
        if (sliderBrillo != null)
            sliderBrillo.SetValueWithoutNotify(brilloGuardado);

        if (sliderVolumen != null)
            sliderVolumen.SetValueWithoutNotify(volumenGuardado);

        // Aplica los valores recuperados.
        AplicarBrillo(brilloGuardado);
        AudioListener.volume = volumenGuardado;
    }

    // Botón Salir: vuelve al menú inicial y muestra el LoginCanvas.
    public void Salir()
    {
        // Evita que dos clics soliciten dos cargas simultáneas.
        if (regresandoAlLogin)
            return;

        StartCoroutine(RegresarAlLogin());
    }

    // Carga la escena inicial y muestra el panel de inicio de sesión.
    private IEnumerator RegresarAlLogin()
    {
        regresandoAlLogin = true;

        // Restablece el tiempo por si el museo estaba pausado.
        Time.timeScale = 1f;

        // Oculta los paneles de opciones antes de cambiar de escena.
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(false);

        if (panelControles != null)
            panelControles.SetActive(false);

        // Carga la escena que contiene el LoginCanvas.
        AsyncOperation carga = SceneManager.LoadSceneAsync(nombreEscenaMenu);

        if (carga == null)
        {
            Debug.LogError(
                "No se pudo cargar la escena inicial: " + nombreEscenaMenu
            );

            regresandoAlLogin = false;
            yield break;
        }

        // Espera a que termine la transición a la escena inicial.
        while (!carga.isDone)
            yield return null;

        // Espera un frame para permitir que Unity inicialice los objetos de la escena.
        yield return null;

        // Busca el panel de inicio de sesión por su nombre.
        if (panelLogin == null)
            panelLogin = GameObject.Find("LoginCanvas");

        if (panelLogin != null)
        {
            // Muestra el inicio de sesión.
            panelLogin.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "No se encontró LoginCanvas. Comprueba su nombre en la jerarquía."
            );
        }

        // Deja el menú preparado para una nueva interacción.
        regresandoAlLogin = false;
    }


    // Botón de cierre del panel de controles.
    public void CerrarControles()
    {
        // Cierra el panel de controles si está asignado.
        if (panelControles != null)
            panelControles.SetActive(false);

        // Si las opciones estaban abiertas, vuelve a mostrarlas.
        if (pantallaOpciones != null)
            pantallaOpciones.SetActive(true);

        // Restaura el tiempo normal del juego.
        Time.timeScale = 1f;
    }


}
