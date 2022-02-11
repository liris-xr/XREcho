using System.Collections.Generic;
using UnityEngine;

public class PositionHeatmapManager : MonoBehaviour
{
    private static PositionHeatmapManager _instance;
    
    private GameObject _heatmapPlane;
    private ReplayManager _replayManager;
    private PositionHeatmapProvider _positionHeatmapProvider;

    private Material _heatmapMaterial;

    private void Awake()
    {
        _positionHeatmapProvider = GetComponent<PositionHeatmapProvider>();
        
        if (_instance)
            Debug.LogError("2 position heatmap manager: singleton design pattern broken");

        _instance = this;
        InitHeatmapPlane();
    }
    
    private void Start()
    {
        _replayManager = ReplayManager.GetInstance();
        _heatmapMaterial = MaterialManager.GetInstance().GetMaterial("heatmap");
    }
    
    private void InitHeatmapPlane()
    {
        if (_heatmapPlane != null)
            Destroy(_heatmapPlane);

        _heatmapPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _heatmapPlane.name = "Position heatmap";
        _heatmapPlane.transform.parent = transform;
        
        // Put the plane above ground level to see it
        _heatmapPlane.transform.position += new Vector3(0.0f, 0.01f, 0.0f);
        
        _heatmapPlane.SetActive(false);
    }

    /**
     * Method called from the GUI when selecting the "Position heatmap" checkbox in the "Analyze" tab.
     * It recomputes the heatmap when the checkbox is selected.
     */
    public void TogglePositionHeatmap(bool show)
    {
        _heatmapPlane.SetActive(show);

        if (show)
        {
            ComputeAndApplyHeatmap();
        }
    }

    public void ForceRegenerate()
    {
        ComputeAndApplyHeatmap();
    }
    
    /**
     * - Fetch the positions from the record save
     * - Create a 2D texture of the heatmap from the positions
     * - Instantiate a new material and apply the texture to it
     * - Update the material of the heatmap plane
     */
    private void ComputeAndApplyHeatmap()
    {
        var positions = LoadRecordPositions();
        var heatmapTexture = _positionHeatmapProvider.CreatePositionHeatmapTexture(positions, _heatmapPlane);
        var heatmapMaterial = new Material(_heatmapMaterial)
        {
            mainTexture = heatmapTexture, mainTextureScale = Vector2.one
        };
        _heatmapPlane.GetComponent<Renderer>().material = heatmapMaterial;
    }
    
    private IEnumerable<Vector3> LoadRecordPositions()
    {
        var positions = new List<Vector3>();

        if (_replayManager.objectsData == null || _replayManager.objectsData.Count == 0)
        {
            Debug.LogError("Can't compute position heatmap without recordings");
            return positions;
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