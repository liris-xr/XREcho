using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ObjectHeatmapData
{
    private bool usingNew;
    private Texture2D oldTex;
    private Vector2 oldScale;

    private Texture2D newTex;

    private int texWidth;
    private int texHeight;
    private float area;
    private float pixelsPerSquareMeter;

    // In mm
    private int objWidth;
    private int objHeight;

    // Use to convert from object to texture coordinates and undersample
    private float objByTexWidth;
    private float objByTexHeight;

    private int[] hits;
    private float[] objBuffer;
    public float max;
    private Color[] pixels;

    private float resolution = 1f;

    public GameObject go;
    private Renderer rend;
    private MeshCollider col;

    private static int meterScale = 100; // what was 1 meter becomes 100 as size which means centimeter precision
    private static int gaussianDiameter;
    private static float[] gaussian = Utils.GetGaussian(15f, out gaussianDiameter); //15cm as std
    private static Color textureBackgroundColor = Color.white;
    private static Color textureForegroundColor = Color.black;

    public ObjectHeatmapData(RaycastHit hit, float _resolution)
    {
        resolution = _resolution;

        usingNew = true;
        max = 0f;

        go = hit.transform.gameObject;
        rend = hit.transform.GetComponent<Renderer>();
        col = hit.collider as MeshCollider;

        if (rend == null)
        {
            Debug.Log("No renderer found");
            return;
        }

        if (rend.material == null)
        {
            Debug.Log("No material found on renderer");
            return;
        }

        if (rend.material.mainTexture == null)
        {
            Debug.Log("No texture found");
            return;
        }

        if (col == null)
        {
            Debug.Log("No mesh collider found");
            return;
        }

        oldTex = rend.material.mainTexture as Texture2D;
        oldScale = rend.material.mainTextureScale;

        texWidth = Mathf.RoundToInt(oldTex.width * oldScale.x * resolution);
        texHeight = Mathf.RoundToInt(oldTex.height * oldScale.y * resolution);

        objWidth = Mathf.RoundToInt(rend.bounds.size.x * meterScale);
        objHeight = Mathf.RoundToInt(rend.bounds.size.z * meterScale);

        objByTexWidth = (float)objWidth / (float)texWidth;
        objByTexHeight = (float)objHeight / (float)texHeight;

        area = Utils.GetSurfaceArea(go.GetComponent<MeshFilter>().mesh);
        pixelsPerSquareMeter = (float)(texWidth * texHeight) / area;

        Debug.Log("object = " + hit.transform.name + ", width = " + texWidth + ", height = " + texHeight + ", area = " + area + ", pixels/m2 = " + pixelsPerSquareMeter);

        newTex = NewTexture(texWidth, texHeight);
        InitBuffers();
        // To avoid looping of the texture
        rend.material.mainTextureScale = Vector2.one;

        rend.material.mainTexture = newTex;
    }

    private void InitBuffers()
    {
        hits = new int[objWidth * objHeight];
        objBuffer = new float[objWidth * objHeight];

        for (int i = 0; i < objWidth * objHeight; i++)
        {
            hits[i] = 0;
            objBuffer[i] = 0f;
        }

        pixels = new Color[texWidth * texHeight];

        for (int i = 0; i < texWidth * texHeight; i++)
            pixels[i] = textureBackgroundColor;
    }

    public void ComputePixelsColorBins(float globalMax, Color[] colors)
    {
        int n = colors.Length;
        float[] thresholds = new float[n - 1];

        for (int i = 0; i < n - 1; i++)
            thresholds[i] = globalMax / (float)n * (float)(i+1);

        int xRadius = Mathf.CeilToInt((objByTexWidth - 1) / 2.0f);
        int yRadius = Mathf.CeilToInt((objByTexHeight - 1) / 2.0f);

        for (int j = 0; j < texHeight; j++)
        {
            for (int i = 0; i < texWidth; i++)
            {
                int index = j * texWidth + i;
                int x = Mathf.RoundToInt(i * objByTexWidth);
                int y = Mathf.RoundToInt(j * objByTexHeight);

                // Undersampling on object buffer to compute value for texture
                float value = 0;
                int weight = 0;

                for (int dx = -xRadius; dx <= xRadius; dx++)
                    for (int dy = -yRadius; dy <= yRadius; dy++)
                    {
                        int newX = x + dx;
                        int newY = y + dy;

                        if (newX < 0 || newX >= objWidth || newY < 0 || newY >= objHeight)
                            continue;

                        weight++;
                        int objIndex = newY * objWidth + newX;
                        
                        if (objBuffer[objIndex] != 0)
                            value += objBuffer[objIndex];
                    }

                if (value == 0)
                {
                    pixels[index] = textureBackgroundColor;
                    continue;
                }

                value /= (float)weight;

                int k;
                for (k = 0; k < (n - 1) && value >= thresholds[k]; k++);

                pixels[index] = colors[k];
            }
        }
    }

    public void Apply()
    {
        newTex.SetPixels(pixels);
        newTex.Apply();
    }

    public void ToggleTexture()
    {
        if (usingNew)
        {
            rend.material.mainTexture = oldTex;
            rend.material.mainTextureScale = oldScale;
        }
        else
        {
            rend.material.mainTexture = newTex;
            rend.material.mainTextureScale = Vector2.one;
        }

        usingNew = !usingNew;
    }

    public void RegisterFromUV(Vector2 pixelUV)
    {
        Vector2 tiling;
        Vector2 finalObjCoords = new Vector2();

        try
        {
            pixelUV.x *= newTex.width;
            pixelUV.y *= newTex.height;

            tiling = rend.material.mainTextureScale;
            pixelUV.x *= tiling.x;
            pixelUV.y *= tiling.y;

            finalObjCoords.x = Mathf.RoundToInt(pixelUV.x * objByTexWidth);
            finalObjCoords.y = Mathf.RoundToInt(pixelUV.y * objByTexHeight);

            RegisterHit(finalObjCoords);
        }
        catch (Exception e)
        {
            Debug.Log("pixelUV = " + pixelUV.ToString("F8"));
            Debug.Log("Real size UV = " + pixelUV.ToString("F8"));
            Debug.Log("Tiled UV = " + pixelUV.ToString("F8"));
            Debug.Log("objByTexWidth =  " + objByTexWidth + ", objByTexHeight = " + objByTexHeight);
            Debug.Log("final obj coords = " + finalObjCoords);
            Debug.Log("objWidth = " + objWidth + ", objHeight = " + objHeight);
        }
    }

    private void RegisterHit(Vector2 coords)
    {
        hits[(int)coords.y * objWidth + (int)coords.x]++;
    }

    public void HitsToGaussians()
    {
        int radius = gaussianDiameter / 2;

        for (int i = 0; i < objWidth; i++)
            for (int j = 0; j < objHeight; j++)
            {
                int hitsIndex = j * objWidth + i;
                if (hits[hitsIndex] == 0)
                    continue;
                
                for (int dx = -radius; dx <= radius; dx++)
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = i + dx;
                        int y = j + dy;

                        if (x < 0 || x >= objWidth || y < 0 || y >= objHeight)
                            continue;

                        int index = Mathf.RoundToInt(y) * objWidth + Mathf.RoundToInt(x);
                        objBuffer[index] += hits[hitsIndex] * gaussian[(dy + radius) * gaussianDiameter + (dx + radius)];

                        if (objBuffer[index] > max)
                            max = objBuffer[index];
                    }
            }
    }

    private Texture2D NewTexture(int width, int height, Color? c = null)
    {
        Color color = c ?? textureBackgroundColor;
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        tex.SetPixels(pixels);

        return tex;
    }

}

