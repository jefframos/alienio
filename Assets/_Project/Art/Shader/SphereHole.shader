Shader "Custom/SphereHoleStandard" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SphereCenter ("Sphere Center", Vector) = (0, 0, 0, 0)
        _SphereRadius ("Sphere Radius", Float) = 1.0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Use a surface shader with a custom vertex modifier.
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _SphereCenter;
        float _SphereRadius;

        // Global bending parameters (set via a separate updater script).
        // _BendCenter is the world - space center of the bending effect.
        // _BendRadius defines the influence area.
        // _BendWeight (0 - 1) controls the intensity.
        float3 _BendCenter;
        float _BendRadius;
        float _BendWeight;

        // Include our bending function.
        #include "WorldBend.cginc"

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // Custom vertex function that applies a smooth bending effect.
        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // Calculate the original world position.
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            // Compute horizontal offset (ignoring Y) from the bend center.
            float3 offset = worldPos - _BendCenter;
            float horizontalDist = length(offset.xz);

            // Compute a smooth bending factor :
            // - At horizontalDist = 0, factor is 1 (full bend).
            // - At horizontalDist = _BendRadius, factor is 0 (no bend).
            float smoothFactor = 1.0 - smoothstep(0.0, _BendRadius, horizontalDist);

            // If smoothFactor is non - zero, compute the bent position.
            if (smoothFactor > 0.0) {
                // Compute the sphere dome height at this horizontal distance.
                float sphereY = sqrt(saturate(_BendRadius * _BendRadius - horizontalDist * horizontalDist));
                // Build the target bent position.
                float3 bentPos = float3(worldPos.x, _BendCenter.y + sphereY, worldPos.z);
                // Blend the original and bent positions by the smooth factor scaled by _BendWeight.
                worldPos = lerp(worldPos, bentPos, smoothFactor * _BendWeight);
            }

            // Convert the modified world position back to object space.
            v.vertex = mul(unity_WorldToObject, float4(worldPos, 1.0));

            // Pass the final world position and UV to the fragment shader.
            o.worldPos = worldPos;
            o.uv_MainTex = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Bend the sphere center with the same function so that the hole remains aligned.
            float3 bentSphereCenter = BendWorld(_SphereCenter.xyz, _BendCenter, _BendRadius, _BendWeight);

            // Compute distance from the fragment's bent world position to the bent sphere center.
            float d = distance(IN.worldPos, bentSphereCenter);
            // Discard fragments inside the sphere to create the "hole".
            clip(d - _SphereRadius);

            // Sample the texture and assign shading.
            fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = col.rgb;
            o.Metallic = 0.0;
            o.Smoothness = 0.5;
            o.Alpha = col.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
