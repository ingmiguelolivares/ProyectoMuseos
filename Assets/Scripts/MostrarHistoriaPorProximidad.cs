
using UnityEngine;
using Photon.Pun;

public class MostrarHistoriaPorProximidad : MonoBehaviour
{
    [Header("Panel de historia")]
    [SerializeField] private GameObject panelHistoria;

    private void Start()
    {
        // El panel comienza cerrado mientras el jugador está lejos.
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "MostrarHistoriaPorProximidad: asigna el panel de historia.",
                this
            );
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        // Comprueba que el objeto que entró tenga un PhotonView.
        PhotonView vistaPhoton = otro.GetComponentInParent<PhotonView>();

        // Solo el jugador controlado por este usuario puede abrir el panel.
        if (vistaPhoton == null || !vistaPhoton.IsMine)
            return;


        // Muestra el panel de historia.
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otro)
    {
        // Identifica si el objeto que salió pertenece al jugador local.
        PhotonView vistaPhoton = otro.GetComponentInParent<PhotonView>();

        // Ignora a los demás jugadores.
        if (vistaPhoton == null || !vistaPhoton.IsMine)
            return;


        // Oculta el panel cuando el visitante se aleja.
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }
    }
}
