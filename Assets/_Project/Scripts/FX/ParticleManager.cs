using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleEntry
{
    public string id;
    public GameObject prefab;
}

public class ParticleManager : MonoBehaviour
{
    private static ParticleManager instance;
    public static ParticleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ParticleManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ParticleManager");
                    instance = go.AddComponent<ParticleManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public ParticleEntry[] particleEntries;

    private Dictionary<string, GameObject> particleDict = new Dictionary<string, GameObject>();

    // Pool per id.
    private Dictionary<string, Queue<GameObject>> poolDict =
        new Dictionary<string, Queue<GameObject>>();

    private Transform poolParent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Build lookup dictionaries.
        foreach (ParticleEntry entry in particleEntries)
        {
            if (entry != null && entry.prefab != null && !particleDict.ContainsKey(entry.id))
            {
                particleDict.Add(entry.id, entry.prefab);
                poolDict.Add(entry.id, new Queue<GameObject>());
            }
        }
        // Create a parent to hold pooled objects.
        GameObject poolGO = new GameObject("ParticlePool");
        poolGO.transform.parent = transform;
        poolParent = poolGO.transform;
    }

    public GameObject PlayParticle(string id, Vector3 position, Transform parent = null)
    {
        if (!particleDict.ContainsKey(id))
        {
            Debug.LogWarning("Particle id not found: " + id);
            return null;
        }

        GameObject particleGO = GetPooledParticle(id);
        // Set parent if provided.
        if (parent != null)
        {
            particleGO.transform.SetParent(parent, false);
            particleGO.transform.localPosition = position;
        }
        else
        {
            particleGO.transform.SetParent(null);
            particleGO.transform.position = position;
        }
        particleGO.SetActive(true);

        // Ensure it has a ParticlePoolItem; add it if missing.
        ParticlePoolItem poolItem = particleGO.GetComponent<ParticlePoolItem>();
        if (poolItem == null)
        {
            poolItem = particleGO.AddComponent<ParticlePoolItem>();
        }
        poolItem.poolId = id;
        poolItem.PlayAndReturn();

        return particleGO;
    }

    private GameObject GetPooledParticle(string id)
    {
        Queue<GameObject> pool = poolDict[id];
        if (pool.Count > 0)
        {
            GameObject go = pool.Dequeue();
            go.SetActive(true);
            return go;
        }
        else
        {
            GameObject newGO = Instantiate(particleDict[id]);
            newGO.name = particleDict[id].name;
            return newGO;
        }
    }

    public void ReturnToPool(string id, GameObject particleObject)
    {
        particleObject.SetActive(false);
        particleObject.transform.SetParent(poolParent);
        poolDict[id].Enqueue(particleObject);
    }
}
