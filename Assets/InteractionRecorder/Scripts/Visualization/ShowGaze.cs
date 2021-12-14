using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

// include required packages
using Curve;
using Tubular;

public class ShowGaze : MonoBehaviour
{
    public float rayVerticalOffset = 0.05f;
    public float radius = 0.02f;
    public int radialSegments = 8;
    public Color gazeColor = new Color(1, 0, 0, 0.2f);

    private GameObject gazeObject;
    private MeshRenderer gazeRenderer;
    private MeshFilter gazeMesh;

    private Camera cam;

    private Vector3 lastPosition;
    private Vector3 lastDirection;

    private RaycastHit hitInfo = new RaycastHit();
    private CatmullRomCurve curve;

    private MaterialManager materialManager;

    private void Start()
    {
        materialManager = MaterialManager.GetInstance();
        InitializeGazeObject();
    }

    public void BuildCurve()
    {
        curve = new CatmullRomCurve(new List<Vector3>() { transform.position - transform.up * rayVerticalOffset, hitInfo.point });
        RenderTubularMesh();
    }

    private void InitializeGazeObject()
    {
        lastPosition = transform.position;
        lastDirection = transform.forward;

        gazeObject = new GameObject("Gaze Visualization");
        DontDestroyOnLoad(gazeObject);
        gazeMesh = gazeObject.AddComponent<MeshFilter>();
        gazeRenderer = gazeObject.AddComponent<MeshRenderer>();
        gazeRenderer.material = materialManager.GetMaterial("gaze");

        UpdateColor();
        gazeObject.SetActive(false);
    }

    private void RenderTubularMesh()
    {
        gazeMesh.mesh = Tubular.Tubular.Build(curve, 1, radius, radialSegments, false);
    }

    private void UpdateColor()
    {
        gazeRenderer.material.color = gazeColor;
    }

    private void OnValidate()
    {
        if (gazeMesh != null && curve != null && Application.isPlaying)
            RenderTubularMesh();

        if (gazeRenderer != null)
            UpdateColor();
    }

    private void ComputeHitPoint()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out hitInfo))
            hitInfo.point = transform.position + transform.forward * 1000;
    }

    private void Update()
    {
        if (transform.position.Equals(lastPosition) && transform.forward.Equals(lastDirection))
            return;

        gazeObject.SetActive(true);
        ComputeHitPoint();
        BuildCurve();
    }

    private void OnDisable()
    {
        if (gazeObject != null)
            gazeObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Destroy(gazeObject);
    }
}