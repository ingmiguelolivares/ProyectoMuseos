using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerNameDisplay : MonoBehaviourPun
{
    [Header("UI")]
    public TMP_Text nombreText;
    public Canvas canvas;

    [Header("Configuración")]
    public float alturaOffset = 2f;
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas no asignado en PlayerNameDisplay");
            return;
        }

        // Configurar canvas para que mire a la cámara
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Obtener y mostrar nombre
        if (photonView.IsMine)
        {
            // Mi jugador - usar nombre local
            nombreText.text = PhotonNetwork.NickName;
        }
        else
        {
            // Otro jugador - obtener nombre del PhotonView
            nombreText.text = photonView.Owner.NickName;
        }

        // Color diferente para mi jugador
        if (photonView.IsMine)
        {
            nombreText.color = Color.green;
        }
        else
        {
            nombreText.color = Color.white;
        }
    }

    void LateUpdate()
    {
        // Hacer que el nombre siempre mire a la cámara
        if (Camera.main != null)
        {
            canvas.transform.LookAt(Camera.main.transform);
            canvas.transform.Rotate(0, 180, 0);
        }

        // Posicionar sobre la cabeza del jugador
        canvas.transform.position = transform.position + offset;
    }
}