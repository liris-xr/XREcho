using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GUIStylesManager))]
[RequireComponent(typeof(LogToGUI))]
public class GUIManager : MonoBehaviour
{
    private enum GUITab
    {
        TabRecord,
        TabReplay,
        TabAnalyze
    }
    
    [Header("GUI position")]
    public float leftOffset = 20;
    public float upOffset = 20;

    // managers
    private XREcho xrEcho;
    private XREchoConfig config;
    private GUIStylesManager stylesManager;
    private LogToGUI logger;
    private RecordingManager recordingManager;
    private ReplayManager replayManager;
    
    // general variables
    private bool recording;
    private bool replaying;
    private bool paused;
    private bool loading;
    private float guiWidth = 270;
    private float guiHeight;
    private string project;
    private string session;

    // Toolbar variables
    private GUITab currentTab;
    private string[] toolbarStrings = { "Record", "Replay", "Analyze" };
    private bool currentTabHasChanged;
    private bool currentCameraIsTopView;
    private bool shouldEndLoading;
    private bool autoRead;
    private float currentBlinkingAlpha;
    private float blinkingAlphaStep = 0.01f;
    private float currentTurningAngle;
    private float turningAngleStep = 3;

    // Replay variables
    private bool displayCameraView;
    private bool displayGazeVisu;
    private bool needReplayListUpdate;
    private int nextFFMode;
    private float readerSliderValue;
    private string replayProject;
    private string replaySession;
    private string replayRecord;
    private CustomGUIDropdown replayProjectDropdown;
    private CustomGUIDropdown replaySessionDropdown;
    private CustomGUIDropdown replayRecordDropdown;

    // Analyze variables
    private bool displaySceneTHM;
    
    private GUIHeatmap _guiHeatmap;
    private GUITrajectory _guiTrajectory;

    private void Start()
    {
        xrEcho = XREcho.GetInstance();
        config = XREchoConfig.GetInstance();
        stylesManager = GetComponent<GUIStylesManager>();
        recordingManager = RecordingManager.GetInstance();
        replayManager = ReplayManager.GetInstance();
        // _trajectoryManagerOld = TrajectoryManagerOld.GetInstance();

        _guiHeatmap = new GUIHeatmap(PositionHeatmapManager.GetInstance(), stylesManager);
        _guiTrajectory = new GUITrajectory(TrajectoryManager.GetInstance(), stylesManager);
        
        OnValidate();
        project = config.project;
        session = config.session;

        currentTab = GUITab.TabRecord;
        currentTabHasChanged = true;
        currentCameraIsTopView = false;
        shouldEndLoading = false;
        autoRead = false;
        currentBlinkingAlpha = 1;
        currentTurningAngle = 0;
        
        displayCameraView = false;
        displayGazeVisu = recordingManager.recordEyeTracking;
        needReplayListUpdate = false;
        nextFFMode = 0;
        replayProject = "";
        replaySession = "";
        replayRecord = "";

        displaySceneTHM = false;

        if (xrEcho.autoExecution == XREcho.AutoMode.AutoStartReplay)
        {
            autoRead = true;
            currentTab = GUITab.TabReplay;
        }
    }
    
    private void OnValidate()
    {
        guiHeight = Screen.height - upOffset;
        logger = GetComponent<LogToGUI>();
        logger.guiRect = new Rect(leftOffset, upOffset + 5, guiWidth, guiHeight - 10);
    }

    private void Update()
    {
        if (shouldEndLoading)
        {
            shouldEndLoading = false;
            string root = Path.Combine(config.GetRecordingsFolder(), replayProject);
            root = Path.Combine(root, replaySession);
            int recId = replayRecordDropdown.SelectedId;
            replayManager.ReadRecording(root, recId);
            ActivateGazeVisu();
            if (autoRead)
            {
                autoRead = false;
                replayManager.StartReplaying();
            }
            loading = false;
            
            _guiTrajectory.OnNewRecordLoaded();
            _guiHeatmap.OnNewRecordLoaded();
        }
    }
    

