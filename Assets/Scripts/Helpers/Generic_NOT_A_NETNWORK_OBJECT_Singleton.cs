using Unity.Netcode;
using UnityEngine;

namespace TTT.Helpers
{
    /// <summary>
    /// guys pls, there's already a generic network singleton, you don't need to keep turning the normal one into network stuff
    /// </summary>
    public class Generic_NOT_A_NETWORK_OBJECT_Singleton<T> : MonoBehaviour
        where T : Component
    {
        // create a private reference to T instance
        private static T _instance;

        public static T Instance
        {
            get
            {
                // if instance is null
                if (_instance == null)
                {
                    // See if any generic instance already exists
                    _instance = Object.FindAnyObjectByType<T>();

                    // if it's null again create a new object
                    // and attach the generic instance
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            // create the instance
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
