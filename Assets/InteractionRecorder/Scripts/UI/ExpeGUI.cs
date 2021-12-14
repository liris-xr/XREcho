using System;
using UnityEngine;

//------------------UNCOMMENT IF YOU NEED EYETRACKING FOR HTC VIVE PRO EYE--------------------
//using ViveSR;
//using ViveSR.anipal.Eye;

public class ExpeGUI : MonoBehaviour
{
    public bool dontDestroyOnLoad = false;

    public bool showReplayButtons = true;
    private bool showGUI = true;

    private RecordingManager recordingManager;
    private ReplayManager replayManager;
    private TrajectoryManager trajectoryManager;
    private ExpeRecorderConfig config;

    private TrajectoryHeatmap currentTrajectoryHeatmap;
    private TrajectoryHeatmap sceneTrajectoryHeatmap;

    public float guiLeftOffset = 20;
    public float guiUpOffset = 100;
    public float guiWidth = 300;
    private float guiHeight;
    public bool showBox = false;

    private string expeRecorderPath;
    private string project;
    private string session;

    [Header("Trajectory Heatmap")]
    public float resolution = 1f;

    [Header("Screenshot Parameters")]
    public CameraCapture cameraCapture;

    private void Awake()
    {
        if(dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        recordingManager = RecordingManager.GetInstance();
        replayManager = ReplayManager.GetInstance();
        trajectoryManager = TrajectoryManager.GetInstance();
        config = ExpeRecorderConfig.GetInstance();
        
        OnValidate();

        project = config.project;
        session = config.session;
    }

    private void OnValidate()
    {
        guiHeight = Screen.height - guiUpOffset;
    }

    private void Update()
    {
        //TODO : Convert to Action Based
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    showGUI = !showGUI;
        //}
    }

    private void OnGUI()
    {
        if (!showGUI) return;
        
        // Getting system state
        bool recording = recordingManager.IsRecording();
        int nbRecordings = replayManager.GetNbRecording();
        bool replaying = replayManager.IsReplaying();
        bool paused = replayManager.IsPaused();

        GUI.BeginGroup(new Rect(guiLeftOffset, guiUpOffset, guiWidth, guiHeight));

        if (showBox)
            GUI.Box(new Rect(0, 0, guiWidth, guiHeight), GUIContent.none);

        string newExpeRecorderPath = expeRecorderPath,
               newProject = project,
               newSession = session;
       
        newProject = GUILayout.TextField(newProject);
        newSession = GUILayout.TextField(newSession);

        if (!newProject.Equals(project))
        {
            if (!recording && !replaying)
            {
                project = newProject;
                config.NewProject(project);
            }
            else
                Debug.LogError("Can't change project name while recording or replaying");
        }

        if (!newSession.Equals(session))
        {
            if (!recording && !replaying)
            {
                session = newSession;
                config.NewSession(session);
            }
            else
                Debug.LogError("Can't change session name while recording or replaying");
        }


        string recordButtonText = (!recording ? "Start Recording" : "Stop Recording");

        if (GUILayout.Button(recordButtonText))
        {
            recordingManager.ToggleRecording();
        }

        string sceneTrajectoryHeatmapText = (sceneTrajectoryHeatmap == null) ? "Compute Scene THM" : "Toggle Scene THM";

        if (GUILayout.Button(sceneTrajectoryHeatmapText))
        {
            if (sceneTrajectoryHeatmap == null)
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                sceneTrajectoryHeatmap = new TrajectoryHeatmap(resolution);
                sceneTrajectoryHeatmap.ComputeSceneHeatmap(0);
                
                sw.Stop();
                Debug.Log("Heatmap Computation Time: " + sw.Elapsed);
            }
            else
                sceneTrajectoryHeatmap.ToggleHeatmap();
        }

        if (showReplayButtons)
        {
            if (GUILayout.Button("Read recordings: " + nbRecordings + " found"))
            {
                if (!replaying && !recording)
                    replayManager.ReadRecordings();
                else
                    Debug.LogError("Can't read recordings while recording or replaying");
            }
            if (nbRecordings > 0)
            {
                string currentTrajectoryHeatmapText = (currentTrajectoryHeatmap == null) ? "Compute Current THM" : "Toggle Current THM";

                if (GUILayout.Button(currentTrajectoryHeatmapText))
                {
                    if (currentTrajectoryHeatmap == null)
                    {
                        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                        currentTrajectoryHeatmap = new TrajectoryHeatmap(resolution);
                        currentTrajectoryHeatmap.ComputeCurrentHeatmap(0);
                        
                        sw.Stop();
                        Debug.Log("Heatmap Computation Time: " + sw.Elapsed);
                    }
                    else
                        currentTrajectoryHeatmap.ToggleHeatmap();
                }

                string replayButtonText = "";
                if (!replaying)
                    replayButtonText = "Start Replaying";
                else
                    replayButtonText = (!paused ? "Pause Replaying" : "Resume Replaying");

                if (GUILayout.Button(replayButtonText))
                {
                    if (!replaying)
                        replayManager.StartReplaying();
                    else
                        replayManager.TogglePause();
                }

                if (replaying)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("X1"))
                        replayManager.FastForward(1);
                    if (GUILayout.Button("X2"))
                        replayManager.FastForward(2);
                    if (GUILayout.Button("X8"))
                        replayManager.FastForward(8);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+5s"))
                        replayManager.JumpForwardBy(5);
                    if (GUILayout.Button("+30s"))
                        replayManager.JumpForwardBy(30);
                    if (GUILayout.Button("+2min"))
                        replayManager.JumpForwardBy(120);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Change Camera"))
                        replayManager.ChangeCamera();

                    if (GUILayout.Button("Stop Replaying"))
                        replayManager.StopReplaying();
                }

                if (GUILayout.Button("Toggle Trajectories"))
                    trajectoryManager.ToggleTrajectories();

                if (GUILayout.Button("Toggle Control Points"))
                    trajectoryManager.ToggleControlPoints();
            }

            if (GUILayout.Button("Toggle Gaze Visualization"))
                foreach (MonoBehaviour showViz in transform.root.GetComponentsInChildren<ShowGaze>(true))
                    showViz.enabled = !showViz.enabled;
        }

        /*
        if (GUILayout.Button("Set Parameter"))
        {
            EyeParameter parameter = new EyeParameter
            {
                gaze_ray_parameter = new GazeRayParameter(),
            };
            Error error = SRanipal_Eye_API.GetEyeParameter(ref parameter);
            Debug.Log("GetEyeParameter: " + error + "\n" +
                      "sensitive_factor: " + parameter.gaze_ray_parameter.sensitive_factor);

            parameter.gaze_ray_parameter.sensitive_factor = parameter.gaze_ray_parameter.sensitive_factor == 1 ? 0.015f : 1;
            error = SRanipal_Eye_API.SetEyeParameter(parameter);
            Debug.Log("SetEyeParameter: " + error + "\n" +
                      "sensitive_factor: " + parameter.gaze_ray_parameter.sensitive_factor);
        }*/
        
        //------------------UNCOMMENT IF YOU NEED EYETRACKING FOR HTC VIVE PRO EYE--------------------
        //if (GUILayout.Button("Launch Calibration"))
        //{
        //    SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
        //}

        //TODO : Manual Configuration of Screenshot path
        if (GUILayout.Button("Take Screenshot"))
        {
            cameraCapture.Capture("visitors/" + session, "topviewscreenshot" + DateTime.Now.ToString(config.dateFormat));
        }

        if (GUILayout.Button("Quit Application"))
            Application.Quit();

        GUI.EndGroup();
    }
}
