using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PinchControllerLocal : MonoBehaviour
{
    [Tooltip("Material using the PinchDiffuseLocal shader.")]
    public Material targetMaterial;

    [Header("Pinch Settings")]
    [Tooltip("Maximum distance (in world units) that the pinch can offset from the mesh center.")]
    public float maxPinchDistance = 0.5f;

    [Tooltip("Speed at which the pinch position lerps toward the target position.")]
    public float positionLerpSpeed = 5f;

    [Tooltip("Speed at which the pinch strength lerps.")]
    public float strengthLerpSpeed = 5f;

    [Tooltip("Threshold to consider the object as moving (speed in world units per second).")]
    public float movementThreshold = 0.001f;

    [Tooltip("Maximum pinch strength value.")]
    public float maxPinch = 0.5f;

    // Current state for pinch position and strength (in world space)
    private Vector3 currentPinchPosition;
    private float currentPinchStrength = 0f;

    // Cached mesh center (from Renderer bounds) and renderer reference
    private Vector3 worldMeshCenter;
    private Renderer meshRenderer;

    // Movement values for direction and speed
    private Vector3 currentDirection = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;
    private float currentSpeed = 0f;

    public float monsterScale = 1f;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("PinchControllerLocal requires a Renderer component.");
            enabled = false;
            return;
        }

        worldMeshCenter = meshRenderer.bounds.center;
        currentPinchPosition = worldMeshCenter;
        targetMaterial = GetComponent<MeshRenderer>().material;
        UpdateShaderProperties();
    }

    private void Update()
    {
        if (targetMaterial == null)
            return;

        // Update the mesh center from the renderer bounds.
        worldMeshCenter = meshRenderer.bounds.center;

        // Determine target pinch position and strength based on current speed.
        Vector3 targetPinchPos =
            (currentSpeed > movementThreshold)
                ? worldMeshCenter + currentDirection * maxPinchDistance
                : worldMeshCenter;
        float targetStrength = (currentSpeed > movementThreshold) ? maxPinch : 0f;

        // Smoothly interpolate the pinch strength.
        currentPinchStrength = Mathf.Lerp(
            currentPinchStrength,
            targetStrength,
            Time.deltaTime * strengthLerpSpeed
        );

        // Smoothly interpolate current direction toward target direction.
        currentDirection = Vector3
            .Lerp(currentDirection, targetDirection, Time.deltaTime * positionLerpSpeed)
            .normalized;

        // Smoothly interpolate the distance from the mesh center.
        float currentDistance = (currentPinchPosition - worldMeshCenter).magnitude;
        float targetDistance = Mathf.Min(
            (targetPinchPos - worldMeshCenter).magnitude,
            maxPinchDistance
        );
        float newDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            Time.deltaTime * positionLerpSpeed
        );

        // Update the current pinch position.
        currentPinchPosition = worldMeshCenter + currentDirection * newDistance;

        UpdateShaderProperties();

        // Update scale and white scale on the material.
        transform.localScale = new Vector3(monsterScale, 1, monsterScale);
        float matScale = (monsterScale - 1f) / 5f;
        matScale = Mathf.Lerp(1f, 0.5f, matScale);
        matScale = Mathf.Clamp(matScale, 0.65f, 1.5f);
        targetMaterial.SetFloat("_WhiteScale", matScale);

        // Set the transform's Y rotation based on the current direction (in the XZ plane).
        if (currentDirection.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(currentDirection.x, currentDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 0.2f
            );
        }
    }

    /// <summary>
    /// Allows external setting of the current movement direction.
    /// </summary>
    public void SetDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
            targetDirection = direction.normalized;
    }

    /// <summary>
    /// Allows external setting of the current speed.
    /// </summary>
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    /// <summary>
    /// Updates the shader properties by converting world space positions to local space.
    /// </summary>
    private void UpdateShaderProperties()
    {
        Vector3 localMeshCenter = transform.InverseTransformPoint(worldMeshCenter);
        Vector3 localPinchPosition = transform.InverseTransformPoint(currentPinchPosition);

        targetMaterial.SetVector(
            "_MeshCenter",
            new Vector4(localMeshCenter.x, localMeshCenter.y, localMeshCenter.z, 0)
        );
        targetMaterial.SetVector(
            "_PinchPosition",
            new Vector4(localPinchPosition.x, localPinchPosition.y, localPinchPosition.z, 0)
        );
        targetMaterial.SetFloat("_PinchStrength", currentPinchStrength);
    }

    // Optional: Draw gizmos to visualize the mesh center and pinch position.
    private void OnDrawGizmos()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(meshRenderer.bounds.center, 0.05f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentPinchPosition, 0.05f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(meshRenderer.bounds.center, currentPinchPosition);
    }
}
