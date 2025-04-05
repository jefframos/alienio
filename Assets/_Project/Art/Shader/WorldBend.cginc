#ifndef WORLDBEND_CGINC
#define WORLDBEND_CGINC

// BendWorld : Warps a given position based on a bending center and a radius,
// simulating a spherical curvature effect.
// pos : the original world position
// bendCenter : the center of the bending effect (e.g., the planet center)
// radius : the bending radius (points within this distance will be warped)
// weight : intensity of the bending effect (0 = no bending, 1 = full bending)
inline float3 BendWorld(float3 pos, float3 bendCenter, float radius, float weight)
{
    float3 dir = pos - bendCenter;
    float len = length(dir);

    // Only apply bending within the influence radius.
    if (len < radius)
    {
        // Compute a bending factor that is 1 at the center and 0 at the radius.
        float bendFactor = saturate(1.0 - (len / radius));

        // Map the bending factor to an angle (up to 90 degrees = PI / 2).
        float theta = bendFactor * (3.14159265 * 0.5);

        // Calculate a new length along the arc of a circle with the given radius.
        float newLen = radius * theta;

        // Compute the new position along the same direction.
        float3 newPos = bendCenter + normalize(dir) * newLen;

        // Interpolate between the original and the bent position.
        return lerp(pos, newPos, weight * bendFactor);
    }

    return pos;
}

#endif
