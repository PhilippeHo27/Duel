Shader "Custom/SpaceGif"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AnimationSpeed ("Animation Speed", Range(0.1, 5.0)) = 1.0
        _Offset ("Pattern Offset", Vector) = (0, 0, 0, 0)
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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AnimationSpeed;
            float4 _Offset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            float2 vec2_ctor(float x0, float x1)
            {
                return float2(x0, x1);
            }
            
            float4 vec4_ctor(float x0)
            {
                return float4(x0, x0, x0, x0);
            }
            
            float4 vec4_ctor(float3 x0, float x1)
            {
                return float4(x0, x1);
            }

            void mainImage(inout float4 fragColor, in float2 fragCoord)
            {
                // Get screen resolution
                float3 iResolution = float3(_ScreenParams.xy, 1.0);
                float iTime = _Time.y * _AnimationSpeed;
                
                // Apply offset to fragment coordinates
                fragCoord += _Offset.xy;
                
                float2 uv = ((fragCoord.xy - (iResolution.xy * 0.5)) / iResolution.y);
                
                // Apply rotation matrix (45 degrees)
                float2x2 rotMatrix = float2x2(0.707, -0.707, 0.707, 0.707);
                uv = mul(uv, transpose(rotMatrix));
                uv *= 15.0;
                
                float2 gv = (frac(uv) - 0.5);
                float2 id = floor(uv);
                float m = 0.0;
                float t = 0.0;
                
                // Double loop for neighboring cells
                for(float y = -1.0; y <= 1.0; y++)
                {
                    for(float x = -1.0; x <= 1.0; x++)
                    {
                        float2 offs = vec2_ctor(x, y);
                        t = ((-iTime) + (length((id - offs)) * 0.2));
                        float r = lerp(0.4, 1.5, ((sin(t) * 0.5) + 0.5));
                        float c = smoothstep(r, (r * 0.9), length((gv + offs)));
                        m = ((m * (1.0 - c)) + (c * (1.0 - m)));
                    }
                }
                
                fragColor = vec4_ctor(m);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Convert screen position back to fragment coordinates like the original
                float2 fragCoord = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                
                float4 col = float4(1, 1, 1, 1);
                mainImage(col, fragCoord);
                
                return fixed4(col.xyz, 1.0);
            }
            ENDCG
        }
    }
}
