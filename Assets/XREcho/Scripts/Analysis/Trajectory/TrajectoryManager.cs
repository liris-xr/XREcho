using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class TrajectoryManager : MonoBehaviour
{
    private MaterialManager _materialManager;
    private static TrajectoryManager _instance;
    private IRecordDataProvider _recordDataProvider;

    private SplineMeshProvider _splineMeshProvider;

    public float tolerance = 0.1f;
    public float thickness = 0.1f;
    public float teleportationThreshold = 0.5f;
    private bool _show;

    private bool _dirty;
    
    public void OnValidate()
    {
        _dirty = true;
    }
    
    public void Update()
    {
        if (_dirty)
        {
            _dirty = false;
            if(_show) ForceRegenerate();
        }
    }
    
    private void Awake()
    {
        _recordDataProvider = GetComponent<IRecordDataProvider>();
        _splineMeshProvider = GetComponent<SplineMeshProvider>();

        if (_instance)
            Debug.LogError("2 trajectory manager: singleton design pattern broken");

        _instance = this;
    }
    
    private void Start()
    {
        _materialManager = MaterialManager.GetInstance();
        // GetComponent<Renderer>().material = _materialManager.GetMaterial("trajectory");
    }
    
    public void ToggleTrajectory(bool show)
    {
        _show = show;
        GetComponent<MeshRenderer>().enabled = show;
        
        if (show)
        {
            ForceRegenerate();
        }
    }

    public void ForceRegenerate()
    {
        _recordDataProvider.LoadSelectedRecordData(out var positions, out _, out _);

        var sections = new List<SplineSection> {new SplineSection()};

        var currentSection = sections[0];
        var lastPosition = positions[0];
        
        currentSection.points.Add(positions[0]);

        for (var i = 1; i < positions.Count; i++)
        {
            // Create new section if the player teleported himself
            if ((positions[i] - lastPosition).sqrMagnitude > teleportationThreshold * teleportationThreshold)
            {
                var newSection = new SplineSection();
                sections.Add(newSection);
                currentSection = newSection;
            }
            
            currentSection.points.Add(positions[i]);
            lastPosition = positions[i];
        }

        foreach (var section in sections)
        {
            var simplifiedPoints = new List<Vector3>();
            LineUtility.Simplify(section.points, tolerance, simplifiedPoints);

            if (simplifiedPoints.Count == 0)
                continue;
            
            section.points = simplifiedPoints;
        }

        _splineMeshProvider.SetSections(sections);
        _splineMeshProvider.SetThickness(thickness);
        _splineMeshProvider.ForceRegenerate();
    }

    public static TrajectoryManager GetInstance()
    {
        return _instance;
    }
    
}