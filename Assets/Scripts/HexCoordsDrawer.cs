using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoords))]
public class HexCoordsDrawer : PropertyDrawer {
    public override void OnGUI (
        Rect pos, SerializedProperty property, GUIContent label
    ) {
        HexCoords coords = new HexCoords(
            property.FindPropertyRelative("x").intValue,
            property.FindPropertyRelative("z").intValue );
        pos = EditorGUI.PrefixLabel(pos, label);
        GUI.Label(pos, coords.ToString());
    }
}
