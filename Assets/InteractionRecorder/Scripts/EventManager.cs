using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private RecordingManager recordingManager;

    // TODO : Convert Headset ON/OFF and Teleport Event to OpenXR

    //private SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
    //private SteamVR_Action_Boolean teleporting = SteamVR_Input.GetBooleanAction("Teleport");

    //public void HeadsetOnHead(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    recordingManager.LogEvent("HeadsetMounted");
    //}

    //public void HeadsetOffHead(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    recordingManager.LogEvent("HeadsetRemoved");
    //}

    private void Start()
    {
        recordingManager = RecordingManager.GetInstance();

        //headsetOnHead.AddOnStateDownListener(HeadsetOnHead, SteamVR_Input_Sources.Head);
        //headsetOnHead.AddOnStateUpListener(HeadsetOffHead, SteamVR_Input_Sources.Head);
    }
}
