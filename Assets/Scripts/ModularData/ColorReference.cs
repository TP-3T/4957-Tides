using System;
using UnityEngine;

namespace TTT.ModularData
{
    [Serializable]
    public class ColorReference
    {
        [Tooltip("Whether to use an inline (constant) value or an injected SO")]
        public bool UseConstant = true;

        /// <summary>
        /// Defined by a constant/inline value from the inspector
        /// </summary>
        public Color Constant;

        /// <summary>
        /// Defined by a scriptable object injected by the inspector
        /// </summary>
        public ColorVariable Variable;

        public ColorReference() { }

        public ColorReference(Color value)
        {
            UseConstant = true;
            Constant = value;
        }

        /// <summary>
        /// The actual color held by this object
        /// </summary>
        public Color Value => UseConstant ? Constant : Variable.Value;
    }
}
