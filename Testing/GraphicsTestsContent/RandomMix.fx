#include "FullScreenQuad.fxh"

texture Left;
sampler leftSampler = sampler_state
{
	Texture = (Left);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

texture Right;
sampler rightSampler = sampler_state
{
	Texture = (Right);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

float Random : SEED;

void PixelShaderFunction(in float2 in_TexCoord: TEXCOORD0,
						 out float4 out_Colour: COLOR0)
{
	float x = (in_TexCoord.x + 1) * (in_TexCoord.y + 1) * (Random + 1) * 1000;	// make some noise
	x = fmod(x, 13) * fmod(x, 123);												// make some noise
	float n = fmod(x, 1);														// make some noise

	out_Colour = float4(lerp(tex2D(leftSampler, in_TexCoord).rgb, tex2D(rightSampler, in_TexCoord).rgb, n), 1);
}

technique Mix
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}