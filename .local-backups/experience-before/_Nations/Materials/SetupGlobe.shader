Shader "Nations/Setup Globe"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 positionWS:TEXCOORD1; float2 uv:TEXCOORD2; };
            V vert(A a) { V o; o.positionWS=TransformObjectToWorld(a.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.positionWS); o.normalWS=TransformObjectToWorldNormal(a.normalOS); o.uv=a.uv; return o; }
            half4 frag(V i):SV_Target
            {
                float3 n=normalize(i.normalWS), view=normalize(GetWorldSpaceViewDir(i.positionWS));
                float rim=pow(1-saturate(dot(n,view)),4);
                float2 grid=abs(frac(i.uv*float2(24,12)+0.5)-0.5)/max(fwidth(i.uv*float2(24,12)),0.0001);
                float gridLine=1-saturate(min(grid.x,grid.y));
                float light=0.3+0.7*saturate(dot(n,normalize(float3(-0.4,0.6,-0.5))));
                return half4(float3(0.015,0.045,0.075)*light+gridLine*float3(0.018,0.035,0.05)+rim*float3(0.025,0.09,0.18),1);
            }
            ENDHLSL
        }
    }
}
