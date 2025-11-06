using TTT.Managers;
using TTT.DataClasses.HexData;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // HexGrid t = (HexGrid)target;

        // if (GUILayout.Button("Clear Map"))
        // {
        //     t.ClearMap();
        // }

        // if (GUILayout.Button("Build Map"))
        // {
        //     t.BuildMap();
        // }
    } // THIS WILL BE FOR THE MESH INSTANCE, CHANGGE

    void OnSceneGUI()
    {
        MapManager t = (MapManager)target;

        if (!t.DrawDebugLabels)
            return;

        foreach (HexCell hexCell in t.HexCells)
        {
            Handles.Label(
                Vector3.zero + hexCell.CellPosition,
                $"{hexCell.CellCubeCoordinates}"
            );
        }
    }
}
