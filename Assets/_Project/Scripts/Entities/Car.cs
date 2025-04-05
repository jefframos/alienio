using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour, ICollidable
{
    public float speed = 5f;

    public Vector3 direction = Vector3.forward;

    // Cached components.
    private Collider carCollider;
    private Rigidbody rb;

    // Store the fixed direction in world space.
    private Vector3 initialWorldDirection;

    public Transform sensorPoint; // The designated sensor transform.
    public float collisionCheckRadius = 0.5f; // Radius for the sphere check.

    // Flag to indicate if movement should be disabled.
    private bool movementDisabled = false;

    public event Action OnDisableMove;

    private void Awake()
    {
        carCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        // Convert the local direction to a world-space direction.
        initialWorldDirection = transform.TransformDirection(direction.normalized);
        // Ensure the car initially points in the correct direction.
        transform.rotation = Quaternion.LookRotation(initialWorldDirection);
    }

    public Collider GetCollider()
    {
        return carCollider;
    }

    public void DisableMovement()
    {
        movementDisabled = true;
        Debug.Log("Car movement disabled: " + gameObject.name);
        OnDisableMove.Invoke();
    }

    public bool CanMove(List<ICollidable> collidables)
    {
        // If movement is disabled, return false immediately.
        if (movementDisabled)
            return false;

        // Compute how far we intend to move this frame.
        Vector3 moveDelta = initialWorldDirection * speed * Time.deltaTime;

        // Use the sensorPoint as the reference. If not assigned, default to this transform.
        Vector3 checkOrigin = sensorPoint != null ? sensorPoint.position : transform.position;

        // Determine the future sensor position.
        Vector3 futureSensorPos = checkOrigin + moveDelta;

        // Perform an overlap sphere check at the future sensor position.
        Collider[] overlaps = Physics.OverlapSphere(futureSensorPos, collisionCheckRadius);

        // For each collidable, if our simulated sphere hits its collider, double-check with a raycast.
        foreach (ICollidable c in collidables)
        {
            // Skip self.
            if (c == this)
                continue;

            Collider otherCollider = c.GetCollider();

            // Check if the simulated sphere (at the future sensor position) overlaps the other collider.
            bool sphereIntersects = false;
            foreach (Collider col in overlaps)
            {
                if (col == otherCollider)
                {
                    sphereIntersects = true;
                    break;
                }
            }

            // If the sphere test detects a collision, perform a raycast from the sensor point.
            if (sphereIntersects)
            {
                // Set ray distance to the move distance plus a small margin.
                float rayDistance = moveDelta.magnitude + 0.1f;
                Ray ray = new Ray(checkOrigin, initialWorldDirection);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, rayDistance))
                {
                    if (hit.collider == otherCollider)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void Move(List<ICollidable> collidables)
    {
        // If movement is disabled, skip movement.
        if (movementDisabled)
            return;

        // Compute the target rotation from the fixed world direction.
        Quaternion targetRotation = Quaternion.LookRotation(initialWorldDirection);
        // Get current rotation Euler angles.
        Vector3 currentEuler = transform.rotation.eulerAngles;
        // Update only the Y component to match the target rotation.
        transform.rotation = Quaternion.Euler(
            currentEuler.x,
            targetRotation.eulerAngles.y,
            currentEuler.z
        );

        if (CanMove(collidables))
        {
            transform.position += initialWorldDirection * speed * Time.deltaTime;
        }
    }
}
