using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteAlways]
public class HueController : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 360f)]
    public float hueValue = 0f;

    // Cache materials from child renderers that have a _Hue property.
    [SerializeField]
    private List<Material> hueMaterials = new List<Material>();

    private void Awake()
    {
        // Get all Renderers in children.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // If the material has a property called _Hue, add it to our list.
                if (mat.HasProperty("_Hue"))
                {
                    hueMaterials.Add(mat);
                }
            }
        }
    }

    public void Update()
    {
        hueValue = GameSettings.Instance.GetCharacterHUE();
        foreach (Material mat in hueMaterials)
        {
            mat.SetFloat("_Hue", hueValue);
        }
    }
}
