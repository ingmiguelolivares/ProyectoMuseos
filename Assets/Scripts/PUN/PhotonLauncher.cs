using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.LogLevel = PunLogLevel.Full;

        Debug.Log("🚀 PhotonLauncher iniciado en escena del Museo");

        string playerName = PlayerPrefs.GetString("PlayerName", "");

        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
            Debug.Log("🔄 Conectando a Photon con nombre: " + playerName);
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogWarning("⚠️ No hay nombre guardado. Conectando con nombre aleatorio...");
            PhotonNetwork.NickName = "Visitante" + Random.Range(1000, 9999);
            PhotonNetwork.ConnectUsingSettings();
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
        Vector3 spawnPosition = new Vector3(8, 1, 52);

        // ✅ Rotación en Y = 90°
        Quaternion spawnRotation = Quaternion.Euler(0, 90, 0);

        // Instanciar el jugador con rotación
        GameObject player = PhotonNetwork.Instantiate("Jugador", spawnPosition, spawnRotation);

        if (player != null)
        {
            Debug.Log("✅ Jugador instanciado con rotación en Y = 90°!");
        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("❌ Error al unirse a la sala: " + message);
    }
}