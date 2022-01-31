using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.IO;
using System.Collections.Generic;

public class Utils
{
    public static Type GetClassType(string className)
    {
        return typeof(Utils).Assembly.GetType(className);
    }

    public static string GetRenderingPipeline()
    {
        if (GraphicsSettings.currentRenderPipeline)
        {
            if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
            {
                return "HDRP";
            }
            else
            {
                return "URP";
            }
        }
        else
        {
            return "Legacy";
        }
    }

    public static bool CreateDirectoryIfNotExists(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }

        return true;
    }

    public static List<string> GetSubfolders(string folder)
    {
        DirectoryInfo dir;
        DirectoryInfo[] subs;
        List<string> toreturn = new List<string>();
        try
        {
            dir = new DirectoryInfo(folder);
            subs = dir.GetDirectories();
        }
        catch
        {
            subs = new DirectoryInfo[] { };
        }

        foreach (DirectoryInfo s in subs)
            toreturn.Add(s.Name);

        return toreturn;
    }


    /*
     * Gets the full path of a game object
     */
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    public static long GetTicksSinceDateBegin(DateTime dateTime)
    {
        return (dateTime.Ticks - XREchoConfig.GetInstance().dateBegin.Ticks); // Ticks are in 1/10th of a microsecond
    }

    public static long GetMicroSecondsSinceDateBegin(DateTime dateTime)
    {
        return (dateTime.Ticks - XREchoConfig.GetInstance().dateBegin.Ticks) / 10L; // Goes from 1/10th of a microsecond to microseconds
    }

    /*
     * Thanks to DMGregory on https://gamedev.stackexchange.com/questions/165643
     */
    public static float GetSurfaceArea(Mesh mesh)
    {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        double sum = 0.0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            sum += Vector3.Cross(a, b).magnitude;
        }

        return (float)(sum / 2.0);
    }

    public static float[] GetGaussian(float std, out int diameter)
    {
        int radius = Mathf.RoundToInt(3 * std);
        diameter = 2 * radius + 1;

        float[] gaussian = new float[diameter * diameter];

        for (int i = -radius; i <= radius; i++)
            for (int j = -radius; j <= radius; j++)
            {
                int index = (j + radius) * diameter + (i + radius);

                int dist = i * i + j * j;
                if (dist > radius * radius)
                {
                    gaussian[index] = 0;
                    continue;
                }

                gaussian[index] = Mathf.Exp(-dist / (2 * std * std)) / std;
            }

        return gaussian;
    }

    public static void TakeScreenshot(Camera camera, string path = "Screenshots", string filename = "screenshot", int width = 1024, int height = 512)
    {
        RenderTexture tempRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = tempRT;

        camera.targetTexture = tempRT;
        camera.Render();

        Texture2D image = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        RenderTexture.active = null;

        byte[] bytes = image.EncodeToPNG();
        UnityEngine.Object.Destroy(image);

        camera.targetTexture = null;

        File.WriteAllBytes(path + '/' + filename + ".png", bytes);
    }

    public static string TimeString(int timeSec)
    {
        string toreturn = "";
        int cropTime = timeSec;
        if (timeSec >= 3600)
        {
            toreturn = "" + timeSec / 3600 + ":";
            cropTime = timeSec % 3600;
            if (cropTime < 600)
            {
                toreturn += "0";
            }
        }
        toreturn += "" + cropTime / 60 + ":";
        if (cropTime >= 60)
        {
            cropTime = timeSec % 60;
        }
        if (cropTime < 10)
        {
            toreturn += "0";
        }
        toreturn += cropTime;

        return toreturn;
    }

}
