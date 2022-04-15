using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: add generation progress bar inside Unity Game GUI
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

    public void OnNewRecordLoaded()
    {
        if (_displayHeatmap)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...", -1);
#endif
            _positionHeatmapManager.SetScaleBounds(0f, 1f);
            _positionHeatmapManager.ForceRegenerate();
            _heatmapScaleLowerBoundStr = "0";
            _heatmapScaleUpperBoundStr = $"{_positionHeatmapManager.GetMaxDuration():0.###}";
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }
    }
    
    private void DisplayToolbarIcon(Texture icon, int x, int y, bool blink = false, bool turnAround = false)
    {
        var matrixBackup = GUI.matrix;
        GUI.color = Color.white;
        var size = (int)_stylesManager.tabButtonsStyle.lineHeight;
        var pivotPoint = new Vector2(x + size / 2, y + size / 2);
        if (turnAround) GUIUtility.RotateAroundPivot(Time.time % 360, pivotPoint);
        GUI.Box(new Rect(x, y, size, size), icon, GUIStyle.none);
        if (turnAround) GUI.matrix = matrixBackup;
        GUI.color = Color.white;
    }
    
    public void OnGui()
    {
        var showPositionHeatmap = GUILayout.Toggle(_displayHeatmap, "Position heatmap", _stylesManager.toggleStyle);

        if (showPositionHeatmap != _displayHeatmap)
        {
            _displayHeatmap = showPositionHeatmap;

#if UNITY_EDITOR
            if (showPositionHeatmap)
                EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...", -1);
#endif
            _positionHeatmapManager.TogglePositionHeatmap(showPositionHeatmap);
#if UNITY_EDITOR
            if (showPositionHeatmap) EditorUtility.ClearProgressBar();
#endif

            if (showPositionHeatmap)
            {
                _heatmapScaleLowerBoundStr = "0";
                _heatmapScaleUpperBoundStr = $"{_positionHeatmapManager.GetMaxDuration():0.###}";
            }
        }

        if (showPositionHeatmap)
        {
            GUILayout.BeginVertical("box");

            var showAggregatedHeatmap = GUILayout.Toggle(_displayAggregatedHeatmap, "Aggregate all records",
                _stylesManager.toggleStyle);

            if (showAggregatedHeatmap != _displayAggregatedHeatmap)
            {
                _displayAggregatedHeatmap = showAggregatedHeatmap;

#if UNITY_EDITOR
                if (showAggregatedHeatmap)
                    EditorUtility.DisplayProgressBar("Generating aggregated heatmap",
                        "Generating aggregated position heatmap texture...", -1);
                else
                    EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...",
                        -1);
#endif

                _positionHeatmapManager.ToggleAggregatedPositionHeatmap(showAggregatedHeatmap);

#if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
#endif
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
#if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Generating aggregated heatmap",
                        "Generating aggregated position heatmap texture...", -1);
#endif
                    _positionHeatmapManager.ForceRegenerate();
#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
                }
            }
            else
            {
                var heatmapScaleLowerBoundStyle = new GUIStyle(GUI.skin.textField);
                var heatmapScaleUpperBoundStyle = new GUIStyle(GUI.skin.textField);

                var maxDuration = Math.Round(_positionHeatmapManager.GetMaxDuration(), 3);

                var lowerBoundValid = float.TryParse(_heatmapScaleLowerBoundStr, out var heatmapScaleLowerBound) &&
                                      heatmapScaleLowerBound >= 0 && heatmapScaleLowerBound <= maxDuration + 10e-4;
                var upperBoundValid = float.TryParse(_heatmapScaleUpperBoundStr, out var heatmapScaleUpperBound)
                                      && heatmapScaleUpperBound <= maxDuration + 10e-4
                                      && heatmapScaleUpperBound > heatmapScaleLowerBound;

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
#if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar("Generating heatmap", "Generating position heatmap texture...",
                        -1);
#endif
                    _positionHeatmapManager.ForceRegenerate();
#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
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