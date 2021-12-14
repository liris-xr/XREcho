using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ShowCameraFrustum : MonoBehaviour
{
    public Color frustumColor = new Color(0.25f, 0.25f, 0.5f, 0.2f);
    public float zFarFrustumViz = 1.5f;

    private Camera cam;
    private MeshRenderer frustumRenderer;
    private MeshFilter frustumMesh;
    private GameObject frustum;

    string renderingPipeline;

    MaterialManager materialManager;

    void Start()
    {
        cam = gameObject.GetComponent<Camera>();

        if (cam == null)
            return;

        materialManager = MaterialManager.GetInstance();

        InitializeFrustumObject();
        InitializeMesh();
    }

    void OnEnable()
    {
        if (frustum == null)
            return;

        frustum.SetActive(true);
    }

    void OnDisable()
    {
        if (frustum == null)
            return;

        frustum.SetActive(false);
    }

    void Update()
    {
        UpdateVertices();
    }

    void OnDestroy()
    {
        Destroy(frustum);
    }

    void UpdateColor()
    {
        frustumRenderer.material.color = frustumColor;
    }

    void InitializeFrustumObject()
    {
        frustum = new GameObject("Camera Frustum Visualization");   
        frustumMesh = frustum.AddComponent<MeshFilter>();
        frustumRenderer = frustum.AddComponent<MeshRenderer>();

        frustumRenderer.material = materialManager.GetMaterial("cameraFrustum");
        UpdateColor();
    }

    void InitializeMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = ComputeVertices();
        mesh.triangles = ComputeTriangles();

        frustumMesh.mesh = mesh;
        frustumMesh.mesh.MarkDynamic();
    }

    void OnValidate()
    {
        if (cam != null)
            UpdateVertices();

        if (frustumRenderer != null)
            UpdateColor();
    }

    int[] ComputeTriangles()
    {
        int[] triangles = new int[4 * 12];

        for (int i = 0; i < 4; i++)
        {
            triangles[12 * i] = i;
            triangles[12 * i + 1] = triangles[12 * i + 4] = i + 4;
            triangles[12 * i + 2] = triangles[12 * i + 3] = (i + 1) % 4;
            triangles[12 * i + 5] = (i + 1) % 4 + 4;

            
            triangles[12 * i + 6] = i;
            triangles[12 * i + 7] = triangles[12 * i + 10] = (i + 1) % 4;
            triangles[12 * i + 8] = triangles[12 * i + 9] = i + 4;
            triangles[12 * i + 11] = (i + 1) % 4 + 4;
        }

        return triangles;
    }

    // Compute vertices from Camera parameters
    Vector3[] ComputeVertices()
    {
        // Fetching camera parameters

        float zNear = cam.nearClipPlane;
        float fov = cam.fieldOfView; // Vertical FOV
        float aspect = cam.aspect;   // width / height
        float f = Mathf.Tan(fov * Mathf.Deg2Rad / 2);

        Vector3 position = cam.transform.position;
        Quaternion rotation = cam.transform.rotation;

        // Computing frustum vertices

        // Near plane
        Vector3 yNearOffset = new Vector3(0, zNear * f, 0);
        Vector3 xNearOffset = new Vector3(aspect * yNearOffset.y, 0, 0);
        Vector3 zNearOffset = new Vector3(0, 0, zNear);

        xNearOffset = rotation * xNearOffset;
        yNearOffset = rotation * yNearOffset;
        zNearOffset = rotation * zNearOffset;

        Vector3 nearPosition = position + zNearOffset;

        // Visualization far plane
        float ratio = zFarFrustumViz / zNear;

        Vector3 xFarOffset = xNearOffset * ratio;
        Vector3 yFarOffset = yNearOffset * ratio;
        Vector3 farPosition = position + zNearOffset * ratio;

        Vector3[] vertices = new Vector3[4 * 2]
        {
            nearPosition - xNearOffset - yNearOffset,
            nearPosition - xNearOffset + yNearOffset,
            nearPosition + xNearOffset + yNearOffset,
            nearPosition + xNearOffset - yNearOffset,
            farPosition - xFarOffset - yFarOffset,
            farPosition - xFarOffset + yFarOffset,
            farPosition + xFarOffset + yFarOffset,
            farPosition + xFarOffset - yFarOffset
        };

        return vertices;
    }

    void UpdateVertices()
    {
        frustumMesh.mesh.vertices = ComputeVertices();
        frustumMesh.mesh.RecalculateBounds();
    }
}