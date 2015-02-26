#include "../FullScreenQuad.fxh"
#include "../EncodeNormals.fxh"

uniform texture GbufferDiffuse;
sampler gbufferDiffuseSampler = sampler_state
{
	Texture = (GbufferDiffuse);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

uniform texture GbufferNormals;
sampler gbufferNormalSampler = sampler_state
{
	Texture = (GbufferNormals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

uniform texture DecalDiffuse;
sampler decalDiffuseSampler = sampler_state
{
	Texture = (DecalDiffuse);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

uniform texture DecalNormals;
sampler decalNormalSampler = sampler_state
{
	Texture = (DecalNormals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

void MixDecalBufferPixelShaderFunction(in float2 in_TexCoord: TEXCOORD0, out float4 out_normals : COLOR0, out float4 out_diffuse : COLOR1)
{
	//Mix colours according to the alpha of the decal. Output the original alpha of the gbuffer (the specular value, unmodified by decal)
	float4 gbufferDiffuse = tex2D(gbufferDiffuseSampler, in_TexCoord);
	float4 decalDiffuse = tex2D(decalDiffuseSampler, in_TexCoord);
	out_diffuse = float4(gbufferDiffuse.rgb * (1 - decalDiffuse.a) + decalDiffuse.rgb * decalDiffuse.a, gbufferDiffuse.a);

	//Mix normals according to the alpha of the decal normal buffer.
	float4 gbufferNormalSample = tex2D(gbufferNormalSampler, in_TexCoord);
	float3 gbufferNormal = DecodeNormal(gbufferNormalSample.xy);
	float4 decalNormalSample = tex2D(decalNormalSampler, in_TexCoord);
	float3 decalNormal = DecodeNormal(decalNormalSample.xy);
	out_normals = float4(EncodeNormal(normalize(gbufferNormal * (1 - decalNormalSample.a) + decalNormal * decalNormalSample.a)), gbufferNormalSample.ba);
}

technique MixDecalBuffers
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 FullScreenQuadVS();
		PixelShader = compile ps_2_0 MixDecalBufferPixelShaderFunction();
	}
}
