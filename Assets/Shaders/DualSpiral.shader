Shader "Custom/DualSpiralShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Spiral Parameters
        _SpiralSpeed ("Spiral Animation Speed", Float) = 0.3
        _SpiralFrequency ("Spiral Frequency", Float) = 100.0
        _SpiralTightness ("Spiral Tightness", Float) = 0.02
        _Sharpness ("Sharpness (less blur)", Range(1.0, 10.0)) = 3.0
        
        // Orbit Parameters
        _OrbitSpeed ("Orbit Speed", Float) = 0.5
        _OrbitRadius ("Orbit Radius", Float) = 0.3
        _OrbitCenter ("Orbit Center", Vector) = (0.5, 0.5, 0, 0)
        
        // Colors
        _SpiralColor1 ("Spiral Color 1", Color) = (1, 0.5, 0.2, 1)
        _SpiralColor2 ("Spiral Color 2", Color) = (0.2, 0.8, 1, 1)
        _BackgroundColor ("Background Color", Color) = (0.05, 0.05, 0.1, 1)
        
        // Blend mode
        _BlendMode ("Blend Mode", Float) = 1.0
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
            
            // Shader parameters
            float _SpiralSpeed;
            float _SpiralFrequency;
            float _SpiralTightness;
            float _Sharpness;
            float _OrbitSpeed;
            float _OrbitRadius;
            float2 _OrbitCenter;
            float4 _SpiralColor1;
            float4 _SpiralColor2;
            float4 _BackgroundColor;
            float _BlendMode;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            // Helper functions
            float2 vec2_ctor(float x0, float x1)
            {
                return float2(x0, x1);
            }
            
            float3 vec3_ctor(float x0)
            {
                return float3(x0, x0, x0);
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
            
            // Enhanced spiral function with sharpness control
            float spiral(float2 m, float t)
            {
                float r = length(m);
                float a = atan_emu(m.y, m.x);
                float v = sin(_SpiralFrequency * ((sqrt(r) - (_SpiralTightness * a)) - (_SpiralSpeed * t)));
                
                // Apply sharpness to reduce blur
                v = pow(abs(v), 1.0 / _Sharpness) * sign(v);
                
                return clamp(v, 0.0, 1.0);
            }
            
            // Function to rotate a point around another point
            float2 rotateAround(float2 centerpoint, float2 center, float angle)
            {
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 offset = centerpoint - center;
                return center + float2(
                    offset.x * cosA - offset.y * sinA,
                    offset.x * sinA + offset.y * cosA
                );
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Get screen coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 fragCoord = screenUV * _ScreenParams.xy;
                float2 uv = fragCoord.xy / _ScreenParams.y; // Keep aspect ratio
                
                float t = _Time.y;
                
                // Calculate orbiting spiral centers
                float orbitAngle = t * _OrbitSpeed;
                
                // First spiral orbits clockwise
                float2 center1 = _OrbitCenter + float2(
                    _OrbitRadius * cos(orbitAngle),
                    _OrbitRadius * sin(orbitAngle)
                );
                
                // Second spiral orbits opposite (180 degrees apart)
                float2 center2 = _OrbitCenter + float2(
                    _OrbitRadius * cos(orbitAngle + 3.14159),
                    _OrbitRadius * sin(orbitAngle + 3.14159)
                );
                
                // Calculate spiral values
                float spiral1 = spiral(center1 - uv, t);
                float spiral2 = spiral(center2 - uv, t);
                
                // Color mixing
                float3 col = _BackgroundColor.rgb;
                
                if (_BlendMode > 0.5)
                {
                    // Additive blending
                    col += spiral1 * _SpiralColor1.rgb;
                    col += spiral2 * _SpiralColor2.rgb;
                }
                else
                {
                    // Mix blending
                    col = lerp(col, _SpiralColor1.rgb, spiral1);
                    col = lerp(col, _SpiralColor2.rgb, spiral2 * (1.0 - spiral1));
                }
                
                // Enhanced contrast and brightness
                col = saturate(col);
                
                fixed4 finalColor = fixed4(col, 1.0);
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
