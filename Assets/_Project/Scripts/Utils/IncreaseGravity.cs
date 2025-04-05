using UnityEngine;

public class IncreasedGravity : MonoBehaviour
{
    private float gravityMultiplier = 3f;
    private Rigidbody rb;

    public void SetGravityMultiplier(float multiplier)
    {
        gravityMultiplier = multiplier;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            // Apply additional gravity
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }
}
