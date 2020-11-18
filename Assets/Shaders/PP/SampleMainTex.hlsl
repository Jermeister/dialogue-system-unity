//TEXTURE2D(_MainTex);
//SAMPLER(sampler_MainTex);
//TEXTURE2D(_CameraColorTexture);
//SAMPLER(sampler_CameraColorTexture);

#ifndef SAMPLEMAINTEX_INCLUDED
#define SAMPLEMAINTEX_INCLUDED

void SampleMainTex_float(float2 uv, out float4 Color) {

	//sampler2D _MainTex;
	//SAMPLER(sampler_MainTex);


	//half4 _MainTex_ST;

	//SAMPLER2D(_MainTex, sampler_MainTex);
	//half4 main = tex2D(_MainTex, uv);
	float4 source = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv);

	//float alpha = step(t, 0.3) * lerp(0, 0.7, t * 3.3);
	//alpha += step(0.3, t) * lerp(0.7, 1, (t - 0.3) * 1.42);

	//float4 color = step(t, 0.3) * lerp(noFog, a, t * 3.3);
	//color += step(0.3, t) * step(t, 0.6) * lerp(a, b, (t - 0.3) * 3.3);
	//color += step(0.6, t) * lerp(b, c, saturate((t - 0.6) * 2.5));

	//Out = noFog;//float4(color.r, color.g, color.b, saturate(alpha));

	Color = source;// float4(1, 1, 0, 1);
}
#endif