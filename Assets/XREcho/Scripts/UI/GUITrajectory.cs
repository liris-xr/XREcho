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
    }
    
}