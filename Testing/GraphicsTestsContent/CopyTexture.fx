#include "FullScreenQuad.fxh"

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

void PixelShaderFunction(in float2 in_TexCoord: TEXCOORD0,
						 out float4 out_Colour: COLOR0)
{
	out_Colour = float4(tex2D(textureSampler, in_TexCoord).rgb, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}