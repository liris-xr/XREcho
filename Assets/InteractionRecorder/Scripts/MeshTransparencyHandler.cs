using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MeshTransparencyHandler : MonoBehaviour
{
    public float defaultTransparency = 0.8f;

    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material transparentMaterial;

    MaterialManager materialManager;

    private void Awake()
    {
        materialManager = MaterialManager.GetInstance();
        meshRenderer = GetComponent<MeshRenderer>();

        originalMaterial = meshRenderer.material;

        transparentMaterial = materialManager.GetMaterial("object");
        //transparentMaterial = new Material(originalMaterial);
        //transparentMaterial.CopyPropertiesFromMaterial(originalMaterial);
        transparentMaterial.mainTexture = originalMaterial.mainTexture;
        //transparentMaterial = originalMaterial;
        //transparentMaterial.shader = tmpMaterial.shader;

        SetTransparencyAlpha(defaultTransparency);
        SetTransparencyActive(true);
    }

    public void SetTransparencyAlpha(float newAlpha)
    {
        /*
        Color transparentColor = originalMaterial.color;
        transparentColor.a = newAlpha;
        transparentMaterial.color = transparentColor;

        meshRenderer.material = transparentMaterial;
        */
        Color transparentColor = transparentMaterial.color;
        transparentColor.a = newAlpha;
        transparentMaterial.color = transparentColor;
    }

    public void SetTransparencyActive(bool active)
    {
        if (active)
            meshRenderer.material = transparentMaterial;
        else
            meshRenderer.material = originalMaterial;
    }
}
