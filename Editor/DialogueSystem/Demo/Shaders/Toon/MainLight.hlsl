//Shader "Hidden/LightData"
//{
//	Properties
//	{
//	}
//
//	SubShader
//	{
//		Cull Off
//		ZWrite Off
//		ZTest Always
//		Pass
//		{
//			HLSLPROGRAM
//			//#pragma vertex vert
//			//#pragma fragment frag
//
//			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
//			//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
//			#include "HLSLSupport.cginc"
//			#include "UnityCG.cginc"
//
//			struct vertStruct {
//				float4 vertex : POSITION;
//				float2 uv : TEXCOORD0;
//			};
//
//			struct fragStruct {
//				float4 vertex : SV_POSITION;
//				float3 color : COLOR0;
//				float2 uv : TEXCOORD0;
//			};
//
//			void GetLightData(out float3 Direction, out float3 Color, out float Attenuation) {
//				Light light = GetMainLight();
//				Direction = light.direction;
//				Attenuation = light.distanceAttenuation;
//				Color = light.color;
//			}
//
//		}
//	}
//}

//#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/UnityInput.hlsl" 

void GetLightingInformation_float(out float3 Direction, out float3 Color, out float Attenuation)
{
//#ifdef SHADERGRAPH_PREVIEW
//        Direction = float3(-0.5,0.5,-0.5);
//        Color = float3(1,1,1);
//        Attenuation = 0.4;
//#endif

	Direction = float3 (0, 0, 0);
	Color = float3 (0, 0, 0);
	Attenuation = 0;
#ifdef LIGHTWEIGHT_LIGHTING_INCLUDED
		Light light = GetMainLight();
		Direction = light.direction;
		Attenuation = light.distanceAttenuation;
		Color = light.color;
#endif
}