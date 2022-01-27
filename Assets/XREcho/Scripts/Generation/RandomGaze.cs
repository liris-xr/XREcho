using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGaze : MonoBehaviour
{
    Camera cam;
    float timeSinceLastGaze;
    public float gazeInterval = 2.0f;

    int index;

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.parent.GetComponent<Camera>();
        timeSinceLastGaze = gazeInterval;

        index = 0;
    }

    void NewRandomGaze()
    {
        timeSinceLastGaze = 0;

        Vector2[] viewportPoints = new Vector2[] { 
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)};

        Vector3 newGazeDirection = PointInViewport(viewportPoints[index]);
        index = (index + 1) % 4;
        transform.LookAt(newGazeDirection);
    }

    Vector3 PointInViewport(Vector2 point)
    {
        float fov = cam.fieldOfView; // Vertical FOV
        float aspect = cam.aspect;   // width / height
        float f = Mathf.Tan(fov * Mathf.Deg2Rad / 2);

        Vector3 position = cam.transform.position;
        Quaternion rotation = cam.transform.rotation;

        // Computing frustum vertices

        // Near plane
        Vector3 yOffset = new Vector3(0, f, 0);
        Vector3 xOffset = new Vector3(aspect * yOffset.y, 0, 0);
        Vector3 zOffset = new Vector3(0, 0, 1);

        xOffset = rotation * xOffset;
        yOffset = rotation * yOffset;
        zOffset = rotation * zOffset;

        Vector3 center = position + zOffset;

        return center + 2 * (point.x - 0.5f) * xOffset + 2 * (point.y - 0.5f) * yOffset;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastGaze += Time.deltaTime;

        if (timeSinceLastGaze >= gazeInterval)
            NewRandomGaze();
    }
}
