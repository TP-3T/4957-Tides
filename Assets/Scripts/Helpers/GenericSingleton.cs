using UnityEngine;

/// <summary>
/// Credit to awesometuts.com/blog/singletons-unity.
/// Use by passing in your class during declaration:
/// eg. public class MyClass: GenericSingleton<MyClass>{...}
/// </summary>
/// <typeparam name="T"> The class to make into a singleton</typeparam>
public class GenericSingleton<T> : MonoBehaviour
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
