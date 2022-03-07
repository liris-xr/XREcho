using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class SplineSection
{
    public List<Vector3> points = new List<Vector3>();
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SplineMeshProvider : MonoBehaviour
{
    public List<SplineSection> sections;

    private float _thickness = 1f;
    
    public Mesh lineMesh;
    public Mesh jointMesh;
    
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
            ForceRegenerate();
        }
    }

    public void ForceRegenerate()
    {
        var nMeshes = 0;
        var totalLengthSquared = 0.0f;

        foreach (var section in sections)
        {
            nMeshes += section.points.Count + section.points.Count - 1;
            
            for (var i = 0; i < section.points.Count - 1; i++)
            {
                totalLengthSquared += (section.points[i] - section.points[i + 1]).sqrMagnitude;
            }
        }

        var combine = new CombineInstance[nMeshes];

        var meshIdx = 0;
        var lengthSquaredOffset = 0.0f;

        foreach (var section in sections)
        {
            for (var i = 0; i < section.points.Count - 1; i++)
            {
                var segmentLengthSquared = (section.points[i] - section.points[i + 1]).sqrMagnitude;
                var t0 = lengthSquaredOffset / totalLengthSquared;
                var t1 = (lengthSquaredOffset + segmentLengthSquared) / totalLengthSquared;
                combine[meshIdx++] = CreateJointMesh(section.points[i], _thickness, Color.Lerp(Color.white, Color.black, t0));
                combine[meshIdx++] = CreateLineMesh(section.points[i], section.points[i + 1], _thickness,
                    Color.Lerp(Color.white, Color.black, t0), Color.Lerp(Color.white, Color.black, t1));
                lengthSquaredOffset += segmentLengthSquared;
            }

            // Line cap
            combine[meshIdx++] = CreateJointMesh(section.points[section.points.Count - 1], _thickness, Color.Lerp(Color.white, Color.black, lengthSquaredOffset / totalLengthSquared));
        }
        
        var mesh = new Mesh
        {
            indexFormat = IndexFormat.UInt32
        };
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        gameObject.GetComponent<MeshFilter>().mesh.Optimize();
    }

    public void SetSections(List<SplineSection> sections)
    {
        this.sections = sections;
    }

    public void SetThickness(float thickness)
    {
        this._thickness = thickness;
    }

    private CombineInstance CreateJointMesh(Vector3 position, float radius, Color color)
    {
        var transformMatrix = Matrix4x4.identity;
        transformMatrix *= Matrix4x4.Translate(position);
        transformMatrix *= Matrix4x4.Scale(new Vector3(radius, radius, radius));
        transformMatrix *= Matrix4x4.Scale(new Vector3(2, 2, 2));

        var newJointMesh = new Mesh();
        newJointMesh.SetVertices(jointMesh.vertices);
        newJointMesh.SetNormals(jointMesh.normals);
        newJointMesh.SetTriangles(jointMesh.triangles, 0);

        var colors = new Color[jointMesh.vertices.Length];

        for (var i = 0; i < jointMesh.vertices.Length; ++i)
        {
            colors[i] = color;
        }

        newJointMesh.SetColors(colors);
        
        return new CombineInstance
        {
            mesh = newJointMesh,
            transform = transformMatrix
        };
    }
    
    private CombineInstance CreateLineMesh(Vector3 from, Vector3 to, float radius, Color colorStart, Color colorEnd)
    {
        var vector = to - from;
        var rotation = Quaternion.FromToRotation(Vector3.up, vector.normalized);
        var height = vector.magnitude;

        var transformMatrix = Matrix4x4.identity;
        transformMatrix *= Matrix4x4.Translate(from);
        transformMatrix *= Matrix4x4.Rotate(rotation);
        transformMatrix *= Matrix4x4.Scale(new Vector3(radius, height, radius));
        transformMatrix *= Matrix4x4.Scale(new Vector3(2, 0.5f, 2));
        transformMatrix *= Matrix4x4.Translate(new Vector3(0, 1, 0));

        var newLineMesh = new Mesh();
        newLineMesh.SetVertices(lineMesh.vertices);
        newLineMesh.SetNormals(lineMesh.normals);
        newLineMesh.SetTriangles(lineMesh.triangles, 0);

        var colors = new Color[lineMesh.vertices.Length];

        for (var i = 0; i < lineMesh.vertices.Length; ++i)
        {
            colors[i] = Color.Lerp(colorStart, colorEnd, (lineMesh.vertices[i].y + 1) / 2);
        }

        newLineMesh.SetColors(colors);

        return new CombineInstance
        {
            mesh = newLineMesh,
            transform = transformMatrix
        };
    }

}