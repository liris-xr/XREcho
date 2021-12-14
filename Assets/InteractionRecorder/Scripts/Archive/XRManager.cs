using UnityEngine;
using UnityEngine.XR;

public class XRManager : MonoBehaviour
{
    public bool enableXR;
    public GameObject[] XRGameObjects;

    void Awake()
    {
        foreach (var go in XRGameObjects)
        {
            go.SetActive(enableXR);
        }
    }
}
