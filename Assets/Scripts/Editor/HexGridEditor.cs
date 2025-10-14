using System.Runtime.ExceptionServices;
using Hex;
using UnityEditor;
using UnityEngine;

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

        if (!t.DrawDebugLabels)
            return;

        foreach (HexCell hexCell in t.HexCells)
        {
            if (hexCell == null)
                continue;

            Handles.Label(
                t.transform.position + hexCell.CellPosition,
                $"{hexCell.CellCubeCoordinates}"
            );
        }
    }
}
