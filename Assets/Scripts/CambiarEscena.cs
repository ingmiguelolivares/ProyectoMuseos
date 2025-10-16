using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{

    public bool pasarEscena;
    public int indiceEscena;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Escena(indiceEscena);
        }

        if (pasarEscena)
        {
            Escena(indiceEscena);
        }
    }

    public void Escena(int indice)
    {
        SceneManager.LoadScene(indice);
    }
}
