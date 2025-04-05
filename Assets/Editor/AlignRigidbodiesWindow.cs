using UnityEditor;
using UnityEngine;

public class AlignRigidbodiesWindow : EditorWindow
{
    private Collider targetPlane;
    private GameObject containerObject;

    [MenuItem("Swallow/Align Rigidbodies To Plane")]
    public static void ShowWindow()
    {
        GetWindow<AlignRigidbodiesWindow>("Align Rigidbodies");
    }

    private void OnGUI()
    {
        GUILayout.Label("Align Rigidbodies To Plane", EditorStyles.boldLabel);

        targetPlane = (Collider)
            EditorGUILayout.ObjectField(
                "Target Plane Collider",
                targetPlane,
                typeof(Collider),
                true
            );
        containerObject = (GameObject)
            EditorGUILayout.ObjectField(
                "Rigidbodies Container",
                containerObject,
                typeof(GameObject),
                true
            );

        if (GUILayout.Button("Align Rigidbodies"))
        {
            AlignRigidbodies();
        }
    }

    private void AlignRigidbodies()
    {
        if (targetPlane == null)
        {
            Debug.LogError("Please assign a Target Plane Collider.");
            return;
        }
        if (containerObject == null)
        {
            Debug.LogError("Please assign a container GameObject with rigidbodies.");
            return;
        }

        // Get all Rigidbody components in the container's children.
        Rigidbody[] rigidbodies = containerObject.GetComponentsInChildren<Rigidbody>();
        int countAligned = 0;

        foreach (Rigidbody rb in rigidbodies)
        {
            Collider rbCollider = rb.GetComponent<Collider>();
            if (rbCollider == null)
            {
                Debug.LogWarning($"No Collider found on {rb.gameObject.name}, skipping.");
                continue;
            }

            // Cast a ray downward from a point above the object.
            Vector3 rayOrigin = rb.transform.position + Vector3.up * 10f;
            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;

            // Use the target plane's collider to perform the raycast.
            if (targetPlane.Raycast(ray, out hit, 100f))
            {
                // Calculate the current bottom of the object's collider.
                float currentBottom = rbCollider.bounds.min.y;
                // Compute the vertical offset needed to snap the bottom to the hit point.
                float deltaY = hit.point.y - currentBottom;

                // Adjust the object's position.
                Vector3 newPos = rb.transform.position + new Vector3(0, deltaY, 0);
                Undo.RecordObject(rb.transform, "Snap Rigidbody to Plane");
                rb.transform.position = newPos;
                countAligned++;
            }
            else
            {
                Debug.LogWarning($"Raycast did not hit target plane for {rb.gameObject.name}");
            }
        }

        Debug.Log(
            $"Aligned {countAligned} rigidbody objects to plane '{targetPlane.gameObject.name}'."
        );
    }
}
