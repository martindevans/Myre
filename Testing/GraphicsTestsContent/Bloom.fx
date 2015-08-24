#include "FullScreenQuad.fxh"
#include "ToneMapFunctions.fxh"

float Threshold;

texture Texture;
sampler pointSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};
sampler linearSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
};

texture Luminance;
sampler luminanceSampler = sampler_state
{
	Texture = (Luminance);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

float3 BilinearSample(float2 texCoord)
{
	float offset = 0.5 / (Resolution * 2);

	float3 a = tex2D(pointSampler, texCoord + float2(-offset, -offset)).rgb;
	float3 b = tex2D(pointSampler, texCoord + float2(offset, -offset)).rgb;
	float3 c = tex2D(pointSampler, texCoord + float2(-offset, offset)).rgb;
	float3 d = tex2D(pointSampler, texCoord + float2(offset, offset)).rgb;

	return (a + b + c + d) / 4;
}

void ThresholdPS(in float2 in_TexCoord : TEXCOORD0,
				 out float4 out_Colour : COLOR0)
{
	float3 colour = BilinearSample(in_TexCoord);
	float averageLuminance = tex2D(luminanceSampler, float2(0.5, 0.5)).x;

	float3 exposed = CalcExposedColor(colour, averageLuminance, Threshold);
	out_Colour = float4(ToneMapFilmicALU(exposed), 1);
}

void ScalePS(in float2 in_TexCoord : TEXCOORD0,
			 out float4 out_Colour : COLOR0)
{
	out_Colour = tex2D(linearSampler, in_TexCoord);
}


technique ThresholdDownsample2X
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ThresholdPS();
	}
}

technique Scale
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ScalePS();
	}
}