public class TrajectoryHeatmap
{
    private Dictionary<GameObject, ObjectHeatmapData> objectData;
    private int actionId;
    public float resolution;
    private float globalMax;

    private ReplayManager replayManager;
    private ExpeRecorderConfig config;

    private static int floorLayer = 1 << LayerMask.NameToLayer("Heatmap Floor");
    private static float maxDistance = 3; //  max distance
    
    public TrajectoryHeatmap(float _resolution)
    {
        Init();
        resolution = _resolution;
        replayManager = ReplayManager.GetInstance();
        config = ExpeRecorderConfig.GetInstance();
    }

    public void Init()
    {
        objectData = new Dictionary<GameObject, ObjectHeatmapData>();
        globalMax = 0;
    }

    public void ToggleHeatmap()
    {
        foreach (ObjectHeatmapData ohd in objectData.Values)
            ohd.ToggleTexture();
    }

    public void DrawAtClick(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Debug.Log("mousePosition = " + mousePosition + ", ray.origin = " + ray.origin + ", ray.direction = " + ray.direction);

        RayToTexture(ray);
    }

    public void ComputeSceneHeatmap(int _actionId)
    {
        actionId = _actionId;

        string recordingsFolder = config.GetRecordingsFolder();
        string pattern = "*objectsData" + config.GetFileSuffix() + ".csv";
        //string pattern = "*14-17-37_objectsData" + config.GetFileSuffix() + ".csv";
        IEnumerable<string> files = Directory.GetFiles(recordingsFolder, pattern, SearchOption.AllDirectories);

        foreach (string file in files)
        {
            Debug.Log("Computing heatmap of file = " + file);
            ComputeFileHeatmap(file);
        }
    }

