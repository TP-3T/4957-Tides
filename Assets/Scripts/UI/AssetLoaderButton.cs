using UnityEngine;
using TTT.GameEvents;

public class AssetLoaderButton : MonoBehaviour
{
    public GameEvent loadAssets;

    public void LoadAssets()
    {
        loadAssets.Raise();
    }

    public void OnAssetsLoaded()
    {

    }
}
