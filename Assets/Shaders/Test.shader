Shader "Custom/WavySun"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SunColor ("Sun Color", Color) = (1, 0.9, 0.5, 1)
        _BackgroundColor ("Background Color", Color) = (0.2, 0.3, 0.4, 1)
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveIntensity ("Wave Intensity", Float) = 0.02
        _SunSize ("Sun Size", Float) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _SunColor;
            float4 _BackgroundColor;
            float _WaveSpeed;
            float _WaveIntensity;
            float _SunSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV coordinates
                float2 uv = i.uv;
                
                // Time-based distortion for wavy effect
                float distortionX = sin(uv.y * 10.0 + _Time.y * _WaveSpeed) * _WaveIntensity;
                float distortionY = cos(uv.x * 10.0 + _Time.y * _WaveSpeed) * _WaveIntensity;
                uv += float2(distortionX, distortionY);
                
                // Calculate distance from center
                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);
                
                // Create radial gradient for the sun
                float sunIntensity = saturate(1.0 - dist * _SunSize);
                
                // Apply wave effect to sun's intensity
                sunIntensity *= 0.5 + 0.5 * sin(_Time.y * 3.0 + dist * 20.0);
                
                // Combine colors
                float3 color = lerp(_BackgroundColor.rgb, _SunColor.rgb, sunIntensity);
                
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
