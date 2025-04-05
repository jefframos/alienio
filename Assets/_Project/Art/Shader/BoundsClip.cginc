// BoundsClip.cginc
inline void ClipOutsideBounds(float3 worldPos, float3 minBound, float3 maxBound)
{
    if (worldPos.x < minBound.x || worldPos.x > maxBound.x ||
    worldPos.y < minBound.y || worldPos.y > maxBound.y ||
    worldPos.z < minBound.z || worldPos.z > maxBound.z)
    {
        clip(- 1);
    }
}
