using GameEvents;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom GUI code for GameEvents from Ryan Hipple (Unite 2017)
/// </summary>
[CustomEditor(typeof(GameEvent), editorForChildClasses: true)]
public class EventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        GameEvent e = target as GameEvent;
        if (GUILayout.Button("Raise"))
            e.Raise();
    }
}
