using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class PositionHeatmapProvider : MonoBehaviour
{
    [Tooltip(
        "Number of pixels in one (in-game) meter. Increase this if you want a better resolution for your heatmap.")]
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

    private static float[,] CreatePositionHeatmapGaussianGrid(Gaussian gaussian, int heatmapWidth, int heatmapHeight,
        IReadOnlyList<Vector3> positions, IReadOnlyList<float> timestamps, float recordDuration, Vector3 planeOrigin,
        Vector3 planeSize)
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
                duration = recordDuration - currentTimestamp;
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

            for (var dx = -gaussian.Radius; dx <= gaussian.Radius; ++dx)
            {
                for (var dy = -gaussian.Radius; dy <= gaussian.Radius; ++dy)
                {
                    if (cellX + dx < 0 || cellX + dx >= heatmapWidth || cellY + dy < 0 || cellY + dy >= heatmapHeight)
                        continue;

                    heatmap[cellY + dy, cellX + dx] +=
                        duration * gaussian.Coefficients[dy + gaussian.Radius, dx + gaussian.Radius];
                }
            }
        }

        for (var i = 0; i < heatmapHeight; i++)
        {
            for (var j = 0; j < heatmapWidth; j++)
            {
                heatmap[i, j] /= recordDuration;
            }
        }

        return heatmap;
    }

    private static float[,] CreatePositionHeatmapGrid(int heatmapWidth, int heatmapHeight,
        IReadOnlyList<Vector3> positions, IReadOnlyList<float> timestamps, float recordDuration, Vector3 planeOrigin,
        Vector3 planeSize)
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
                duration = recordDuration - currentTimestamp;
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

    public Texture2D CreatePositionHeatmapTexture(IReadOnlyList<Vector3> positions, IReadOnlyList<float> timestamps,
        float recordDuration, float scaleLowerBound, float scaleUpperBound, GameObject heatmapPlane)
    {
        var bounds = heatmapPlane.GetComponent<Renderer>().bounds;
        var heatmapWidth = Mathf.FloorToInt(pixelsPerMeter * bounds.size.x);
        var heatmapHeight = Mathf.FloorToInt(pixelsPerMeter * bounds.size.z);

        var gaussian = _gaussianProvider.CreateGaussian();
        var posHeatmap = CreatePositionHeatmapGrid(heatmapWidth, heatmapHeight, positions, timestamps, recordDuration,
            bounds.min, bounds.size);
        var posHeatmapGaussian = CreatePositionHeatmapGaussianGrid(gaussian, heatmapWidth, heatmapHeight, positions,
            timestamps, recordDuration, bounds.min, bounds.size);

        _cachedMaxDuration = posHeatmap.Cast<float>().Max();
        _cachedRawHeatmap = posHeatmap;
        _cachedGaussianHeatmap = posHeatmapGaussian;

        var maxGaussian = posHeatmapGaussian.Cast<float>().Max();

        return _heatmapTextureProvider.HeatmapToTexture(NormalizeHeatmap(posHeatmapGaussian,
            scaleLowerBound * maxGaussian, scaleUpperBound * maxGaussian));
    }

    public Texture2D CreateAggregatedPositionHeatmapTexture(IReadOnlyList<List<Vector3>> positions,
        IReadOnlyList<List<float>> timestamps, List<float> recordDurations, GameObject heatmapPlane)
    {
        var bounds = heatmapPlane.GetComponent<Renderer>().bounds;
        var heatmapWidth = Mathf.FloorToInt(pixelsPerMeter * bounds.size.x);
        var heatmapHeight = Mathf.FloorToInt(pixelsPerMeter * bounds.size.z);
        var gaussian = _gaussianProvider.CreateGaussian();

        Assert.AreEqual(positions.Count, timestamps.Count);
        Assert.AreEqual(positions.Count, recordDurations.Count);

        var nRecords = positions.Count;
        var aggregatedHeatmap = new float[heatmapHeight, heatmapWidth];
        var aggregatedHeatmapGaussian = new float[heatmapHeight, heatmapWidth];

        for (var i = 0; i < nRecords; ++i)
        {
            // Skip record where the player doesn't move at all because this would cause a duration of 0s in the current system
            // This will be fixed with the new sampling system in XREcho v2
            if(positions[i].Count <= 1)
                continue;

            var posHeatmap = CreatePositionHeatmapGrid(heatmapWidth, heatmapHeight, positions[i], timestamps[i],
                recordDurations[i], bounds.min, bounds.size);
            var normalizedHeatmap = NormalizeHeatmap(posHeatmap, 0, posHeatmap.Cast<float>().Max());

            var posHeatmapGaussian = CreatePositionHeatmapGaussianGrid(gaussian, heatmapWidth, heatmapHeight,
                positions[i], timestamps[i], recordDurations[i], bounds.min, bounds.size);
            var normalizedGaussianHeatmap =
                NormalizeHeatmap(posHeatmapGaussian, 0, posHeatmapGaussian.Cast<float>().Max());

            for (var x = 0; x < heatmapWidth; x++)
            {
                for (var y = 0; y < heatmapHeight; y++)
                {
                    aggregatedHeatmap[y, x] += normalizedHeatmap[y, x];
                    aggregatedHeatmapGaussian[y, x] += normalizedGaussianHeatmap[y, x];
                }
            }
        }

        // var aggregatedHeatmapGaussian = gaussian.Apply(aggregatedHeatmap);

        _cachedMaxDuration = -1; // no meaning when normalized
        _cachedRawHeatmap = aggregatedHeatmap;
        _cachedGaussianHeatmap = aggregatedHeatmapGaussian;

        var maxGaussian = aggregatedHeatmapGaussian.Cast<float>().Max();
        return _heatmapTextureProvider.HeatmapToTexture(NormalizeHeatmap(aggregatedHeatmapGaussian, 0, maxGaussian));
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