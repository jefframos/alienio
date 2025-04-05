using UnityEngine;

public class OutBoundsTeleporter : MonoBehaviour
{
    [Tooltip("Box Collider defining the world bounds.")]
    public BoxCollider worldBounds;

    private void Update()
    {
        if (worldBounds == null)
            return;

        Bounds bounds = worldBounds.bounds;
        Vector3 pos = transform.position;
        bool teleported = false;

        // Check X axis.
        if (pos.x > bounds.max.x)
        {
            pos.x = bounds.min.x;
            teleported = true;
        }
        else if (pos.x < bounds.min.x)
        {
            pos.x = bounds.max.x;
            teleported = true;
        }

        // Check Z axis.
        if (pos.z > bounds.max.z)
        {
            pos.z = bounds.min.z;
            teleported = true;
        }
        else if (pos.z < bounds.min.z)
        {
            pos.z = bounds.max.z;
            teleported = true;
        }

        if (teleported)
        {
            transform.position = pos;
        }
    }
}
