using System;
using UnityEngine;

namespace TTT.DataClasses
{
    [Serializable]
    public class Amount<T>
    {
        /// <summary>
        /// The thing you're counting.
        /// </summary>
        [field: Tooltip("The thing you're counting")]
        [field: SerializeField]
        public T Thing { get; protected set; }

        /// <summary>
        /// How much of the thing you have.
        /// </summary>
        [field: Tooltip("How much of the thing you have.")]
        [field: SerializeField]
        public int Count { get; protected set; }
    }
}
