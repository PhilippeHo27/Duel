Shader "Custom/SuperStringTheory"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _AnimationSpeed ("Animation Speed", Range(0.1, 5.0)) = 1.0
        _Intensity ("Intensity", Range(0.1, 2.0)) = 1.0
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
            float _AnimationSpeed;
            float _Intensity;  // You were missing this declaration!
            
            // Helper functions
            float2x2 mat2_ctor_float4(float4 x0)
            {
                return float2x2(x0.x, x0.y, x0.z, x0.w);
            }
            
            float3 vec3_ctor(float2 x0, float x1)
            {
                return float3(x0, x1);
            }
            
            float4 vec4_ctor(float3 x0, float x1)
            {
                return float4(x0, x1);
            }
            
            float4 vec4_ctor_int(float3 x0, int x1)
            {
                return float4(x0, float(x1));
            }
            
            float4 tanh_emu(float4 x)
            {
                return (abs(x) > 15.0) ? sign(x) : tanh(x);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            void f_mainImage_float4(inout float4 _O, in float2 _C, float3 _iResolution, float _iTime)
            {
                float _d3042 = 0;
                float _s3043 = 0;
                float _j3044 = 0;
                float _i3045 = 0;
                float _z3046 = 0;
                float _N3047 = 0;
                float _D3048 = 0;
                float _k3049 = 0;
                float _t3050 = _iTime;
                float4 _o3051 = float4(0, 0, 0, 0);
                float4 _p3052 = float4(0, 0, 0, 0);
                float4 _U3053 = float4(3.0, 1.0, 2.0, 0.0);
                
                float2 _q3054 = float2(0, 0);
                float2 _r3055 = _iResolution.xy;
                bool sbf1 = ((++_i3045) < 70.0);
                
                for(; sbf1; )
                {
                    _p3052 = vec4_ctor_int((_z3046 * normalize(vec3_ctor((_C - (0.5 * _r3055)), _r3055.y))), 0);
                    _p3052.z -= 3.0;
                    _p3052.xz = mul(_p3052.xz, transpose(float2x2(0.800000012, 0.600000024, -0.800000012, 0.600000024)));
                    _p3052 *= (_k3049 = (8.0 / dot(_p3052, _p3052)));
                    _q3054 = _p3052.xy;
                    _q3054 -= (round((_q3054 / 5.0)) * 5.0);
                    float2x2 _R3056 = mat2_ctor_float4(cos((((0.5 * _t3050) + log2(_k3049)) + (11.0 * _U3053.wxyw))));
                    
                    for((_d3042 = ((_s3043 = ((_j3044 = (1.0 + 0.0)) + 0.0)) + 0.0)); ((++_j3044) < 6.0); (_s3043 *= (0.5 + 0.0)))
                    {
                        _q3054 = ((abs(mul(_q3054, transpose(_R3056))) - ((2.0 * _s3043) / _j3044)) + float2(0.0, 0.0));
                        _D3048 = ((length(_q3054) - (_s3043 / 8.0)) + 0.0);
                        
                        if ((_D3048 < _d3042))
                        {
                            _N3047 = (_j3044 + 0.0);
                            _d3042 = (_D3048 + 0.0);
                        }
                    }
                    
                    _d3042 = (abs(_d3042) / _k3049);
                    _p3052 = (1.0 + sin((((_p3052.z + _U3053.zywz) - _t3050) + _N3047)));
                    _o3051 += (((_p3052.w / max(_d3042, 0.00100000005)) * _p3052) + ((exp((0.300000012 * _k3049)) * 6.0) * _U3053));
                    
                    _z3046 += ((0.5 * _d3042) + 0.00100000005);
                    sbf1 = ((++_i3045) < 70.0);
                }
                
                _O = tanh_emu((_o3051 / (30000.0 / _Intensity)));
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenPos = i.screenPos.xy / i.screenPos.w;
                float2 fragCoord = screenPos * _ScreenParams.xy;
                float3 iResolution = float3(_ScreenParams.xy, 1.0);
                float iTime = _Time.y * _AnimationSpeed;
                
                float4 out_shadertoy_out_color = float4(1.0, 1.0, 1.0, 1.0);
                float4 _color3038 = float4(100000002004087734272.0, 100000002004087734272.0, 100000002004087734272.0, 100000002004087734272.0);
                
                f_mainImage_float4(_color3038, fragCoord, iResolution, iTime);
                
                if ((_color3038.x < 0.0))
                {
                    _color3038 = float4(1.0, 0.0, 0.0, 1.0);
                }
                if ((_color3038.y < 0.0))
                {
                    _color3038 = float4(0.0, 1.0, 0.0, 1.0);
                }
                if ((_color3038.z < 0.0))
                {
                    _color3038 = float4(0.0, 0.0, 1.0, 1.0);
                }
                if ((_color3038.w < 0.0))
                {
                    _color3038 = float4(1.0, 1.0, 0.0, 1.0);
                }
                
                out_shadertoy_out_color = vec4_ctor(_color3038.xyz, 1.0);
                return out_shadertoy_out_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
