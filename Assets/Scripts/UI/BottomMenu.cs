using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BottomMenu : MonoBehaviour
{
    [SerializeField]
    private FeatureType featureType = new FeatureType();

    public void SetFeatureTypeTitle()
    {
        GetComponentInChildren<TextMeshPro>().text = featureType.UniqueID;
    }
}
