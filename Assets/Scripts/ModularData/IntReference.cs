using System;
using UnityEngine;

[Serializable]
public class IntReference
{
    [Tooltip("Whether to use an inline (constant) value or an injected SO")]
    public bool UseConstant = true;

    /// <summary>
    /// Defined by a constant/inline value from the inspector
    /// </summary>
    public int Constant;

    /// <summary>
    /// Defined by a scriptable object injected by the inspector
    /// </summary>
    public IntVariable Variable;

    public IntReference() { }

    public IntReference(int value)
    {
        UseConstant = true;
        Constant = value;
    }

    /// <summary>
    /// The actual float value held by this object
    /// </summary>
    public int Value => UseConstant ? Constant : Variable.Value;
}
