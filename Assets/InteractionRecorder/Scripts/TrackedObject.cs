using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackedObject : System.IEquatable<TrackedObject>
{
    public GameObject obj;
    public string objPath;
    public bool trackPosition;
    public bool trackRotation;
    public bool trackCamera;
    public float trackingRate;
    [HideInInspector]
    public float trackingInterval;
    [HideInInspector]
    public float timeSinceLastWrite;
    [HideInInspector]
    public Vector3 lastPosition;
    [HideInInspector]
    public Vector3 lastRotation;
    public GameObject replayGameObject;
    public string[] replayScripts;

    public bool Equals(TrackedObject other)
    {
        if (!objPath.Equals(other.objPath))
            return false;

        if (trackPosition != other.trackPosition)
            return false;

        if (trackRotation != other.trackRotation)
            return false;

        if (trackCamera != other.trackCamera)
            return false;

        return true;
    }
};