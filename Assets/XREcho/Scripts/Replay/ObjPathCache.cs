using UnityEngine;
using System.Collections.Generic;

public class ObjPathCache
{
    private Dictionary<string, List<GameObject>> objects;

    public ObjPathCache()
    {
        objects = new Dictionary<string, List<GameObject>>();

        foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject), true) as GameObject[])
        {
            string objPath = Utils.GetGameObjectPath(obj);

            if (!objects.ContainsKey(objPath))
                objects[objPath] = new List<GameObject>();

            objects[objPath].Add(obj);
        }
    }

    public GameObject GetNextObject(string objPath)
    {
        if (!objects.ContainsKey(objPath))
            return null;

        List<GameObject> objectsAtPath = objects[objPath];
        GameObject nextObject = null;

        if (objectsAtPath.Count > 0)
        {
            nextObject = objectsAtPath[0];
            objectsAtPath.RemoveAt(0);
        }

        return nextObject;
    }
}
