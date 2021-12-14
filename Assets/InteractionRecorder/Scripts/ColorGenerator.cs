using UnityEngine;

public class ColorGenerator {
    public static Color GetNthColor(int n, int numberOfColors, Color startingColor)
    {
        int numerator;
        int denominator;

        // Step 0
        if (n < numberOfColors)
        {
            numerator = n + 1;
            denominator = numberOfColors;
        }
        // Next steps, the number of intervals is multiplied by 2**(step - 1)
        else
        {
            int step = (int)Mathf.Log(n / numberOfColors, 2) + 1;
            int nbIntervals = numberOfColors * (int)Mathf.Pow(2, step - 1);
            numerator = 2 * (n - nbIntervals) + 1;
            denominator = nbIntervals * 2;
        }

        float H, S, V;
        Color.RGBToHSV(startingColor, out H, out S, out V);

        H = ((float)numerator / (float)denominator + H - (1.0f / (float)numberOfColors)) % 1.0f;
        
        return Color.HSVToRGB(H, S, V);
    }

    public static Color[] GetLinearColorPalette(int n, Color c1, Color c2)
    {
        if (n == 0)
            return null;

        if (n == 1)
            return new Color[1] { c1 };

        Color[] colors = new Color[n];

        for (int i = 0; i < n; i++)
        {
            float H1, S1, V1, H2, S2, V2;
            Color.RGBToHSV(c1, out H1, out S1, out V1);
            Color.RGBToHSV(c2, out H2, out S2, out V2);

            float t = (float)i / (float)(n - 1);
            float H = Mathf.Lerp(H1, H2, t);
            float S = Mathf.Lerp(S1, S2, t);
            float V = Mathf.Lerp(V1, V2, t);
            colors[i] = Color.HSVToRGB(H, S, V);
        }

        return colors;
    }
}
