using System;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;

public class HtcEyeTrackingProvider : EyeTrackingProvider
{
    private EyeData_v2 _eyeData;
    
    private void Start()
    {
        gameObject.AddComponent(typeof(SRanipal_Eye_Framework));
        SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic) EyeDataCallback));
        ResetSensitivityFactor();
    }

    private static void ResetSensitivityFactor()
    {
        var eyeParameter = new EyeParameter();
        SRanipal_Eye_API.GetEyeParameter(ref eyeParameter);
        eyeParameter.gaze_ray_parameter.sensitive_factor = 0.5;
        SRanipal_Eye_API.SetEyeParameter(eyeParameter);
    }

    public override void RequestEyeCalibration()
    {
        SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
    }

    public override Vector3? GetGazeOrigin()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return null;

        SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out var combinedGazeOriginLocal,
            out _, _eyeData);

        return combinedGazeOriginLocal;
    }

    public override Vector3? GetGazeDirection()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return null;
        
        SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out _,
            out var combinedGazeDirectionLocal, _eyeData);

        return combinedGazeDirectionLocal;
    }

    public override float? GetLeftPupilDiameter()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return null;
        
        return _eyeData.verbose_data.left.pupil_diameter_mm;
    }

    public override float? GetRightPupilDiameter()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return null;

        return _eyeData.verbose_data.right.pupil_diameter_mm;
    }
    
    public override EyeTrackingProviderType GetProviderType()
    {
        return EyeTrackingProviderType.HTC;
    }

    private void EyeDataCallback(ref EyeData_v2 data)
    {
        _eyeData = data;
    }
}