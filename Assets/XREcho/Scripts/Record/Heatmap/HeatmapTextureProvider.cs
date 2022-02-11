﻿﻿using System.Linq;
using UnityEngine;

public class HeatmapTextureProvider : MonoBehaviour, IHeatmapTextureProvider
{
    public Texture2D HeatmapToTexture(float[,] heatmap, int w, int h)
    {
        var max = heatmap.Cast<float>().Max();
        var pixels = new Color[w * h];
        var texture = new Texture2D(w, h);

        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = HeatMapColorForValue(heatmap[h - i / h - 1, w - i % h - 1] / max);
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private static Color HeatMapColorForValue(float value)
    {
        var h = (1.0f - value) * (240.0f / 360.0f);
        return Color.HSVToRGB(h, 1, 1);
    }
}