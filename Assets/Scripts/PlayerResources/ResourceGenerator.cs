using UnityEngine;

/// <summary>
/// A test GameObject to generate some resources
/// </summary>
public class ResourceGenerator : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The resource to generate")]
    private PlayerResource resource;

    // Update is called once per frame
    void Update()
    {
        Generate(1);
    }

    private void Generate(int amount)
    {
        resource.ApplyChange(1);
    }
}
