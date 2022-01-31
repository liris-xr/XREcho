using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAutoMaterial : MonoBehaviour
{

    public string materialName;

    void Start()
    {
        MeshRenderer rend = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
        rend.materials = new Material[] { MaterialManager.GetInstance().GetMaterial(materialName) };
    }
    
}
