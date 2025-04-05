using System.Collections.Generic;
using UnityEngine;

public class PlayerCutoutController : MonoBehaviour
{
    [Tooltip("Reference to the player's transform.")]
    public Transform playerTransform;

    [Tooltip("Target cutout radius in normalized screen units for obstructing objects.")]
    public float targetCutoutRadius = 0.1f;

    [Tooltip("Speed of fading transition for the cutout effect.")]
    public float fadeSpeed = 5f;

    // Cache of GameObjects that have a Renderer with a cutout property.
    private Dictionary<GameObject, CachedRendererInfo> cachedRenderers =
        new Dictionary<GameObject, CachedRendererInfo>();

    // Struct to store cached renderer info.
    private struct CachedRendererInfo
    {
        public Renderer renderer;
        public Color originalColor;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 playerPos = playerTransform.position;
        Vector3 camPos = cam.transform.position;
        float distance = Vector3.Distance(playerPos, camPos);

        // Calculate player's screen position (normalized: 0-1).
        Vector3 playerScreenPos = cam.WorldToViewportPoint(playerPos);

        // Prepare a hash set to collect all obstructing objects.
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();

        // Cast ray from player to camera.
        RaycastHit[] hitsFromPlayer = Physics.RaycastAll(playerPos, camPos - playerPos, distance);
        // Cast ray from camera to player.
        RaycastHit[] hitsFromCamera = Physics.RaycastAll(camPos, playerPos - camPos, distance);

        // Add all hit objects from both raycasts.
        foreach (RaycastHit hit in hitsFromPlayer)
        {
            hitObjects.Add(hit.collider.gameObject);
        }
        foreach (RaycastHit hit in hitsFromCamera)
        {
            hitObjects.Add(hit.collider.gameObject);
        }

        // Also check if the player or camera are inside a collider.
        float overlapRadius = 0.01f;
        Collider[] overlapPlayer = Physics.OverlapSphere(playerPos, overlapRadius);
        Collider[] overlapCamera = Physics.OverlapSphere(camPos, overlapRadius);

        foreach (Collider col in overlapPlayer)
        {
            hitObjects.Add(col.gameObject);
        }
        foreach (Collider col in overlapCamera)
        {
            hitObjects.Add(col.gameObject);
        }

        // Process each hit object that supports the cutout property.
        foreach (GameObject hitObj in hitObjects)
        {
            Renderer rend = hitObj.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial.HasProperty("_PlayerScreenPos"))
            {
                // Cache the renderer if not already cached.
                if (!cachedRenderers.ContainsKey(hitObj))
                {
                    CachedRendererInfo info = new CachedRendererInfo();
                    info.renderer = rend;
                    info.originalColor = rend.material.color;
                    cachedRenderers[hitObj] = info;
                }
                // Update the material cutout parameters.
                Material mat = rend.material;
                // Update the cutout center (screen position) instantly.
                mat.SetVector(
                    "_PlayerScreenPos",
                    new Vector4(playerScreenPos.x, playerScreenPos.y, 0, 0)
                );
                // Smoothly animate _PlayerRadius toward the target cutout radius.
                float currentRadius = mat.GetFloat("_PlayerRadius");
                float newRadius = Mathf.Lerp(
                    currentRadius,
                    targetCutoutRadius,
                    Time.deltaTime * fadeSpeed
                );
                mat.SetFloat("_PlayerRadius", newRadius);
            }
        }

        // For any cached object that was not hit this frame, smoothly restore _PlayerRadius to 0.
        List<GameObject> keys = new List<GameObject>(cachedRenderers.Keys);
        foreach (GameObject obj in keys)
        {
            if (!hitObjects.Contains(obj))
            {
                CachedRendererInfo info = cachedRenderers[obj];
                if (info.renderer != null)
                {
                    Material mat = info.renderer.material;
                    float currentRadius = mat.GetFloat("_PlayerRadius");
                    float newRadius = Mathf.Lerp(currentRadius, 0f, Time.deltaTime * fadeSpeed);
                    mat.SetFloat("_PlayerRadius", newRadius);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        // Draw a red line from the camera's position to the player's position.
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cam.transform.position, playerTransform.position);
        // Optionally, draw a sphere at the player's position.
        Gizmos.DrawSphere(playerTransform.position, 0.1f);
    }
}
