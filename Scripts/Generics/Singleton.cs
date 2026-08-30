using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T instance;
    public static T Instance
    {
        get {
            instance ??= new T();
            return instance;
        }
    }
}
