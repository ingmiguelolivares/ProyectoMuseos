
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Photon.Pun;
using System;
using System.Collections;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("Referencias de la interfaz")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject panelGlobal;
    [SerializeField] private TMP_InputField nombreInput;
    [SerializeField] private Button entrarButton;
    [SerializeField] private TMP_Text mensajeText;

    private DatabaseReference databaseReference;
    private FirebaseAuth firebaseAuth;

    private bool firebaseListo = false;
    private bool procesandoLogin = false;

    private void Start()
    {
        // Comprueba que todas las referencias de la interfaz estén asignadas.
        if (loginPanel == null)
            Debug.LogError("LoginManager: falta asignar Login Panel.", this);

        if (panelGlobal == null)
            Debug.LogError("LoginManager: falta asignar Panel Global.", this);

        if (nombreInput == null)
            Debug.LogError("LoginManager: falta asignar Nombre Input.", this);

        if (entrarButton == null)
            Debug.LogError("LoginManager: falta asignar Entrar Button.", this);

        if (mensajeText == null)
            Debug.LogError("LoginManager: falta asignar Mensaje Text.", this);

        // Registra el evento del botón Continuar una sola vez.
        if (entrarButton != null)
        {
            entrarButton.onClick.RemoveListener(OnEntrarClicked);
            entrarButton.onClick.AddListener(OnEntrarClicked);
            entrarButton.interactable = false;
        }

        // Informa al visitante que se está preparando la conexión.
        if (mensajeText != null)
            mensajeText.text = "Conectando con el servidor...";

        // Inicializa Firebase y autentica al visitante.
        StartCoroutine(InicializarFirebase());
    }

    private IEnumerator InicializarFirebase()
    {
        // Comprueba que las dependencias necesarias de Firebase estén disponibles.
        var tareaDependencias = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => tareaDependencias.IsCompleted);

        if (tareaDependencias.IsCanceled || tareaDependencias.IsFaulted)
        {
            Debug.LogError(
                "Error al comprobar las dependencias de Firebase: " +
                tareaDependencias.Exception, this);

            MostrarError("No se pudo iniciar Firebase.");
            yield break;
        }

        if (tareaDependencias.Result != DependencyStatus.Available)
        {
            Debug.LogError(
                "Dependencias de Firebase no disponibles: " +
                tareaDependencias.Result, this);

            MostrarError("No se pudo conectar con el servidor.");
            yield break;
        }

        // Obtiene las instancias de autenticación y base de datos.
        try
        {
            firebaseAuth = FirebaseAuth.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            MostrarError("No se pudo preparar Firebase.");
            yield break;
        }

        if (firebaseAuth == null || databaseReference == null)
        {
            MostrarError("No se pudo preparar la conexión.");
            yield break;
        }

        // Autentica al visitante anónimamente si aún no existe una sesión.
        if (firebaseAuth.CurrentUser == null)
        {
            var tareaAuth = firebaseAuth.SignInAnonymouslyAsync();

            yield return new WaitUntil(() => tareaAuth.IsCompleted);

            if (tareaAuth.IsCanceled || tareaAuth.IsFaulted)
            {
                Debug.LogError(
                    "Error en la autenticación anónima: " +
                    tareaAuth.Exception, this);

                MostrarError("No se pudo autenticar al visitante.");
                yield break;
            }
        }

        if (firebaseAuth.CurrentUser == null)
        {
            MostrarError("Firebase no pudo identificar al visitante.");
            yield break;
        }

        // Indica que Firebase está listo para registrar visitantes.
        firebaseListo = true;

        Debug.Log(
            "Firebase inicializado. Usuario autenticado: " +
            firebaseAuth.CurrentUser.UserId, this);

        if (mensajeText != null)
            mensajeText.text = "Ingresa tu nombre para continuar.";

        // Habilita Continuar cuando la conexión esté preparada.
        if (entrarButton != null)
            entrarButton.interactable = true;
    }

    public void OnEntrarClicked()
    {
        // Comprueba que existan las referencias necesarias para procesar el login.
        if (nombreInput == null || mensajeText == null || entrarButton == null)
        {
            Debug.LogError(
                "LoginManager: revisa las referencias de la interfaz.", this);
            return;
        }

        // Evita procesar varios registros al mismo tiempo.
        if (procesandoLogin)
            return;

        // No permite continuar hasta que Firebase esté listo.
        if (!firebaseListo || firebaseAuth == null ||
            firebaseAuth.CurrentUser == null || databaseReference == null)
        {
            mensajeText.text =
                "El servidor todavía se está conectando. Intenta de nuevo.";
            return;
        }

        // Obtiene el nombre y elimina espacios al inicio y al final.
        string nombre = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            mensajeText.text = "Por favor ingresa tu nombre.";
            return;
        }

        if (nombre.Length < 3)
        {
            mensajeText.text = "El nombre debe tener al menos 3 caracteres.";
            return;
        }

        // Bloquea temporalmente el botón mientras se guarda el registro.
        procesandoLogin = true;
        entrarButton.interactable = false;
        mensajeText.text = "Registrando visitante...";

        // Guarda los datos del visitante en Firebase.
        StartCoroutine(GuardarRegistroEnFirebase(nombre));
    }

    private IEnumerator GuardarRegistroEnFirebase(string nombre)
    {
        // Comprueba que exista una sesión válida antes de escribir en la base de datos.
        if (databaseReference == null || firebaseAuth == null ||
            firebaseAuth.CurrentUser == null)
        {
            MostrarError("No hay una sesión válida de Firebase.");
            yield break;
        }

        // Utiliza el identificador autenticado para guardar el registro del visitante.
        string userId = firebaseAuth.CurrentUser.UserId;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Prepara la información que se almacenará en Firebase.
        VisitanteData registro = new VisitanteData
        {
            nombre = nombre,
            fecha = timestamp,
            dispositivo = SystemInfo.deviceModel,
            sistemaOperativo = SystemInfo.operatingSystem
        };

        string json = JsonUtility.ToJson(registro);
        Task tarea;

        // Inicia la escritura del registro.
        try
        {
            tarea = databaseReference
                .Child("visitantes")
                .Child(userId)
                .SetRawJsonValueAsync(json);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            MostrarError("No se pudo iniciar el registro.");
            yield break;
        }

        // Espera a que Firebase termine sin bloquear Unity.
        yield return new WaitUntil(() => tarea.IsCompleted);

        if (tarea.IsCanceled || tarea.IsFaulted)
        {
            Debug.LogError(
                "Error al guardar en Firebase: " + tarea.Exception, this);

            MostrarError("Error al registrar. Intenta de nuevo.");
            yield break;
        }

        Debug.Log("Registro guardado correctamente en Firebase.", this);

        // Guarda el nombre localmente y lo configura para Photon.
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.Save();
        PhotonNetwork.NickName = nombre;

        // Oculta el inicio de sesión después del registro correcto.
        if (loginPanel != null)
            loginPanel.SetActive(false);

        // Muestra el menú principal.
        if (panelGlobal != null)
        {
            panelGlobal.SetActive(true);
            Debug.Log("Login completado. Mostrando Panel Global.", this);
        }
        else
        {
            Debug.LogError(
                "LoginManager: Panel Global no está asignado en el Inspector.", this);

            MostrarError("No se pudo mostrar el menú principal.");

            if (loginPanel != null)
                loginPanel.SetActive(true);

            yield break;
        }

        // Libera el estado de procesamiento.
        procesandoLogin = false;
    }

    private void MostrarError(string mensaje)
    {
        // Muestra el error en consola y en la interfaz.
        Debug.LogWarning(mensaje, this);

        if (mensajeText != null)
            mensajeText.text = mensaje;

        // Permite volver a intentar el registro.
        procesandoLogin = false;

        if (entrarButton != null)
            entrarButton.interactable = firebaseListo;
    }

    private void OnDestroy()
    {
        // Elimina el evento para evitar listeners duplicados.
        if (entrarButton != null)
            entrarButton.onClick.RemoveListener(OnEntrarClicked);
    }

    [Serializable]
    private class VisitanteData
    {
        // Datos que se guardan para cada visitante.
        public string nombre;
        public string fecha;
        public string dispositivo;
        public string sistemaOperativo;
    }

    // Regresa desde el Panel Global al inicio de sesión.
    public void VolverAlLogin()
    {
        // Evita dejar el juego pausado.
        Time.timeScale = 1f;

        // Oculta el menú principal.
        if (panelGlobal != null)
            panelGlobal.SetActive(false);

        // Reactiva el contenedor del login.
        if (loginPanel != null)
            loginPanel.SetActive(true);

        // Reactiva y limpia el campo de nombre.
        if (nombreInput != null)
        {
            nombreInput.gameObject.SetActive(true);
            nombreInput.enabled = true;
            nombreInput.interactable = true;
            nombreInput.readOnly = false;
            nombreInput.SetTextWithoutNotify("");
            nombreInput.ForceLabelUpdate();
        }

        // Restablece el estado del botón Continuar.
        // Solo estará habilitado si Firebase terminó de inicializarse.
        procesandoLogin = false;

        if (entrarButton != null)
            entrarButton.interactable = firebaseListo;

        // Actualiza el mensaje del login.
        if (mensajeText != null)
        {
            mensajeText.text = firebaseListo
                ? "Ingresa tu nombre para continuar."
                : "Conectando con el servidor...";
        }

        // Espera un frame antes de activar la escritura.
        StartCoroutine(PrepararCampoNombre());

        Debug.Log("Se volvió al inicio de sesión.", this);
    }

    // Prepara el campo después de que Unity actualice la interfaz.
    private IEnumerator PrepararCampoNombre()
    {
        yield return null;

        // Comprueba que el campo siga disponible y visible.
        if (nombreInput == null ||
            !nombreInput.gameObject.activeInHierarchy)
            yield break;

        // Restaura la capacidad de recibir texto.
        nombreInput.enabled = true;
        nombreInput.interactable = true;
        nombreInput.readOnly = false;

        // Actualiza el texto y enfoca el campo.
        nombreInput.ForceLabelUpdate();
        nombreInput.Select();
        nombreInput.ActivateInputField();

        Debug.Log(
            "Campo de nombre preparado. Interactable: " +
            nombreInput.interactable, this);
    }
}
