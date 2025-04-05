using UnityEngine;

public class MeshHoleController : MonoBehaviour
{
    public Material[] holeMaterials;

    public SphereCollider sphereCollider;

    void Update()
    {
        UpdateShaderProperties();
    }

    void OnValidate()
    {
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        foreach (var holeMaterial in holeMaterials)
        {
            if (holeMaterial != null && sphereCollider != null)
            {
                // Get the collider's world position
                Vector3 sphereCenter = sphereCollider.transform.position;

                // Adjust the collider's radius by its transform scale (assuming uniform scale on X)
                float radius = sphereCollider.radius * sphereCollider.transform.lossyScale.x;

                // Update the shader properties
                holeMaterial.SetVector("_SphereCenter", sphereCenter);
                holeMaterial.SetFloat("_SphereRadius", radius);
            }
        }
    }
}
