Shader "Custom/PinchShader" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        // World-space positions (set these via script or Inspector).
        _MeshCenter ("Mesh Center (World)", Vector) = (0,0,0,0)
        _PinchPosition ("Pinch Position (World)", Vector) = (0,0,0,0)
        // How strong the pinch is.
        _PinchStrength ("Pinch Strength", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Use a surface shader with Lambert lighting.
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _MeshCenter;
        float4 _PinchPosition;
        float _PinchStrength;

        struct Input {
            float2 uv_MainTex;
        };

        // Vertex modification: compute the pinch effect in object space.
        void vert (inout appdata_full v) {
            // Convert provided world-space positions to object space.
            float3 objectMeshCenter = mul(unity_WorldToObject, _MeshCenter).xyz;
            float3 objectPinchPosition = mul(unity_WorldToObject, _PinchPosition).xyz;
            
            // Compute the vector from the mesh center to the vertex in object space.
            float3 V = v.vertex.xyz - objectMeshCenter;
            // Compute the vector from the mesh center to the pinch position.
            float3 D = objectPinchPosition - objectMeshCenter;
            float dLen = length(D);
            if (dLen > 0.0001) {
                float3 Dnorm = D / dLen;
                // Compute the projection factor of V onto Dnorm.
                // 0 means the vertex is at the mesh center, 1 means at (or beyond) the pinch position.
                float proj = saturate(dot(V, Dnorm) / dLen);
                // Displacement along Dnorm.
                float3 displacement = Dnorm * _PinchStrength * proj;
                // Apply the displacement in object space.
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
