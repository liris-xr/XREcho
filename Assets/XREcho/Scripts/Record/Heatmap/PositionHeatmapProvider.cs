using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class PositionHeatmapProvider : MonoBehaviour
{
    [Tooltip("Number of pixels in one (in-game) meter. Increase this if you want a better resolution for your heatmap.")]
    public int pixelsPerMeter = 20;
    
    // The raw heatmap grid contains the time spent in each cell (not normalized)
    private float[,] _cachedRawHeatmap;
    // The gaussian heatmap grid contains the post-processed raw heatmap when applying gaussian (not normalized)
    private float[,] _cachedGaussianHeatmap;

    private float _cachedMaxDuration;
    
    private IGaussianProvider _gaussianProvider;
    private IHeatmapTextureProvider _heatmapTextureProvider;

    private void Awake()
    {
        _gaussianProvider = gameObject.GetComponent<GaussianProvider>();
        _heatmapTextureProvider = gameObject.GetComponent<HeatmapTextureProvider>();
    }

    /**
     * Apply a min-max normalization on the given heatmap
     */
    public static float[,] NormalizeHeatmap(float[,] heatmap, float min, float max)
    {
        var heatmapHeight = heatmap.GetLength(0);
        var heatmapWidth = heatmap.GetLength(1);
        var normalizedHeatmap = new float[heatmapHeight, heatmapWidth];
        
        for (var x = 0; x < heatmapWidth; x++)
        {
            for (var y = 0; y < heatmapHeight; y++)
            {
                normalizedHeatmap[y, x] = Mathf.Clamp((heatmap[y, x] - min) / (max - min), 0, 1);
            }
        }

        return normalizedHeatmap;
    }
    
    private static float[,] CreatePositionHeatmapGrid(int heatmapWidth, int heatmapHeight, float recordTotalDuration, IReadOnlyList<Vector3> positions, IReadOnlyList<float> timestamps, Vector3 planeOrigin, Vector3 planeSize)
    {
        Assert.AreEqual(positions.Count, timestamps.Count);
        
        var heatmap = new float[heatmapHeight, heatmapWidth];

        for (var i = 0; i < positions.Count; ++i)
        {
            var currentPosition = positions[i];
            var currentTimestamp = timestamps[i];
            float duration;
            
            if (i == positions.Count - 1)
            {
                duration = recordTotalDuration - currentTimestamp;
            }
            else
            {
                var nextTimestamp = timestamps[i + 1];
                duration = nextTimestamp - currentTimestamp;
            }
            
            // Convert to relative coordinates (between 0 and 1)
            var relPos = currentPosition - planeOrigin;
            relPos.x /= planeSize.x;
            relPos.z /= planeSize.z;

            // Skip positions outside of the heatmap
            if (relPos.x < 0 || relPos.x >= heatmapWidth || relPos.z < 0 || relPos.z >= heatmapHeight)
                continue;
            
            var cellX = Mathf.CeilToInt(relPos.x * heatmapWidth) - 1;
            var cellY = Mathf.CeilToInt(relPos.z * heatmapHeight) - 1;
            heatmap[cellY, cellX] += duration;
        }
        
        return heatmap;
    }

    public Texture2D CreatePositionHeatmapTexture(IReadOnlyList<Vector3> positions, IReadOnlyList<float> timestamps, float scaleLowerBound, float scaleUpperBound, float recordTotalDuration, GameObject heatmapPlane)
    {
        var bounds = heatmapPlane.GetComponent<Renderer>().bounds;
        var heatmapWidth = Mathf.FloorToInt(pixelsPerMeter * bounds.size.x);
        var heatmapHeight = Mathf.FloorToInt(pixelsPerMeter * bounds.size.z);

        var gaussian = _gaussianProvider.CreateGaussian();
        var posHeatmap = CreatePositionHeatmapGrid(heatmapWidth, heatmapHeight, recordTotalDuration, positions, timestamps, bounds.min, bounds.size);
        var posHeatmapGaussian = gaussian.Apply(posHeatmap);

        _cachedMaxDuration = posHeatmap.Cast<float>().Max();
        _cachedRawHeatmap = posHeatmap;
        _cachedGaussianHeatmap = posHeatmapGaussian;
        
        var maxGaussian = posHeatmapGaussian.Cast<float>().Max();
        
        return _heatmapTextureProvider.HeatmapToTexture(NormalizeHeatmap(posHeatmapGaussian, scaleLowerBound * maxGaussian, scaleUpperBound * maxGaussian));
    }

    public float GetMaxDuration()
    {
        return _cachedMaxDuration;
    }

    public float[,] GetRawHeatmap()
    {
        return _cachedRawHeatmap;
    }
    
    public float[,] GetGaussianHeatmap()
    {
        return _cachedGaussianHeatmap;
    }
}
