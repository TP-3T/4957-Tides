using UnityEngine;
using TTT.GameEvents;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class AssetLoaderButton : MonoBehaviour
{
    public GameEvent loadAssets;

    [SerializeField]
    private Dropdown _mapDropdown;

    [SerializeField]
    private List<TextAsset> _maps = new List<TextAsset>(3);

    public void LoadAssets()
    {
        Debug.Log(_mapDropdown.value);
        Debug.Log(_maps[_mapDropdown.value].name);
        loadAssets.Raise(new NewMapEventArgs { DataFile = _maps[_mapDropdown.value]});
    }

    public void OnAssetsLoaded()
    {

    }
}
