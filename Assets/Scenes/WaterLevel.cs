using UnityEngine;
using UnityEngine.Events;

public class WaterLevel : MonoBehaviour
{
    [SerializeField]
    private int _maxLevel = 100;
    private int _wl;

    public int MaxWaterLevel => _maxLevel;

    public int WaterLevelValue
    {
        get => _wl;
        set
        {
            var isRised = value > _wl;
            _wl = Mathf.Clamp(value, 0, _maxLevel);
            if (isRised)
            {
                OnWaterLevelRised?.Invoke(_wl);
            }
            else
            {
                OnWaterLevelLowered?.Invoke(_wl);
            }
        }
    }

    public UnityEvent<int> OnWaterLevelRised;
    public UnityEvent<int> OnWaterLevelLowered;

    private void Start()
    {
        _wl = 0;
    }

    public void RiseWaterLevel(int amount) => WaterLevelValue += amount;
    public void LowerWaterLevel(int amount) => WaterLevelValue -= amount;
    public void SetWaterLevel(int amount) => WaterLevelValue = amount;
}