    // -----------------------------------------------------------------------
    //                        OnGUI methods
    // -----------------------------------------------------------------------
    private void OnGUI()
    {
        if (!xrEcho.displayGUI) return;

        GUI.depth = 0;

        // Getting system state
        recording = recordingManager.IsRecording();
        if (replayManager != null)
        {
            replaying = replayManager.IsReplaying();
            paused = replayManager.IsPaused();
        }
        
        GUILayout.BeginArea(new Rect(leftOffset, upOffset, guiWidth, guiHeight));
        GUILayout.BeginVertical(stylesManager.boxStyle);

        MakeCustomToolbar();
        GUILayout.BeginVertical(stylesManager.tabStyle);
        if (currentTab == GUITab.TabRecord)
        {
            RecordGUI();
        } else if (replayManager == null) {
            GUILayout.Label("All replay and analyze features have been disabled due to the lack of Replay Manager.\nCheck the XR Echo script settings to activate it.", stylesManager.missingLabelStyle);
        } else if (currentTab == GUITab.TabReplay)
        {
            ReplayGUI();
        } else if (currentTab == GUITab.TabAnalyze)
        {
            AnalyzeGUI();
        }
        currentTabHasChanged = false;
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        if (recording)
        {
            DisplayToolbarIcon(stylesManager.recSprite.texture, 0, true);
        }
        if (replaying)
        {
            DisplayToolbarIcon(!paused ? stylesManager.playSprite.texture : stylesManager.pauseSprite.texture, 1, true);
        }
        if (loading)
        {
            DisplayLoadingIcon();
        }

        if (GUILayout.Button("Quit Application", IsADropdownDisplayed() ? stylesManager.noHoverStyle : "button"))
            Application.Quit();
        if (Event.current.type == EventType.Repaint)
        {
            Rect quitrect = GUILayoutUtility.GetLastRect();
            float y = quitrect.y + quitrect.height;
            logger.guiRect = new Rect(leftOffset, upOffset + y + 5, guiWidth, guiHeight - y - 10);
        }

        if (replayManager == null)
        {
            GUILayout.EndArea();
            return;
        }

        if (currentTab == GUITab.TabReplay)
        {
            replayProjectDropdown.DropdownOnGUI();
            replaySessionDropdown.DropdownOnGUI();
            replayRecordDropdown.DropdownOnGUI();
        }

        GUILayout.EndArea();

        if (Event.current.type == EventType.MouseDown)
        {
            CloseOtherDropdowns(null);
        }
    }

    private void RecordGUI()
    {
        string newProject = project,
               newSession = session;

        if (currentTabHasChanged)
        {
            SetCurrentCamera(false);
            _guiTrajectory.ShowTrajectory(false);
        }

        newProject = stylesManager.LabeledTextField(" Project:  ", newProject);
        newSession = stylesManager.LabeledTextField("Session:  ", newSession);

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


        GUILayout.BeginHorizontal();
        GUILayout.Space(guiWidth / 2 - stylesManager.bigImageButtonStyle.fixedWidth);
        if (GUILayout.Button(!recording ? stylesManager.recSprite.texture : stylesManager.stopSprite.texture, stylesManager.bigImageButtonStyle) || ShortcutIsPressed(recordingManager.recordShortcut))
        {
            if (!recording)
                currentBlinkingAlpha = 1;
            else
                needReplayListUpdate = true;
            recordingManager.ToggleRecording();
        }
        string timeStr = "0:00";
        string sizeStr = "0 Ko";
        if (recording)
        {
            timeStr = Utils.TimeString((int)recordingManager.GetTimeSinceStartOfRecording());
            float recSize = recordingManager.GetFilesSize();
            sizeStr = recSize >= 1000000 ? (recSize / 1000000).ToString("n2") + " Mo" : (recSize / 1000).ToString("n0") + " Ko";
        }
        else
        {
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        }
        string lab = "<size=17>" + timeStr + "</size>\n<size=10>" + sizeStr + "</size>";
        GUILayout.Label(lab, stylesManager.recLabelStyle);
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (recordingManager.recordEyeTracking)
        {
            if (GUILayout.Button("Launch Eye Calibration"))
            {
                Utils.GetClassType("GetEyeTrackingData").GetMethod("LaunchEyeCalibration").Invoke(null, null);
            }
        }
    }

