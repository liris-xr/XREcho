using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using Varjo;
using Debug = UnityEngine.Debug;

public class VarjoEyeTrackingProvider : EyeTrackingProvider
{
    private Process _varjoRuntimeProcess;
    private NamedPipeServerStream _namedPipe;

    private StreamReader _runtimeReader;
    private BinaryWriter _runtimeWriter;

    private bool _receiveData = true;
    private Thread _receptionThread;

    private EyeTrackingData? _lastEyeTrackingData;

    private void OnEnable()
    {
        foreach (var process in Process.GetProcessesByName("VarjoRuntime"))
        {
            Debug.Log("Killing old VarjoRuntime process.");
            process.Kill();
        }

        _namedPipe = new NamedPipeServerStream("varjo-runtime", PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        _runtimeReader = new StreamReader(_namedPipe);
        _runtimeWriter = new BinaryWriter(_namedPipe);

        var runtimePath = Application.streamingAssetsPath + "/XREcho/EyeTracking/Varjo/VarjoRuntime.exe";
        _varjoRuntimeProcess = new Process();
        _varjoRuntimeProcess.StartInfo.FileName = runtimePath;
        _varjoRuntimeProcess.StartInfo.UseShellExecute = true;
        _varjoRuntimeProcess.Start();

        _namedPipe.WaitForConnection();

        _receptionThread = new Thread(ReceiveDataTask);
        _receptionThread.Start();
    }

    private void OnDisable()
    {
        _receiveData = false;
        _receptionThread.Join();
        _namedPipe.Dispose();
        _varjoRuntimeProcess.Kill();
    }

    private void ReceiveDataTask()
    {
        while (_receiveData)
        {
            var json = _runtimeReader.ReadLine();

            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonConvert.DeserializeObject<EyeTrackingData>(json);
                _lastEyeTrackingData = data;
            }
            else
            {
                _lastEyeTrackingData = null;
            }
            
            Thread.Sleep(50);
        }
    }

    private void SendRequest(Request req)
    {
        var bytes = new byte[Marshal.SizeOf(typeof(Request))];
        var pinStructure = GCHandle.Alloc(req, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
        }
        finally
        {
            pinStructure.Free();
        }

        _runtimeWriter.Write(bytes, 0, bytes.Length);
        _runtimeWriter.Flush();
    }

    public override void RequestEyeCalibration()
    {
        var req = new Request
        {
            type = RequestType.CALIBRATE_EYE_TRACKING
        };

        SendRequest(req);
    }

    public override Vector3? GetGazeOrigin()
    {
        if (_lastEyeTrackingData?.trackingStatus != EyeTrackingStatus.CALIBRATED)
            return null;

        if (_lastEyeTrackingData?.gazeStatus != EyeGazeStatus.TRACKED &&
            _lastEyeTrackingData?.gazeStatus != EyeGazeStatus.COMPENSATED)
            return null;

        var origin = _lastEyeTrackingData?.gaze.origin;
        return new Vector3((float) origin[0], (float) origin[1], (float) origin[2]);
    }

    public override Vector3? GetGazeDirection()
    {
        if (_lastEyeTrackingData?.trackingStatus != EyeTrackingStatus.CALIBRATED)
            return null;

        if (_lastEyeTrackingData?.gazeStatus != EyeGazeStatus.TRACKED &&
            _lastEyeTrackingData?.gazeStatus != EyeGazeStatus.COMPENSATED)
            return null;

        var direction = _lastEyeTrackingData?.gaze.forward;
        
        return new Vector3((float) direction[0], (float) direction[1],
            (float) direction[2]);
    }

    public override float? GetLeftPupilDiameter()
    {
        if (_lastEyeTrackingData?.trackingStatus != EyeTrackingStatus.CALIBRATED || _lastEyeTrackingData?.leftPupilDiameterInMM == 0)
        {
            return null;
        }
        
        return _lastEyeTrackingData?.leftPupilDiameterInMM;
    }

    public override float? GetRightPupilDiameter()
    {
        if (_lastEyeTrackingData?.trackingStatus != EyeTrackingStatus.CALIBRATED || _lastEyeTrackingData?.rightPupilDiameterInMM == 0)
        {
            return null;
        }
        
        return _lastEyeTrackingData?.rightPupilDiameterInMM;
    }

    public override EyeTrackingProviderType GetProviderType()
    {
        return EyeTrackingProviderType.Varjo;
    }
}