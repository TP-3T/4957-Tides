using UnityEngine;

/// <summary>
/// A test GameObject to generate some resources
/// </summary>
public class ResourceGenerator : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Points to the resource count")]
    private IntReference resourceCount;

    // Update is called once per frame
    void Update()
    {
        Generate(1);
    }

    private void Generate(int amount)
    {
        resourceCount.Variable.ApplyChange(amount);
    }
}
