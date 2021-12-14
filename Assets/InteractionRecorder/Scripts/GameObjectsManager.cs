using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The <c>GameObjectsManager</c> instance is just here to reference GameObjects you may want
/// to clone but could be absent at runtime (VR Hand models when no headset is plugged for example).
/// </summary>
public class GameObjectsManager : MonoBehaviour
{
    public GameObject[] gameObjects;
    private static GameObjectsManager instance;
    private Dictionary<string, GameObject> gameObjectsStore;

    private void Awake()
    {
        if (instance)
            Debug.LogError("2 Game Objects Managers: singleton design pattern broken");

        instance = this;

        gameObjectsStore = new Dictionary<string, GameObject>();
        foreach (GameObject go in gameObjects)
            gameObjectsStore[go.transform.name] = go;
    }

    public static GameObjectsManager GetInstance()
    {
        return instance;
    }

    public GameObject GetGameObject(string gameObjectName)
    {
        if (!gameObjectsStore.ContainsKey(gameObjectName))
            return null;

        Debug.Log("Game Object " + gameObjectName + " found in GameObjectsManager");

        return gameObjectsStore[gameObjectName];
    }
}
