using UnityEngine;

[ExecuteAlways]
public class WorldBendUpdater : MonoBehaviour
{
    // Center of the bending effect (e.g., the "planet" center)
    public Vector3 bendCenter = Vector3.zero;

    // Radius within which the bending effect applies.
    public float bendRadius = 10f;

    // Intensity of the bending effect (0 = no bending, 1 = full bending).
    [Range(0f, 1f)]
    public float bendWeight = 1f;

    // Cache shader property IDs for efficiency.
    private static readonly int BendCenterID = Shader.PropertyToID("_BendCenter");
    private static readonly int BendRadiusID = Shader.PropertyToID("_BendRadius");
    private static readonly int BendWeightID = Shader.PropertyToID("_BendWeight");

    private void Update()
    {
        bendCenter = this.transform.position;
        // Update global shader parameters each frame.
        Shader.SetGlobalVector(BendCenterID, bendCenter);
        Shader.SetGlobalFloat(BendRadiusID, bendRadius);
        Shader.SetGlobalFloat(BendWeightID, bendWeight);
    }
}
