using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// include required packages
using Curve;
using Tubular;

public class TrajectoryManager : MonoBehaviour
{
    private static TrajectoryManager instance;

    public float radius = 0.03f;
    public int nbRadialSegments = 8;
    public bool closed = false;

    [Header("Display")]
    public bool showTrajectories;
    public void ToggleTrajectories() { showTrajectories = !showTrajectories; CheckTrajectoriesVisibility(); }
    public bool showControlPoints;
    public void ToggleControlPoints() { showControlPoints = !showControlPoints; CheckControlPointsVisibility(); }
    public float sphereSize = 0.1f;

    [Header("Trajectory colors")]
    public int numberOfColors = 3;
    public Color startingColor = Color.HSVToRGB(0f, 0.8f, 1.0f);

    private int nbTrajectories;

    private GameObject trajectoryObject;
    private List<GameObject> trajectories;
    private List<CatmullRomCurve> curves;

    private List<GameObject> controlPointsObjects;
    private List<List<GameObject>> cpSpheres;
    private List<List<Vector3>> controlPoints;

    private void Awake()
    {
        if (instance)
            Debug.LogError("2 Trajectory Managers: singleton design pattern broken");

        instance = this;
        InitTrajectories();
    }

    public void InitTrajectories()
    {
        showTrajectories = false;
        showControlPoints = false;
        nbTrajectories = 0;
        trajectories = new List<GameObject>();
        curves = new List<CatmullRomCurve>();

        controlPointsObjects = new List<GameObject>();
        cpSpheres = new List<List<GameObject>>();
        controlPoints = new List<List<Vector3>>();

        if (trajectoryObject != null)
            Destroy(trajectoryObject);

        trajectoryObject = new GameObject("Trajectories");
        trajectoryObject.transform.parent = transform;

        CheckTrajectoriesVisibility();
        CheckControlPointsVisibility();
    }

    public static TrajectoryManager GetInstance()
    {
        return instance;
    }

    public int NewTrajectory(List<Vector3> controlPointsList = null)
    {
        GameObject trajectory = new GameObject("Trajectory " + nbTrajectories);
        trajectory.transform.parent = trajectoryObject.transform;
        trajectories.Add(trajectory);
        curves.Add(null);

        trajectory.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = trajectory.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/Color"));
        meshRenderer.material.color = ColorGenerator.GetNthColor(nbTrajectories, numberOfColors, startingColor);

        GameObject controlPointsObject = new GameObject("Control Points");
        controlPointsObject.transform.parent = trajectory.transform;
        controlPointsObjects.Add(controlPointsObject);

        if (controlPointsList == null)
            controlPointsList = new List<Vector3>();

        controlPoints.Add(controlPointsList);
        cpSpheres.Add(new List<GameObject>());

        foreach (Vector3 cp in controlPointsList)
            NewCPSphere(nbTrajectories, cp);

        BuildCurve(nbTrajectories);

        return nbTrajectories++;
    }

    public void AddControlPoint(int i, Vector3 cp)
    {
        controlPoints[i].Add(cp);

        if (controlPoints[i].Count > 1)
            BuildCurve(i);

        NewCPSphere(i, cp);
    }

    private void NewCPSphere(int i, Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = controlPointsObjects[i].transform;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);

        cpSpheres[i].Add(sphere);
    }

    private void ChangeCPSphereSize()
    {
        if (cpSpheres == null)
            return;

        foreach (List<GameObject> listOfSpheres in cpSpheres)
            foreach (GameObject sphere in listOfSpheres)
                sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
    }

    private void BuildCurve(int i)
    {
        curves[i] = new CatmullRomCurve(controlPoints[i]);
        BuildTubularMesh(i);
    }

    private void BuildTubularMesh(int i)
    {
        // Build tubular mesh with Curve
        int nbTubularSegments = controlPoints[i].Count - 1;

        if (nbTubularSegments < 1)
            return;

        var mesh = Tubular.Tubular.Build(curves[i], nbTubularSegments, radius, nbRadialSegments, closed);

        // visualize mesh
        trajectories[i].GetComponent<MeshFilter>().sharedMesh = mesh;

        CheckTrajectoriesVisibility();
        CheckControlPointsVisibility();
    }

    private void RebuildTubularMeshes()
    {
        for (int i = 0; i < nbTrajectories; i++)
            BuildTubularMesh(i);
    }

    private void ChangeColors()
    {
        for (int i = 0; i < nbTrajectories; i++)
            trajectories[i].GetComponent<MeshRenderer>().material.color = ColorGenerator.GetNthColor(i, numberOfColors, startingColor);
    }

    private void CheckTrajectoriesVisibility()
    {
        if (trajectories == null)
            return;

        foreach (GameObject trajectory in trajectories)
            trajectory.SetActive(showTrajectories);
    }


    private void CheckControlPointsVisibility()
    {
        if (controlPointsObjects == null)
            return;

        foreach (GameObject controlPointsObject in controlPointsObjects)
            controlPointsObject.SetActive(showControlPoints);
    }

    private void OnValidate()
    {
        RebuildTubularMeshes();
        ChangeCPSphereSize();
        ChangeColors();
        CheckTrajectoriesVisibility();
        CheckControlPointsVisibility();
    }
}