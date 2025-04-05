Shader "Custom/MonsterShader" {
    Properties {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _WhiteScale("White Vertex Scale", Range(0.65, 3)) = 1.0

        _MeshCenter("Mesh Center (Local)", Vector) = (0, 0, 0, 0)
        _PinchPosition("Pinch Position (Local)", Vector) = (0, 0, 0, 0)
        _WaveStrenght("Wave Strength (Local)", Vector) = (0, 0, 0, 0)
        _PinchStrength("Pinch Strength", Float) = 1.0
        _WaveAmplitude("Wave Amplitude", Float) = 0.1
        _WaveFrequency("Wave Frequency", Float) = 2.0
        _WaveSpeed("Wave Speed", Float) = 1.0

        // New Hue and Saturation properties.
        _Hue("Hue", Range(0, 360)) = 0
        _Saturation("Saturation", Range(0, 2)) = 1
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        #include "BoundsClip.cginc"

        // Declare the global clipping properties.
        float3 _ClipBoxMin;
        float3 _ClipBoxMax;

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        float _WhiteScale;

        float4 _MeshCenter;
        float4 _PinchPosition;
        float4 _WaveStrenght;
        float _PinchStrength;
        float _WaveAmplitude;
        float _WaveFrequency;
        float _WaveSpeed;

        // New uniforms for hue and saturation.
        float _Hue;
        float _Saturation;

        struct Input {
            float2 uv_MainTex;
            float3 color : COLOR; // Capture vertex color for scaling.
        };

        void vert(inout appdata_full v) {
            // -- - Monster Mouth Scaling -- -
            float3 vc = v.color.rgb;
            float intensity = (vc.r + vc.g + vc.b) / 3.0;
            float scaleFactor = lerp(1.0, _WhiteScale, intensity);
            v.vertex.xyz *= scaleFactor;

            // -- - Pinch Effect -- -
            float3 pos = v.vertex.xyz;
            float3 V = pos - _MeshCenter.xyz;
            float3 D = _PinchPosition.xyz - _MeshCenter.xyz;
            float dLen = length(D);
            if (dLen > 0.0001) {
                float3 Dnorm = D / dLen;
                float proj = saturate(dot(V, Dnorm) / dLen);
                float3 pinchDisplacement = Dnorm * _PinchStrength * proj;
                pos += pinchDisplacement;
            }

            // -- - Wave Offset -- -
            float3 radial = pos - _MeshCenter.xyz;
            float dist = length(radial);
            if (dist > 0.0001) {
                float waveRadial = sin(_Time.y * _WaveSpeed + dist * _WaveFrequency) * _WaveAmplitude;
                float waveX = sin(_Time.y * _WaveSpeed + pos.x * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.x;
                float waveY = sin(_Time.y * _WaveSpeed + pos.y * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.y;
                float waveZ = sin(_Time.y * _WaveSpeed + pos.z * _WaveFrequency) * _WaveAmplitude * _WaveStrenght.z;
                float3 axisOffset = float3(waveX, waveY, waveZ) * 0.5;
                float3 finalWaveOffset = normalize(radial) * waveRadial + axisOffset;
                pos += finalWaveOffset;
            }

            v.vertex.xyz = pos;
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Apply hue rotation.
            float angle = _Hue * 3.14159265 / 360.0;
            float s = sin(angle);
            float cosp = cos(angle);
            float3x3 hueRotation = float3x3(
            0.299 + 0.701 * cosp + 0.168 * s, 0.587 - 0.587 * cosp + 0.330 * s, 0.114 - 0.114 * cosp - 0.497 * s,
            0.299 - 0.299 * cosp - 0.328 * s, 0.587 + 0.413 * cosp + 0.035 * s, 0.114 - 0.114 * cosp + 0.292 * s,
            0.299 - 0.300 * cosp + 1.250 * s, 0.587 - 0.588 * cosp - 1.050 * s, 0.114 + 0.886 * cosp - 0.203 * s
            );
            fixed3 hueAdjusted = mul(hueRotation, c.rgb);

            // Apply saturation adjustment.
            float gray = dot(hueAdjusted, float3(0.299, 0.587, 0.114));
            hueAdjusted = lerp(float3(gray, gray, gray), hueAdjusted, _Saturation);

            o.Albedo = hueAdjusted;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
