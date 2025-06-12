Shader "Custom/Zippy"
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
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            // Direct copy from compiled HLSL
            float2x2 mat2_ctor_float4(float4 x0)
            {
                return float2x2(x0);
            }

            float2 tanh_emu(float2 x)
            {
                return (abs(x) > 15.0) ? sign(x) : tanh(x);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get fragment coordinates like gl_FragCoord
                float2 gl_FragCoord = (i.screenPos.xy / i.screenPos.w) * _ScreenParams.xy;
                
                float2 _iResolution = _ScreenParams.xy;
                float _iTime = _Time.y;
                
                // Direct translation of the mainImage function
                float4 _o;
                float2 _u = gl_FragCoord;
                
                // Exact copy of the compiled logic
                float2 _v3044 = _iResolution.xy;
                _u = (0.200000003 * ((_u + _u) - _v3044)) / _v3044.y;
                float4 _z3045 = (_o = float4(1.0, 2.0, 3.0, 0.0));
                
                {
                    float _a3046 = 0.5;
                    float _t3047 = _iTime;
                    float _i3048 = 0;
                    bool sbe9 = ((++_i3048) < 19.0);
                    
                    for(; sbe9; )
                    {
                        {
                            _v3044 = (cos(((++_t3047) - ((7.0 * _u) * pow((_a3046 += 0.0299999993), _i3048)))) - (5.0 * _u));
                            _u += (((tanh_emu(((40.0 * dot((_u = mul(_u, transpose(mat2_ctor_float4(cos(((_i3048 + (0.0199999996 * _t3047)) - float4(0.0, 11.0, 33.0, 0.0))))))), _u)) * cos(((100.0 * _u.yx) + _t3047)))) / 200.0) + ((0.200000003 * _a3046) * _u)) + (cos(((4.0 / exp((dot(_o, _o) / 100.0))) + _t3047)) / 300.0));
                        }
                        _o += ((1.0 + cos((_z3045 + _t3047))) / length(((1.0 + (_i3048 * dot(_v3044, _v3044))) * sin(((((1.5 * _u) / (0.5 - dot(_u, _u))) - (9.0 * _u.yx)) + _t3047)))));
                        sbe9 = ((++_i3048) < 19.0);
                    }
                }
                
                _o = ((25.6000004 / (min(_o, 13.0) + (164.0 / _o))) - (dot(_u, _u) / 250.0));
                
                return _o;
            }
            ENDCG
        }
    }
}
