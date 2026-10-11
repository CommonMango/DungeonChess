Shader "DungeonChess/PlacementTile"
{
    Properties { _Color ("Color", Color) = (0.15,1,0.45,0.8) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float edge = max(abs(input.uv.x - 0.5), abs(input.uv.y - 0.5));
                float border = smoothstep(0.42, 0.45, edge);
                float outer = 1.0 - smoothstep(0.47, 0.5, edge);
                return half4(_Color.rgb, _Color.a * lerp(0.18, 1.0, border) * outer);
            }
            ENDHLSL
        }
    }
}
