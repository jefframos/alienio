using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PushDirectionUpdater : MonoBehaviour
{
    [Tooltip("Material using the OrganicPushBend shader.")]
    public Material targetMaterial;

    [Tooltip("Speed at which the push force lerps (higher = faster response).")]
    public float lerpSpeed = 5f;

    // Minimum movement threshold to consider the object as moving.
    public float movementThreshold = 0.001f;

    // Current push force value (0 to 1).
    private float currentPushForce = 0f;

    // Last frame position for movement calculation.
    private Vector3 lastPosition;

    // Cached renderer to compute the mesh center.
    private Renderer meshRenderer;

    private void Start()
    {
        lastPosition = transform.position;
        meshRenderer = GetComponent<Renderer>();

        // Ensure targetMaterial is set.
        if (targetMaterial == null)
        {
            Debug.LogWarning("Target Material is not assigned on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (targetMaterial == null)
            return;

        // Calculate movement delta and direction.
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        float distanceMoved = delta.magnitude;

        // Determine target push force: 1 if moving, 0 if not.
        float targetForce = (distanceMoved > movementThreshold) ? 1f : 0f;
        // Smoothly interpolate the current push force toward the target.
        currentPushForce = Mathf.Lerp(currentPushForce, targetForce, Time.deltaTime * lerpSpeed);

        // Update shader push force.
        targetMaterial.SetFloat("_PushForce", currentPushForce);

        // If moving, update push direction.
        if (distanceMoved > movementThreshold)
        {
            Vector3 pushDir = delta.normalized;
            targetMaterial.SetVector(
                "_PushDirection",
                new Vector4(pushDir.x, pushDir.y, pushDir.z, 0f)
            );
        }

        // Update _MeshCenter using the Renderer bounds.
        if (meshRenderer != null)
        {
            // This gives the geometric center of the mesh in world space.
            Vector3 meshCenter = meshRenderer.bounds.center;
            targetMaterial.SetVector(
                "_MeshCenter",
                new Vector4(meshCenter.x, meshCenter.y, meshCenter.z, 0f)
            );
        }
        else
        {
            // Fallback: use transform.position if Renderer is missing.
            targetMaterial.SetVector(
                "_MeshCenter",
                new Vector4(transform.position.x, transform.position.y, transform.position.z, 0f)
            );
        }

        lastPosition = currentPosition;
    }
}
