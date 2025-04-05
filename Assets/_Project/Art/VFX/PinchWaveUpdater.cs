using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PinchWaveUpdater : MonoBehaviour
{
    [Tooltip("Material using the OrganicPinchWave shader.")]
    public Material targetMaterial;

    private Renderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (targetMaterial == null)
        {
            Debug.LogError("Target Material not assigned on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (targetMaterial != null && meshRenderer != null)
        {
            // Calculate the mesh center using the renderer's bounds.
            Vector3 center = meshRenderer.bounds.center;
            targetMaterial.SetVector("_MeshCenter", new Vector4(center.x, center.y, center.z, 0));
        }
    }
}
