using UnityEngine;

namespace ModularData
{
    [CreateAssetMenu(
        fileName = "IntVariable",
        menuName = "Scriptable Objects/Modular Data/IntVariable"
    )]
    public class IntVariable : ScriptableObject
    {
        public int Value;
    }
}
