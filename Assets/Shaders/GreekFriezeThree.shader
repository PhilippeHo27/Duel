Shader "Custom/GreekFriezeThree"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AnimationSpeed ("Animation Speed", Range(0.1, 5.0)) = 1.0
        _Offset ("Pattern Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AnimationSpeed;
            float4 _Offset;
            
            // Helper functions from original HLSL
            float4 vec4_ctor(float3 x0, float x1)
            {
                return float4(x0, x1);
            }
            
            float mod_emu(float x, float y)
            {
                return x - y * floor(x / y);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            // Main image function converted from original
            void mainImage(inout float4 fragColor, in float2 fragCoord)
            {
                // Get screen resolution and time - using the properties
                float3 iResolution = float3(_ScreenParams.xy, 1.0);
                float iTime = _Time.y * _AnimationSpeed;
                
                // Apply offset to fragment coordinates (safe implementation)
                fragCoord += _Offset.xy;
                
                float4 _O = float4(0, 0, 0, 0);
                float2 _u = fragCoord;
                
                _O *= 0.0;
                float2 _U3042 = ((8.0 * _u) / iResolution.y);
                float2 _V3043 = float2(0, 0);
                _U3042.x -= iTime;
                _V3043 = floor(_U3042);
                float _s3044 = sign((mod_emu(_U3042.y, 2.0) - 1.0));
                float sbe5 = 0.0;
                
                if (_V3043.y > 3.0)
                {
                    sbe5 = _s3044;
                }
                else
                {
                    sbe5 = 1.0;
                }
                
                _U3042.y = dot(cos(((((2.0 * (iTime + _V3043.x)) * sbe5) * max(0.0, (0.5 - length((_U3042 = (frac(_U3042) - 0.5)))))) - float2(33.0, 0.0))), _U3042);
                _O += smoothstep(-1.0, 1.0, ((_s3044 * _U3042) / fwidth(_U3042))).y;
                
                fragColor = _O;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Convert screen position back to fragment coordinates (same as SpaceGif)
                float2 fragCoord = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                
                float4 col = float4(1, 1, 1, 1);
                mainImage(col, fragCoord);
                
                // Error visualization from original (optional)
                if (col.x < 0.0)
                    col = float4(1.0, 0.0, 0.0, 1.0);
                if (col.y < 0.0)
                    col = float4(0.0, 1.0, 0.0, 1.0);
                if (col.z < 0.0)
                    col = float4(0.0, 0.0, 1.0, 1.0);
                if (col.w < 0.0)
                    col = float4(1.0, 1.0, 0.0, 1.0);
                
                return fixed4(col.xyz, 1.0);
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
