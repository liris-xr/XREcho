using System;
using UnityEditor;
using UnityEngine;

public class GUIHeatmap
{
    private readonly PositionHeatmapManager _positionHeatmapManager;
    private readonly GUIStylesManager _stylesManager;
    
    private bool _displayHeatmap;
    private bool _displayAggregatedHeatmap;

    private string _heatmapScaleLowerBoundStr;
    private string _heatmapScaleUpperBoundStr;
    private float _lastHeatmapTransparency = 1f;
    private float _lastHeatmapScaleLowerBound;
    private float _lastHeatmapScaleUpperBound;

    public GUIHeatmap(PositionHeatmapManager positionHeatmapManager, GUIStylesManager stylesManager)
    {
        _stylesManager = stylesManager;
        _positionHeatmapManager = positionHeatmapManager;
    }
    
    public void OnGui()
    {
        var showPositionHeatmap = GUILayout.Toggle(_displayHeatmap, "Position heatmap", _stylesManager.toggleStyle);
        
        if (showPositionHeatmap != _displayHeatmap)
        {
            _displayHeatmap = showPositionHeatmap;
            if(showPositionHeatmap) EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...", -1);
            _positionHeatmapManager.TogglePositionHeatmap(showPositionHeatmap);
            if(showPositionHeatmap) EditorUtility.ClearProgressBar();

            // Reset the heatmap scale
            if (showPositionHeatmap)
            {
                _heatmapScaleLowerBoundStr = "0";
                _heatmapScaleUpperBoundStr = $"{_positionHeatmapManager.GetMaxDuration():0.###}";
            }
        }

        if (showPositionHeatmap)
        {
            GUILayout.BeginVertical("box");

            var showAggregatedHeatmap = GUILayout.Toggle(_displayAggregatedHeatmap, "Aggregate all records", _stylesManager.toggleStyle);

            if (showAggregatedHeatmap != _displayAggregatedHeatmap)
            {
                _displayAggregatedHeatmap = showAggregatedHeatmap;
                
                if(showAggregatedHeatmap)
                    EditorUtility.DisplayProgressBar("Generating aggregated heatmap", "Generating aggregated position heatmap texture...", -1);
                else
                    EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...", -1);
                
                _positionHeatmapManager.ToggleAggregatedPositionHeatmap(showAggregatedHeatmap);
                
                EditorUtility.ClearProgressBar();
            }
            
            GUILayout.Label("Heatmap transparency");
            var transparencyValue = GUILayout.HorizontalSlider(_lastHeatmapTransparency, 0f, 1f);

            if (Math.Abs(_lastHeatmapTransparency - transparencyValue) > 10e-6)
            {
                _positionHeatmapManager.SetTransparency(transparencyValue);
                _lastHeatmapTransparency = transparencyValue;
            }

            if (showAggregatedHeatmap)
            {
                if (GUILayout.Button("Force re-generate heatmap"))
                {
                    EditorUtility.DisplayProgressBar("Generating aggregated heatmap", "Generating aggregated position heatmap texture...", -1);
                    _positionHeatmapManager.ForceRegenerate();
                    EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                var heatmapScaleLowerBoundStyle = new GUIStyle(GUI.skin.textField);
                var heatmapScaleUpperBoundStyle = new GUIStyle(GUI.skin.textField);

                var lowerBoundValid = float.TryParse(_heatmapScaleLowerBoundStr, out var heatmapScaleLowerBound) &&
                                      Math.Abs(heatmapScaleLowerBound) <= 10e-3;
                var upperBoundValid = float.TryParse(_heatmapScaleUpperBoundStr, out var heatmapScaleUpperBound) &&
                                      Math.Abs(heatmapScaleUpperBound - _positionHeatmapManager.GetMaxDuration()) <=
                                      10e-3 && heatmapScaleUpperBound > heatmapScaleLowerBound;

                heatmapScaleLowerBoundStyle.normal.textColor = lowerBoundValid ? Color.white : Color.red;
                heatmapScaleUpperBoundStyle.normal.textColor = upperBoundValid ? Color.white : Color.red;
                heatmapScaleLowerBoundStyle.hover.textColor = lowerBoundValid ? Color.white : Color.red;
                heatmapScaleUpperBoundStyle.hover.textColor = upperBoundValid ? Color.white : Color.red;
                heatmapScaleLowerBoundStyle.focused.textColor = lowerBoundValid ? Color.white : Color.red;
                heatmapScaleUpperBoundStyle.focused.textColor = upperBoundValid ? Color.white : Color.red;
                heatmapScaleLowerBoundStyle.stretchWidth = true;
                heatmapScaleUpperBoundStyle.stretchWidth = true;

                var minMaxBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedWidth = 50
                };

                GUILayout.BeginHorizontal();
                GUILayout.Label("Scale lower bound (s): ", GUI.skin.label);
                _heatmapScaleLowerBoundStr =
                    GUILayout.TextField(_heatmapScaleLowerBoundStr, heatmapScaleLowerBoundStyle);
                var setLowerScaleToMin = GUILayout.Button("Min", minMaxBtnStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Scale upper bound (s): ", GUI.skin.label);
                _heatmapScaleUpperBoundStr =
                    GUILayout.TextField(_heatmapScaleUpperBoundStr, heatmapScaleUpperBoundStyle);
                var setUpperScaleToMax = GUILayout.Button("Max", minMaxBtnStyle);
                GUILayout.EndHorizontal();

                if (setLowerScaleToMin)
                {
                    _heatmapScaleLowerBoundStr = "0";
                }

                if (setUpperScaleToMax)
                {
                    _heatmapScaleUpperBoundStr = $"{_positionHeatmapManager.GetMaxDuration():0.###}";
                }

                if (lowerBoundValid && upperBoundValid)
                {
                    heatmapScaleLowerBound /= _positionHeatmapManager.GetMaxDuration();
                    heatmapScaleUpperBound /= _positionHeatmapManager.GetMaxDuration();

                    if (Math.Abs(heatmapScaleLowerBound - _lastHeatmapScaleLowerBound) > 10e-6 ||
                        Math.Abs(heatmapScaleUpperBound - _lastHeatmapScaleUpperBound) > 10e-6)
                    {
                        _positionHeatmapManager.SetScaleBounds(heatmapScaleLowerBound, heatmapScaleUpperBound);
                        _lastHeatmapScaleLowerBound = heatmapScaleLowerBound;
                        _lastHeatmapScaleUpperBound = heatmapScaleUpperBound;
                    }
                }

                GUI.enabled = lowerBoundValid && upperBoundValid;
                if (GUILayout.Button("Force re-generate heatmap"))
                {
                    EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...",
                        -1);
                    _positionHeatmapManager.ForceRegenerate();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = true;

                if (GUILayout.Button("Export raw data"))
                {
                    _positionHeatmapManager.Export();
                }
            }

            GUILayout.EndVertical();
        }
    }
    
}