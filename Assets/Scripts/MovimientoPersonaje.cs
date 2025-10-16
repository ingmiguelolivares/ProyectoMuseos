using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MovimientoPersonaje : MonoBehaviourPun
{

    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float velocidadRotacion;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform transformPersonaje;
    [SerializeField] private Camera camaraPersonaje;


    private Vector3 movimiento;
    private float rotacionX;
    
    private void Start()
{
    if (!photonView.IsMine)
    {
        if (camaraPersonaje != null)
        {
            camaraPersonaje.gameObject.SetActive(false);
        }
    }
}

    private void Update()
    {
        if (!photonView.IsMine) return;

        MovimientoDelPersonaje();
        MovimientoDeCamara();


    }


    void MovimientoDelPersonaje()
    {
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        movimiento = transform.right * hor + transform.forward * ver;
        characterController.SimpleMove(movimiento * velocidadMovimiento);
    }

    void MovimientoDeCamara()
    {
        float ratonX = Input.GetAxis("Mouse X") * velocidadRotacion;
        float ratonY = Input.GetAxis("Mouse Y") * velocidadRotacion;

        rotacionX -= ratonY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        camaraPersonaje.transform.localRotation = Quaternion.Euler(rotacionX, 0, 0);
        transformPersonaje.Rotate(Vector3.up * ratonX);

    }


}