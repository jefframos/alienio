using System.Collections.Generic;
using UnityEngine;

public class RaycastTransparency : MonoBehaviour
{
    [Tooltip("Target alpha value for objects between the source and the camera.")]
    [Range(0f, 1f)]
    public float transparentAlpha = 0.3f;

    [Tooltip("Speed of fading transition.")]
    public float fadeSpeed = 5f;

    // Caches the renderer and its original color for objects that have been faded.
    private Dictionary<GameObject, CachedRendererInfo> cachedRenderers =
        new Dictionary<GameObject, CachedRendererInfo>();

    // Struct to hold cached info.
    private struct CachedRendererInfo
    {
        public Renderer renderer;
        public Color originalColor;
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 origin = cam.transform.position;
        Vector3 direction = (transform.position - origin).normalized;

        // Use RaycastAll to detect all objects along the ray, even if the origin is inside a collider.
        float maxDistance = Vector3.Distance(origin, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance);

        // Track the closest GameObject hit this frame.
        GameObject hitObject = null;
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            hitObject = hits[0].collider.gameObject;
        }

        // If we hit an object that has a renderer, cache it if not already.
        if (hitObject != null)
        {
            CachedRendererInfo info;
            if (!cachedRenderers.TryGetValue(hitObject, out info))
            {
                Renderer rend = hitObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    info.renderer = rend;
                    info.originalColor = rend.material.color;
                    cachedRenderers[hitObject] = info;
                }
            }
            // Fade the hit object's renderer if it's still valid.
            if (
                cachedRenderers.ContainsKey(hitObject)
                && cachedRenderers[hitObject].renderer != null
            )
            {
                FadeRendererAlpha(cachedRenderers[hitObject].renderer, transparentAlpha);
            }
        }

        // For all cached renderers that were not hit this frame, restore alpha.
        List<GameObject> keys = new List<GameObject>(cachedRenderers.Keys);
        foreach (GameObject obj in keys)
        {
            // Remove from the dictionary if the GameObject has been destroyed.
            if (obj == null)
            {
                cachedRenderers.Remove(obj);
                continue;
            }

            // If this object wasn't hit, and its renderer is still valid, restore its alpha.
            if (obj != hitObject)
            {
                CachedRendererInfo info = cachedRenderers[obj];
                if (info.renderer != null)
                {
                    RestoreRendererAlpha(info.renderer, info.originalColor);
                }
                else
                {
                    cachedRenderers.Remove(obj);
                }
            }
        }
    }

    private void FadeRendererAlpha(Renderer rend, float targetAlpha)
    {
        Color col = rend.material.color;
        col.a = Mathf.Lerp(col.a, targetAlpha, Time.deltaTime * fadeSpeed);
        rend.material.color = col;
    }

    private void RestoreRendererAlpha(Renderer rend, Color originalColor)
    {
        Color col = rend.material.color;
        col.a = Mathf.Lerp(col.a, originalColor.a, Time.deltaTime * fadeSpeed);
        rend.material.color = col;
    }
}
