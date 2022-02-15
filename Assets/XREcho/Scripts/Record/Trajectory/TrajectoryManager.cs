using UnityEngine;

public class TrajectoryManager : MonoBehaviour
{
    
    private static TrajectoryManager _instance;
    private IRecordDataProvider _recordDataProvider;

    public float tolerance = 0.5f;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _recordDataProvider = GetComponent<IRecordDataProvider>();
        _lineRenderer = GetComponent<LineRenderer>();
        
        if (_instance)
            Debug.LogError("2 trajectory manager: singleton design pattern broken");

        _instance = this;
    }
    
    public void ToggleTrajectory(bool show)
    {
        if (show)
        {
            _recordDataProvider.LoadRecordData(out var positions, out _, out _);
            
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.positionCount = positions.Count;
            for (var i = 0; i < positions.Count; ++i)
            {
                _lineRenderer.SetPosition(i, positions[i]);
            }
            
            _lineRenderer.Simplify(tolerance);
        }
    }
    
    public static TrajectoryManager GetInstance()
    {
        return _instance;
    }
    
}