using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.Helpers
{
    //Made by CB

    /// <summary>
    /// Class to hold references to objects that can be used for dependency injection.
    /// Didn't work quite as expected at this level, but I want to keep playing with it, so It's staying here for now.
    /// </summary>
    public class ObjectResolver
    {
        private readonly Dictionary<Type, object> _instancePerMapType = new();

        public void RegisterInstance<T>(T instance)
        {
            _instancePerMapType[typeof(T)] = instance;
        }

        public void UnregisterType<T>()
        {
            _instancePerMapType.Remove(typeof(T));
        }

        public T Resolve<T>()
        {
            Type instanceType = typeof(T);
            if (_instancePerMapType.TryGetValue(instanceType, out var instance))
            {
                return (T)instance;
            }

            Debug.LogError($"Could not resolve type {instanceType}");
            return default;
        }
    }
}
