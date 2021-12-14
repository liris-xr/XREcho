using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MaterialEntry
{
    public string name;
    public Material mat;
}

public class MaterialManager : MonoBehaviour
{
    static MaterialManager instance;

    private string renderingPipeline;

    [Header("Legacy")]
    public MaterialEntry[] legacyMaterials;
    private Dictionary<string, Material> _legacyMaterials;

    [Header("URP")]
    public MaterialEntry[] URPMaterials;
    private Dictionary<string, Material> _URPMaterials;

    void Awake()
    {
        if (instance)
            Debug.LogError("2 Material Managers: singleton design pattern broken");

        instance = this;

        renderingPipeline = Utils.GetRenderingPipeline();

        _legacyMaterials = new Dictionary<string, Material>();
        _URPMaterials = new Dictionary<string, Material>();

        foreach (MaterialEntry me in legacyMaterials)
            _legacyMaterials[me.name] = me.mat;

        foreach (MaterialEntry me in URPMaterials)
            _URPMaterials[me.name] = me.mat;
    }

    public static MaterialManager GetInstance()
    {
        return instance;
    }

    public Material GetMaterial(string name)
    {
        switch (renderingPipeline)
        {
            case "Legacy":
                return _legacyMaterials[name];
            case "URP":
                return _URPMaterials[name];
        }

        return null;
    }
}