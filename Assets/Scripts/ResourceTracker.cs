using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTT.DataClasses.PlayerResources;
using UnityEngine;

public class ResourceTracker : MonoBehaviour
{
    [SerializeField]
    private List<PlayerResourceUI> resourceTextFields;

    void Start()
    {
        bool anyNull = resourceTextFields.Any(res =>
        {
            return res.Resource == null || res.TextField == null;
        });

        if (anyNull)
        {
            Debug.LogError("ResourceTracker Component is missing references.");
        }
    }

    /// <summary>
    /// Update the UI
    /// </summary>
    void Update() => resourceTextFields.ForEach(res => res.UpdateCount());
}

[Serializable]
class PlayerResourceUI
{
    [field: SerializeField]
    public PlayerResource Resource { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI TextField { get; private set; }

    private int cachedAmount = 0;

    public void UpdateCount()
    {
        int resourceCount = Resource.AmountOwned;

        // only update UI when needed
        if (cachedAmount != resourceCount)
        {
            TextField.text = resourceCount.ToString();
            cachedAmount = resourceCount;
        }
    }
}
