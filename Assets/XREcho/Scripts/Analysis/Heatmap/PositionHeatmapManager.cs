using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PositionHeatmapManager : MonoBehaviour
{
    public GameObject heatmapProjectionPlane;
    
    private static PositionHeatmapManager _instance;

    private float _transparency = 1f;
    private float _scaleLowerBound;
    private float _scaleUpperBound = 1f;

    private bool aggregationMode;
    
    private XREchoConfig _config;
    private PositionHeatmapProvider _positionHeatmapProvider;
    private IRecordDataProvider _recordDataProvider;
    private IHeatmapWriter _heatmapWriter;

    private Material _heatmapMaterial;
    private static readonly int Transparency = Shader.PropertyToID("_Transparency");
    private static readonly int ScaleLowerBound = Shader.PropertyToID("_ScaleLowerBound");
    private static readonly int ScaleUpperBound = Shader.PropertyToID("_ScaleUpperBound");

    private void Awake()
    {
        _positionHeatmapProvider = GetComponent<PositionHeatmapProvider>();
        _recordDataProvider = GetComponent<IRecordDataProvider>();
        _heatmapWriter = GetComponent<IHeatmapWriter>();
        
        if (_instance)
            Debug.LogError("2 position heatmap manager: singleton design pattern broken");

        _instance = this;
    }
    
    private void Start()
    {
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
            aggregationMode = false;
            ComputeAndApplyHeatmap();
        }
    }

    public void ToggleAggregatedPositionHeatmap(bool show)
    {
        aggregationMode = show;
        ComputeAndApplyHeatmap();
    }
    
    public void ForceRegenerate()
    {
        ComputeAndApplyHeatmap();
    }

    public void SetTransparency(float transparency)
    {
        _transparency = transparency;
        _heatmapMaterial.SetFloat(Transparency, transparency);
    }
    
    public void SetScaleBounds(float heatmapScaleLowerBound, float heatmapScaleUpperBound)
    {
        _scaleLowerBound = heatmapScaleLowerBound;
        _scaleUpperBound = heatmapScaleUpperBound;
        _heatmapMaterial.SetFloat(ScaleLowerBound, _scaleLowerBound);
        _heatmapMaterial.SetFloat(ScaleUpperBound, _scaleUpperBound);
        // ComputeAndApplyHeatmap();
    }

    public void Export()
    {
        var path = Path.Combine(_config.GetExpeRecorderPath(), "Heatmaps");
        path = Path.Combine(path, _config.project);
        path = Path.Combine(path, _config.session);
        Utils.CreateDirectoryIfNotExists(path);
        
        var filename = "position_heatmap_" + DateTime.Now.ToString(_config.dateFormat);
        var rawHeatmapPath = path + '/' + filename + "_raw.csv";
        var gaussianHeatmapPath = path + '/' + filename + "_gaussian.csv";

        var rawHeatmap = _positionHeatmapProvider.GetRawHeatmap();
        var gaussianHeatmap = _positionHeatmapProvider.GetGaussianHeatmap();
        var normalizedRawHeatmap = PositionHeatmapProvider.NormalizeHeatmap(rawHeatmap, 0, rawHeatmap.Cast<float>().Max());
        var normalizedGaussianHeatmap = PositionHeatmapProvider.NormalizeHeatmap(gaussianHeatmap, 0, gaussianHeatmap.Cast<float>().Max());

        Debug.Assert(rawHeatmap.GetLength(0) == gaussianHeatmap.GetLength(0));
        Debug.Assert(rawHeatmap.GetLength(1) == gaussianHeatmap.GetLength(1));

        _heatmapWriter.Write(rawHeatmapPath, rawHeatmap, normalizedRawHeatmap);
        _heatmapWriter.Write(rawHeatmapPath, rawHeatmap, normalizedRawHeatmap);
        _heatmapWriter.Write(gaussianHeatmapPath, gaussianHeatmap, normalizedGaussianHeatmap);
    }
    
    /**
     * - Fetch the positions from the record save
     * - Create a 2D texture of the heatmap from the positions
     * - Instantiate a new material and apply the texture to it
     * - Update the material of the heatmap plane
     */
    private void ComputeAndApplyHeatmap()
    {
        try
        {
            Texture2D heatmapTexture;

            if (aggregationMode)
            {
                _recordDataProvider.LoadAllProjectRecordsData(out var positions, out var timestamps,
                    out var recordDurations);
                heatmapTexture = _positionHeatmapProvider.CreateAggregatedPositionHeatmapTexture(positions, timestamps,
                    recordDurations, heatmapProjectionPlane);
            }
            else
            {
                _recordDataProvider.LoadSelectedRecordData(out var positions, out var timestamps,
                    out var recordDuration);
                heatmapTexture = _positionHeatmapProvider.CreatePositionHeatmapTexture(positions, timestamps,
                    recordDuration, 0, 1, heatmapProjectionPlane);
            }

            var heatmapMaterial = new Material(_heatmapMaterial)
            {
                mainTexture = heatmapTexture, mainTextureScale = Vector2.one
            };
            heatmapProjectionPlane.GetComponent<Renderer>().material = heatmapMaterial;
            _heatmapMaterial = heatmapMaterial;
            _heatmapMaterial.SetFloat(Transparency, _transparency);

            if (aggregationMode)
            {
                _heatmapMaterial.SetFloat(ScaleLowerBound, 0);
                _heatmapMaterial.SetFloat(ScaleUpperBound, 1);
            }
            else
            {
                _heatmapMaterial.SetFloat(ScaleLowerBound, _scaleLowerBound);
                _heatmapMaterial.SetFloat(ScaleUpperBound, _scaleUpperBound);
            }
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError(ex);
        }
    }
    
    /**
     * Return the duration associated with the hottest point of the heatmap (in seconds).
     * This is useful for interpreting the scale of the heatmap, the hottest point is where the player stayed the longest.
     */
    public float GetMaxDuration()
    {
        return _positionHeatmapProvider.GetMaxDuration();
    }

    public static PositionHeatmapManager GetInstance()
    {
        return _instance;
    }

}