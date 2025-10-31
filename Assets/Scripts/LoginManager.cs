using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using System;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loginPanel;
    public TMP_InputField nombreInput;
    public Button entrarButton;
    public TMP_Text mensajeText;

    void Start()
    {
        entrarButton.onClick.AddListener(OnEntrarClicked);
        Debug.Log("LoginManager iniciado");
    }

    void OnEntrarClicked()
    {
        string nombre = nombreInput.text.Trim();

        // Desactivar el botón mientras se valida
        entrarButton.interactable = false;

        // Validaciones
        if (string.IsNullOrEmpty(nombre))
        {
            mensajeText.text = "Por favor ingresa tu nombre.";
            entrarButton.interactable = true; // Rehabilitar el botón
            return;
        }

        if (nombre.Length < 3)
        {
            mensajeText.text = "El nombre debe tener al menos 3 caracteres.";
            entrarButton.interactable = true; // Rehabilitar el botón
            return;
        }

        // Si pasa las validaciones
        mensajeText.text = "Conectando...";
        GuardarDatosJugador(nombre);
    }

    void GuardarDatosJugador(string nombre)
    {
        // Guardar nombre localmente
        PlayerPrefs.SetString("PlayerName", nombre);
        PlayerPrefs.SetString("JoinDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();

        // Configurar nombre en Photon
        PhotonNetwork.NickName = nombre;
        Debug.Log("Nombre guardado: " + nombre);

        // Guardar propiedades personalizadas en Photon
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "DisplayName", nombre },
            { "JoinDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "Visits", PlayerPrefs.GetInt("TotalVisits", 0) + 1 }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Incrementar visitas
        PlayerPrefs.SetInt("TotalVisits", PlayerPrefs.GetInt("TotalVisits", 0) + 1);
        PlayerPrefs.Save();

        // Cambiar a la escena del museo
        ConectarPhoton(nombre);
    }

    void ConectarPhoton(string nombre)
    {
        // Cerrar panel de login
        loginPanel.SetActive(false);

        Debug.Log("Cambiando a escena del museo con nombre: " + nombre);

        // Cambiar a la escena del museo
        SceneManager.LoadScene("Museo SiSiColombia");
    }
}
