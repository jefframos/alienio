using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AttractionForceModifier : MonoBehaviour
{
    private Rigidbody rb;
    private Transform attractor;
    private float baseAttractionForce;
    private float minDistance; // below this distance, rotation is unlocked
    private float maxDistance; // beyond this distance, no force is applied

    public bool isActive = true;

    // Store the original constraints to restore later.
    private RigidbodyConstraints originalConstraints;

    public void Initialize(Transform attractor, float baseForce, float minDist, float maxDist)
    {
        this.attractor = attractor;
        baseAttractionForce = baseForce;
        minDistance = minDist;
        maxDistance = maxDist;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Cache the original constraints to restore them when needed.
        originalConstraints = rb.constraints;
    }

    private void FixedUpdate()
    {
        if (!isActive || attractor == null || rb == null)
            return;

        // return;
        // Calculate the direction toward the attractor on the XZ plane.
        Vector3 directionToAttractor = new Vector3(
            attractor.position.x - transform.position.x,
            0f,
            attractor.position.z - transform.position.z
        );
        float distance = directionToAttractor.magnitude;

        // If too close, unlock rotation by removing constraints.
        if (maxDistance > 5f)
        {
            rb.constraints = RigidbodyConstraints.None;
            return;
        }
        else if (rb.constraints != originalConstraints)
        {
            // Restore the original constraints when not too close.
            //rb.constraints = originalConstraints;
        }

        // If too far, don't apply any force.
        // if (distance > maxDistance)
        //return;

        // Compute a factor so that the force is stronger when closer.
        float factor = 1f - Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        Vector3 force = directionToAttractor.normalized * baseAttractionForce * factor;
        rb.AddForce(force, ForceMode.Impulse);
    }
}
