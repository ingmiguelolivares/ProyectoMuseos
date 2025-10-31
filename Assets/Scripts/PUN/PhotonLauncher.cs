using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("🚀 PhotonLauncher iniciado en escena del Museo");

        // Verificar si el jugador ya ingresó su nombre
        string playerName = PlayerPrefs.GetString("PlayerName", "");

        if (!string.IsNullOrEmpty(playerName))
        {
            // Configurar nombre en Photon
            PhotonNetwork.NickName = playerName;

            Debug.Log("🔄 Conectando a Photon con nombre: " + playerName);

            // Conectar a Photon
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("❌ No hay nombre guardado. El usuario debe pasar por el login primero.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Conectado al servidor Photon.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Unido al Lobby.");
        PhotonNetwork.JoinOrCreateRoom("SalaGlobal", new RoomOptions
        {
            MaxPlayers = 10,
            IsVisible = true,
            IsOpen = true
        }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✅ Entró a la sala: " + PhotonNetwork.CurrentRoom.Name);

        // Posición fija
        Vector3 spawnPosition = new Vector3(45, 1, 24);

        // ✅ Rotación en Y = 90°
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        // Instanciar el jugador con rotación
        GameObject player = PhotonNetwork.Instantiate("Jugador", spawnPosition, spawnRotation);

        if (player != null)
        {
            Debug.Log("✅ Jugador instanciado");
        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("❌ Error al unirse a la sala: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("⚠️ Desconectado de Photon: " + cause);
    }

    public void ConectarConNombre(string nombre)
    {
        PhotonNetwork.NickName = nombre;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("🔄 Conectando a Photon con nombre: " + nombre);
    }
}