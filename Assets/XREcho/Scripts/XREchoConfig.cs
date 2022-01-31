using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

public class XREchoConfig : MonoBehaviour
{
    private static XREchoConfig instance;

    // Culture related classes and variables
    public enum Culture
    {
        AmericanEnglish,
        France
    }

    // References to other managers
    private RecordingManager recordingManager;
    private ReplayManager replayManager;

    // Scene related variables
    private string currentScene;
    public string GetCurrentScene() { return currentScene; }

    public Culture culture;
    private CultureInfo cultureInfo;

    // Filepaths related variables
    [Header("Paths and Names")]
    public string expeRecorderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "XREcho");
    public string GetExpeRecorderPath() { return expeRecorderPath; }
    private string recordingsFolder;
    public string GetRecordingsFolder() { return recordingsFolder; }

    public string project = "project";
    private string projectFolder;

    public string session = "session";
    private string sessionFolder;
    public string GetSessionFolder() { return sessionFolder; }

    [Space(8)]
    public bool AutoLoadLastVisitorSession = false;
    public string visitorFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "XREcho", "visitors_metadata.csv");
    private string configFile = "./XREcho_config.csv";

    [Header("Separators and formats")]
    public char filenameFieldsSeparator = '_';
    public char replayScriptsSeparator = '|';
    public char sceneLoadEventSeparator = ':';
    public DateTime dateBegin = new DateTime(1970, 1, 1);
    public string dateFormat = "yy-MM-dd-HH-mm-ss";


    private void Awake()
    {
        if (instance)
            Debug.LogError("2 XREchoConfigs: singleton design pattern broken");

        instance = this;
        ApplyCulture();

        currentScene = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        ImportConfig();
        ComputePaths();
    }

    private void Start()
    {
        recordingManager = RecordingManager.GetInstance();
        replayManager = ReplayManager.GetInstance();

        if(AutoLoadLastVisitorSession) FetchVisitorId();

        NewSession(session);
    }

    public static XREchoConfig GetInstance()
    {
        return instance;
    }

    public string GetFileSuffix()
    {
        return filenameFieldsSeparator + currentScene;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
    }

    public void NewPath(string newPath)
    {
        Debug.Log("new path " + newPath);
        expeRecorderPath = newPath;
        ComputePaths();
        recordingManager.NewSession();
    }

    public void NewProject(string newProject)
    {
        project = newProject;
        ComputePaths();
        recordingManager.NewSession();
    }

    public void NewSession(string newSession)
    {
        session = newSession;
        ComputePaths();
        recordingManager.NewSession();
    }

    private void ApplyCulture()
    {
        switch (culture)
        {
            case Culture.AmericanEnglish:
                cultureInfo = new CultureInfo("en-US");
                break;

            case Culture.France:
                cultureInfo = new CultureInfo("fr-FR");
                break;
        }

        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private void ComputePaths()
    {
        recordingsFolder = Path.Combine(expeRecorderPath, "Recordings");
        projectFolder = Path.Combine(recordingsFolder, project);
        sessionFolder = Path.Combine(projectFolder, session);
    }

    public void OnValidate()
    {
        if (project.Equals(""))
        {
            Debug.LogError("Project name can't be empty");
            project = "project";
        }

        if (session.Equals(""))
        {
            Debug.LogError("Session name can't be empty");
            session = "session";
        }

        ComputePaths();
        ApplyCulture();

#if UNITY_EDITOR
        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null) {
            ExportConfig();
        }
#endif
    }

    private void ImportConfig()
    {
        Debug.Log("Importing config from " + configFile);

        if (!File.Exists(configFile))
        {
            Debug.Log("Creating config file: " + configFile);
            ExportConfig();
            return;
        }

        Dictionary<string, object> config = CSVReader.ReadCSV(configFile)[0];
        expeRecorderPath = (string)config["expeRecorderPath"];
        project = (string)config["lastProjectName"];
        session = (string)config["lastSessionName"];
    }

    private void ExportConfig()
    {
        CSVWriter configWriter = new CSVWriter(configFile);
        configWriter.WriteLine("expeRecorderPath", "lastProjectName", "lastSessionName");
        configWriter.WriteLine(expeRecorderPath.Replace(@"\","/"), project, session);
        configWriter.Close();
    }

    private void FetchVisitorId()
    {
        Debug.Log("Fetching Visitor ID from " + visitorFilePath);

        if (!File.Exists(visitorFilePath))
        {
            Debug.LogError("Visitor file not found");
            return;
        }

        List<Dictionary<string, object>> visitorList = CSVReader.ReadCSV(visitorFilePath);
        Dictionary<string, object> currentVisitor = visitorList[visitorList.Count - 1];

        var currentVisitorID = currentVisitor["ID Visitor"];
        session = "visitor" + currentVisitorID.ToString();
    }

    private void OnDestroy()
    {
        ExportConfig();
    }
}
