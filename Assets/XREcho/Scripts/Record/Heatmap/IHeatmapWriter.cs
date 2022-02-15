public interface IHeatmapWriter
{
    void Write(string filepath, float[,] heatmap, float[,] normalizedHeatmap);
}