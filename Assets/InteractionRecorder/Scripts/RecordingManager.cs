using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System.IO;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR.Interaction.Toolkit;

/*
 * TODO:
 * - trajectories not cleaned up ?
 * - should pause music also on replay
 * - add a generic possibility to reset scenes to beginning (such as gazedUpon false, active/inactive game objects, animations and so on)
 * - replay manager add callbacks to events or give ability to add replay scripts on specific scenes ?
 * - Change semantics (clone, targetObj, etc) clones are clones, some clones you want to hide on start (found in original scene) others you don't
 * - should put every objects in their beginning state after end of replay, maybe store it as event (active/inactive) ?
 * - choose dateBegin in inspector
 * - should separate config from recordings
 * - place prise par textfield
 * - metadata revoir system et get/set
 * - no combined eye in 120hz callback
 * - Use targetObj in recording? (only for clones)
 * - Fix jump forward and implement reverse playing
 * - Put every camera on different display + switch display buttons + switch display on headset + fix camera offset in HTC
 * - Don't create empty game objects for interactable not here
 * - remove scene from metadata file name?
 * - PRIORITY+++++++++++++: check pour stocker eyeData � partir du recordingManager?
 * - PRIORITY++++: Record/replay on scene load on ExpeRecorder
 * - PRIORITY+++++: Check if writing empty string works
 * - PRIORITY++: voir comment enregistrer button presses on HTC Vive AND ADD HEADSET REMOVED
 * - PRIORITY++: Store last project and session in appdata
 * - PRIORITY++: timestamp absolu dans metadata
 * - make eye_valid stuff read-only in runtime
 * - cultureInfo list of fr-FR formats instead of enum in inspector
 * - Benchmark reading 600s recording and optimize -> to put in report
 * - PRIORITY++: read different cultures, either auto-detect or put in metadata
 * - PRIORITY: viewport points to see gaze
 * - PRIORITY++: Replay scripts should be able to observe data instead of attaching to abstract game objects
 * - show toggling icon on 3D canvas for camera frustum, show gaze on eyes, etc
 * - PRIORITY: Not have to copy camera but COPY IT if here for correct VR tracking
 * - Reproduce hierarchy in clones
 * - check why 2 control points found
 * - not track gameobject without meshFilter
 * - need to detect when object are children of other and recreate hierarchy
 * - Add replayScript for pupil diameter
 * - Make a correct logging system
 * - should use namespaces and split code into different well documented classes
 * - maybe control also gaze and all visualization in the trajectoryManager
 * - Possibility of using local positions/rotations
 * - Put up tests and continuous integration
 */

/// <summary>
/// The <c>RecordingManager</c> class contains all methods to track and log data about objects, eye-trackers, various scripts, and everything else.
/// </summary>
public class RecordingManager : MonoBehaviour
{
    private static RecordingManager instance;

    private bool recording;
    public bool IsRecording() { return recording; }
    private float timeSinceStartOfRecording;
    public float GetTimeSinceStartOfRecording() { return timeSinceStartOfRecording; }
    private float timeSinceLastControlPoint;
    private int nbOriginalTrackedObjects;

    private CSVWriter metadataWriter;
    private CSVWriter objectsWriter;
    private CSVWriter eventsWriter;
    private CSVWriter objectsFormatWriter;
    private CSVWriter eventsFormatWriter;

    private List<RecordingMetadata> metadata;
    public List<TrackedObject> trackedObjects = new List<TrackedObject>();
    private List<TrackingAction> actions;

    private Dictionary<string, int> eventToId;
    private List<string> events;
    
    private bool keepRecordingOnNewScene = true;
    private bool recordingOnSceneUnloaded; // Used to track if we were recording on scene change if keepRecordingOnNewScene is true
    public bool trackXRInteractables = true;
    public float interactablesTrackingRateHz = 100;
    [Space(8)]
    public KeyCode recordShortcut = KeyCode.S;
    private float recordKeyCooldown = 0.4f;
    private float recAlpha = 1.0f;

    private ExpeRecorderConfig config;

    // This code is used to display the trajectory while recording which can be useful to debug trajectories
    public bool showControlPointsWhileRecording = false;

    private TrajectoryManager trajectoryManager;
    private int trajectoryIndex;
    private List<Vector3> controlPoints;
    private Vector3 lastControlPoint;

    private void Awake()
    {
        if (instance)
            Debug.LogError("2 Recording Managers: singleton design pattern broken");

        instance = this;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        nbOriginalTrackedObjects = trackedObjects.Count;

        recording = false;
        keepRecordingOnNewScene = XREcho.GetInstance().dontDestroyOnLoad;
        recordingOnSceneUnloaded = false;

        trajectoryManager = TrajectoryManager.GetInstance();
        config = ExpeRecorderConfig.GetInstance();

        NewScene(SceneManager.GetActiveScene().name);

        if (XREcho.GetInstance().autoExecution == XREcho.AutoMode.AutoStartRecord) StartRecording();
    }

