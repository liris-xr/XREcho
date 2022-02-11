////========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ViveSR;
using ViveSR.anipal.Eye;

public class GetEyeTrackingData : MonoBehaviour
{
    private static EyeData_v2 eyeData = new EyeData_v2();
    private static EyeParameter eye_parameter;
    private static bool eye_callback_registered = false;

    // Tracked Data Variables
    private static long localTimestamp;
    private static float timestamp;
    private static int frame;

    private static UInt64 eye_valid_L, eye_valid_R, eye_valid_C;                 // The bits explaining the validity of eye data.
    private static float openness_L, openness_R, openness_C;                    // The level of eye openness.
    private static float pupil_diameter_L, pupil_diameter_R, pupil_diameter_C;        // Diameter of pupil dilation.
    private static Vector2 pos_sensor_L, pos_sensor_R, pos_sensor_C;              // Positions of pupils.
    private static Vector3 gaze_origin_L, gaze_origin_R, gaze_origin_C;            // Position of gaze origin.
    private static Vector3 gaze_direct_L, gaze_direct_R, gaze_direct_C;            // Direction of gaze ray.
    private static float frown_L, frown_R;                          // The level of user's frown.
    private static float squeeze_L, squeeze_R;                      // The level to show how the eye is closed tightly.
    private static float wide_L, wide_R;                            // The level to show how the eye is open widely.
    private static double gaze_sensitive;                           // The sensitive factor of gaze ray.
    private static float distance_C;                                // Distance from the central point of right and left eyes.
    private static bool distance_valid_C;                           // Validity of combined data of right and left eyes.

    private static int track_imp_cnt = 0;
    private static TrackingImprovement[] track_imp_item;

    // File Saving Variables
    private static RecordingManager recordingManager;
    private static string eyeDataPath;
    private static StreamWriter eyeDataWriter;

    public float sensitivityFactor = 0.5f;

    public static void LaunchEyeCalibration()
    {
        SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
    }

    private void OnValidate()
    {
        ResetSensitivityFactor();
    }

    private void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
                    
        recordingManager = RecordingManager.GetInstance();
        
