using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sea))]
public class SeaEditor : Editor
{
    /// <summary>
    /// Button to cause sea level rise.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sea s = (Sea)target;

        if (GUILayout.Button("Sea level rise"))
        {
            s.RaiseSea();
        }
    }
}