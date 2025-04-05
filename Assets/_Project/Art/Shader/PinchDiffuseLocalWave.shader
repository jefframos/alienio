Shader "Custom/PinchDiffuseLocalWave" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        // These positions are in object (local) space.
        _MeshCenter ("Mesh Center (Local)", Vector) = (0, 0, 0, 0)
        _PinchPosition ("Pinch Position (Local)", Vector) = (0, 0, 0, 0)
        _WaveStrenght ("Wave Strenght (Local)", Vector) = (0, 0, 0, 0)
        _PinchStrength ("Pinch Strength", Float) = 1.0
        // Wave properties.
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 2.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Using a surface shader for standard diffuse lighting.
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _MeshCenter;
        float4 _PinchPosition;
        float4 _WaveStrenght;
        float _PinchStrength;
        float _WaveAmplitude;
        float _WaveFrequency;
        float _WaveSpeed;

        struct Input {
            float2 uv_MainTex;
        };

        // Vertex modifier : first applies the pinch effect, then adds a wave offset.
        void vert (inout appdata_full v) {
            // -- - Pinch Effect -- -
            // Calculate the vector from the mesh center to the vertex.
            float3 V = v.vertex.xyz - _MeshCenter.xyz;
            // Determine the pinch direction (from the mesh center toward the pinch target).
            float3 D = _PinchPosition.xyz - _MeshCenter.xyz;
            float dLen = length(D);
            if (dLen > 0.0001) {
                float3 Dnorm = D / dLen;
                // Projection factor : 0 at the center, 1 at (or beyond) the pinch position.
                float proj = saturate(dot(V, Dnorm) / dLen);
                float3 pinchDisplacement = Dnorm * _PinchStrength * proj;
                // Apply the pinch displacement.
                v.vertex.xyz += pinchDisplacement;
            }

            // -- - Wave Offset -- -
            // Calculate the new position after pinch.
            float3 newPos = v.vertex.xyz;
            // Compute the radial vector from the mesh center to the new position.
            float3 radial = newPos - _MeshCenter.xyz;
            float dist = length(radial);
            if (dist > 0.0001) {
                // float3 radialDir = radial / dist;
                // // Compute the wave offset based on the distance and time.
                // float wave = sin(_Time.y * _WaveSpeed + dist * _WaveFrequency + v.vertex.x) * _WaveAmplitude;
                // // Offset the vertex along the radial direction.
                // v.vertex.xyz += radialDir * wave;

                float3 newPos = v.vertex.xyz;
                // Compute radial vector from the mesh center (for a basic radial component).
                float3 radial = newPos - _MeshCenter.xyz;
                float dist = length(radial);
                float waveRadial = sin(_Time.y * _WaveSpeed + dist * _WaveFrequency) * _WaveAmplitude;

                // Now add individual axis offsets.
                // You can use the vertex's own position for additional variation.
                float waveX = sin(_Time.y * _WaveSpeed + newPos.x * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.x;
                float waveY = sin(_Time.y * _WaveSpeed + newPos.y * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.y; // 90° phase shift for Y
                float waveZ = sin(_Time.y * _WaveSpeed + newPos.z * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.z; // 180° phase shift for Z

                // Combine the radial wave and the axis - specific offsets.
                // You can decide how much each component contributes (here we average the axis offsets).
                float3 axisOffset = float3(waveX, waveY, waveZ) * 0.5;

                // Optionally, blend between the radial and axis offsets based on your taste.
                float3 finalWaveOffset = radial != 0 ? normalize(radial) * waveRadial + axisOffset : axisOffset;

                // Apply the combined wave offset.
                v.vertex.xyz += finalWaveOffset;
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
