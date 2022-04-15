#nullable enable
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GazeRay : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private Dictionary<string, object>? GetEyeTrackingDataAtTime(float t)
    {
        var data = ReplayManager.GetInstance().EyeTrackingData;

        if (data.Count == 0)
            return null;

        // Retrieve closest data in time
        // This is dirty and will change in the future thanks to refactoring of the codebase
        for (var i = 0; i < data.Count - 1; ++i)
        {
            var current = data[i];
            var next = data[i + 1];

            if ((float) current["timestamp"] <= t && (float) next["timestamp"] >= t)
            {
                return current;
            }
        }

        // Return last recorded tracking data
        return data[^1];
    }

    private static bool IsDataValid(IReadOnlyDictionary<string, object>? eyeTrackingData)
    {
        if (eyeTrackingData == null)
            return false;

        return eyeTrackingData["gazeOrigin.x"] is float && eyeTrackingData["gazeOrigin.y"] is float && eyeTrackingData["gazeOrigin.z"] is float
            && eyeTrackingData["gazeDirection.x"] is float && eyeTrackingData["gazeDirection.y"] is float && eyeTrackingData["gazeDirection.z"] is float;
    }
    
    public void Update()
    {
        _lineRenderer.enabled = true;
        
        if (!ReplayManager.GetInstance().IsReplaying() || !ReplayManager.GetInstance().ShowGaze)
        {
            _lineRenderer.enabled = false;
            return;
        }
        
        var t = ReplayManager.GetInstance().GetCurrentReplayTime();

        var eyeTrackingData = GetEyeTrackingDataAtTime(t);

        if (!IsDataValid(eyeTrackingData))
        {
            _lineRenderer.enabled = false;
            return;
        }

        var origin = new Vector3((float) eyeTrackingData!["gazeOrigin.x"], (float) eyeTrackingData["gazeOrigin.y"],
            (float) eyeTrackingData["gazeOrigin.z"]);
        var direction = new Vector3((float) eyeTrackingData["gazeDirection.x"], (float) eyeTrackingData["gazeDirection.y"],
            (float) eyeTrackingData["gazeDirection.z"]);
        
        // TODO: display intersection point (maybe record it too ?)
        
        _lineRenderer.SetPositions(new[] { origin, origin + direction * 100 });
    }
}