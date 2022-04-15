using UnityEngine;

public abstract class EyeTrackingProvider : MonoBehaviour
{

    public abstract void RequestEyeCalibration();
    
    public abstract Vector3? GetGazeOrigin();

    public abstract Vector3? GetGazeDirection();

    public abstract float? GetLeftPupilDiameter();

    public abstract float? GetRightPupilDiameter();

    public abstract EyeTrackingProviderType GetProviderType();

}