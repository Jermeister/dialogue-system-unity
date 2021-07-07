//#ifndef CUSTOM_LIGHTING_INCLUDED
//#define CUSTOM_LIGHTING_INCLUDED

void DetectPosition_float(float3 pos, float3 charPos, float1 radius, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else

	float dist = sqrt((charPos.x - pos.x) * 2 + (charPos.y - pos.y) * 2);

	if (dist < radius)
		Out = float3(1, 1, 1);
	else
		Out = float3(0, 0, 0);
#endif
}

//void DetectPosition_half(float3 pos, float3 charPos, float1 radius, out float3 Out)
//{
//#if SHADERGRAPH_PREVIEW
//	Out = 0;
//#else
//
//	if (pos.x < 5)
//		Out = float3(1, 1, 1);
//	else
//		Out = float3(0, 0, 0);
//	//Smoothness = exp2(10 * Smoothness + 1);
//	//WorldNormal = normalize(WorldNormal);
//	//WorldView = SafeNormalize(WorldView);
//	//Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, float4(Specular, 0), Smoothness);
//#endif
//}

//#endif
