Shader "Custom/GreekFriezeFive"
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
                float4 screenUV : TEXCOORD3;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AnimationSpeed;
            float4 _Offset;
            
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
            
            float atan_emu(float y, float x)
            {
                if(x == 0 && y == 0) x = 1;
                return atan2(y, x);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.screenUV = ComputeScreenPos(o.vertex);
                return o;
            }
            
            void mainImage(inout float4 fragColor, in float2 fragCoord)
            {
                float3 iResolution = float3(_ScreenParams.xy, 1.0);
                float iTime = _Time.y * _AnimationSpeed;
                
                fragCoord += _Offset.xy;
                
                float4 _O = float4(0, 0, 0, 0);
                float2 _U = fragCoord;
                
                float2 _R3042 = iResolution.xy;
                float2 _V3043 = float2(0, 0);
                
                float2 originalU = ((5.0 * ((_U + _U) - _R3042)) / _R3042.y);
                float2 dUdx = ddx(originalU);
                float2 dUdy = ddy(originalU);
                float derivMagnitude = max(length(abs(dUdx) + abs(dUdy)), 0.0001);
                
                _U = originalU;
                _U = vec2_ctor(((atan_emu(_U.y, _U.x) / 6.28299999) + 0.5), length(_U));
                _U.x = fmod(_U.x + 1.0, 1.0);
                _U.y -= _U.x;
                _U.x = (((2.5999999 * (ceil(_U.y) + _U.x)) * (ceil(_U.y) + _U.x)) - iTime);
                _O = vec4_ctor((1.0 - pow(abs(((2.0 * frac(_U.y)) - 1.0)), 10.0)));
                _V3043 = ceil(_U);
                _U = (frac(_U) - 0.5);
                _U.y = dot(_U, cos((float2(-33.0, 0.0) + ((0.300000012 * (iTime + _V3043.x)) * max(0.0, (0.5 - length(_U)))))));
                _O *= smoothstep(-1.0, 1.0, (_U.y / derivMagnitude));
                
                fragColor = _O;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelCoord = i.screenUV.xy / i.screenUV.w;
                float2 fragCoord = pixelCoord * _ScreenParams.xy;
                fragCoord = floor(fragCoord) + 0.5;
                
                float4 col = float4(1, 1, 1, 1);
                mainImage(col, fragCoord);
                
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
