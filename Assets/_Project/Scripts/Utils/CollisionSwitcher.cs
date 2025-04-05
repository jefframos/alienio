using System.Collections.Generic;
using UnityEngine;

public class CollisionSwitcher : MonoBehaviour
{
    [SerializeField]
    private string targetTag = "Swallowable";

    [SerializeField]
    private LayerMask switchedLayerMask;

    [SerializeField]
    private float gravityMultiplier = 2f;

    // The impulse force applied immediately when an object enters.
    [SerializeField]
    private float initialAttractionForce = 5f;

    // The continuous attraction force magnitude.
    [SerializeField]
    private float continuousAttractionForce = 3f;

    private int switchedLayer = -1;

    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();

    private void Awake()
    {
        if (switchedLayerMask.value == 0)
        {
            Debug.LogWarning("Switched Layer Mask is empty. Please assign a single layer to it.");
        }
        else
        {
            switchedLayer = LayerMaskToLayer(switchedLayerMask);
        }
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layerValue = mask.value;
        int layerNumber = 0;
        while (layerValue > 1)
        {
            layerValue = layerValue >> 1;
            layerNumber++;
        }
        return layerNumber;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Check the collider's size (width and depth) and ignore if its depth is larger than the transform's depth scale.
            Collider col = other.GetComponent<Collider>();
            if (col != null)
            {
                float colliderDepth = col.bounds.size.z;
                float transformDepth = transform.lossyScale.z;
                if (colliderDepth > transformDepth)
                {
                    //return;
                }
            }

            GameObject targetObject = other.gameObject;
            if (!originalLayers.ContainsKey(targetObject))
            {
                originalLayers[targetObject] = targetObject.layer;
            }

            if (switchedLayer != -1)
            {
                targetObject.layer = switchedLayer;
            }
            else
            {
                Debug.LogWarning("Switched layer is not set correctly.");
            }

            // Increase gravity: if the object has a Rigidbody, add the IncreasedGravity component.
            Rigidbody rb = targetObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Add or configure the IncreasedGravity component as before.
                IncreasedGravity incGrav = targetObject.GetComponent<IncreasedGravity>();
                if (incGrav == null)
                {
                    incGrav = targetObject.AddComponent<IncreasedGravity>();
                }
                incGrav.SetGravityMultiplier(gravityMultiplier);

                // Apply an initial impulse force toward the center.
                Vector3 forceDirection = (
                    transform.position - targetObject.transform.position
                ).normalized;
                rb.AddForce(forceDirection * initialAttractionForce, ForceMode.Impulse);
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                }

                // Add the AttractionForceModifier if not already present.
                AttractionForceModifier attraction =
                    targetObject.GetComponent<AttractionForceModifier>();
                if (attraction == null)
                {
                    attraction = targetObject.AddComponent<AttractionForceModifier>();
                    float switcherScale = transform.lossyScale.magnitude;
                    float minDistance = switcherScale * 0.2f;
                    float maxDistance = switcherScale * 1.1f;
                    attraction.Initialize(
                        transform,
                        continuousAttractionForce,
                        minDistance,
                        maxDistance
                    );
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            GameObject targetObject = other.gameObject;
            if (originalLayers.TryGetValue(targetObject, out int originalLayer))
            {
                targetObject.layer = originalLayer;
                originalLayers.Remove(targetObject);
            }

            // Remove IncreasedGravity component if it exists.
            IncreasedGravity incGrav = targetObject.GetComponent<IncreasedGravity>();
            if (incGrav != null)
            {
                Destroy(incGrav);
            }

            // Remove AttractionForceModifier component if it exists.
            AttractionForceModifier attraction =
                targetObject.GetComponent<AttractionForceModifier>();
            if (attraction != null)
            {
                Destroy(attraction);
            }
        }
    }
}
