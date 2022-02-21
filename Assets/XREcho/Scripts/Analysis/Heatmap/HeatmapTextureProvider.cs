using UnityEngine;

public class HeatmapTextureProvider : MonoBehaviour, IHeatmapTextureProvider
{
    public Gradient heatmapColorGradient = DefaultHeatmapGradient();

    public Texture2D HeatmapToTexture(float[,] heatmap)
    {
        var h = heatmap.GetLength(0);
        var w = heatmap.GetLength(1);
        var pixels = new Color[w * h];
        var texture = new Texture2D(w, h);

        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = HeatmapColorForValue(heatmap[h - i / h - 1, w - i % h - 1]);
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private Color HeatmapColorForValue(float value)
    {
        return heatmapColorGradient.Evaluate(value);
    }
    
    private static Gradient DefaultHeatmapGradient()
    {
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        var colorKey = new GradientColorKey[5];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.cyan;
        colorKey[1].time = 0.25f;
        colorKey[2].color = Color.green;
        colorKey[2].time = 0.5f;
        colorKey[3].color = Color.yellow;
        colorKey[3].time = 0.75f;
        colorKey[4].color = Color.red;
        colorKey[4].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        var alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        var gradient = new Gradient();
        gradient.SetKeys(colorKey, alphaKey);
        return gradient;
    }
}