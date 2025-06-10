Shader "Custom/PsychedelicSunset"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            // Helper functions
            float3 vec3_ctor(float2 x0, float x1)
            {
                return float3(x0, x1);
            }
            
            float4 vec4_ctor(float3 x0, float x1)
            {
                return float4(x0, x1);
            }
            
            float3 tanh_emu(float3 x)
            {
                return (abs(x) > 15.0) ? sign(x) : tanh(x);
            }
            
            // Main shader function adapted from the original
            float4 mainImage(float2 fragCoord, float3 iResolution, float iTime)
            {
                float z = 0.0;
                float d = 0.0;
                float s = 0.0;
                
                float3 dir = normalize(vec3_ctor(((2.0 * fragCoord) - iResolution.xy), (-1.0 * iResolution.y)));
                float3 col = float3(0.0, 0.0, 0.0);
                
                for(float i = 0.0; i < 100.0; i++)
                {
                    float3 p = z * dir;
                    
                    // Fractal noise generation
                    float j = 0.0;
                    float f = 5.0;
                    
                    for(int iter = 0; iter < 8; iter++)
                    {
                        p += ((0.6 * sin((p * f) - (float3(0.2, 0.2, 0.2) * iTime)).yzx) / f);
                        j++;
                        f *= 1.8;
                    }
                    
                    s = 0.3 - abs(p.y);
                    d = 0.005 + (max(s, (-s) * 0.2) / 4.0);
                    z += d;
                    
                    float phase = ((14.0 * s) + dot(p, float3(1.0, -1.0, 0.0))) + (0.5 * iTime);
                    col += ((cos(phase - float3(0.0, 1.0, 2.0)) + 1.5) * exp(s * 10.0)) / d;
                }
                
                col *= 0.00005;
                return vec4_ctor(tanh_emu(col * col), 1.0);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Get screen coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 fragCoord = screenUV * _ScreenParams.xy;
                
                // Unity resolution and time
                float3 iResolution = float3(_ScreenParams.xy, 1.0);
                float iTime = _Time.y;
                
                // Calculate the main shader effect
                fixed4 col = mainImage(fragCoord, iResolution, iTime);
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
