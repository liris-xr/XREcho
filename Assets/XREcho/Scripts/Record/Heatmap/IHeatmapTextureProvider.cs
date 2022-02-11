﻿using UnityEngine;

public interface IHeatmapTextureProvider
{
    Texture2D HeatmapToTexture(float[,] heatmap, int w, int h);
}