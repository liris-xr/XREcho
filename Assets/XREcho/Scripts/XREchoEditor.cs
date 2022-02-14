using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(XREcho))]
[CanEditMultipleObjects]
public class XREchoEditor : Editor
{
    SerializedProperty autoExecution;
    SerializedProperty displayGUI;
    SerializedProperty monitoringCamera;
    SerializedProperty dontDestroyOnLoad;

    private bool isCheckGroup;
    private bool allIsOk;
    private bool replayIsPossible;
    private bool guiIsPossible;

    void OnEnable()
    {
        autoExecution = serializedObject.FindProperty("autoExecution");
        displayGUI = serializedObject.FindProperty("displayGUI");
        monitoringCamera = serializedObject.FindProperty("monitoringCamera");
        dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");

        isCheckGroup = true;
        allIsOk = false;
        replayIsPossible = false;
        guiIsPossible = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (((MonoBehaviour)target).isActiveAndEnabled && UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            if (allIsOk)
            {
                allIsOk = CheckAllHierarchy();
                if (!allIsOk)
                {
                    isCheckGroup = true;
                }
            }
            else
            {
                isCheckGroup = EditorGUILayout.BeginFoldoutHeaderGroup(isCheckGroup, "Hierarchy check");
                if (isCheckGroup)
                {
                    allIsOk = CheckAllHierarchy();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.PropertyField(autoExecution);

        if (!guiIsPossible)
        {
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            EditorGUILayout.PropertyField(displayGUI);
            displayGUI.boolValue = false;
            GUI.color = Color.white;
        } else
        {
            EditorGUILayout.PropertyField(displayGUI);
        }

        EditorGUILayout.PropertyField(monitoringCamera);
        if (monitoringCamera.objectReferenceValue == null && replayIsPossible)
        {
            if (GUILayout.Button("Add a Monitoring Camera"))
            {
                GameObject master = ((MonoBehaviour)target).gameObject;
                GameObject instance = new GameObject("MonitoringCamera", typeof(Camera));
                instance.transform.parent = master.transform;
                instance.transform.position = new Vector3(0, 5, 0);
                instance.transform.rotation = Quaternion.Euler(90, 0, 0);
                instance.AddComponent(typeof(OrthoCameraManipulator));
                Camera cam = (Camera)instance.GetComponent(typeof(Camera));
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.targetDisplay = 2;
                cam.stereoTargetEye = StereoTargetEyeMask.None;
                monitoringCamera.objectReferenceValue = instance;
            }
            EditorGUILayout.HelpBox("Without a monitoring camera, replays monitoring and screenshots will be irrelevant (an orthographic top view is advised). Click on the button above to automatically add one.", MessageType.Warning);
            EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(dontDestroyOnLoad);

        serializedObject.ApplyModifiedProperties();
    }

    private bool CheckAllHierarchy()
    {
        bool allOK = true;
        allOK &= CheckNeededSingleton(typeof(RecordingManager));
        allOK &= CheckNeededSingleton(typeof(TrajectoryManager));
        allOK &= CheckNeededSingleton(typeof(MaterialManager));
        replayIsPossible = CheckNeededSingleton(typeof(ReplayManager), "All replay and analyze features will be disabled", false);
        //if (CheckNeededSingleton(typeof(ReplayManager), "All replay and analyze features will be disabled", false))
        //{
        //    replayIsPossible = CheckNeededSingleton(typeof(GameObjectsManager), "Replay features cannot work");
        //} else
        //{
        //    replayIsPossible = false;
        //}
        allOK &= replayIsPossible;
        bool newGuiIsPossible = CheckNeededSingleton(typeof(GUIManager), "The runtime GUI will not be available", false);
        if (newGuiIsPossible && !guiIsPossible)
        {
            displayGUI.boolValue = true;
        }
        guiIsPossible = newGuiIsPossible;
        allOK &= guiIsPossible;
        return allOK;
    }

    private bool CheckNeededSingleton(Type t, string messagePrefix="", bool isError=true)
    {
        string prefix = messagePrefix != "" ? messagePrefix : "XR Echo cannot work";

        UnityEngine.Object[] instances = FindObjectsOfType(t);
        if (instances.Length == 0)
        {
            if (MakeButtonAlertField(""+t, "Add a " + t + " instance", prefix + " without an instance of " + t + ". Click on the button above to automatically add one.", isError))
            {
                GameObject master = ((MonoBehaviour)target).gameObject;
                GameObject instance = new GameObject("" + t, t);
                instance.transform.parent = master.transform;
                return true;
            }
            return false;
        } else if (instances.Length > 1)
        {
            if (MakeButtonAlertField(""+t, "Remove spare " + t + " instance(s)", "There should be only one instance of " + t + ". Click on the button above to automatically remove all Game Objects containing a spare one."))
            {
                for (int i=0; i< instances.Length-1; i++)
                {
                    GameObject current = ((MonoBehaviour)instances[i]).gameObject;
                    DestroyImmediate(current);
                }
                return true;
            }
            return false;
        }
        return true;
    }

    private bool MakeButtonAlertField(string title, string buttonText, string alertText, bool isError=true)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        bool clicked = GUILayout.Button(buttonText);
        EditorGUILayout.HelpBox(alertText, isError ? MessageType.Error : MessageType.Warning);
        EditorGUILayout.Space();
        return clicked;
    }
}

#endif
