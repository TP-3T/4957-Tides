using UnityEngine;
using UnityEditor;

public class SetupOcclusionCulling : EditorWindow
{
    [MenuItem("Tools/Mark Buildings as Static Occluders")]
    public static void MarkBuildingsAsStaticOccluders()
    {
        // Find all building objects in the scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Check if it's a building (has a renderer and is not the hex mesh)
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && !obj.name.Contains("Hex") && !obj.name.Contains("Sea"))
            {
                // Mark as static for occlusion culling
                GameObjectUtility.SetStaticEditorFlags(obj, 
                    StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
                count++;
            }
        }

        Debug.Log($"Marked {count} buildings as static occluders");
    }

    [MenuItem("Tools/Bake Occlusion Culling")]
    public static void BakeOcclusionCulling()
    {
        // Open the occlusion culling window
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
        Debug.Log("Occlusion Culling window opened. Click 'Bake' button to generate occlusion data.");
    }
}
