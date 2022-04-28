using System;
using UnityEngine;

public class GUITrajectory
{
    private readonly TrajectoryManager _trajectoryManager;
    private readonly GUIStylesManager _stylesManager;

    private bool _displayTrajectory;

    public GUITrajectory(TrajectoryManager trajectoryManager, GUIStylesManager stylesManager)
    {
        _stylesManager = stylesManager;
        _trajectoryManager = trajectoryManager;
    }

    public void OnNewRecordLoaded()
    {
        if (_displayTrajectory)
        {
            _trajectoryManager.ForceRegenerate();
        }
    }

    public void ShowTrajectory(bool show)
    {
        _displayTrajectory = show;
    }
    
    public void OnGui()
    {
        var showTrajectory = GUILayout.Toggle(_displayTrajectory, "Trajectory", _stylesManager.toggleStyle);
        
        if (showTrajectory != _displayTrajectory)
        {
            _displayTrajectory = showTrajectory;
            _trajectoryManager.ToggleTrajectory(showTrajectory);
        }

        if (showTrajectory)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Quality: {(1 - _trajectoryManager.tolerance) * 100:0}%");
            
            // Limited to 99% due to performance issues
            var tolerance = 1 - GUILayout.HorizontalSlider(1 - _trajectoryManager.tolerance, 0f, 0.99f);
            
            if (Math.Abs(_trajectoryManager.tolerance - tolerance) > 10e-3)
            {
                _trajectoryManager.tolerance = tolerance;
                _trajectoryManager.ForceRegenerate();
            }
            
            GUILayout.Label($"Thickness: {_trajectoryManager.thickness:0.##}");
            var thickness = GUILayout.HorizontalSlider(_trajectoryManager.thickness, 0.01f, 3f);
            
            if (Math.Abs(_trajectoryManager.thickness - thickness) > 10e-3)
            {
                _trajectoryManager.thickness = thickness;
                _trajectoryManager.ForceRegenerate();
            }
            
            GUILayout.Label($"Teleportation threshold: {_trajectoryManager.teleportationThreshold:0.##}");
            var teleportationThreshold = GUILayout.HorizontalSlider(_trajectoryManager.teleportationThreshold, 0.01f, 5f);
            
            if (Math.Abs(_trajectoryManager.teleportationThreshold - teleportationThreshold) > 10e-3)
            {
                _trajectoryManager.teleportationThreshold = teleportationThreshold;
                _trajectoryManager.ForceRegenerate();
            }
            
            GUILayout.EndVertical();
        }
    }
    
}