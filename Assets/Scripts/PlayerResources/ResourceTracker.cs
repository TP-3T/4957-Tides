using TMPro;
using UnityEngine;

/// <summary>
/// Tracks the resource count and updates UI elements
/// </summary>
public class ResourceTracker : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The textbox to update with the resource count")]
    private TextMeshProUGUI tmpText;

    [SerializeField]
    [Tooltip("Points to the resource count")]
    private IntReference resourceCount;

    /// <summary>
    /// Updates textbox
    /// </summary>
    private void UpdateUI()
    {
        tmpText.text = $"Resources: {resourceCount.Value}";
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }
}
