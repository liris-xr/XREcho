using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoCameraManipulator : MonoBehaviour
{
    private Camera cam;
    private Vector2 translationFactor = new Vector2(1, 1);
    private Vector2 screen;

    public float zoomFactor = 0.1f;

    void Start()
    {
        cam = GetComponent<Camera>();
        screen = new Vector2(Screen.width, Screen.height);
        ComputeTranslationFactor();
    }
    
    private void ComputeTranslationFactor()
    {
        translationFactor = new Vector2(cam.orthographicSize * 2.0f * cam.aspect, cam.orthographicSize * 2.0f) / screen;
    }

    void OnGUI()
    {
        if (Event.current.displayIndex != cam.targetDisplay) return;

        Vector2 move = Event.current.delta;

        if (Event.current.type == EventType.MouseDrag && Event.current.button != 0)
        {
            transform.position += transform.right * (-move.x * translationFactor.x) + transform.up * (move.y * translationFactor.y);
        }
        else if (Event.current.type == EventType.ScrollWheel)
        {
            cam.orthographicSize *= (1.0f + move.y * zoomFactor);
            ComputeTranslationFactor();
        }
    }
}
