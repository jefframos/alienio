using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PinchController : MonoBehaviour
{
    [Tooltip("Material using the PinchDiffuse shader.")]
    public Material targetMaterial;

    [Header("Pinch Settings")]
    [Tooltip("Maximum distance (in world units) that the pinch can offset from the mesh center.")]
    public float maxPinchDistance = 0.5f;

    [Tooltip("Speed at which the pinch position lerps toward the target position.")]
    public float positionLerpSpeed = 5f;

    [Tooltip("Speed at which the pinch strength lerps.")]
    public float strengthLerpSpeed = 5f;

    [Tooltip("Threshold to consider the object as moving.")]
    public float movementThreshold = 0.001f;

    // Current state for pinch position and strength.
    private Vector3 currentPinchPosition;
    private float currentPinchStrength = 0f;

    // Cached mesh center (world space), based on Renderer bounds.
    private Vector3 meshCenter;

    // Last frame position for movement calculation.
    private Vector3 lastPosition;
    private Renderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("PinchController requires a Renderer component.");
            enabled = false;
            return;
        }

        lastPosition = transform.position;
        // Initialize mesh center and pinch position.
        meshCenter = meshRenderer.bounds.center;
        currentPinchPosition = meshCenter;

        // Initialize shader properties.
        if (targetMaterial != null)
        {
            targetMaterial.SetVector(
                "_MeshCenter",
                new Vector4(meshCenter.x, meshCenter.y, meshCenter.z, 0)
            );
            targetMaterial.SetVector(
                "_PinchPosition",
                new Vector4(
                    currentPinchPosition.x,
                    currentPinchPosition.y,
                    currentPinchPosition.z,
                    0
                )
            );
            targetMaterial.SetFloat("_PinchStrength", currentPinchStrength);
        }
    }

    private void Update()
    {
        if (targetMaterial == null)
            return;

        // Update mesh center from the Renderer bounds.
        meshCenter = meshRenderer.bounds.center;
        targetMaterial.SetVector(
            "_MeshCenter",
            new Vector4(meshCenter.x, meshCenter.y, meshCenter.z, 0)
        );

        // Calculate movement delta.
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        float distanceMoved = delta.magnitude;

        // Determine target pinch position and strength.
        Vector3 targetPinchPos;
        float targetStrength;

        if (distanceMoved > movementThreshold)
        {
            // Object is moving.
            Vector3 movementDir = delta.normalized;
            // Pinch target is set from the mesh center, in the movement direction, clamped to maxPinchDistance.
            targetPinchPos = meshCenter + movementDir * maxPinchDistance;
            targetStrength = 1.0f;
        }
        else
        {
            // Object is stationary; target pinch is at the mesh center and strength goes to zero.
            targetPinchPos = meshCenter;
            targetStrength = 0f;
        }

        // Lerp current values toward target values.
        currentPinchPosition = Vector3.Lerp(
            currentPinchPosition,
            targetPinchPos,
            Time.deltaTime * positionLerpSpeed
        );
        currentPinchStrength = Mathf.Lerp(
            currentPinchStrength,
            targetStrength,
            Time.deltaTime * strengthLerpSpeed
        );

        // Update shader properties.
        targetMaterial.SetVector(
            "_PinchPosition",
            new Vector4(currentPinchPosition.x, currentPinchPosition.y, currentPinchPosition.z, 0)
        );
        targetMaterial.SetFloat("_PinchStrength", currentPinchStrength);

        lastPosition = currentPosition;
    }

    private void OnDrawGizmos()
    {
        // Ensure the renderer is available.
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
            return;

        // Get the mesh center from the renderer's bounds.
        Vector3 center = meshRenderer.bounds.center;

        // Draw a blue sphere for the mesh center.
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(center, 0.05f);

        // Draw a red sphere for the current pinch position.
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentPinchPosition, 0.05f);

        // Draw a line connecting the mesh center and the pinch position.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, currentPinchPosition);
    }
}
