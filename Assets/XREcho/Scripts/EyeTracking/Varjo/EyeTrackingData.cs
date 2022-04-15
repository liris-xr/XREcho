using System.Runtime.InteropServices;

namespace Varjo
{

    public enum EyeTrackingStatus
    {
        // Application is not allowed to access gaze data (privacy setting in VarjoBase)
        NOT_AVAILABLE,

        // Headset is not connected
        NOT_CONNECTED,

        // Gaze tracking is not calibrated
        NOT_CALIBRATED,

        // Gaze tracking is being calibrated
        CALIBRATING,

        // Gaze tracking is calibrated and can provide data for application
        CALIBRATED
    }

    public enum EyeGazeStatus
    {
        INVALID, //!< Eye is not tracked. (e.g. not visible or is shut).
        VISIBLE, //!< Eye is visible but not reliably tracked (e.g. saccade or blink).
        COMPENSATED, //!< Eye is tracked but quality compromised (e.g. headset has moved after calibration).
        TRACKED //!< Eye is tracked.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeGazeRay
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] origin; //!< Origin of the ray.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] forward; //!< Direction of the ray.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeTrackingData
    {
        public EyeTrackingStatus trackingStatus;

        public float interPupillaryDistanceInMM; //!< Estimate of user's IPD in mm.
        public float leftPupilIrisDiameterRatio; //!< Ratio of left pupil to iris diameter in mm. In range [0..1].
        public float rightPupilIrisDiameterRatio; //!< Ratio of right pupil to iris diameter in mm. In range [0..1].
        public float leftPupilDiameterInMM; //!< Estimate of left eye pupil diameter in mm.
        public float rightPupilDiameterInMM; //!< Estimate of right eye pupil diameter in mm.
        public float leftIrisDiameterInMM; //!< Estimate of left eye iris diameter in mm.
        public float rightIrisDiameterInMM; //!< Estimate of right eye iris diameter in mm.

        public EyeGazeRay leftEye; //!< Left eye gaze ray.
        public EyeGazeRay rightEye; //!< Right eye gaze ray.
        public EyeGazeRay gaze; //!< Normalized gaze direction ray.

        public EyeGazeStatus leftGazeStatus; //!< Status of left eye data.
        public EyeGazeStatus rightGazeStatus; //!< Status of right eye data.
        public EyeGazeStatus gazeStatus; //!< Tracking main status.
    }

}