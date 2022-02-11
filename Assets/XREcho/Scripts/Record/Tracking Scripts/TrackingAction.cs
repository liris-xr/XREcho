using UnityEngine;

public class TrackingAction
{
    public ActionType type;
    public TrackedObject trackedObj;
    public GameObject targetObj;

    public bool HasPosition()
    {
        return (type == ActionType.POSITION || type == ActionType.POS_AND_ROT);
    }
}