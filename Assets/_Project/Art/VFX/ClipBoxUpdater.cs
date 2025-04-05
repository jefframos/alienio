using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteAlways]
public class ClipBoxUpdater : MonoBehaviour
{
    private BoxCollider boxCollider;

    // Names of the global shader properties
    private static readonly int ClipBoxMinID = Shader.PropertyToID("_ClipBoxMin");
    private static readonly int ClipBoxMaxID = Shader.PropertyToID("_ClipBoxMax");

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        // Get the world-space bounds from the collider.
        Bounds bounds = boxCollider.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        // Set the global shader properties for all shaders that support clipping.
        Shader.SetGlobalVector(ClipBoxMinID, min);
        Shader.SetGlobalVector(ClipBoxMaxID, max);
    }
}