    private void ReplayGUI()
    {
        string root = config.GetRecordingsFolder();
        if (currentTabHasChanged)
        {
            SetCurrentCamera(true);

            replayProjectDropdown = new CustomGUIDropdown(Utils.GetSubfolders(root));
            if (!replayProjectDropdown.SetSelectedEntry(project))
            {
                Debug.LogWarning("No records corresponding to the current project have been found.");
                replayProjectDropdown.SetSelectedEntry(replayProject);
            }
        }
        LabeledDropdown(" Project:  ", replayProjectDropdown);
        string curReplayProject = replayProjectDropdown.GetCurrentEntry();
        root = Path.Combine(root, curReplayProject);

        if (curReplayProject != replayProject)
        {
            replayProject = curReplayProject;
            replaySessionDropdown = new CustomGUIDropdown(Utils.GetSubfolders(root));
            if (!replaySessionDropdown.SetSelectedEntry(session))
            {
                replaySessionDropdown.SetSelectedEntry(replaySession);
            }
        }
        LabeledDropdown("Session:  ", replaySessionDropdown);
        string curReplaySession = replaySessionDropdown.GetCurrentEntry();
        root = Path.Combine(root, curReplaySession);

        if (curReplaySession != replaySession || needReplayListUpdate)
        {
            replaySession = curReplaySession;

            List<string> recordsNames = new List<string>();
            char sep = config.filenameFieldsSeparator;
            foreach (string file in replayManager.GetObjectsDataFiles(root))
                recordsNames.Add(file.Split(sep)[0]);
            replayRecordDropdown = new CustomGUIDropdown(recordsNames);
            replayRecordDropdown.SelectedId = replayRecordDropdown.ListLength - 1;
            needReplayListUpdate = false;
        }
        int recId = LabeledDropdown(" Record:  ", replayRecordDropdown);
        string curReplayRecord = replayRecordDropdown.GetCurrentEntry();

        if (curReplayRecord != replayRecord && curReplayRecord != "--- empty list ---")
        {
            replayRecord = curReplayRecord;
            currentTurningAngle = 0;
            readerSliderValue = 0;

            loading = true;
            shouldEndLoading = false;
            Debug.Log("Loading replay " + curReplayRecord + "...");
            Thread t = new Thread(new ThreadStart(AsyncLoad));
            t.Start();
        }
        ReaderGUI(replayManager.GetNbRecording() > 0 && !loading ? replayManager.GetTotalReplayTime() : -1);


        bool newGazeVisu = GUILayout.Toggle(displayGazeVisu, " Gaze Visualization", stylesManager.toggleStyle);
        if (newGazeVisu != displayGazeVisu)
        {
            displayGazeVisu = newGazeVisu;
            ActivateGazeVisu();
        }
    }

