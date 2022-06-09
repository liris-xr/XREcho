using System.Linq;
using UnityEngine;

public class HeatmapTextureProvider : MonoBehaviour, IHeatmapTextureProvider
{
    public Texture2D HeatmapToTexture(float[,] heatmap)
    {
        var h = heatmap.GetLength(0);
        var w = heatmap.GetLength(1);
        var pixels = new Color[h, w];
        var texture = new Texture2D(w, h, TextureFormat.RFloat, false);

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                pixels[h - y - 1, w - x - 1] = new Color(heatmap[y, x], 0, 0, 1);
            }
        }

        texture.SetPixels(pixels.Cast<Color>().ToArray());
        texture.Apply();
        return texture;
    }
}