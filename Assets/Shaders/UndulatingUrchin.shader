Shader "Custom/UndulatingUrchin"
{
    Properties
    {
        _MainTex ("Texture", Cube) = "white" {}
        _Speed ("Speed", Float) = 1.0
        _Amplitude ("Amplitude", Float) = 9.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            HLSLPROGRAM
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
            
            samplerCUBE _MainTex;
            float _Speed;
            float _Amplitude;
            
            // Helper functions
            float3 Hue(float a)
            {
                return cos(float3(3.14159, 1.5708, 0.0) + a * 6.2832) * 0.5 + 0.5;
            }
            
            float Map(float3 u, float v)
            {
                float t = _Time.y * _Speed / 300.0;
                float l = 5.0;
                float f = 1e10;
                float i = 0.0;
                float y, z;
                
                // Polar transform
                u.xy = float2(atan2(u.x, u.y), length(u.xy));
                u.x += t * v * 3.1416 * 0.7;
                
                for (i = 0.0; i < l; i++)
                {
                    float3 p = u;
                    y = round((p.y - i) / l) * l + i;
                    p.x *= y;
                    p.x -= y * y * t * 3.1416;
                    p.x -= round(p.x / 6.2832) * 6.2832;
                    p.y -= y;
                    z = cos(y * t * 6.2832) * 0.5 + 0.5;
                    f = min(f, max(length(p.xy), -p.z - z * _Amplitude) - 0.1 - z * 0.2 - p.z / 100.0);
                }
                
                return f;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 U = (i.screenPos.xy / i.screenPos.w) * _ScreenParams.xy;
                float2 R = _ScreenParams.xy;
                
                // Fixed mouse position (no interaction)
                float2 m = float2(0.0, 0.5);
                
                float3 o = float3(0.0, 0.0, -130.0);  // camera
                float3 u = normalize(float3(U - R/2.0, R.y));  // 3d coords
                float3 c = float3(0, 0, 0);
                float3 p, k;
                
                float t = _Time.y * _Speed / 300.0;
                float v = -o.z / 3.0;  // pattern scale
                float rayI = 0.0;
                float d = rayI;
                float s, f, z, r;
                bool b;
                
                // Raymarch loop
                for (rayI = 0.0; rayI < 70.0; rayI++)
                {
                    p = u * d + o;
                    p.xy /= v;           // scale down
                    r = length(p.xy);    // radius
                    z = abs(1.0 - r * r); // z warp
                    b = r < 1.0;         // inside?
                    
                    if (b) z = sqrt(z);
                    
                    p.xy /= (z + 1.0);   // spherize
                    p.xy -= m;           // fixed position
                    p.xy *= v;           // scale back up
                    p.xy -= cos(p.z/8.0 + t*300.0 + float2(0.0, 1.5708) + z/2.0) * 0.2;
                    
                    s = Map(p, v);  // sdf
                    
                    r = length(p.xy);
                    f = cos(round(r) * t * 6.2832) * 0.5 + 0.5;
                    k = Hue(0.2 - f/3.0 + t + p.z/200.0);
                    
                    if (b) k = 1.0 - k;
                    
                    // Accumulate color
                    c += min(exp(s / -0.05), s)
                       * (f + 0.01)
                       * min(z, 1.0)
                       * sqrt(cos(r * 6.2832) * 0.5 + 0.5)
                       * k * k;
                    
                    if (s < 0.001 || d > 1000.0) break;
                    d += s * clamp(z, 0.2, 0.9);
                }
                
                // Additional effects
                c += texCUBE(_MainTex, u * d + o).rrr * float3(0, 0.4, s) * s * z * 0.03;
                c += min(exp(-p.z - f * _Amplitude) * z * k * 0.01 / s, 1.0);
                
                float2 j = p.xy / v + m;
                c /= clamp(dot(j, j) * 2.0, 0.04, 2.0);
                
                // Gamma correction
                c = exp(log(c) / 2.2);
                
                c = clamp(c, 0.0, 1.0);

                return float4(c, 1.0);
            }
            ENDHLSL
        }
    }
}
