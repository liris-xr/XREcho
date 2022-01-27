using UnityEngine;
using System;
using System.IO;

public class LogToGUI : MonoBehaviour
{
    public bool displayOnScreen = true;
    public bool printToFile = false;
    public string logFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Logs");

    [Header("Log Filters")]
    public bool showLogs = true;
    public bool showWarnings = true;
    public bool showErrors = true;

    private bool needGUIInit;
    private int maxChars = 100000;
    private string logs = "";
    private string filename;
    private Vector2 scrollPosition;
    private GUIStyle textStyle;
    [HideInInspector]
    public Rect guiRect = new Rect(5, 5, 600, 400);

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }
    
    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    
    private void Start()
    {
        if (printToFile)
            System.IO.Directory.CreateDirectory(logFolder);
        filename = Path.Combine(logFolder, "log-" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".txt");

        needGUIInit = true;
    }

    private void OnValidate()
    {
        if (printToFile)
            System.IO.Directory.CreateDirectory(logFolder);
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        string toLog = logString;
        if (type == LogType.Log)
        {
            if (!showLogs)
                return;
        } else if (type == LogType.Warning)
        {
            if (!showWarnings)
                return;
            toLog = "<color=orange>" + toLog + "</color>";
        } else
        {
            if (!showErrors)
                return;
            toLog = "<color=red>" + toLog + "</color>";
        }
        
        logs += toLog + "\n\n";
        if (logs.Length > maxChars)
        {
            logs = logs.Substring(logs.Length - maxChars);
            logs = logs.Substring(logs.IndexOf("\n")+1);
        }
        scrollPosition.y = 100000;

        if (printToFile)
        {
            try { System.IO.File.AppendAllText(filename, logString + "\n"); }
            catch { }
        }
    }

    private void OnGUI()
    {
        if (!displayOnScreen) { return; }
        GUI.depth = 1;

        if (needGUIInit)
        {
            textStyle = new GUIStyle("textArea");
            textStyle.margin = new RectOffset(0, 0, 0, 0);
            int padd = 4;
            textStyle.richText = true;
            textStyle.padding = new RectOffset(padd, padd, padd, padd);
            needGUIInit = false;
        }

        GUILayout.BeginArea(guiRect, GUIStyle.none);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none);
        if (logs != "") GUILayout.TextArea(logs, textStyle);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}