    private void OnGUI()
    {
        if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == recordShortcut)
        {
            ToggleRecording(recordKeyCooldown);
        }
        if (!XREcho.GetInstance().displayGUI && recording)
        {
            GUILayout.BeginArea(new Rect(10, 5, 200, 100));
            recAlpha = (recAlpha + 0.01f) % 2.0f;
            float curAlpha = recAlpha > 1.0f ? 2.0f - recAlpha : recAlpha;
            GUI.color = new Color(0.9f, 0.0f, 0.0f, curAlpha);
            string lab = "<size=30>[Rec.]</size>";
            GUILayout.Label(lab);
            GUI.color = Color.white;
            GUILayout.EndArea();
        }
    }

    public void NewSession()
    {
        metadataWriter = null;
        metadata = new List<RecordingMetadata>();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        NewScene(scene.name);
        if (recordingOnSceneUnloaded)
        {
            recordingOnSceneUnloaded = false;
            StartRecording();
        }
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene current)
    {
        if (recording)
        {
            recordingOnSceneUnloaded = true;
            StopRecording();
        }
    }

    public static int GetInstances()
    {
        return 1;
    }

    public static RecordingManager GetInstance()
    {
        return instance;
    }

    private void OnValidate()
    {
        foreach (TrackedObject to in trackedObjects)
        {
            if (to.obj != null)
                to.objPath = Utils.GetGameObjectPath(to.obj);
        }
    }

    public List<TrackedObject> GetTrackedObjects()
    {
        return trackedObjects;
    }

    /*
     * Computes the list of actions to track the tracked objects, used at each frame to know what to log
     */
    public static List<TrackingAction> ComputeActions(List<TrackedObject> trackedObjects, bool recording = true)
    {
        List<TrackingAction> actions = new List<TrackingAction>();

        foreach (TrackedObject to in trackedObjects)
        {
            if (to.obj == null)
            {
                to.obj = GameObject.Find(to.objPath);

                if (to.obj == null && recording)
                    Debug.Log("Couldn't find object " + to.objPath + ", you can hot-plug it during the recording.");
            }

            if (to.trackPosition || to.trackRotation)
            {
                TrackingAction action = new TrackingAction();
                action.trackedObj = to;
                action.targetObj = to.obj;

                if (to.trackPosition && to.trackRotation)
                    action.type = ActionType.POS_AND_ROT;
                else if (to.trackPosition)
                    action.type = ActionType.POSITION;
                else
                    action.type = ActionType.ROTATION;

                actions.Add(action);
            }

            if (to.trackCamera)
            {
                TrackingAction action = new TrackingAction();
                action.trackedObj = to;
                action.targetObj = to.obj;
                action.type = ActionType.CAMERA;
                actions.Add(action);
            }
        }

        return actions;
    }

    private void TrackInteractableGameObjects()
    {
        //DONE : Convert from SteamVR Interactable to XRInteractable
        foreach (XRBaseInteractable interactable in GameObject.FindObjectsOfType<XRBaseInteractable>(true))
        {
            TrackedObject to = new TrackedObject();
            trackedObjects.Add(to);

            if (interactable.gameObject.activeInHierarchy)
                to.obj = interactable.gameObject;
            to.objPath = Utils.GetGameObjectPath(interactable.gameObject);
            to.trackPosition = true;
            to.trackRotation = true;
            to.trackCamera = false;
            to.trackingRate = interactablesTrackingRateHz;
        }
    }

    public void NewScene(string newSceneName)
    {
        if (nbOriginalTrackedObjects != trackedObjects.Count)
        {
            trackedObjects.RemoveRange(nbOriginalTrackedObjects, trackedObjects.Count - nbOriginalTrackedObjects);
        }

        if (trackXRInteractables)
            TrackInteractableGameObjects();
        ComputeTrackingIntervals();
    }

    private void ComputeTrackingIntervals()
    {
        foreach (TrackedObject to in trackedObjects)
            to.trackingInterval = 1.0f / to.trackingRate;
    }

    /*
     * Creates files to write the recording to
     */
    private void CreateDataFiles()   
    {
        string filePath = GetSavePath("objectsData");
        objectsWriter = new CSVWriter(filePath);
        Debug.Log("Started recording objects data to " + filePath);

        filePath = GetSavePath("eventsData");
        eventsWriter = new CSVWriter(filePath);
        Debug.Log("Started recording events data to " + filePath);
    }

    public void ToggleRecording(float minRecordTime = 0.0f)
    {
        if (!recording)
            StartRecording();
        else if (timeSinceStartOfRecording >= minRecordTime)
            StopRecording();
    }

    private void StartRecording()
    {
        Debug.Log("Session folder = " + config.GetSessionFolder());
        if (!Utils.CreateDirectoryIfNotExists(config.GetSessionFolder()))
        {
            Debug.LogError("Can't create directory to save recordings, aborting recording");
            return;
        }

        metadata.Add(new RecordingMetadata());
        recording = true;
        actions = ComputeActions(trackedObjects);

        events = new List<string>();
        eventToId = new Dictionary<string, int>();
        
        trajectoryIndex = trajectoryManager.NewTrajectory();
        ResetTimers();

        if (!ExportFormat())
        {
            StopRecording();
            return;
        }

        CreateDataFiles();
        WriteHeaders();

        controlPoints = new List<Vector3>();

        // First Frame
        WriteCameraParameters();
    }

    private void OnDisable()
    {
        if (recording)
            StopRecording();

        if (metadataWriter != null)
            metadataWriter.Close();
    }

    private string ExportReplayScripts(TrackedObject to)
    {
        if (to.replayScripts == null)
            return "";

        return String.Join(config.replayScriptsSeparator.ToString(), to.replayScripts);
    }

    private string ExportReplayGameObject(TrackedObject to)
    {
        if (to.replayGameObject == null)
            return "";

        return Utils.GetGameObjectPath(to.replayGameObject);
    }

    private bool ExportFormat()
    {
        /*
         * Exporting Objects Format
         */
        string objectsFormatSavePath = GetSavePath("objectsFormat");

        if (File.Exists(objectsFormatSavePath))
        {
            Debug.LogError("A format file named " + objectsFormatSavePath + " would be overwritten by recording, aborting recording.");
            return false;
        }

        objectsFormatWriter = new CSVWriter(objectsFormatSavePath);
        Debug.Log("Started recording objects format to " + objectsFormatSavePath);
        objectsFormatWriter.WriteLine("type", "trackedData", "position", "rotation", "camera", "trackingRate", "replayGameObject", "replayScripts");

        foreach (TrackedObject to in trackedObjects)
            objectsFormatWriter.WriteLine("object", to.objPath, to.trackPosition.ToString(), to.trackRotation.ToString(), to.trackCamera.ToString(),
                                                    to.trackingRate.ToString(), ExportReplayGameObject(to), ExportReplayScripts(to));

        /*
         * Exporting Events Format
         */
        string eventsFormatSavePath = GetSavePath("eventsFormat");
        eventsFormatWriter = new CSVWriter(eventsFormatSavePath);
        Debug.Log("Started recording events format to " + eventsFormatSavePath);
        eventsFormatWriter.WriteLine("id", "event");

        return true;
    }

    private void StopRecording()
    {
        Debug.Log("Stopping Recording");
        recording = false;
        objectsWriter.Close();
        eventsWriter.Close();
        objectsFormatWriter.Close();
        eventsFormatWriter.Close();
        WriteMetadata();
        
        Debug.Log("Recording Stopped and all data written to disk.");
    }

    private void ResetTimers()
    {
        timeSinceStartOfRecording = 0;
        timeSinceLastControlPoint = trajectoryManager.minTimeInterval;

        foreach (TrackedObject to in trackedObjects)
            to.timeSinceLastWrite = to.trackingInterval;
    }

    private void Update()
    {
        timeSinceStartOfRecording += Time.deltaTime;

        if (!recording)
            return;
        
        // Here we are recording
        UpdateTimers();
        WriteTrackedObjects();
    }

    public int GetFilesSize()
    {
        int size = 0;

        if (objectsWriter != null)
            size += objectsWriter.GetSizeOfFile();

        if (eventsWriter != null)
            size += eventsWriter.GetSizeOfFile();

        return size;
    }

    private void WriteTrackedObjects()
    {
        int i = 0;
        foreach (TrackingAction action in actions)
        {
            TrackedObject to = action.trackedObj;

            if (action.targetObj == null)
            {
                to.obj = GameObject.Find(to.objPath);

                if (to.obj != null)
                {
                    Debug.Log(to.objPath + " was found, now tracking it.");
                    action.targetObj = to.obj;
                } else
                {
                    i++;
                    continue;
                }
            }

            if (showControlPointsWhileRecording && i == 0)
            {
                if (timeSinceLastControlPoint >= trajectoryManager.minTimeInterval)
                {
                    if (Vector3.Distance(to.obj.transform.position, lastControlPoint) > trajectoryManager.minDistanceInterval)
                        NewControlPoint(to.obj.transform.position);
                }
            }

            if (to.timeSinceLastWrite >= to.trackingInterval)
            {
                to.timeSinceLastWrite = 0;

                if (action.type == ActionType.POS_AND_ROT)
                {
                    if (!to.obj.transform.position.Equals(to.lastPosition) || !to.obj.transform.eulerAngles.Equals(to.lastRotation))
                        WriteObjectsDataEntry(i, to.obj.transform.position, to.obj.transform.rotation);

                    to.lastPosition = to.obj.transform.position;
                    to.lastRotation = to.obj.transform.eulerAngles;
                }

                else if (action.type == ActionType.POSITION)
                {
                    if (!to.obj.transform.position.Equals(to.lastPosition))
                        WriteObjectsDataEntry(i, to.obj.transform.position);

                    to.lastPosition = to.obj.transform.position;
                }

                else if (action.type == ActionType.ROTATION)
                {
                    if (!to.obj.transform.eulerAngles.Equals(to.lastRotation))
                        WriteObjectsDataEntry(i, SpecialChars.EMPTY_VECTOR3, to.obj.transform.rotation);

                    to.lastRotation = to.obj.transform.eulerAngles;
                }
            }

            i++;
        }
    }

    public void LogEvent(string e)
    {
        if (!recording)
            return;

        if (!eventToId.ContainsKey(e))
        {
            eventToId[e] = events.Count;
            eventsFormatWriter.WriteLine(events.Count, e);
            events.Add(e);
        }

        eventsWriter.WriteLine(timeSinceStartOfRecording, Utils.GetTicksSinceDateBegin(DateTime.Now), eventToId[e]);
    }

    public void LogSceneLoad(string sceneName)
    {
        LogEvent("SceneLoad" + config.sceneLoadEventSeparator + sceneName);
    }

    private void UpdateTimers()
    {
        timeSinceLastControlPoint += Time.deltaTime;

        foreach (TrackedObject to in trackedObjects)
            to.timeSinceLastWrite += Time.deltaTime;
    }

    private string GetTimestamp()
    {
        return timeSinceStartOfRecording.ToString();
    }

    private void WriteObjectsDataEntry(params dynamic[] args)
    {
        WriteEntry(objectsWriter, args);
    }

    private void WriteEventsDataEntry(params dynamic[] args)
    {
        WriteEntry(eventsWriter, args);
    }

    private void WriteEntry(CSVWriter writer, params dynamic[] args)
    {
        writer.Write(GetTimestamp(), true);
        writer.Write(Utils.GetTicksSinceDateBegin(DateTime.Now).ToString(), true);
        writer.WriteLine(args);
    }

    private void WriteCameraParameters()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].type == ActionType.CAMERA)
            {
                GameObject go = actions[i].targetObj;

                if (go == null)
                    return;

                Camera cam = go.GetComponent<Camera>();

                if (cam != null)
                    WriteObjectsDataEntry(i, cam.fieldOfView, cam.aspect, cam.nearClipPlane, cam.farClipPlane);
            }
        }
    }

    private void NewControlPoint(Vector3 position)
    {
        timeSinceLastControlPoint = 0;
        lastControlPoint = position;
        trajectoryManager.AddControlPoint(trajectoryIndex, position);
    }

    private void WriteHeaders()
    {
        objectsWriter.WriteLine("timestamp", "ticksSince1970 (100ns)", "actionId", "position.x", "position.y", "position.z", "rotation.x", "rotation.y", "rotation.z");
        eventsWriter.WriteLine("timestamp", "ticksSince1970 (100ns)", "eventId");

        if (metadataWriter == null)
        {
            metadataWriter = new CSVWriter(GetSavePath("metadata"));
            metadataWriter.WriteLine("timestamp", "ticksSince1970 (100ns)", "scene", "duration", "nbTrackedObjects", "nbEvents", "trackingScripts");
        }
    }

    private void WriteMetadata()
    {
        RecordingMetadata rm = metadata[metadata.Count - 1];
        TimeSpan duration = DateTime.Now - rm.GetDateTime();

        metadataWriter.WriteLine(rm.GetDate(), rm.GetTicks(), rm.GetSceneName(), duration.ToString(), trackedObjects.Count, events.Count, "");
    }

    public string GetSavePath(string type, string extension = "csv")
    {
        RecordingMetadata rm = metadata[metadata.Count - 1];
        string filename = rm.GetDate();
        filename += config.filenameFieldsSeparator + type;
        filename += config.GetFileSuffix();
        filename += '.' + extension;
        string fullPath = Path.Combine(config.GetSessionFolder(), filename);

        return fullPath;
    }
}
