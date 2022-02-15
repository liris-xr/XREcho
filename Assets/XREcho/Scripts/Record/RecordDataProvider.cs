using System;
using System.Collections.Generic;
using UnityEngine;

public class RecordDataProvider : MonoBehaviour, IRecordDataProvider
{
    
    private ReplayManager _replayManager;
    
    private void Start()
    {
        _replayManager = ReplayManager.GetInstance();
    }
    
    public void LoadRecordData(out List<Vector3> positions, out List<float> timestamps, out float totalRecordTime)
    {
        positions = new List<Vector3>();
        timestamps = new List<float>();
        
        if (_replayManager.objectsData == null || _replayManager.objectsData.Count == 0)
        {
            throw new InvalidOperationException("Can't compute position heatmap without recordings or if recording is empty");
        }

        var logs = _replayManager.objectsData[0];

        foreach (var entry in logs)
        {
            if ((int)(float)entry["actionId"] == (int) ActionType.POSITION)
            {
                positions.Add(ReplayManager.LoadPosition(entry));
                timestamps.Add((float) entry["timestamp"]);
            }
        }

        totalRecordTime = _replayManager.GetTotalReplayTime();
    }
}