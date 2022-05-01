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
        GUILayout.BeginVertical("Button");
        Brush.SetType(brushType);
        if(brushType != BrushType.Rivers) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Brush Size");
            Brush.brushSize = EditorGUILayout.IntSlider(Brush.brushSize,0,10);
            GUILayout.EndHorizontal();
        } else Brush.brushSize = 0;
        if(brushType == BrushType.Terrain) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevation");
            Brush.activeElevation = EditorGUILayout.IntSlider(Brush.activeElevation,1,10);
            GUILayout.EndHorizontal();
        } else if (brushType == BrushType.Paint) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Color");
            Brush.activeColor = EditorGUILayout.ColorField(Brush.activeColor);
            GUILayout.EndHorizontal(); 
        } else if (brushType == BrushType.Rivers) {
            GUILayout.BeginVertical();
            GUILayout.Label("Add or remove rivers?");
            Brush.removeRivers = Convert.ToBoolean(
                GUILayout.SelectionGrid(Convert.ToInt32(Brush.removeRivers), new string[2]{"Add","Remove"} ,2)
            );
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
    }
}