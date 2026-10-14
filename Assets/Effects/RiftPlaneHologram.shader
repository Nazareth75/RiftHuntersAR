Shader "RiftHunters/PlaneHologram"
{
    Properties { _BaseColor("Color",Color)=(0.02,0.65,1,0.10) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            Varyings vert(Attributes input)
            {
                Varyings output; output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS=TransformWorldToHClip(output.positionWS); return output;
            }
            half4 frag(Varyings input):SV_Target
            {
                float2 uv=input.positionWS.xz/0.2;
                float2 edge=abs(frac(uv-0.5)-0.5)/max(fwidth(uv),0.001);
                float gridLine=1-saturate(min(edge.x,edge.y));
                return half4(_BaseColor.rgb,_BaseColor.a+gridLine*0.18);
            }
            ENDHLSL
        }
    }
}


