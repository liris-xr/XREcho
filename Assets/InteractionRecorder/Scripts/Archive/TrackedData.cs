using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackedData : System.IEquatable<TrackedData>
{
    public string trackingScriptName;
    public float trackingRate;
    [HideInInspector]
    public string trackedDataName;
    [HideInInspector]
    public float trackingInterval;
    [HideInInspector]
    public float timeSinceLastWrite;
    [HideInInspector]
    public float[] lastData;
    public List<string> replayScripts;
    [HideInInspector]
    public TrackingScript trackingScript;

    public bool Equals(TrackedData other)
    {
        if (!trackedDataName.Equals(other.trackedDataName))
            return false;

        return true;
    }
};