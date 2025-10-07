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
    [Tooltip("The resource to track")]
    private PlayerResource resource;

    /// <summary>
    /// Updates textbox
    /// </summary>
    private void UpdateUI()
    {
        tmpText.text = $"{resource.Name}: {resource.Amount}";
    }

    void Start()
    {
        resource.Set(0);
    }

    void Update()
    {
        UpdateUI();
    }
}
