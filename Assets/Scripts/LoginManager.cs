
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
    [Header("UI References")]
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
        // Validar referencias de la interfaz.
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

        // Configurar el botón.
        if (entrarButton != null)
        {
            entrarButton.onClick.RemoveListener(OnEntrarClicked);
            entrarButton.onClick.AddListener(OnEntrarClicked);
            entrarButton.interactable = false;
        }

        if (mensajeText != null)
            mensajeText.text = "Conectando con el servidor...";

        StartCoroutine(InicializarFirebase());
    }

    private IEnumerator InicializarFirebase()
    {
        var tareaDependencias =
            FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(
            () => tareaDependencias.IsCompleted
        );

        if (tareaDependencias.IsCanceled ||
            tareaDependencias.IsFaulted)
        {
            Debug.LogError(
                "Error al comprobar las dependencias de Firebase: " +
                tareaDependencias.Exception, this
            );

            MostrarError("No se pudo iniciar Firebase.");
            yield break;
        }

        if (tareaDependencias.Result != DependencyStatus.Available)
        {
            Debug.LogError(
                "Dependencias de Firebase no disponibles: " +
                tareaDependencias.Result, this
            );

            MostrarError("No se pudo conectar con el servidor.");
            yield break;
        }

        try
        {
            firebaseAuth = FirebaseAuth.DefaultInstance;

            databaseReference =
                FirebaseDatabase.DefaultInstance.RootReference;
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

        // Autenticar anónimamente al visitante.
        if (firebaseAuth.CurrentUser == null)
        {
            var tareaAuth = firebaseAuth.SignInAnonymouslyAsync();

            yield return new WaitUntil(() => tareaAuth.IsCompleted);

            if (tareaAuth.IsCanceled || tareaAuth.IsFaulted)
            {
                Debug.LogError(
                    "Error en la autenticación anónima: " +
                    tareaAuth.Exception, this
                );

                MostrarError(
                    "No se pudo autenticar al visitante."
                );

                yield break;
            }
        }

        if (firebaseAuth.CurrentUser == null)
        {
            MostrarError(
                "Firebase no pudo identificar al visitante."
            );

            yield break;
        }

        firebaseListo = true;

        Debug.Log(
            "Firebase inicializado. Usuario autenticado: " +
            firebaseAuth.CurrentUser.UserId, this
        );

        if (mensajeText != null)
            mensajeText.text =
                "Ingresa tu nombre para continuar.";

        if (entrarButton != null)
            entrarButton.interactable = true;
    }

    public void OnEntrarClicked()
    {
        if (nombreInput == null ||
            mensajeText == null ||
            entrarButton == null)
        {
            Debug.LogError(
                "LoginManager: revisa las referencias de la interfaz.",
                this
            );

            return;
        }

        if (procesandoLogin)
            return;

        if (!firebaseListo ||
            firebaseAuth == null ||
            firebaseAuth.CurrentUser == null ||
            databaseReference == null)
        {
            mensajeText.text =
                "El servidor todavía se está conectando. Intenta de nuevo.";

            return;
        }

        string nombre = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            mensajeText.text =
                "Por favor ingresa tu nombre.";

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
        if (databaseReference == null ||
            firebaseAuth == null ||
            firebaseAuth.CurrentUser == null)
        {
            MostrarError(
                "No hay una sesión válida de Firebase."
            );

            yield break;
        }

        // UID de Firebase para cumplir las reglas de seguridad.
        string userId = firebaseAuth.CurrentUser.UserId;

        string timestamp =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        VisitanteData registro = new VisitanteData
        {
            nombre = nombre,
            fecha = timestamp,
            dispositivo = SystemInfo.deviceModel,
            sistemaOperativo = SystemInfo.operatingSystem
        };

        string json = JsonUtility.ToJson(registro);

        Task tarea;

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

        // Esperar sin bloquear Unity.
        yield return new WaitUntil(() => tarea.IsCompleted);

        if (tarea.IsCanceled || tarea.IsFaulted)
        {
            Debug.LogError(
                "Error al guardar en Firebase: " +
                tarea.Exception, this
            );

            MostrarError(
                "Error al registrar. Intenta de nuevo."
            );

            yield break;
        }

        Debug.Log(
            "Registro guardado correctamente en Firebase.", this
        );

        // Guardar el nombre localmente.
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.Save();

        // Configurar el nombre para Photon.
        PhotonNetwork.NickName = nombre;

        // Cerrar el panel de login.
        if (loginPanel != null)
            loginPanel.SetActive(false);

        // Mostrar el menú global en la misma escena.
        if (panelGlobal != null)
        {
            panelGlobal.SetActive(true);

            Debug.Log(
                "Login completado. Mostrando Panel Global.", this
            );
        }
        else
        {
            Debug.LogError(
                "LoginManager: Panel Global no está asignado en el Inspector.",
                this
            );

            MostrarError(
                "No se pudo mostrar el menú principal."
            );

            if (loginPanel != null)
                loginPanel.SetActive(true);

            yield break;
        }

        procesandoLogin = false;
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
