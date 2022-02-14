using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PositionHeatmapManager : MonoBehaviour
{
    public GameObject heatmapProjectionPlane;
    
    private static PositionHeatmapManager _instance;

    private float _transparency = 1f;
    
    private ReplayManager _replayManager;
    private RecordingManager _recordingManager;
    private XREchoConfig _config;
    private PositionHeatmapProvider _positionHeatmapProvider;

    private Material _heatmapMaterial;

    private void Awake()
    {
        _positionHeatmapProvider = GetComponent<PositionHeatmapProvider>();
        
        if (_instance)
            Debug.LogError("2 position heatmap manager: singleton design pattern broken");

        _instance = this;
    }
    
    private void Start()
    {
        _replayManager = ReplayManager.GetInstance();
        _recordingManager = RecordingManager.GetInstance();
        _config = XREchoConfig.GetInstance();
        _heatmapMaterial = MaterialManager.GetInstance().GetMaterial("heatmap");
    }

    /**
     * Method called from the GUI when selecting the "Position heatmap" checkbox in the "Analyze" tab.
     * It recomputes the heatmap when the checkbox is selected.
     */
    public void TogglePositionHeatmap(bool show)
    {
        heatmapProjectionPlane.SetActive(show);

        if (show)
        {
            ComputeAndApplyHeatmap();
        }
    }

    public void ForceRegenerate()
    {
        ComputeAndApplyHeatmap();
    }

    public void SetTransparency(float transparency)
    {
        _transparency = transparency;
        heatmapProjectionPlane.GetComponent<Renderer>().material.color = new Color(1, 1, 1, _transparency);
    }

    public void ExportRawData()
    {
        var path = Path.Combine(_config.GetExpeRecorderPath(), "Heatmaps");
        path = Path.Combine(path, _config.project);
        path = Path.Combine(path, _config.session);
        Utils.CreateDirectoryIfNotExists(path);
        
        var filename = "position_heatmap_" + DateTime.Now.ToString(_config.dateFormat);
        path = path + '/' + filename + ".csv";

        var rawHeatmap = PositionHeatmapProvider.NormalizeHeatmap(_positionHeatmapProvider.GetCachedRawHeatmap());
        var gaussianHeatmap = PositionHeatmapProvider.NormalizeHeatmap(_positionHeatmapProvider.GetCachedGaussianHeatmap());

        Debug.Assert(rawHeatmap.GetLength(0) == gaussianHeatmap.GetLength(0));
        Debug.Assert(rawHeatmap.GetLength(1) == gaussianHeatmap.GetLength(1));
        
        var csvWriter = new CSVWriter(path);
        csvWriter.WriteLine("gridX", "gridY", "Normalized raw value", "Normalized gaussian value");
        
        for (var y = 0; y < rawHeatmap.GetLength(0); y++)
        {
            for (var x = 0; x < rawHeatmap.GetLength(1); x++)
            {
                csvWriter.WriteLine(x, y, rawHeatmap[y, x], gaussianHeatmap[y, x]);
            }
        }

        csvWriter.Close();
    }
    
    /**
     * - Fetch the positions from the record save
     * - Create a 2D texture of the heatmap from the positions
     * - Instantiate a new material and apply the texture to it
     * - Update the material of the heatmap plane
     */
    private void ComputeAndApplyHeatmap()
    {
        EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...", -1);

        try
        {
            var positions = LoadRecordPositions();
            var heatmapTexture =
                _positionHeatmapProvider.CreatePositionHeatmapTexture(positions, heatmapProjectionPlane);
            var heatmapMaterial = new Material(_heatmapMaterial)
            {
                mainTexture = heatmapTexture, mainTextureScale = Vector2.one, color = new Color(1, 1, 1, _transparency)
            };
            heatmapProjectionPlane.GetComponent<Renderer>().material = heatmapMaterial;
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError(ex);
        }
        
        EditorUtility.ClearProgressBar();
    }
    
    /**
     * Return the duration associated with the hottest point of the heatmap (in seconds).
     * This is useful for interpreting the scale of the heatmap, the hottest point is where the player stayed the longest.
     */
    public float GetMaxDurationValue()
    {
        var max = _positionHeatmapProvider.GetCachedRawHeatmap().Cast<float>().Max();
        return max * 1000 / _recordingManager.trackingRateHz;
    }
    
    private IEnumerable<Vector3> LoadRecordPositions()
    {
        var positions = new List<Vector3>();

        if (_replayManager.objectsData == null || _replayManager.objectsData.Count == 0)
        {
            throw new InvalidOperationException("Can't compute position heatmap without recordings");
        }

        var logs = _replayManager.objectsData[0];

        foreach (var entry in logs)
        {
            if ((int)(float)entry["actionId"] == (int) ActionType.POSITION)
            {
                positions.Add(ReplayManager.LoadPosition(entry));
            }
        }

        return positions;
    }
    
    public static PositionHeatmapManager GetInstance()
    {
        return _instance;
    }
}