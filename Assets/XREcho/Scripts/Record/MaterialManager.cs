using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    private bool ContainsMaterial(MaterialEntry[] list, string name)
    {
        for (int i=0; i< list.Length; i++)
        {
            if (list[i].name == name) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void AddMaterial(string name, string materialName, bool isLegacy)
    {
        MaterialEntry[] prelist = isLegacy ? legacyMaterials : URPMaterials;
        MaterialEntry[] list;
        if (prelist == null) {
            prelist = new MaterialEntry[0];
        }
        if (!ContainsMaterial(prelist, name))
        {
            list = new MaterialEntry[prelist.Length+1];
            System.Array.Copy(prelist, list, prelist.Length);
            MaterialEntry newMat = new MaterialEntry();
            newMat.name = name;
            string materialSuffix = isLegacy ? "_Legacy" : "_URP";
            newMat.mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/XREcho/" + materialName + materialSuffix + ".mat", typeof(Material));
            list[list.Length-1] = newMat;
            if (isLegacy)
            {
                legacyMaterials = list;
            } else
            {
                URPMaterials = list;
            }
        }
    }
    
    private void OnValidate() 
    {
        for (int i=0; i<2; i++)
        {
            AddMaterial("gaze", "Materials/GazeMaterial", i == 0);
            AddMaterial("cameraFrustum", "Materials/GazeMaterial", i == 0);
            AddMaterial("object", "Materials/ObjectMaterial", i == 0);
            AddMaterial("hand", "Prefabs/Hands/Materials/HandMaterial", i == 0);
            AddMaterial("replayHand", "Prefabs/Hands/Materials/ReplayHandMaterial", i == 0);
            AddMaterial("notFound", "Materials/NotFoundMaterial", i == 0);
        }
    }
#endif

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