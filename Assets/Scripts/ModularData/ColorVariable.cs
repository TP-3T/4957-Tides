using UnityEngine;

namespace TTT.ModularData
{
    [CreateAssetMenu(
        fileName = "ColorVariable",
        menuName = "Scriptable Objects/Modular Data/ColorVariable"
    )]
    public class ColorVariable : ScriptableObject
    {
        [Tooltip("RGBA color")]
        public Color Value;
    }
}
