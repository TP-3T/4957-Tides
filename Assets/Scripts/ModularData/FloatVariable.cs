using UnityEngine;

namespace ModularData
{
    [CreateAssetMenu(
        fileName = "FloatVariable",
        menuName = "Scriptable Objects/Modular Data/FloatVariable"
    )]
    public class FloatVariable : ScriptableObject
    {
        public float Value;
    }
}
