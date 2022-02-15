using UnityEngine;

public class HeatmapCsvWriter : MonoBehaviour, IHeatmapWriter
{
    public void Write(string filepath, float[,] heatmap, float[,] normalizedHeatmap)
    {
        var csvWriter = new CSVWriter(filepath);
        csvWriter.WriteLine("gridX", "gridY", "Value (s)", "Normalized value");
        
        for (var y = 0; y < heatmap.GetLength(0); y++)
        {
            for (var x = 0; x < heatmap.GetLength(1); x++)
            {
                csvWriter.WriteLine(x, y, heatmap[y, x], normalizedHeatmap[y, x]);
            }
        }

        csvWriter.Close();
    }
}