using UnityEngine;
using System.IO;

public class CapturaDesdeCamara : MonoBehaviour
{
    void Start()
    {
        // Buscar la cámara llamada "Camera" en la escena
        Camera cam = GameObject.Find("Camera").GetComponent<Camera>();

        // Resolución de la imagen
        int width = 1920;
        int height = 1080;

        // Crear un RenderTexture temporal
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        // Crear la textura para guardar los píxeles
        Texture2D imagen = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Renderizar y leer los píxeles
        cam.Render();
        RenderTexture.active = rt;
        imagen.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        imagen.Apply();

        // Limpiar
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Guardar la imagen como PNG
        byte[] bytes = imagen.EncodeToPNG();
        string ruta = Application.dataPath + "/CapturaCamara.png";
        File.WriteAllBytes(ruta, bytes);

        Debug.Log("Imagen guardada en: " + ruta);
    }
}
