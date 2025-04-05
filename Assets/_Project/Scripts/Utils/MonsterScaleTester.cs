using UnityEngine;

[ExecuteAlways]
public class MonsterScaleTester : MonoBehaviour
{
    // Reference to another GameObject whose transform we'll use.
    public Transform referenceObject;

    [SerializeField]
    private float scaleScale = 0.7f;
    private Material mat;

    private void Awake()
    {
        // Get the material from the MeshRenderer on this GameObject.
        mat = GetComponent<MeshRenderer>().sharedMaterial;
    }

    void Update()
    {
        if (referenceObject != null)
        {
            // Set this object's position to match the reference object's position.
            transform.position = referenceObject.position;

            float monsterScale = referenceObject.localScale.x * scaleScale;
            transform.localScale = new Vector3(monsterScale, monsterScale, monsterScale);

            float matScale = (monsterScale - 1f) / 5f;
            matScale = Mathf.Lerp(1f, 0.5f, matScale);

            matScale = Mathf.Clamp(matScale, 0.65f, 1.5f);

            // Update the shader parameter.
            mat.SetFloat("_WhiteScale", matScale);
        }
    }
}
