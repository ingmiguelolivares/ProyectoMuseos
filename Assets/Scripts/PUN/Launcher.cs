using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Verificar si el jugador ya ingresó su nombre
        string playerName = PlayerPrefs.GetString("PlayerName", "");

        if (!string.IsNullOrEmpty(playerName))
        {
            // Configurar nombre en Photon
            PhotonNetwork.NickName = playerName;

            // Conectar a Photon
            PhotonNetwork.ConnectUsingSettings();

            Debug.Log("🔄 Conectando a Photon con nombre: " + playerName);
        }
        else
        {
            Debug.Log("⏳ Esperando que el usuario ingrese su nombre en el LoginManager...");
            // El LoginManager llamará a ConectarPhoton() después del login
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
        Debug.Log("👥 Jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Posición de spawn aleatoria
        Vector3 spawnPosition = new Vector3(
            Random.Range(-5, 5),
            1f,
            Random.Range(-5, 5)
        );

        // Instanciar jugador
        PhotonNetwork.Instantiate("Jugador", spawnPosition, Quaternion.identity);

        Debug.Log("🎮 Jugador instanciado con nombre: " + PhotonNetwork.NickName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("❌ Error al unirse a la sala: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("⚠️ Desconectado de Photon: " + cause);
    }

    // Método público que el LoginManager puede llamar para conectar
    public void ConectarConNombre(string nombre)
    {
        PhotonNetwork.NickName = nombre;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("🔄 Conectando a Photon con nombre: " + nombre);
    }
}