using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // conecta al servidor
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al servidor Photon.");
        PhotonNetwork.JoinLobby(); // se une al lobby
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Unido al Lobby.");
        PhotonNetwork.JoinOrCreateRoom("SalaGlobal", new RoomOptions { MaxPlayers = 10, IsVisible = true, IsOpen = true }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Entró a la sala.");
        PhotonNetwork.Instantiate("Jugador", new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
    }
}