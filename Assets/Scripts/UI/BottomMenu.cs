using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BottomMenu : MonoBehaviour
{
    [SerializeField]
    private FeatureType featureType;

    public void Awake()
    {
        if (featureType == null){
            throw new System.Exception("bozo: FeatureType not set for bottom Menu");
        }
    }

    public void SetFeatureTypeTitle()
    {
        GetComponentInChildren<TextMeshPro>().text = featureType.UniqueID;
    }
}