        ResetSensitivityFactor();
    }

    private void ResetSensitivityFactor ()
    {
        if (!enabled) return;

        eye_parameter = new EyeParameter();
        Error error = SRanipal_Eye_API.GetEyeParameter(ref eye_parameter);
        eye_parameter.gaze_ray_parameter.sensitive_factor = sensitivityFactor;
        error = SRanipal_Eye_API.SetEyeParameter(eye_parameter);
        //Debug.Log("SetEyeParameter: " + error + "\n" + "sensitive_factor: " + eye_parameter.gaze_ray_parameter.sensitive_factor);
    }

    private void OnDisable()
    {
        StopRecording();
    }

    private static void WriteHeader()
    {
        List<string> fields = new List<string>();
        fields.Add("time(100ns)");
        fields.Add("time_stamp(ms)");
        fields.Add("frame");
        fields.Add("eye_valid_L");
        fields.Add("eye_valid_R");
        fields.Add("eye_valid_C");
        fields.Add("openness_L");
        fields.Add("openness_R");
        fields.Add("openness_C");
        fields.Add("pupil_diameter_L(mm)");
        fields.Add("pupil_diameter_R(mm)");
        fields.Add("pupil_diameter_C(mm)");
        fields.Add("pos_sensor_L.x");
        fields.Add("pos_sensor_L.y");
        fields.Add("pos_sensor_R.x");
        fields.Add("pos_sensor_R.y");
        fields.Add("pos_sensor_C.x");
        fields.Add("pos_sensor_C.y");
        fields.Add("gaze_origin_L.x(mm)");
        fields.Add("gaze_origin_L.y(mm)");
        fields.Add("gaze_origin_L.z(mm)");
        fields.Add("gaze_origin_R.x(mm)");
        fields.Add("gaze_origin_R.y(mm)");
        fields.Add("gaze_origin_R.z(mm)");
        fields.Add("gaze_origin_C.x(mm)");
        fields.Add("gaze_origin_C.y(mm)");
        fields.Add("gaze_origin_C.z(mm)");
        fields.Add("gaze_direct_L.x");
        fields.Add("gaze_direct_L.y");
        fields.Add("gaze_direct_L.z");
        fields.Add("gaze_direct_R.x");
        fields.Add("gaze_direct_R.y");
        fields.Add("gaze_direct_R.z");
        fields.Add("gaze_direct_C.x");
        fields.Add("gaze_direct_C.y");
        fields.Add("gaze_direct_C.z");
        fields.Add("gaze_sensitive");
        fields.Add("frown_L");
        fields.Add("frown_R");
        fields.Add("squeeze_L");
        fields.Add("squeeze_R");
        fields.Add("wide_L");
        fields.Add("wide_R");
        fields.Add("distance_valid_C");
        fields.Add("distance_C(mm)");
        fields.Add("track_imp_cnt");

        string text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, fields);
        eyeDataWriter.WriteLine(text);
    }

    private void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;

        if (eye_callback_registered)
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else return;
        }
        else
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else return;
        }

                    
        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
        transform.position = Camera.main.transform.position;
        transform.LookAt(Camera.main.transform.position + GazeDirectionCombined);
    }

    private static void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    private static void StartRecording()
    {
        eyeDataPath = recordingManager.GetSavePath("HTCViveProEyeData");
        eyeDataWriter = new StreamWriter(eyeDataPath);
        Debug.Log("Started recording eye tracking data to " + eyeDataPath);
        WriteHeader();
    }

    private void StopRecording()
    {
        Release();

        if (eyeDataWriter != null)
        {
            eyeDataWriter.Flush();
            eyeDataWriter.Close();
            eyeDataWriter = null;
        }
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;

        if (!recordingManager.IsRecording())
            return;

        if (eyeDataWriter == null)
        {
            StartRecording();
        }

        Error error = SRanipal_Eye_API.GetEyeParameter(ref eye_parameter);
        localTimestamp = DateTime.Now.Ticks;
        timestamp = eyeData.timestamp;
        frame = eyeData.frame_sequence;
        eye_valid_L = eyeData.verbose_data.left.eye_data_validata_bit_mask;
        eye_valid_R = eyeData.verbose_data.right.eye_data_validata_bit_mask;
        eye_valid_C = eyeData.verbose_data.combined.eye_data.eye_data_validata_bit_mask;
        openness_L = eyeData.verbose_data.left.eye_openness;
        openness_R = eyeData.verbose_data.right.eye_openness;
        openness_C = eyeData.verbose_data.combined.eye_data.eye_openness;
        pupil_diameter_L = eyeData.verbose_data.left.pupil_diameter_mm;
        pupil_diameter_R = eyeData.verbose_data.right.pupil_diameter_mm;
        pupil_diameter_C = eyeData.verbose_data.combined.eye_data.pupil_diameter_mm;
        pos_sensor_L = eyeData.verbose_data.left.pupil_position_in_sensor_area;
        pos_sensor_R = eyeData.verbose_data.right.pupil_position_in_sensor_area;
        pos_sensor_C = eyeData.verbose_data.combined.eye_data.pupil_position_in_sensor_area;
        gaze_origin_L = eyeData.verbose_data.left.gaze_origin_mm;
        gaze_origin_R = eyeData.verbose_data.right.gaze_origin_mm;
        gaze_origin_C = eyeData.verbose_data.combined.eye_data.gaze_origin_mm;
        gaze_direct_L = eyeData.verbose_data.left.gaze_direction_normalized;
        gaze_direct_R = eyeData.verbose_data.right.gaze_direction_normalized;
        gaze_direct_C = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
        gaze_sensitive = eye_parameter.gaze_ray_parameter.sensitive_factor;
        frown_L = eyeData.expression_data.left.eye_frown;
        frown_R = eyeData.expression_data.right.eye_frown;
        squeeze_L = eyeData.expression_data.left.eye_squeeze;
        squeeze_R = eyeData.expression_data.right.eye_squeeze;
        wide_L = eyeData.expression_data.left.eye_wide;
        wide_R = eyeData.expression_data.right.eye_wide;
        distance_valid_C = eyeData.verbose_data.combined.convergence_distance_validity;
        distance_C = eyeData.verbose_data.combined.convergence_distance_mm;
        track_imp_cnt = eyeData.verbose_data.tracking_improvements.count;
        track_imp_item = eyeData.verbose_data.tracking_improvements.items;

        WriteRow();
    }

    private static void WriteRow()
    {
        string row = "";
        row += localTimestamp + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += timestamp + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += frame + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += eye_valid_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += eye_valid_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += eye_valid_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += openness_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += openness_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += openness_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pupil_diameter_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pupil_diameter_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pupil_diameter_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pos_sensor_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pos_sensor_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += pos_sensor_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_origin_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_origin_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_origin_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_direct_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_direct_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_direct_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += gaze_sensitive + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += frown_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += frown_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += squeeze_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += squeeze_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += wide_L + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += wide_R + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += distance_valid_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += distance_C + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        row += track_imp_cnt + CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        eyeDataWriter.WriteLine(row);
    }
}
