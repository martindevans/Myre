#include "FullScreenQuad.fxh"
#include "ToneMapFunctions.fxh"
#include "GammaCorrection.fxh"

float BloomMagnitude : HDR_BLOOMMAGNITUDE;

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

texture Luminance;
sampler luminanceSampler = sampler_state
{
	Texture = (Luminance);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

texture Bloom : BLOOM;
sampler bloomSampler = sampler_state
{
	Texture = (Bloom);
	MinFilter = Linear;
	MagFilter = Linear;
};

float4 ToneMapPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float averageLuminance = tex2D(luminanceSampler, float2(0.5, 0.5)).x; //exp2(tex2Dlod(luminanceSampler, float4(in_TexCoord, 0, 11)).x);
	float3 colour = tex2D(textureSampler, in_TexCoord).rgb;
	colour = CalcExposedColor(colour, averageLuminance, 0);
	colour = ToneMapFilmicALU(colour);

	float3 bloom = tex2D(bloomSampler, in_TexCoord).rgb;
	colour += bloom * BloomMagnitude;

	return float4(colour, 1);//LinearToGamma(colour), 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ToneMapPS();
	}
}
