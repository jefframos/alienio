using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform parent;

    public ObjectPool(GameObject prefab, int initialSize = 10, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : GameObject.Instantiate(prefab, parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnSpawn();
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnDespawn();
        }

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
