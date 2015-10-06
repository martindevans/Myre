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

texture Stencil;
sampler stencilSample = sampler_state
{
	Texture = (Stencil);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

void CopyFunction(uniform bool readStencil,
	in float2 in_TexCoord: TEXCOORD0,
	out float4 out_Colour : COLOR0)
{
	if (readStencil)
	{
		float4 sampledStencil = tex2D(stencilSample, in_TexCoord);
		if (sampledStencil.a == 0)
			clip(-1);
	}
	
	out_Colour = float4(tex2D(textureSampler, in_TexCoord).rgb, 1);
}

technique Copy
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 CopyFunction(false);
	}
}

technique CopyWithStencil
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 CopyFunction(true);
	}
}