    private void ComputeFileHeatmap(string filepath)
    {
        List<Dictionary<string, object>> logs = CSVReader.ReadCSV(filepath, "actionID", actionId.ToString());

        foreach (Dictionary<string, object> entry in logs)
        {
            if ((int)(float)entry["actionId"] == actionId)
            {
                Vector3 position = ReplayManager.LoadPosition(entry);
                Ray ray = new Ray(position, Vector3.down);
                RayToTexture(ray);
            }
        }

        Apply();
    }

    public void ComputeCurrentHeatmap(int _actionId)
    {
        actionId = _actionId;

        Debug.Log("Computing Trajectory Heatmap for action " + actionId);
        if (replayManager.objectsData == null || replayManager.objectsData.Count == 0)
        {
            Debug.LogError("Can't compute trajectory heatmaps without recordings");
            return;
        }

        List<Dictionary<string, object>> logs = replayManager.objectsData[0];

        foreach (Dictionary<string, object> entry in logs)
        {
            if ((int)(float)entry["actionId"] == actionId)
            {
                Vector3 position = ReplayManager.LoadPosition(entry);
                Ray ray = new Ray(position, Vector3.down);
                RayToTexture(ray);
            }
        }

        Apply();
    }


    public void RayToTexture(Ray ray)
    {
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, maxDistance, floorLayer))
            return;

        GameObject go = hit.transform.gameObject;

        if (!objectData.ContainsKey(go))
        {
            Debug.Log("Creating ObjectHeatmapData for " + go);

            // We replace any collider with a MeshCollider so we can have texture coords on ray casts
            MeshCollider col = hit.collider as MeshCollider;

            if (col == null)
            {
                Collider currentCollider = go.GetComponent<Collider>();

                if (currentCollider != null)
                    UnityEngine.Object.DestroyImmediate(currentCollider);
                go.AddComponent<MeshCollider>();
                Physics.Raycast(ray, out hit, maxDistance, floorLayer);
            }

            objectData[go] = new ObjectHeatmapData(hit, resolution);
        }

        objectData[go].RegisterFromUV(hit.textureCoord);
    }

    public void Apply()
    {
        Color[] colors = ColorGenerator.GetLinearColorPalette(5, Color.green, Color.red);

        foreach (ObjectHeatmapData ohd in objectData.Values)
        {
            Debug.Log("Object: " + ohd.go + " starting hits to gaussians");
            ohd.HitsToGaussians();
            globalMax = Mathf.Max(globalMax, ohd.max);
        }

        foreach (ObjectHeatmapData ohd in objectData.Values)
        {
            Debug.Log("Object: " + ohd.go + " starting computation of texture");
            ohd.ComputePixelsColorBins(globalMax, colors);
            Debug.Log("Transfering texture to graphic card");
            ohd.Apply();
        }
    }
}