    private void ReaderGUI(float recordDuration)
    {
        int[] fastforwardModes = {1, 2, 8};

        bool isEmpty = recordDuration < 0.1f;
        float duration = Math.Max(recordDuration, 0.1f);

        GUILayout.BeginVertical(stylesManager.uppedMargedBoxStyle);

        if (isEmpty || !replaying) GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        if (replaying && !paused) readerSliderValue = Math.Min(replayManager.GetCurrentReplayTime(), duration);
        float newSliderValue = GUILayout.HorizontalSlider(readerSliderValue, 0, duration);
        if (replaying && newSliderValue != readerSliderValue)
        {
            replayManager.JumpForwardBy(newSliderValue - readerSliderValue);
            readerSliderValue = newSliderValue;
        }

        if (!isEmpty) GUI.color = Color.white;

        GUILayout.BeginHorizontal();

        if ((GUILayout.Button(
                 !replaying || paused ? stylesManager.playSprite.texture : stylesManager.pauseSprite.texture,
                 isEmpty ? stylesManager.blockedImageButtonStyle : stylesManager.imageButtonStyle) ||
             ShortcutIsPressed(replayManager.playPauseShortcut)) && !isEmpty)
        {
            if (!replaying || paused) currentBlinkingAlpha = 1;
            if (!replaying)
            {
                replayManager.StartReplaying();
                replayManager.FastForward(fastforwardModes[nextFFMode]);
                if (displayCameraView)
                {
                    replayManager.ChangeCamera();
                }
            }
            else
            {
                if (paused && readerSliderValue >= duration - 0.1f)
                {
                    replayManager.JumpForwardBy(-duration);
                }

                replayManager.TogglePause();
            }
        }

        if (!replaying) GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        if ((GUILayout.Button(stylesManager.stopSprite.texture,
                 replaying ? stylesManager.imageButtonStyle : stylesManager.blockedImageButtonStyle) ||
             ShortcutIsPressed(replayManager.stopShortcut)) && replaying)
        {
            if (displayCameraView)
            {
                replayManager.ChangeCamera();
            }

            replayManager.StopReplaying();
            readerSliderValue = 0;
        }

        if (!isEmpty) GUI.color = Color.white;

        string lab = isEmpty
            ? "0:00 / 0:00"
            : Utils.TimeString((int) readerSliderValue) + " / " + Utils.TimeString((int) Math.Round(duration));
        GUILayout.Label(lab, stylesManager.bigLabelStyle);
        GUILayout.FlexibleSpace();

        GUI.color = Color.white;

        if (GUILayout.Button(new GUIContent("" + fastforwardModes[nextFFMode], stylesManager.forwardSprite.texture),
                stylesManager.imageButtonStyle))
        {
            nextFFMode = (nextFFMode + 1) % fastforwardModes.Length;
            replayManager.FastForward(fastforwardModes[nextFFMode]);
        }

        if (GUILayout.Button(
                !displayCameraView ? stylesManager.topviewSprite.texture : stylesManager.cameraSprite.texture,
                stylesManager.imageButtonStyle))
        {
            displayCameraView = !displayCameraView;
            if (replaying)
            {
                replayManager.ChangeCamera();
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void AnalyzeGUI()
    {
        if (currentTabHasChanged)
        {
            SetCurrentCamera(true);
        }

        // Display trajectory GUI section (Cf. GUITrajectory)
        _guiTrajectory.OnGui();
        
        // Display heatmap GUI section (Cf. GUIHeatmap)
        _guiHeatmap.OnGui();

        if (GUILayout.Button(new GUIContent(" Take Screenshot", stylesManager.screenshotSprite.texture), stylesManager.screenshotButtonStyle))
        {
            string screenshotpath = Path.Combine(config.GetExpeRecorderPath(), "Screenshots");
            screenshotpath = Path.Combine(screenshotpath, project);
            screenshotpath = Path.Combine(screenshotpath, session);
            Utils.CreateDirectoryIfNotExists(screenshotpath);
            Utils.TakeScreenshot(xrEcho.monitoringCamera, screenshotpath, "topviewscreenshot" + DateTime.Now.ToString(config.dateFormat));
        }
    }



    // -----------------------------------------------------------------------
    //                        private methods
    // -----------------------------------------------------------------------
    private void SetCurrentCamera(bool topView)
    {
        if (currentCameraIsTopView == topView) return;
        currentCameraIsTopView = topView;

        Camera top = xrEcho.monitoringCamera;
        if (top == null)
            top = Camera.main;
        if (replayManager != null)
        {
            replayManager.SetMainCamera(top);
        }

        int tmp;
        tmp = Camera.main.targetDisplay;
        Camera.main.targetDisplay = top.targetDisplay;
        top.targetDisplay = tmp;
    }

    private bool ShortcutIsPressed(KeyCode shortcut)
    {
        return Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == shortcut;
    }

    private void ActivateGazeVisu()
    {
        foreach (MonoBehaviour showViz in transform.root.GetComponentsInChildren<ShowGaze>(true))
            if (showViz.gameObject.name != "XREchoEyeTracker")
                showViz.enabled = displayGazeVisu;
    }

    private void AsyncLoad()
    {
        string root = Path.Combine(config.GetRecordingsFolder(), replayProject);
        root = Path.Combine(root, replaySession);
        int recId = replayRecordDropdown.SelectedId;
        replayManager.ReadObjectsDataFile(root, recId);
        shouldEndLoading = true;
    }

    //                        toolbar related methods
    // -----------------------------------------------------------------------
    private void MakeCustomToolbar()
    {
        GUILayout.BeginHorizontal();

        GUITab newTab = currentTab;
        for (int i = 0; i < toolbarStrings.Length; i++)
        {
            GUIStyle curStyle = recording || (replaying && i == 0) || loading ? stylesManager.tabButtonsStyleBlocked : stylesManager.tabButtonsStyle;
            if (i == (int)currentTab)
            {
                curStyle = stylesManager.tabButtonsStyleSelected;
            }
            if (GUILayout.Button(toolbarStrings[i], curStyle))
            {
                newTab = (GUITab)i;
            }
        }
        if (!recording && !(replaying && newTab==GUITab.TabRecord) && !loading && currentTab != newTab)
        {
            currentTab = newTab;
            currentTabHasChanged = true;
        }

        GUILayout.EndHorizontal();
    }

    private void DisplayToolbarIcon(Texture icon, int tabId, bool blink = false, bool turnAround = false)
    {
        Matrix4x4 matrixBackup = GUI.matrix;
        GUI.color = new Color(1.0f, 1.0f, 1.0f, blink ? GetBlinkingAlpha() : 1.0f);
        int siz = (int)stylesManager.tabButtonsStyle.lineHeight;
        int pad = stylesManager.tabButtonsStyle.padding.top + 1;
        float offset = guiWidth / toolbarStrings.Length;
        int x = pad - tabId + (int)offset * tabId;
        Vector2 pivotPoint = new Vector2(x + siz / 2, pad + siz / 2);
        if (turnAround) GUIUtility.RotateAroundPivot(GetTurningAngle(), pivotPoint);

        GUI.Box(new Rect(x, pad, siz, siz), icon, GUIStyle.none);

        if (turnAround) GUI.matrix = matrixBackup;
        GUI.color = Color.white;
    }

    private void DisplayLoadingIcon()
    {
        DisplayToolbarIcon(stylesManager.loadingSprite.texture, (int)currentTab, false, true);
    }

    private float GetBlinkingAlpha()
    {
        currentBlinkingAlpha += blinkingAlphaStep;
        currentBlinkingAlpha = currentBlinkingAlpha % 2;

        return currentBlinkingAlpha > 1 ? 2.0f - currentBlinkingAlpha : currentBlinkingAlpha;
    }

    private float GetTurningAngle()
    {
        currentTurningAngle += turningAngleStep;
        currentTurningAngle = currentTurningAngle % 360;
        return currentTurningAngle;
    }


    //                        dropdown related methods
    // -----------------------------------------------------------------------
    private void CloseOtherDropdown(CustomGUIDropdown otherDropdown, CustomGUIDropdown openedDropdown = null)
    {
        if (otherDropdown != openedDropdown) otherDropdown.DropdownIsDisplayed = false;
    }

    private void CloseOtherDropdowns(CustomGUIDropdown currentDropdown)
    {
        if (currentTab == GUITab.TabReplay)
        {
            CloseOtherDropdown(replayProjectDropdown, currentDropdown);
            CloseOtherDropdown(replaySessionDropdown, currentDropdown);
            CloseOtherDropdown(replayRecordDropdown, currentDropdown);
        }
    }

    private int LabeledDropdown(String title, CustomGUIDropdown dropdown)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, stylesManager.labelStyle);
        int toReturn = dropdown.OnGUI(!replaying && !loading);
        if (dropdown.DropdownIsDisplayed) CloseOtherDropdowns(dropdown);
        GUILayout.EndHorizontal();
        return toReturn;
    }

    private bool IsADropdownDisplayed()
    {
        if (currentTab == GUITab.TabReplay && replayManager != null)
        {
            return replayProjectDropdown.DropdownIsDisplayed || replaySessionDropdown.DropdownIsDisplayed || replayRecordDropdown.DropdownIsDisplayed;
        }
        return false;
    }
}
