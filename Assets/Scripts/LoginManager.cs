using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public TMP_Text mensajeText;

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
        // ⭐ GUARDAR NOMBRE INMEDIATAMENTE ANTES DE TODO
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.Save();
        PhotonNetwork.NickName = nombre;

        Debug.Log("✅ Nombre guardado localmente: " + nombre);

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

        // Guardar en Firebase (asíncrono, pero no esperamos)
        databaseReference.Child("visitantes").Child(userId).SetRawJsonValueAsync(JsonUtility.ToJson(registroData))
            .ContinueWith(task => {
                if (task.IsCompleted)
                {
                    Debug.Log("✅ Registro guardado en Firebase");
                }
                else
                {
                    Debug.LogError("❌ Error al guardar en Firebase: " + task.Exception);
                }
            });

        // Cerrar panel y cambiar escena INMEDIATAMENTE
        ConectarPhoton(nombre);
    }

    void ConectarPhoton(string nombre)
    {
        // Cerrar panel de login
        loginPanel.SetActive(false);

        Debug.Log("✅ Cambiando a escena del museo con nombre: " + nombre);

        // Cambiar a la escena del museo
        SceneManager.LoadScene("Museo SiSiColombia");
    }
}