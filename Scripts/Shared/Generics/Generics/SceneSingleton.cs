using UnityEngine;

public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();
            }
            return instance;
        }
    }
    // 새로 생성 없이 존재하는 인스턴스만 반환하는 프로퍼티
    public static T InstanceOrNull
    {
        get {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();
            }
            return instance;
        }
    }
    public virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }
}
