Shader "Custom/DitherFade"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _RoughnessMultiply("Roughness Multiply", Range(0, 2)) = 1
        _Metallic("Metallic Multiply", Range(0, 1)) = 1
        [Normal]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Scale", Float) = 1
        _SurfaceType("Surface Type", Float) = 1

        _DitherTex("Dither Texture", 2D) = "white" {} // Dither texture
        _Fade("Fade Amount", Range(0, 1)) = 1.0 // Fade control
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200


        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        #pragma target 3.0



        sampler2D _DitherTex; // Dither texture
        float _Fade;
        sampler2D _MainTex;
        sampler2D _Utility;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float4 screenPos; // Required for screen - space UVs
        };

        half _RoughnessMultiply;
        half _Metallic;
        half _BumpScale;
        half _SurfaceType;
        fixed4 _Color;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {




            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 u = tex2D(_Utility, IN.uv_MainTex);

            o.Albedo = c.rgb;

            // Metallic and smoothness come from slider variables
            o.Metallic = u.g * _Metallic;
            o.Smoothness = (1 - u.r) * (2 - _RoughnessMultiply);

            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);

            float2 screenUV = frac(IN.screenPos.xy / IN.screenPos.w * 0.5 + 0.5);
            fixed ditherValue = tex2D(_DitherTex, screenUV * 16).r;
            if (ditherValue > _Fade)
            {
                c.a = 0; // Fully transparent
            }


            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}