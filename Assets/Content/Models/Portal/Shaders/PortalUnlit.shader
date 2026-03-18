Shader "Portal/PortalUnlit"
{
    Properties
    {
        [HDR] _Color1       ("Color Inner",    Color)  = (0.1, 0.4, 1.0, 1)
        [HDR] _Color2       ("Color Outer",    Color)  = (0.6, 0.1, 1.0, 1)
        [HDR] _EdgeGlow     ("Edge Glow",      Color)  = (1.0, 1.0, 1.0, 1)
        _TwirlSpeed         ("Twirl Speed",    Float)  = 1.2
        _TwirlStrength      ("Twirl Strength", Float)  = 3.5
        _Rings              ("Ring Count",     Float)  = 6.0
        _NoiseScale         ("Noise Scale",    Float)  = 4.0
        _NoiseSpeed         ("Noise Speed",    Float)  = 0.4
        _EdgeWidth          ("Edge Width",     Range(0,0.5)) = 0.08
        _AlphaClip          ("Alpha Clip",     Range(0,1))   = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PortalUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            // Properties
            float4 _Color1;
            float4 _Color2;
            float4 _EdgeGlow;
            float  _TwirlSpeed;
            float  _TwirlStrength;
            float  _Rings;
            float  _NoiseScale;
            float  _NoiseSpeed;
            float  _EdgeWidth;
            float  _AlphaClip;

            // ── Helpers ──────────────────────────────────────────────────────

            // Simple 2D hash-based value noise
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                return lerp(
                    lerp(hash(i),              hash(i + float2(1,0)), u.x),
                    lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x),
                    u.y
                );
            }

            // Twirl: rotate UV around center by angle proportional to distance
            float2 twirl(float2 uv, float strength, float speed)
            {
                float2  center = uv - 0.5;
                float   dist   = length(center);
                float   angle  = dist * strength - _Time.y * speed;
                float   s      = sin(angle);
                float   c      = cos(angle);
                float2x2 rot   = float2x2(c, -s, s, c);
                return mul(rot, center) + 0.5;
            }

            // ── Vertex ───────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            // ── Fragment ─────────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Center the UV to [-1, 1]
                float2 centered = uv * 2.0 - 1.0;
                float  dist     = length(centered);

                // Discard outside the disc
                if (dist > 1.0) discard;

                // ── Twirl ──
                float2 twirlUV = twirl(uv, _TwirlStrength, _TwirlSpeed);

                // ── Noise ──
                float2 noiseUV = twirlUV * _NoiseScale + _Time.y * _NoiseSpeed;
                float  noise   = valueNoise(noiseUV) * 0.5
                               + valueNoise(noiseUV * 2.1 + 3.7) * 0.3
                               + valueNoise(noiseUV * 4.3 + 7.1) * 0.2;

                // ── Rings ──
                float ringPattern = abs(sin(dist * _Rings * 3.14159 + _Time.y * _TwirlSpeed));
                ringPattern = pow(ringPattern, 1.5);

                // ── Color blend ──
                float  blend = saturate(dist + noise * 0.4);
                float4 col   = lerp(_Color1, _Color2, blend);

                // Mix in ring highlights
                col.rgb += ringPattern * 0.15 * _Color1.rgb;

                // ── Edge glow ──
                float edgeFactor = smoothstep(1.0 - _EdgeWidth, 1.0, dist);
                col.rgb = lerp(col.rgb, _EdgeGlow.rgb, edgeFactor);

                // ── Alpha ──
                // Solid disc, soft edge fade
                float alpha = 1.0 - smoothstep(0.92, 1.0, dist);
                col.a = alpha;

                if (col.a < _AlphaClip) discard;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
