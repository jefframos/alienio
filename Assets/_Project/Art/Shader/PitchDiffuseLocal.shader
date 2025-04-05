Shader "Custom/PinchDiffuseLocal" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        // These positions are now in object (local) space.
        _MeshCenter ("Mesh Center (Local)", Vector) = (0,0,0,0)
        _PinchPosition ("Pinch Position (Local)", Vector) = (0,0,0,0)
        _PinchStrength ("Pinch Strength", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Using a surface shader for standard diffuse lighting.
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _MeshCenter;
        float4 _PinchPosition;
        float _PinchStrength;

        struct Input {
            float2 uv_MainTex;
        };

        // Vertex modifier: calculate pinch effect in object (local) space.
        void vert (inout appdata_full v) {
            // Compute the vector from the mesh center (in object space) to the current vertex.
            float3 V = v.vertex.xyz - _MeshCenter.xyz;
            // Compute the pinch direction from the center to the target pinch position.
            float3 D = _PinchPosition.xyz - _MeshCenter.xyz;
            float dLen = length(D);
            if (dLen > 0.0001) {
                float3 Dnorm = D / dLen;
                // Projection factor: 0 at the center, 1 at (or beyond) the pinch position.
                float proj = saturate(dot(V, Dnorm) / dLen);
                // Calculate displacement.
                float3 displacement = Dnorm * _PinchStrength * proj;
                v.vertex.xyz += displacement;
            }
        }

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
