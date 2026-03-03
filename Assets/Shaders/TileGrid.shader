Shader "Custom/TileGrid"
{
    Properties
    {
        _MeshSize      ("Tile World Size", Float) = 1.0
        _GridLineWidth ("Grid Line Width", Range(0.001, 0.1)) = 0.03
        _GridLineColor ("Grid Line Color", Color) = (0, 0, 0, 0.3)
        _HoverLineWidth("Hover Line Width", Range(0.001, 0.15)) = 0.06
        _HoverColor    ("Hover Outline Color", Color) = (1, 1, 1, 0.8)
        _HoverFillColor("Hover Fill Tint", Color) = (1, 1, 1, 0.1)
        _HoveredTile   ("Hovered Tile (xy=coord, z=active)", Vector) = (-1, -1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "TileGridPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionOS  : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 vertexColor : COLOR;
                float  fogFactor   : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float  _MeshSize;
                float  _GridLineWidth;
                float4 _GridLineColor;
                float  _HoverLineWidth;
                float4 _HoverColor;
                float4 _HoverFillColor;
                float4 _HoveredTile;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs  = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS  = posInputs.positionCS;
                OUT.positionOS  = IN.positionOS.xyz;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = normInputs.normalWS;
                OUT.vertexColor = IN.color;
                OUT.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Base color from vertex colors ---
                half3 baseColor = IN.vertexColor.rgb;

                // --- Simple NdotL lighting ---
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalize(IN.normalWS), mainLight.direction));
                half3 litColor = baseColor * (mainLight.color * NdotL * 0.6 + 0.4);

                // --- Tile coordinates from object-space position ---
                float2 tileUV   = IN.positionOS.xz / _MeshSize;       // continuous tile coords
                float2 tileCoord = floor(tileUV);                      // integer tile id
                float2 tileLocal = frac(tileUV);                       // 0-1 within tile

                // --- Grid lines for ALL tiles ---
                // Compute distance to nearest edge (0 or 1) in each axis
                float2 edgeDist = min(tileLocal, 1.0 - tileLocal);
                float  minEdge  = min(edgeDist.x, edgeDist.y);

                // Anti-aliased line using fwidth for screen-space derivatives
                float  fw       = fwidth(minEdge);
                float  gridLine = 1.0 - smoothstep(_GridLineWidth - fw, _GridLineWidth + fw, minEdge);

                // Blend grid line
                half3 color = lerp(litColor, _GridLineColor.rgb, gridLine * _GridLineColor.a);

                // --- Hovered tile outline ---
                if (_HoveredTile.z > 0.5)
                {
                    // Check if this fragment is in the hovered tile
                    bool isHovered = (abs(tileCoord.x - _HoveredTile.x) < 0.5) &&
                                     (abs(tileCoord.y - _HoveredTile.y) < 0.5);

                    if (isHovered)
                    {
                        // Stronger outline
                        float hoverLine = 1.0 - smoothstep(_HoverLineWidth - fw, _HoverLineWidth + fw, minEdge);
                        color = lerp(color, _HoverColor.rgb, hoverLine * _HoverColor.a);

                        // Subtle fill tint
                        color = lerp(color, _HoverFillColor.rgb, _HoverFillColor.a);
                    }
                }

                // --- Fog ---
                color = MixFog(color, IN.fogFactor);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster pass so the grid casts shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
