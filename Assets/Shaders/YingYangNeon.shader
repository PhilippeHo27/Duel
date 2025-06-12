Shader "Custom/YingYangNeon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RotationSpeed ("Rotation Speed", Float) = 1.0
        _YinYangRatio ("Yin Yang Ratio", Range(-1.0, 1.0)) = 0.0
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0.2, 1)
        _GlowIntensity ("Glow Intensity", Float) = 3.0
        _WaveFrequency ("Wave Frequency", Float) = 96.0
        _WaveGlow ("Wave Glow", Float) = 0.05
        _AspectRatio ("Aspect Ratio", Float) = 1.0
        _Scale ("Scale", Float) = 1.0
        _UseMouseControl ("Use Mouse Control", Float) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        LOD 100
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float _RotationSpeed;
            float _YinYangRatio;
            float4 _GlowColor;
            float _GlowIntensity;
            float _WaveFrequency;
            float _WaveGlow;
            float _AspectRatio;
            float _Scale;
            float _UseMouseControl;
            
            // Zucconi color space function
            float3 zucconi(float x)
            {
                float3 coeffs = float3(3.54541731, 2.86670065, 2.29421997);
                float3 offsets = float3(0.695489168, 0.494169354, 0.282697082);
                float3 corrections = float3(0.0232077502, 0.15936245, 0.535200238);
                
                float3 term = coeffs * (x - offsets);
                return clamp(1.0 - (term * term) - corrections, 0.0, 1.0);
            }
            
            // Circle SDF
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }
            
            // Yin Yang SDF
            float sdYinYang(float2 p, float ratio)
            {
                p *= 2.0;
                float dotStrength = 0.125;
                ratio = clamp((ratio * 0.5) + 0.5, 0.0, 1.0);
                
                float outer = sdCircle(p, 1.0);
                float leftHalf = -p.x;
                float topCircle = sdCircle(p - float2(0.0, ratio), 1.0 - ratio);
                float bottomCircle = -sdCircle(p + float2(0.0, 1.0 - ratio), ratio);
                float topDot = sdCircle(p + float2(0.0, 1.0 - ratio), 2.0 * dotStrength * ratio);
                float bottomDot = -sdCircle(p - float2(0.0, ratio), 2.0 * dotStrength * (1.0 - ratio));
                
                return max(
                    min(
                        max(
                            min(
                                max(leftHalf, outer),
                                topCircle
                            ),
                            bottomCircle
                        ),
                        topDot
                    ),
                    bottomDot
                );
            }
            
            // Rotation matrix
            float2x2 rotationMatrix(float angle)
            {
                float c = cos(angle);
                float s = sin(angle);
                return float2x2(c, -s, s, c);
            }
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // Get screen position for shader calculations
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 screenCoord = screenUV * _ScreenParams.xy;
                
                // Convert to normalized coordinates with aspect ratio correction
                float2 p = (screenCoord - (_ScreenParams.xy / 2.0)) / min(_ScreenParams.y, _ScreenParams.x);
                
                // Apply aspect ratio correction
                p.x *= _AspectRatio;
                
                // Apply scale
                p *= _Scale;
                
                // Apply rotation
                float rotationAngle = (0.25 * _Time.y * _RotationSpeed) * 6.28318548;
                p = mul(rotationMatrix(rotationAngle), p);
                
                // Mouse coordinates (for color control if enabled)
                float2 m = float2(0.0, 0.0);
                if (_UseMouseControl > 0.5)
                {
                    m = (1.11111116 * (_ScreenParams.xy/2.0 - (_ScreenParams.xy / 2.0))) / min(_ScreenParams.y, _ScreenParams.x);
                }
                
                // Calculate yin-yang ratio
                float ratio = _YinYangRatio;
                ratio = ((lerp(0.125, 0.875, (ratio + 1.0) * 0.5) - 0.5) * 2.0);
                
                // Create the main shape (circle outline + yin-yang)
                float circle = max(sdCircle(p * 2.0, 1.001), -sdCircle(p * 2.0, 1.0));
                float shape = min(circle, sdYinYang(p, ratio));
                
                // Main glow effect
                float glow = _GlowIntensity * exp(-16.0 * pow(abs(shape), 0.5));
                
                // Color calculation
                float3 glowColor;
                if (_UseMouseControl > 0.5)
                {
                    glowColor = clamp(zucconi(((m.x / 1.1) * 0.5) + 0.5), 0.0, 1.0);
                }
                else
                {
                    glowColor = _GlowColor.rgb;
                }
                
                // Base color from glow
                float3 color = glowColor * glow;
                
                // Wave pattern
                float wave = sin((_WaveFrequency * p.y) + sin(sin(_WaveFrequency * p.x)));
                float waveGlow = _WaveGlow + exp(-2.67 * pow(abs(wave), 0.5));
                
                // Add wave effect to the filled areas
                color += (glowColor * (1.0 - smoothstep(-0.0115, 0.0, shape))) * waveGlow;
                
                // Tone mapping
                color = sqrt(tanh(color));
                
                // Apply UI color tint
                color *= IN.color.rgb;
                
                half4 finalColor = half4(color, 1.0);
                
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif
                
                return finalColor;
            }
            ENDCG
        }
    }
}
