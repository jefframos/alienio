using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SwallowableCreatorEditor : EditorWindow
{
    // You can change these defaults in the window.
    private string targetTag = "Swallowable";
    private int targetLayer = 12;

    // The subfolder (inside Assets) where new prefabs will be created.
    private string prefabSubfolder = "Assets/Prefabs/Subfolder";

    [MenuItem("Swallow/Swallowable Creator")]
    public static void ShowWindow()
    {
        SwallowableCreatorEditor window = GetWindow<SwallowableCreatorEditor>(
            "Swallowable Creator"
        );
        // Optionally, position the window to the bottom right of the main window.
        Rect main = EditorGUIUtility.GetMainWindowPosition();
        window.position = new Rect(main.x + main.width - 250, main.y + main.height - 150, 240, 280);
    }

    private void OnGUI()
    {
        GUILayout.Label("Swallowable Creator", EditorStyles.boldLabel);
        targetTag = EditorGUILayout.TextField("Tag", targetTag);
        targetLayer = EditorGUILayout.IntField("Layer", targetLayer);

        if (GUILayout.Button("Make Selected Object Swallowable Box"))
        {
            CreateSwallowableBox();
            CreateSwallowable();
        }

        if (GUILayout.Button("Make Selected Object Swallowable Capsule"))
        {
            CreateSwallowableCapsule();
            CreateSwallowable();
        }

        if (GUILayout.Button("Select All Same Objects"))
        {
            SelectAllSamePrefabObjects();
        }

        if (GUILayout.Button("Make Selected Swallowables Kinematic"))
        {
            MakeSelectedSwallowablesKinematic();
        }
        if (GUILayout.Button("Make Selected Swallowables NonKinematic"))
        {
            MakeSelectedSwallowablesNonKinematic();
        }

        if (GUILayout.Button("Create Prefabs & Remove from Scene"))
        {
            CreatePrefabsFromSelectionAndRemoveFromScene();
        }
    }

    private void CreateSwallowableCapsule()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        foreach (var selected in selectedObjects)
        {
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "No GameObject selected.", "OK");
                return;
            }

            CapsuleCollider capsuleCollider = selected.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = selected.AddComponent<CapsuleCollider>();
            }

            MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length > 0)
            {
                // Start with the first mesh's bounds (in its local space)
                Bounds combinedBounds = meshFilters[0].sharedMesh.bounds;
                // Encapsulate the bounds of all other meshes
                for (int i = 1; i < meshFilters.Length; i++)
                {
                    combinedBounds.Encapsulate(meshFilters[i].sharedMesh.bounds);
                }

                // Update the CapsuleCollider's properties:
                capsuleCollider.center = combinedBounds.center;
                // Height is the size along the Y-axis (assuming the capsule is oriented along Y)
                capsuleCollider.height = combinedBounds.size.y;
                // Radius is half the maximum of the X or Z extents.
                capsuleCollider.radius =
                    Mathf.Max(combinedBounds.size.x, combinedBounds.size.z) * 0.5f;
                // Ensure the capsule is oriented along the Y axis (direction 1).
                capsuleCollider.direction = 1;
            }

            if (selected.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = selected.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }

    private void CreateSwallowableBox()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        foreach (var selected in selectedObjects)
        {
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "No GameObject selected.", "OK");
                return;
            }

            // Add or update BoxCollider wrapping the meshes in the object and its children.
            BoxCollider boxCollider = selected.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = selected.AddComponent<BoxCollider>();
            }

            MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length > 0)
            {
                // Start with the first mesh's bounds.
                Bounds combinedBounds = meshFilters[0].sharedMesh.bounds;
                // Encapsulate all other meshes' bounds.
                for (int i = 1; i < meshFilters.Length; i++)
                {
                    combinedBounds.Encapsulate(meshFilters[i].sharedMesh.bounds);
                }
                boxCollider.center = combinedBounds.center;
                boxCollider.size = combinedBounds.size;
            }

            // Add a Rigidbody if missing.
            if (selected.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = selected.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }
    }

    private void CreateSwallowable()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        foreach (var selected in selectedObjects)
        {
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "No GameObject selected.", "OK");
                return;
            }

            Undo.RecordObject(selected, "Make Swallowable");

            // Set tag and layer.
            selected.tag = targetTag;
            selected.layer = targetLayer;

            // Add SwalllowableEntity component if not already present.
            if (selected.GetComponent<SwalllowableEntity>() == null)
            {
                selected.AddComponent<SwalllowableEntity>();
            }

            // If the object is part of a prefab instance, apply the changes to the prefab.
            if (PrefabUtility.IsPartOfPrefabInstance(selected))
            {
                PrefabUtility.ApplyPrefabInstance(selected, InteractionMode.AutomatedAction);
                EditorUtility.DisplayDialog(
                    "Swallowable Creator",
                    $"{selected.name} is now set as a swallowable object and saved as prefab.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Swallowable Creator",
                    $"{selected.name} is now set as a swallowable object.",
                    "OK"
                );
            }
        }
    }

    private void SelectAllSamePrefabObjects()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "No GameObject selected.", "OK");
            return;
        }

        // Get the prefab asset corresponding to the selected object.
        GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(selected);
        if (prefabAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object is not a prefab instance.", "OK");
            return;
        }

        // Gather all GameObjects in the scene that come from the same prefab.
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> matchingObjects = new List<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            GameObject objPrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (objPrefab == prefabAsset)
            {
                matchingObjects.Add(obj);
            }
        }

        if (matchingObjects.Count > 0)
        {
            Selection.objects = matchingObjects.ToArray();
            EditorUtility.DisplayDialog(
                "Select All Same Prefab Objects",
                $"Selected {matchingObjects.Count} objects from the same prefab.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Select All Same Prefab Objects",
                "No matching prefab instances found.",
                "OK"
            );
        }
    }

    private void MakeSelectedSwallowablesKinematic()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        int count = 0;
        foreach (GameObject go in selectedObjects)
        {
            // Check if the object is "swallowable" by tag or by having the component.
            if (go.CompareTag(targetTag) || go.GetComponent<SwalllowableEntity>() != null)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    Undo.RecordObject(rb, "Make Kinematic");
                    rb.isKinematic = true;
                    count++;

                    // If this object is part of a prefab instance, apply the change.
                    if (PrefabUtility.IsPartOfPrefabInstance(go))
                    {
                        PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
                    }
                }
            }
        }

        EditorUtility.DisplayDialog(
            "Make Kinematic",
            $"{count} swallowable objects were made kinematic.",
            "OK"
        );
    }

    private void MakeSelectedSwallowablesNonKinematic()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        int count = 0;
        foreach (GameObject go in selectedObjects)
        {
            // Check if the object is "swallowable" by tag or by having the component.
            if (go.CompareTag(targetTag) || go.GetComponent<SwalllowableEntity>() != null)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null && rb.isKinematic)
                {
                    Undo.RecordObject(rb, "Make Kinematic");
                    rb.isKinematic = false;
                    count++;

                    // If this object is part of a prefab instance, apply the change.
                    if (PrefabUtility.IsPartOfPrefabInstance(go))
                    {
                        PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
                    }
                }
            }
        }

        EditorUtility.DisplayDialog(
            "Make Kinematic",
            $"{count} swallowable objects were made non kinematic.",
            "OK"
        );
    }

    private void CreatePrefabsFromSelectionAndRemoveFromScene()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No GameObjects selected.", "OK");
            return;
        }

        // Ensure the target subfolder exists.
        if (!AssetDatabase.IsValidFolder(prefabSubfolder))
        {
            Directory.CreateDirectory(prefabSubfolder);
            AssetDatabase.Refresh();
        }

        int prefabCount = 0;
        foreach (GameObject go in selectedObjects)
        {
            // Skip if the object is already a persistent asset.
            // if (EditorUtility.IsPersistent(go))
            // {
            //     Debug.LogWarning(
            //         $"{go.name} is already a persistent asset. Skipping prefab creation for this object."
            //     );
            //     continue;
            // }

            // Instantiate a clone of the scene object.
            GameObject clone = Instantiate(go);
            clone.name = go.name; // remove the "(Clone)" suffix.

            // Construct a unique path for the new prefab.
            string prefabPath = Path.Combine(prefabSubfolder, go.name + ".prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            // Save the clone as a prefab asset.
            PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);
            // Remove the clone from the scene.
            DestroyImmediate(clone);

            prefabCount++;
        }

        EditorUtility.DisplayDialog(
            "Create Prefabs",
            $"{prefabCount} prefabs have been created in {prefabSubfolder} and removed from the scene.",
            "OK"
        );
    }
}
