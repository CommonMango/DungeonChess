using UnityEngine;
/// <summary>
/// 싱글톤 오브젝트 생성용 클래스 
/// </summary>
/// <typeparam name="T"></typeparam>

public class SingleTon <T>: MonoBehaviour where T: MonoBehaviour 
{
    private static T instance;
    
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if(instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected void SingletonInit()
    {
        if(instance == null)
        {
            instance = this as T;

        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        if(transform.parent != null && transform.root != null)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

