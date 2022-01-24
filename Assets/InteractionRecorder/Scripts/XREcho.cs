using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ExpeRecorderConfig))]
public class XREcho : MonoBehaviour
{
    private static XREcho instance;

    public enum AutoMode
    {
        DoNothing,
        AutoStartRecord,
        AutoStartReplay
    }
    
    public AutoMode autoExecution = AutoMode.DoNothing;
    public bool displayGUI = true;
    public Camera monitoringCamera;
    public bool dontDestroyOnLoad = false;
    public float trajectoryHeatmapResolution = 1.0f;

    public static XREcho GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance)
            Debug.LogError("2 Recording Managers: singleton design pattern broken");

        instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

}