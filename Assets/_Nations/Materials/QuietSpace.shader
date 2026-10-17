Shader "Nations/Quiet Space"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct V { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            V vert(float4 p:POSITION) { V o; o.positionCS=TransformObjectToHClip(p.xyz); o.direction=p.xyz; return o; }
            half4 frag(V i):SV_Target
            {
                float3 d=normalize(i.direction);
                float2 uv=float2(atan2(d.z,d.x)/6.283185+0.5,asin(d.y)/3.141593+0.5)*float2(1400,700);
                float2 cell=floor(uv); float hash=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
                float star=step(0.9985,hash)*(1-smoothstep(0.06,0.3,length(frac(uv)-0.5)))*0.28;
                return half4(float3(0.001,0.002,0.004)+star,1);
            }
            ENDHLSL
        }
    }
}
