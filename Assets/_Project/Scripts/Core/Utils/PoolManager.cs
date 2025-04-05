using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolConfig
{
    public string id;
    public GameObject prefab;
    public int initialSize = 10;
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField]
    private List<PoolConfig> poolConfigs = new();

    private Dictionary<string, ObjectPool> pools = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            if (!pools.ContainsKey(config.id) && config.prefab != null)
            {
                pools[config.id] = new ObjectPool(
                    config.prefab,
                    config.initialSize,
                    this.transform
                );
            }
        }
    }

    public GameObject Spawn(string id, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(id))
        {
            Debug.LogWarning($"No pool with ID {id} exists.");
            return null;
        }

        return pools[id].Get(position, rotation);
    }

    public void Despawn(string id, GameObject obj)
    {
        if (pools.ContainsKey(id))
        {
            pools[id].Return(obj);
        }
        else
        {
            obj.SetActive(false);
        }
    }
}
