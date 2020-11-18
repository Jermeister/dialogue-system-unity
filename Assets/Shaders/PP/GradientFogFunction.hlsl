﻿//#ifndef GRADIENTFOGFUNCTION_INCLUDED
//      #define GRADIENTFOGFUNCTION_INCLUDED
//      
//      float4 noFog = float4(0,0,0,1);
//      
//      void FogGradient_float(float4 a, float4 b, float4 c, float t, out float4 Out) {
//          float alpha = step(t, 0.3) * lerp(0, 0.7, t*3.3);
//          alpha += step(0.3, t) * lerp(0.7, 1, (t-0.3)*1.42);
//          
//          float4 color = step(t, 0.3) * lerp(noFog, a, t*3.3);
//          color += step(0.3, t) * step(t, 0.6) * lerp(a, b, (t-0.3)*3.3);
//          color += step(0.6, t) * lerp(b,c, saturate((t-0.6)*2.5));
//          
//          Out = float4(color.r, color.g, color.b, saturate(alpha));
//      }
//      #endif



#ifndef GRADIENTFOGFUNCTION_INCLUDED
#define GRADIENTFOGFUNCTION_INCLUDED

	  float4 noFog = float4(0, 0, 0, 1);

	  void FogGradient_float(float3 camPos, float3 screenPos, float fogDensity, float fogThreshold, out float fog) {

		  float3 camToObj = camPos - screenPos;
		  float t;
		  if(screenPos.y < fogThreshold)
		  {
			  if (camPos.y > fogThreshold)
			  {
				  t = (fogThreshold - screenPos.y) / camToObj.y;
			  }
			  else
			  {
				  t = 1.0;
			  }
		  }
		  float distance = length(camToObj) * t;
		  fog = exp(distance * fogDensity);

		  if (screenPos.y < fogThreshold)
			  fog = 0;
	  }
#endif