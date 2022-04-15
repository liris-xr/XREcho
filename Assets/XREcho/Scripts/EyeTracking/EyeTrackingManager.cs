using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class EyeTrackingManager : MonoBehaviour
{
    private static EyeTrackingManager instance;

    public void Awake()
    {
        if (instance)
            Debug.LogError("2 eye tracking manager: singleton design pattern broken");

        instance = this;
    }

    public void Start()
    {
        foreach (var provider in GetComponents<EyeTrackingProvider>())
        {
            provider.enabled = provider.GetProviderType() == RecordingManager.GetInstance().eyeTrackingProviderType;
        }
    }

    public static EyeTrackingManager GetInstance()
    {
        return instance;
    }

    [CanBeNull]
    public EyeTrackingProvider GetProviderByType(EyeTrackingProviderType type)
    {
        return GetComponents<EyeTrackingProvider>().FirstOrDefault(provider => provider.GetProviderType() == type);
    }
}