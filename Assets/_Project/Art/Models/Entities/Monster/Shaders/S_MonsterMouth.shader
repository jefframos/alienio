Shader "Jeff/Monster Mouth"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _WhiteScale("White Vertex Scale", Range(0.65,3)) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows vertex:vert
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
                float3 color : COLOR; // Capturing vertex color
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            float _WhiteScale;

            void vert(inout appdata_full v)
            {
                float3 vc = v.color.rgb;
                float intensity = (vc.r + vc.g + vc.b) / 3.0;
                float scaleFactor = lerp(1.0, _WhiteScale, intensity);
                v.vertex.xyz *= scaleFactor;
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}