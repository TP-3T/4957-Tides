using System.Collections.Generic;
using UnityEngine;

namespace TTT.ModularData
{
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        protected List<T> Items = new();

        /// <summary>
        /// Returns a copy of the items in this set.
        /// </summary>
        public T[] GetItems()
        {
            return Items.ToArray();
        }

        public virtual bool Add(T thing)
        {
            if (!Items.Contains(thing))
            {
                Items.Add(thing);
                return true;
            }

            return false;
        }

        public virtual bool Remove(T thing)
        {
            if (Items.Contains(thing))
            {
                Items.Remove(thing);
                return true;
            }

            return false;
        }

        public virtual bool RemoveAt(int index)
        {
            if (index < Items.Count)
            {
                Items.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}
