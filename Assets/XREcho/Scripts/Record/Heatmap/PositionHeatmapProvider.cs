using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PositionHeatmapProvider : MonoBehaviour
{
    [Tooltip("Number of pixels in one (in-game) meter. Increase this if you want a better resolution for your heatmap.")]
    public int pixelsPerMeter = 10;

    private IGaussianProvider _gaussianProvider;
    private IHeatmapTextureProvider _heatmapTextureProvider;

    private void Awake()
    {
        _gaussianProvider = gameObject.GetComponent<GaussianProvider>();
        _heatmapTextureProvider = gameObject.GetComponent<HeatmapTextureProvider>();
    }

    private int[,] CreatePositionHeatmapGrid(int heatmapWidth, int heatmapHeight, IEnumerable<Vector3> positions, Vector3 planeOrigin, Vector3 planeSize)
    {
        var heatmap = new int[heatmapHeight, heatmapWidth];
        
        foreach (var position in positions)
        {
            // Convert to relative coordinates (between 0 and 1)
            var relPos = position - planeOrigin;
            relPos.x /= planeSize.x;
            relPos.z /= planeSize.z;

            // Skip positions outside of the heatmap
            if (relPos.x < 0 || relPos.x >= heatmapWidth || relPos.z < 0 || relPos.z >= heatmapHeight)
                continue;
            
            var cellX = Mathf.CeilToInt(relPos.x * heatmapWidth) - 1;
            var cellY = Mathf.CeilToInt(relPos.z * heatmapHeight) - 1;
            heatmap[cellY, cellX] += 1;
        }

        return heatmap;
    }

    public Texture2D CreatePositionHeatmapTexture(IEnumerable<Vector3> positions, GameObject heatmapPlane)
    {
        var bounds = heatmapPlane.GetComponent<Renderer>().bounds;
        var heatmapWidth = Mathf.FloorToInt(pixelsPerMeter * bounds.size.x);
        var heatmapHeight = Mathf.FloorToInt(pixelsPerMeter * bounds.size.z);
        
        var gaussian = _gaussianProvider.CreateGaussian();
        var posHeatmap = CreatePositionHeatmapGrid(heatmapWidth, heatmapHeight, positions, bounds.min, bounds.size);
        var posHeatmapGaussian = gaussian.Apply(posHeatmap, heatmapWidth, heatmapHeight);

        var heatmapTexture = _heatmapTextureProvider.HeatmapToTexture(posHeatmapGaussian, heatmapWidth, heatmapHeight);
        return heatmapTexture;
    }
}
