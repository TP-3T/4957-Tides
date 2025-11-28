using TMPro;
using TTT.GameEvents;
using TTT.Managers;
using UnityEngine;
using UnityEngine.UI;

public class WaterLevelControl : MonoBehaviour
{
    private float _initialWaterLevel;

    private float _maxMapHeight;

    [SerializeField]
    private RectTransform _barRect;

    [SerializeField]
    private RectMask2D _mask;

    // [SerializeField]
    // private TextMeshProUGUI _waterLevelText;

    // [Tooltip(
    //     "The amount to increase or decrease the water level by for testing."
    // )]
    // private int _testChangeAmount = 1;

    private float _maxBarHeight;
    private float _initialTopMaskPadding;

    void Start()
    {
        _initialTopMaskPadding = _mask.padding.w;
        _maxBarHeight = _barRect.rect.height - _initialTopMaskPadding;
        _initialWaterLevel = 0;
    }

    void OnEnable()
    {
        MapManager.Instance.SeaLevel.OnValueChanged += OnSeaLevelChanged;
    }

    void OnDisable()
    {
        MapManager.Instance.SeaLevel.OnValueChanged -= OnSeaLevelChanged;
    }

    private void OnSeaLevelChanged(float previousValue, float newValue)
    {
        SetValue(Mathf.RoundToInt(newValue));
    }

    public void OnNewMapFinish(Object eventArgs)
    {
        NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;

        if (args.WasSuccessful)
        {
            _maxMapHeight = args.MaxMapHeight;
            _initialWaterLevel = args.SeaLevel;

            Debug.Log(
                $"WaterLevelControl: Map loaded - MaxMapHeight={_maxMapHeight}, SeaLevel={_initialWaterLevel}"
            );

            SetValue(Mathf.RoundToInt(_initialWaterLevel));
        }
    }

    public void SetValue(int newValue)
    {
        float ratio = newValue / _maxMapHeight;

        float targetFilledHeight = ratio * _maxBarHeight;
        float newTopPadding =
            _maxBarHeight - targetFilledHeight + _initialTopMaskPadding;

        newTopPadding = Mathf.Clamp(
            newTopPadding,
            _initialTopMaskPadding,
            _maxBarHeight + _initialTopMaskPadding
        );

        var padding = _mask.padding;
        padding.w = newTopPadding;
        _mask.padding = padding;

        // _waterLevelText.text = $"{newValue}/{_waterLevel.MaxWaterLevel}";
    }

    void Update()
    {
        // TestInput();
    }

    // private void TestInput()
    // {
    //     bool changed = false;

    //     if (Input.GetKeyDown(KeyCode.T))
    //     {
    //         _waterLevel.RiseWaterLevel(_testChangeAmount);
    //         changed = true;
    //     }
    //     else if (Input.GetKeyDown(KeyCode.H))
    //     {
    //         _waterLevel.LowerWaterLevel(_testChangeAmount);
    //         changed = true;
    //     }

    //     if (changed)
    //     {
    //         SetValue(_waterLevel.WaterLevelValue);
    //         Debug.Log($"Water Level changed to {_waterLevel.WaterLevelValue}.");
    //     }
    // }
}
