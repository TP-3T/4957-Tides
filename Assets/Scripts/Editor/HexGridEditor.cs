using UnityEngine;
using UnityEditor;
using System.Runtime.ExceptionServices;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

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

    void OnSceneGUI()
    {
        HexGrid t = (HexGrid)target;

        foreach (HexCell hexCell in t.HexCells)
        {
            Handles.Label(hexCell.CellPosition, $"{hexCell.CellCoordinates}");
        }
    }
}
