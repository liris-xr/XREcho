using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordDataProvider : MonoBehaviour, IRecordDataProvider
{
    
    private ReplayManager _replayManager;
    private XREchoConfig _config;
    
    private void Start()
    {
        _replayManager = ReplayManager.GetInstance();
        _config = XREchoConfig.GetInstance();
    }
    
    public void LoadAllProjectRecordsData(out List<List<Vector3>> positions, out List<List<float>> timestamps, out List<float> totalDurations)
    {
        var projectFolder = Path.Combine(_config.GetRecordingsFolder(), _replayManager.replayProject);
        var projectDirectory = new DirectoryInfo(projectFolder);
        var sessionsDirectories = projectDirectory.GetDirectories();
        
        Debug.Log(projectFolder);
        
        var recordIdx = 0;
        positions = new List<List<Vector3>>();
        timestamps = new List<List<float>>();
        totalDurations = new List<float>();
        
        foreach(var sessionDirectory in sessionsDirectories)
        {
            var objectsDataFiles = sessionDirectory.GetFiles("*objectsData" + _config.GetFileSuffix() + ".csv");
            
            foreach (var objectsDataFile in objectsDataFiles)
            {
                var data = CSVReader.ReadCSV(objectsDataFile.FullName);
                var totalDuration = Convert.ToSingle(data[data.Count - 1]["timestamp"]);

                positions.Add(new List<Vector3>());
                timestamps.Add(new List<float>());
                totalDurations.Add(totalDuration);

                foreach (var entry in data)
                {
                    var actionId = Convert.ToInt32(entry["actionId"]);

                    // Only get position of the HMD
                    if (actionId != 0) continue;
                    
                    positions[recordIdx].Add(ReplayManager.LoadPosition(entry));
                    timestamps[recordIdx].Add(Convert.ToSingle(entry["timestamp"]));
                }

                ++recordIdx;
            }
        }
    }

    public void LoadSelectedRecordData(out List<Vector3> positions, out List<float> timestamps, out float recordDuration)
    {
        positions = new List<Vector3>();
        timestamps = new List<float>();
        
        if (_replayManager.objectsData == null || _replayManager.objectsData.Count == 0)
        {
            throw new InvalidOperationException("No recording loaded");
        }

        var data = _replayManager.objectsData[0];
        recordDuration = Convert.ToSingle(data[data.Count - 1]["timestamp"]);

        foreach (var entry in data)
        {
            var actionId = Convert.ToInt32(entry["actionId"]);

            if (actionId == 0)
            {
                positions.Add(ReplayManager.LoadPosition(entry));
                timestamps.Add(Convert.ToSingle(entry["timestamp"]));
            }
        }
    }
}