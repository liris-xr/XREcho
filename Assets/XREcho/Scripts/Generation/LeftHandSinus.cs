using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHandSinus : MonoBehaviour
{
    float initY;

    void Start()
    {
        initY = transform.position.y;
    }

    void Update()
    {
        Vector3 currentAngles = transform.eulerAngles;
        currentAngles.z = Mathf.Rad2Deg * Mathf.Abs(1 - Time.time % 2) * Mathf.PI / 4.0f;
        transform.eulerAngles = currentAngles;

        Vector3 currentPosition = transform.position;
        currentPosition.y = initY + Mathf.Abs(1 - Time.time % 2) - 0.5f;
        transform.position = currentPosition;
    }
}
