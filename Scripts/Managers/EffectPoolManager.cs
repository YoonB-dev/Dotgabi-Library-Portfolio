using System.Collections.Generic;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public EnumTypes.EffectType type;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<EnumTypes.EffectType, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<EnumTypes.EffectType, Queue<GameObject>>();

        // 게임 시작 시 미리 생성 (동기적 부하를 로딩 시점으로 분산)
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.type, objectPool);
        }
    }

    // 이펙트 가져오기
    public GameObject SpawnFromPool(EnumTypes.EffectType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(type)) return null;

        GameObject objectToSpawn = poolDictionary[type].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 사용 후 다시 큐에 넣음 (재사용)
        poolDictionary[type].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
