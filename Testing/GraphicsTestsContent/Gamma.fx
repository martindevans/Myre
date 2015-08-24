#include "GammaCorrection.fxh"
#include "FullScreenQuad.fxh"

texture Texture : TONEMAPPED;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

float4 GammaCorrectPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float3 colour = tex2D(textureSampler, in_TexCoord).rgb;
	//return float4(colour,1);
	return float4(LinearToGamma(colour), 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 GammaCorrectPS();
	}
}
