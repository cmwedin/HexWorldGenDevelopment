using UnityEngine;
using UnityEditor;
using System;




[CustomEditor(typeof(HexMeshBrush))]
public class HexBrushEditor : Editor {
    private HexMeshBrush Brush;
    private BrushType brushType;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        Brush = (HexMeshBrush)target;

        GUILayout.BeginVertical("Button");
        GUILayout.Label("Brush Type");
        brushType = (BrushType)Enum.ToObject(
            typeof(BrushType),
            GUILayout.SelectionGrid((int)brushType, Enum.GetNames(typeof(BrushType)),3)
        );
        GUILayout.EndVertical();
        Brush.SetType(brushType);
        if(brushType != BrushType.Rivers) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Brush Size");
            Brush.brushSize = EditorGUILayout.IntSlider(Brush.brushSize,1,10);
            GUILayout.EndHorizontal();
        } else Brush.brushSize = 1;
        if(brushType == BrushType.Terrain) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevation");
            Brush.activeElevation = EditorGUILayout.IntSlider(Brush.activeElevation,1,10);
            GUILayout.EndHorizontal();
        }
    }
}