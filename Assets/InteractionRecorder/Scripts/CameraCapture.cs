using System;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCapture : MonoBehaviour
{
    Camera screenshotCamera;

    private void Start()
    {
        screenshotCamera = GetComponent<Camera>();
    }

    public void Capture(string path = "Screenshots", string filename = "screenshot", int width = 1024, int height = 512)
    {
        RenderTexture tempRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = tempRT;

        screenshotCamera.targetTexture = tempRT;
        screenshotCamera.Render();

        Texture2D image = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        RenderTexture.active = null;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        File.WriteAllBytes(path + '/' + filename + ".png", bytes);
    }
}