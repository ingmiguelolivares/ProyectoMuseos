
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField nombreInput;
    [SerializeField] private Button entrarButton;
    [SerializeField] private TMP_Text mensajeText;

    [Header("Configuración")]
    [SerializeField] private string nombreEscenaMuseo = "Museo SiSiColombia";

    private DatabaseReference databaseReference;
    private bool firebaseListo = false;
    private bool procesandoLogin = false;

    private void Start()
    {
        // Validar referencias de la interfaz.
        if (loginPanel == null)
            Debug.LogError("LoginManager: falta asignar Login Panel.", this);

        if (nombreInput == null)
            Debug.LogError("LoginManager: falta asignar Nombre Input.", this);

        if (entrarButton == null)
            Debug.LogError("LoginManager: falta asignar Entrar Button.", this);

        if (mensajeText == null)
            Debug.LogError("LoginManager: falta asignar Mensaje Text.", this);

        // Configurar el botón si está asignado.
        if (entrarButton != null)
        {
            entrarButton.onClick.RemoveListener(OnEntrarClicked);
            entrarButton.onClick.AddListener(OnEntrarClicked);
            entrarButton.interactable = false;
        }

        if (mensajeText != null)
            mensajeText.text = "Conectando con el servidor...";

        // Inicializar Firebase.
        _ = InicializarFirebaseAsync();
    }

    private async Task InicializarFirebaseAsync()
    {
        try
        {
            DependencyStatus estado =
                await FirebaseApp.CheckAndFixDependenciesAsync();

            if (estado != DependencyStatus.Available)
            {
                Debug.LogError(
                    "No se pudo inicializar Firebase: " + estado, this);

                MostrarError("No se pudo conectar con el servidor.");
                return;
            }

            databaseReference =
                FirebaseDatabase.DefaultInstance.RootReference;

            firebaseListo = databaseReference != null;

            if (!firebaseListo)
            {
                MostrarError("No se pudo preparar la conexión.");
                return;
            }

            Debug.Log("Firebase inicializado correctamente.");

            if (mensajeText != null)
                mensajeText.text = "Ingresa tu nombre para continuar.";

            if (entrarButton != null)
                entrarButton.interactable = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            MostrarError("Error al iniciar la conexión.");
        }
    }

    public void OnEntrarClicked()
    {
        // Evitar referencias nulas.
        if (nombreInput == null ||
            mensajeText == null ||
            entrarButton == null)
        {
            Debug.LogError(
                "LoginManager: revisa las referencias de la interfaz.", this);
            return;
        }

        if (procesandoLogin)
            return;

        if (!firebaseListo || databaseReference == null)
        {
            mensajeText.text =
                "El servidor todavía se está conectando. Intenta de nuevo.";
            return;
        }

        string nombre = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            mensajeText.text = "Por favor ingresa tu nombre.";
            return;
        }

        if (nombre.Length < 3)
        {
            mensajeText.text =
                "El nombre debe tener al menos 3 caracteres.";
            return;
        }

        procesandoLogin = true;
        entrarButton.interactable = false;
        mensajeText.text = "Registrando visitante...";

        StartCoroutine(GuardarRegistroEnFirebase(nombre));
    }

    private IEnumerator GuardarRegistroEnFirebase(string nombre)
    {
        if (databaseReference == null)
        {
            MostrarError("La conexión con el servidor no está disponible.");
            yield break;
        }

        string userId = SystemInfo.deviceUniqueIdentifier;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Crear un objeto serializable para Firebase.
        VisitanteData registro = new VisitanteData
        {
            nombre = nombre,
            fecha = timestamp,
            dispositivo = SystemInfo.deviceModel,
            sistemaOperativo = SystemInfo.operatingSystem
        };

        string json = JsonUtility.ToJson(registro);

        Task tarea = null;

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
            MostrarError("No se pudo guardar el registro.");
            yield break;
        }

        // Esperar a que termine Firebase sin bloquear Unity.
        while (!tarea.IsCompleted)
            yield return null;

        if (tarea.IsCanceled || tarea.IsFaulted)
        {
            Debug.LogError(
                "Error al guardar en Firebase: " + tarea.Exception, this);

            MostrarError("Error al registrar. Intenta de nuevo.");
            yield break;
        }

        Debug.Log("Registro guardado correctamente en Firebase.");

        // Guardar el nombre localmente.
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.Save();

        // Configurar Photon.
        PhotonNetwork.NickName = nombre;

        // Cerrar el panel, si existe.
        if (loginPanel != null)
            loginPanel.SetActive(false);

        Debug.Log(
            "Nombre configurado: " + nombre +
            ". Cargando escena: " + nombreEscenaMuseo);

        // Comprobar que la escena esté incluida en Build Settings.
        if (!Application.CanStreamedLevelBeLoaded(nombreEscenaMuseo))
        {
            Debug.LogError(
                "No se puede cargar la escena '" +
                nombreEscenaMuseo +
                "'. Revisa su nombre y Build Profiles.", this);

            if (loginPanel != null)
                loginPanel.SetActive(true);

            MostrarError("No se encontró la escena del museo.");
            yield break;
        }

        SceneManager.LoadScene(nombreEscenaMuseo);
    }

    private void MostrarError(string mensaje)
    {
        Debug.LogWarning(mensaje, this);

        if (mensajeText != null)
            mensajeText.text = mensaje;

        procesandoLogin = false;

        if (entrarButton != null)
            entrarButton.interactable = firebaseListo;
    }

    private void OnDestroy()
    {
        if (entrarButton != null)
            entrarButton.onClick.RemoveListener(OnEntrarClicked);
    }

    [Serializable]
    private class VisitanteData
    {
        public string nombre;
        public string fecha;
        public string dispositivo;
        public string sistemaOperativo;
    }
}
