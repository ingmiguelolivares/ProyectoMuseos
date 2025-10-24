using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ← NUEVO
using TMPro;
using Firebase;
using Firebase.Database;
using Photon.Pun;
using System;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loginPanel;
    public TMP_InputField nombreInput;
    public Button entrarButton;
    public Text mensajeText;

    private DatabaseReference databaseReference;

    void Start()
    {
        // Verificar Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("No se pudo inicializar Firebase: " + task.Result);
            }
        });

        // Configurar botón
        entrarButton.onClick.AddListener(OnEntrarClicked);
    }

    void InitializeFirebase()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("✅ Firebase inicializado correctamente");
    }

    void OnEntrarClicked()
    {
        string nombre = nombreInput.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            mensajeText.text = "Por favor ingresa tu nombre";
            return;
        }

        if (nombre.Length < 3)
        {
            mensajeText.text = "El nombre debe tener al menos 3 caracteres";
            return;
        }

        mensajeText.text = "Conectando...";
        entrarButton.interactable = false;

        // Guardar en Firebase
        GuardarRegistroEnFirebase(nombre);
    }

    void GuardarRegistroEnFirebase(string nombre)
    {
        // Crear ID único para el registro
        string userId = SystemInfo.deviceUniqueIdentifier;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Crear objeto de registro
        var registroData = new
        {
            nombre = nombre,
            fecha = timestamp,
            dispositivo = SystemInfo.deviceModel,
            sistemaOperativo = SystemInfo.operatingSystem
        };

        // Guardar en Firebase
        databaseReference.Child("visitantes").Child(userId).SetRawJsonValueAsync(JsonUtility.ToJson(registroData))
            .ContinueWith(task => {
                if (task.IsCompleted)
                {
                    Debug.Log("✅ Registro guardado en Firebase");

                    // Guardar nombre localmente para Photon
                    PlayerPrefs.SetString("PlayerName", nombre);
                    PlayerPrefs.Save();

                    // Conectar a Photon y cambiar escena
                    ConectarPhoton(nombre);
                }
                else
                {
                    Debug.LogError("❌ Error al guardar en Firebase: " + task.Exception);
                    mensajeText.text = "Error al conectar. Intenta de nuevo.";
                    entrarButton.interactable = true;
                }
            });
    }

    void ConectarPhoton(string nombre)
    {
        // Configurar nombre del jugador en Photon
        PhotonNetwork.NickName = nombre;

        // Cerrar panel de login
        loginPanel.SetActive(false);

        Debug.Log("✅ Nombre configurado: " + nombre + " - Cargando escena del museo...");

        // Cambiar a la escena del museo
        SceneManager.LoadScene("Museo SiSiColombia");
    }
}