using System;
using UnityEngine;

namespace ModularData
{
    [Serializable]
    public class FloatReference
    {
        [Tooltip("Whether to use an inline (constant) value or an injected SO")]
        public bool UseConstant = true;

        /// <summary>
        /// Defined by a constant/inline value from the inspector
        /// </summary>
        public float Constant;

        /// <summary>
        /// Defined by a scriptable object injected by the inspector
        /// </summary>
        public FloatVariable Variable;

        public FloatReference() { }

        public FloatReference(float value)
        {
            UseConstant = true;
            Constant = value;
        }

        /// <summary>
        /// The actual float value held by this object
        /// </summary>
        public float Value => UseConstant ? Constant : Variable.Value;
    }
}
