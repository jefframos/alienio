using UnityEngine;

[CreateAssetMenu(
    fileName = "SwallowableData",
    menuName = "ScriptableObjects/SwallowableData",
    order = 1
)]
public class SwallowableData : ScriptableObject
{
    [Range(0.1f, 2f)]
    public float swallowThreshold = 0.8f;
}
