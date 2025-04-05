Shader "Custom/PlayerCutout"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        // The player's screen position (normalized : 0 - 1).
        _PlayerScreenPos ("Player Screen Position", Vector) = (0.5, 0.5, 0, 0)
        // The base radius of the cutout circle (in normalized screen units).
        _PlayerRadius ("Player Radius", Float) = 0.1
        // How wide the transition edge is.
        _EdgeSoftness ("Edge Softness", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" }
        LOD 200

        CGPROGRAM
        // Use a surface shader with Standard lighting and smooth alpha fading.
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float4 _PlayerScreenPos;
        float _PlayerRadius;
        float _EdgeSoftness;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos : SV_POSITION;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = texColor.rgb;
            o.Metallic = 0.0;
            o.Smoothness = 0.5;
            float baseAlpha = texColor.a;

            // Compute normalized screen coordinates.
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

            // Get the aspect ratio from the built - in _ScreenParams.
            float aspect = _ScreenParams.x / _ScreenParams.y;

            // Correct the x coordinate so that we get a uniform circle.
            float2 correctedScreenUV = float2((screenUV.x - 0.5) * aspect + 0.5, screenUV.y);
            float2 correctedPlayerPos = float2((_PlayerScreenPos.x - 0.5) * aspect + 0.5, _PlayerScreenPos.y);

            // Calculate the distance from the fragment to the player's position.
            float dist = distance(correctedScreenUV, correctedPlayerPos);

            // Compute a smooth factor and clamp it so that alpha never falls below 0.4.
            float alphaFactor = clamp(smoothstep(_PlayerRadius, _PlayerRadius + _EdgeSoftness, dist), 0.4f, 1.0f);

            o.Alpha = baseAlpha * alphaFactor;
        }
        ENDCG
    }
    FallBack "Standard"
}
