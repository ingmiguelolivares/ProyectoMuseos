using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using System;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loginPanel;
    public TMP_InputField nombreInput;
    public Button entrarButton;
    public TMP_Text mensajeText;

    void Start()
    {
        // Configurar botón
        entrarButton.onClick.AddListener(OnEntrarClicked);

        Debug.Log("✅ LoginManager iniciado");
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

        // Guardar nombre
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

        Debug.Log("✅ Nombre guardado: " + nombre);

        // También podemos guardar estadísticas en Custom Properties de Photon
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "DisplayName", nombre },
            { "JoinDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "Visits", PlayerPrefs.GetInt("TotalVisits", 0) + 1 }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        // Incrementar contador de visitas
        PlayerPrefs.SetInt("TotalVisits", PlayerPrefs.GetInt("TotalVisits", 0) + 1);
        PlayerPrefs.Save();

        // Cerrar panel y cambiar escena
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