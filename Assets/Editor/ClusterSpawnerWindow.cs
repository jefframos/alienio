using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClusterSpawnerWindow : EditorWindow
{
    // The region in which to spawn the objects.
    private Collider clusterAreaCollider;

    // The collider used to determine the ground height.
    private Collider groundCollider;

    // List of prefabs to spawn.
    private List<GameObject> spawnPrefabs = new List<GameObject>();

    // The total number of objects to spawn.
    private int numberOfSpawns = 10;

    // Maximum number of attempts per spawn.
    private int maxAttempts = 100;

    // Scroll position for the list.
    private Vector2 scrollPos;

    [MenuItem("Tools/Cluster Spawner")]
    public static void ShowWindow()
    {
        GetWindow<ClusterSpawnerWindow>("Cluster Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cluster Spawner", EditorStyles.boldLabel);

        clusterAreaCollider = (Collider)
            EditorGUILayout.ObjectField(
                "Cluster Area Collider",
                clusterAreaCollider,
                typeof(Collider),
                true
            );
        groundCollider = (Collider)
            EditorGUILayout.ObjectField("Ground Collider", groundCollider, typeof(Collider), true);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Spawn Prefabs:");

        // Show an integer field to control the number of prefabs in the list.
        int count = EditorGUILayout.IntField("Number of Prefab Entries", spawnPrefabs.Count);
        // Resize the list accordingly.
        while (count > spawnPrefabs.Count)
            spawnPrefabs.Add(null);
        while (count < spawnPrefabs.Count)
            spawnPrefabs.RemoveAt(spawnPrefabs.Count - 1);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        for (int i = 0; i < spawnPrefabs.Count; i++)
        {
            spawnPrefabs[i] = (GameObject)
                EditorGUILayout.ObjectField(
                    "Prefab " + i,
                    spawnPrefabs[i],
                    typeof(GameObject),
                    false
                );
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        numberOfSpawns = EditorGUILayout.IntField("Number of Spawns", numberOfSpawns);
        maxAttempts = EditorGUILayout.IntField("Max Attempts per Spawn", maxAttempts);

        if (GUILayout.Button("Spawn Cluster"))
        {
            SpawnCluster();
        }
    }

    private void SpawnCluster()
    {
        if (clusterAreaCollider == null)
        {
            Debug.LogError("Please assign a Cluster Area Collider.");
            return;
        }
        if (groundCollider == null)
        {
            Debug.LogError("Please assign a Ground Collider.");
            return;
        }
        if (spawnPrefabs.Count == 0)
        {
            Debug.LogError("Please assign at least one prefab to spawn.");
            return;
        }
        if (numberOfSpawns <= 0)
        {
            Debug.LogError("Number of Spawns must be greater than zero.");
            return;
        }

        // Create a new parent GameObject called "Cluster"
        GameObject clusterGO = new GameObject("Cluster");

        // Get the bounds of the cluster area.
        Bounds areaBounds = clusterAreaCollider.bounds;
        int countSpawned = 0;

        for (int i = 0; i < numberOfSpawns; i++)
        {
            bool spawnedSuccessfully = false;
            Vector3 spawnPos = Vector3.zero;

            // Randomly select one prefab from the list.
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];
            if (prefab == null)
                continue;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Pick a random point within the area bounds (X and Z; Y will be determined by ground).
                Vector3 randomPoint = new Vector3(
                    Random.Range(areaBounds.min.x, areaBounds.max.x),
                    areaBounds.center.y,
                    Random.Range(areaBounds.min.z, areaBounds.max.z)
                );

                // Check if the point is actually inside the collider using ClosestPoint.
                Vector3 closest = clusterAreaCollider.ClosestPoint(randomPoint);
                if ((closest - randomPoint).sqrMagnitude > 0.001f)
                    continue; // Not inside.

                // Raycast downward from above the area to snap to ground.
                Ray ray = new Ray(
                    new Vector3(randomPoint.x, areaBounds.max.y + 10f, randomPoint.z),
                    Vector3.down
                );
                RaycastHit hit;
                if (!groundCollider.Raycast(ray, out hit, 100f))
                    continue; // No ground found.

                spawnPos = hit.point;

                // Determine a sphere radius based on the prefab's collider.
                float checkRadius = 0.5f;
                Collider prefabCol = prefab.GetComponent<Collider>();
                if (prefabCol != null)
                {
                    // Use a slightly smaller sphere than the prefab's bounds extents.
                    checkRadius = prefabCol.bounds.extents.magnitude * 0.8f;
                }

                // Check for overlap with already spawned objects.
                Collider[] overlaps = Physics.OverlapSphere(spawnPos, checkRadius);
                bool overlapFound = false;
                foreach (Collider col in overlaps)
                {
                    // If the collider belongs to an object that is already a child of our cluster, consider it overlapping.
                    if (col.transform.IsChildOf(clusterGO.transform))
                    {
                        overlapFound = true;
                        break;
                    }
                }

                if (!overlapFound)
                {
                    spawnedSuccessfully = true;
                    break;
                }
            }

            if (spawnedSuccessfully)
            {
                GameObject instance = (GameObject)
                    PrefabUtility.InstantiatePrefab(prefab, clusterGO.transform);
                instance.transform.position = spawnPos;
                countSpawned++;
            }
            else
            {
                Debug.LogWarning(
                    "Could not spawn a prefab without overlap after " + maxAttempts + " attempts."
                );
            }
        }

        Debug.Log($"Spawned {countSpawned} objects into cluster '{clusterGO.name}'.");
    }
}
