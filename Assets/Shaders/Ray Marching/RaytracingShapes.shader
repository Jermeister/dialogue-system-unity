Shader "Unlit/RaytracingShapes"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

            #include "UnityCG.cginc"
            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 0.001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 rayOrigin : TEXCOORD3;
                float3 hitPos : TEXCOORD4;
            };

            sampler2D _MainTex;
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.hitPos = v.vertex;
                return o;
            }

            float Capsule(float p, float3 a, float3 b, float r) 
            {

            }

            float GetDist(float3 p)
            {
                float sphereDist = length(p) - .5; // sphere
                //d = length(float2(length(p.xz) - 0.5, p.y)) - 0.1;
                float capsuleDist = CapsuleDist(p, );

                float d = min(sphereDist, capsuleDist);
                return d;
            }

            float Raymarch(float3 rayOrigin, float3 rayDirection)
            {
                float dO = 0;
                float dS;
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 p = rayOrigin + dO * rayDirection;
                    dS = GetDist(p);
                    dO += dS;
                    if (dS<SURF_DIST || dO > MAX_DIST)
                    {
                        break;
                    }
                }
                return dO;
            }
             
            float3 GetNormal(float3 p) 
            {
                float2 e = float2(1e-2, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx));
                return normalize(n);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float3 rayOrigin = i.rayOrigin;
                float3 rayDir = normalize(i.hitPos - rayOrigin);

                float d = Raymarch(rayOrigin, rayDir);
                fixed4 col = tex2D(_MainTex, i.uv);//fixed4(0, 0, 0, 0);

                if (d < MAX_DIST) {
                    float3 p = rayOrigin + rayDir * d;
                    float3 n = GetNormal(p);
                    col.rgb = n;
                    col.w = 1;
                }
                else
                    discard;

                return col;
            }
            ENDCG
        }
    }
}
