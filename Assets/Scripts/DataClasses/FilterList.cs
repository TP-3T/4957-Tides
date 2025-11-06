using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.DataClasses
{
    [Serializable]
    public class FilterList<T>
    {
        /// <summary>
        /// Whether this is a whitelist or a blacklist.
        /// </summary>
        [field: Tooltip("True = blacklist, False = whitelist.")]
        [field: SerializeField]
        public FilterListMode Mode { get; private set; }

        /// <summary>
        /// The elements to either allow or disallow.
        /// </summary>
        [field: Tooltip("The elements to either allow or disallow.")]
        [field: SerializeField]
        public List<T> List { get; private set; }
    }

    public enum FilterListMode
    {
        WHITELIST,
        BLACKLIST,
    }
}
