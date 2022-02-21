using System.Collections.Generic;
using UnityEngine;

public interface IRecordDataProvider
{
    void LoadAllProjectRecordsData(out List<List<Vector3>> positions, out List<List<float>> timestamps, out List<float> totalDurations);
    
    void LoadSelectedRecordData(out List<Vector3> positions, out List<float> timestamps, out float totalDuration);
}