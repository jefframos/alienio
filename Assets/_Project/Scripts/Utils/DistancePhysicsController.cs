using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DistancePhysicsController : MonoBehaviour
{
    public float disableDistance = 50f;

    public float enableDistance = 45f;

    private Rigidbody rb;
    private Transform camTransform;
    private bool physicsEnabled = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning(
                "No main camera found. Please assign one manually or set the MainCamera tag."
            );
        }
    }

    private void Update()
    {
        if (camTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, camTransform.position);

        if (physicsEnabled && distance > disableDistance)
        {
            // Disable physics simulation.
            rb.isKinematic = false;
            physicsEnabled = false;
        }
        else if (!physicsEnabled && distance < enableDistance)
        {
            // Re-enable physics simulation.
            rb.isKinematic = true;
            physicsEnabled = true;
        }
    }
}
