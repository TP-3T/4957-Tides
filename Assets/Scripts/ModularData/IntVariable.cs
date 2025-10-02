using UnityEngine;

[CreateAssetMenu(
    fileName = "IntVariable",
    menuName = "Scriptable Objects/Modular Data/IntVariable"
)]
public class IntVariable : ScriptableObject
{
    public int Value;

    public void ApplyChange(int amount)
    {
        Value += amount;
    }

    public void ApplyChange(IntVariable amount)
    {
        Value += amount.Value;
    }
}
