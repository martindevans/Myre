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

void DefaultPixelShaderFunction(uniform bool ClipAlpha,
						 in DefaultVertexShaderOutput input,
						 out float4 out_depth : COLOR0,
						 out float4 out_normal : COLOR1,
						 out float4 out_diffuse : COLOR2)
{
    float4 diffuseSample = tex2D(diffuseSampler, input.TexCoord);

	if (ClipAlpha)
	{
		clip(diffuseSample.a < 0.5 ? -1 : 1);
	}

	float4 normalSample = tex2D(normalSampler, input.TexCoord);
	float4 specularSample = tex2D(specularSampler, input.TexCoord);

	float3 normal = normalSample.xyz * 2 - 1;
	normal = mul(normal, input.TangentToView);
	normal = normalize(normal);

	out_depth = float4(input.Depth, 0, 0, 1);
	out_normal = float4(EncodeNormal(normal), specularSample.r, 1);
	out_diffuse = float4(diffuseSample.rgb, specularSample.a);
}

technique Default
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction(true);
	}
}

technique DefaultNoAlphaCutout
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction(false);
	}
}

technique Animated
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 AnimatedVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction(true);
	}
}

technique AnimatedNoAlphaCutout
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 AnimatedVertexShaderFunction();
		PixelShader = compile ps_3_0 DefaultPixelShaderFunction(false);
	}
}