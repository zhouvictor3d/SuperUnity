using UnityEngine;

/// <summary>
/// MonoSingleton
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private const string PARENT_NAME = "_MonoSingletonRoot";

    private static T instance = null;

    public static T Instance
    {
        get
        {
            GameObject parent = GameObject.Find(PARENT_NAME);
            if (parent == null)
            {
                parent = new GameObject();
                parent.name = PARENT_NAME;
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(parent);
                }
#else
                    DontDestroyOnLoad(parent);
#endif

            }

            if (instance == null)
            {
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                {
                    instance = new GameObject("_" + typeof(T).Name).AddComponent<T>();
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(instance);
                    }
#else
                        DontDestroyOnLoad(instance);
#endif

                }
                if (instance != null && instance.transform.parent == null)
                {
                    instance.transform.SetParent(parent.transform);
                }
            }
            return instance;
        }
    }

    void OnApplicationQuit()
    {
        if (instance != null)
        {
            instance = null;
        }
    }
}
