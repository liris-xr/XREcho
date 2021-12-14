using UnityEngine;

/*
 * Code By bboysil on the Unity Forum: https://answers.unity.com/questions/125049/is-there-any-way-to-view-the-console-in-a-build.html
 */

public class ConsoleToGUI : MonoBehaviour
{
    private string myLog = "*begin log";
    private string filename = "";
    private bool doShow = false;
    private int kChars = 700;

    private ExpeRecorderConfig config;

    private void OnEnable() { Application.logMessageReceived += Log; }
    private void OnDisable() { Application.logMessageReceived -= Log; }
    private void Update() { 
        //TODO:Convert to Action Based
        //if (Input.GetKeyDown(KeyCode.Space)) 
        //{ 
        //    doShow = !doShow; 
        //} 
    }

    private void Awake()
    {
        config = ExpeRecorderConfig.GetInstance();

        string d = System.IO.Path.Combine(config.GetExpeRecorderPath(), "Logs");
        System.IO.Directory.CreateDirectory(d);
        string r = Random.Range(1000, 9999).ToString();
        filename = d + "/log-" + r + ".txt";
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        /* You can filter type of logs to let through
        if (type != LogType.Log)
            return;
        */

        // for onscreen...
        myLog = myLog + "\n" + logString;
        if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }

        // for the file...
        try { System.IO.File.AppendAllText(filename, logString + "\n"); }
        catch { }
    }

    private void OnGUI()
    {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
    }
}