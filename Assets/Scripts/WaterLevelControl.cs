using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterLevelControl : MonoBehaviour
{
    [SerializeField]
    private WaterLevel _waterLevel;

    [SerializeField]
    private RectTransform _barRect;

    [SerializeField]
    private RectMask2D _mask;

    [SerializeField]
    private TextMeshProUGUI _waterLevelText;

    [Tooltip("The amount to increase or decrease the water level by for testing.")]
    private int _testChangeAmount = 1;

    private float _maxBarHeight;
    private float _initialTopMaskPadding;

    void Start()
    {
        _initialTopMaskPadding = _mask.padding.w;
        _maxBarHeight = _barRect.rect.height - _initialTopMaskPadding;
        _waterLevelText.SetText($"{_waterLevel.WaterLevelValue}/{_waterLevel.MaxWaterLevel}");
        SetValue(_waterLevel.WaterLevelValue);
    }

    public void SetValue(int newValue)
    {
        // var targetHeight = newValue * _maxTopHeight / _waterLevel.MaxWaterLevel;
        // var newTopPadding = _maxTopHeight + _initialTopPadding - targetHeight;
        // var padding = _mask.padding;
        // padding.y = newTopPadding;
        // _mask.padding = padding;
        // _waterLevelText.SetText($"{_waterLevel.WaterLevelValue}/{_waterLevel.MaxWaterLevel}");
        newValue = Mathf.Clamp(newValue, 0, _waterLevel.MaxWaterLevel);

        float ratio = (float) newValue / _waterLevel.MaxWaterLevel;
        float targetFilledHeight = ratio * _maxBarHeight;
        float newTopPadding = _maxBarHeight - targetFilledHeight + _initialTopMaskPadding;

        newTopPadding = Mathf.Clamp(
            newTopPadding,
            _initialTopMaskPadding,
            _maxBarHeight + _initialTopMaskPadding
        );

        var padding = _mask.padding;
        padding.w = newTopPadding;
        _mask.padding = padding;

        _waterLevelText.text = $"{newValue}/{_waterLevel.MaxWaterLevel}";
    }

    void Update()
    {
        TestInput();
    }

    private void TestInput()
    {
        bool changed = false;

        if (Input.GetKeyDown(KeyCode.UpArrow) ||
        Input.GetKeyDown(KeyCode.W))
        {
            _waterLevel.RiseWaterLevel(_testChangeAmount);
            changed = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) ||
        Input.GetKeyDown(KeyCode.S))
        {
            _waterLevel.LowerWaterLevel(_testChangeAmount);
            changed = true;
        }

        if (changed)
        {
            SetValue(_waterLevel.WaterLevelValue);
            Debug.Log($"Water Level changed to {_waterLevel.WaterLevelValue}.");
        }
    }
}
