using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        HexGrid t = (HexGrid)target;

        if (GUILayout.Button("Clear Map"))
        {
            t.ClearMap();
        }

        if (GUILayout.Button("Build Map"))
        {
            t.BuildMap();
        }
    }
}
