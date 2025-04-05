Shader "Custom/WavePulseBend" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        // Wave parameters
        _WaveAmplitude ("Wave Amplitude", Float) = 0.05
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
        // Pinch parameters:
        // _PinchRadius: The radius within which the pinch is effective.
        // _PinchDistance: How far (from the mesh center) the pinch center is offset.
        // _PinchStrength: How strong the pinch is.
        // _PinchDirection: The direction (normalized) from the mesh center toward the pinch center.
        _PinchRadius ("Pinch Radius", Float) = 1.0
        _PinchDistance ("Pinch Distance", Float) = 0.5
        _PinchStrength ("Pinch Strength", Float) = 0.2
        _PinchDirection ("Pinch Direction", Vector) = (0,0,1,0)
        // The mesh center should be provided in world space.
        _MeshCenter ("Mesh Center", Vector) = (0,0,0,0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Wave parameters
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;

            // Pinch parameters
            float _PinchRadius;
            float _PinchDistance;
            float _PinchStrength;
            float4 _PinchDirection;
            float4 _MeshCenter;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                // Convert vertex position to world space.
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // Compute the world normal (make sure to normalize it).
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // -------- Wave effect --------
                // Use the distance from the mesh center for a wave offset.
                float distFromCenter = length(worldPos - _MeshCenter.xyz);
                float wave = sin(_Time.y * _WaveSpeed + distFromCenter * _WaveFrequency) * _WaveAmplitude;
                float3 waveDisp = worldNormal * wave;

                // -------- Pinch effect --------
                // Compute the pinch center: offset from the mesh center along _PinchDirection.
                float3 pinchDir = normalize(_PinchDirection.xyz);
                float3 pinchCenter = _MeshCenter.xyz + pinchDir * _PinchDistance;

                // Distance from the pinch center.
                float d = distance(worldPos, pinchCenter);
                // Only affect vertices within the pinch radius.
                float pinchFactor = saturate(1.0 - d / _PinchRadius);
                // Pinch displacement pulls vertices toward the pinch center.
                float3 pinchDisp = (pinchCenter - worldPos) * _PinchStrength * pinchFactor;

                // To keep the overall mesh anchored, compute the displacement that would be applied
                // to the mesh center and subtract it from every vertex.
                float centerDist = distance(_MeshCenter.xyz, pinchCenter);
                float centerPinchFactor = saturate(1.0 - centerDist / _PinchRadius);
                float3 anchorDisp = (pinchCenter - _MeshCenter.xyz) * _PinchStrength * centerPinchFactor;
                float3 finalPinchDisp = pinchDisp - anchorDisp;

                // -------- Final position --------
                // The final vertex position combines the original world position with both effects.
                float3 finalPos = worldPos + waveDisp + finalPinchDisp;

                // Transform to clip space.
                o.vertex = UnityObjectToClipPos(float4(finalPos, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}