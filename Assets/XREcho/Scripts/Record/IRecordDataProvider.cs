using System.Collections.Generic;
using UnityEngine;

public interface IRecordDataProvider
{
    void LoadRecordData(out List<Vector3> positions, out List<float> timestamps, out float totalRecordTime);
}