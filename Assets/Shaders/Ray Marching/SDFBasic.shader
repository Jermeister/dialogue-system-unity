Shader "Unlit/SDFBasic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Pass
        {
            //Blend SrcAlpha DstAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

            #include "UnityCG.cginc"

            
            //##include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            //#include "2DSDF.hlslinc"
            
            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 0.001

            struct appdata{
                float4 vertex : POSITION;
            };

            struct v2f{
                float4 position : SV_POSITION;
                float2 screenPos : TEXCOORD0;
                float3 rayOrigin : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            v2f vert(appdata v){
                v2f o;
                //calculate the position in clip space to render the object
                o.position = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(v.vertex);
                o.rayOrigin = _WorldSpaceCameraPos;//GetAbsolutePositionWS(float3(0, 0, 0));
                o.hitPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }


            
            float GetDistSphere(float3 p)
            {
                float d = length(p) - .5; // sphere            
                return d;
            }
            
            float GetDistTorus(float3 p)
            {
                float d = length(p) - .5; // sphere
                d = length(float2(length(p.xz)- 0.5, p.y)) - 0.1;
            
                return d;
            }
            
            float Raymarch(float3 rayOrigin, float3 rayDirection)
            {
                float dO = 0;
                float dS;
                for(int i = 0; i < MAX_STEPS; i++)
                {
                    float3 p = rayOrigin + dO * rayDirection;
                    dS = GetDistTorus(p);
                    dO += dS;
                    if(dS<SURF_DIST || dO > MAX_DIST)
                    {
                        break;
                    }
                }
                return dO;
            }
            
            float3 GetNormal(float3 p)
            {
                float2 offset = float2(0.01, 0);
                float3 n = GetDistTorus(p) - float3(
                    GetDistTorus(p+offset.xyy),
                    GetDistTorus(p+offset.yxy),
                    GetDistTorus(p+offset.yyx)
                );
                
                return normalize(n);
            }

            fixed4 frag(v2f i) : SV_TARGET{
                float2 uv = i.screenPos - 0.5;
                float3 ro = i.rayOrigin;
                float3 rd = normalize(i.hitPos - ro);
                
                float d = Raymarch(ro, rd);
                fixed4 col = fixed4(0, 0, 0, 0);
                
                if(d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    float3 n = GetNormal(p);
                    col.rgb = n;
                    col.w = 1;
                }
                else
                discard;

                return col;
            }
            ENDHLSL
        }
    }
}
