#include "GBufferHeader.fxh"

uniform texture DiffuseMap;
uniform texture NormalMap;
uniform texture SpecularMap;

uniform float Opacity : OPACITY;

sampler diffuseSampler = sampler_state
{
	Texture = (DiffuseMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

sampler specularSampler = sampler_state
{
	Texture = (SpecularMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

float4 DefaultPixelShaderFunction(in DefaultVertexShaderOutput input) : COLOR0
{
	return float4(1, 0, 0, 1);
}

technique Default
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction();
	}
}

technique Animated
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 AnimatedVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction();
	}
}