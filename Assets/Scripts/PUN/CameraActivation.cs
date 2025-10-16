using Photon.Pun;
using UnityEngine;

public class CamaraLocal : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine)
        {
            // Desactiva la cámara si no es el jugador local
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.gameObject.SetActive(false);
        }
    }
}