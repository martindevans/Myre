// adapted from a RenderMonkey implementation
// by ArKano22, http://www.gamedev.net/community/forums/topic.asp?topic_id=556187

#include "FullScreenQuad.fxh"
#include "EncodeNormals.fxh"

texture Depth : GBUFFER_DEPTH;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Random;
sampler randomSampler = sampler_state
{
	Texture = (Random);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

float SampleRadius : SSAO_RADIUS;
float Intensity : SSAO_INTENSITY;
float Scale : SSAO_SCALE;
float FarClip : FARCLIP;
int RandomResolution;

float3 GetPosition(in float2 uv)
{
	float3 frustumCorner = FrustumCorners[0];
	float x = lerp(frustumCorner.x, -frustumCorner.x, uv.x);
	float y = lerp(frustumCorner.y, -frustumCorner.y, uv.y);

	return tex2Dlod(depthSampler, float4(uv, 0, 0)).r * float3(x, y, frustumCorner.z);
}

float3 GetNormal(in float2 uv)
{
	return DecodeNormal(tex2D(normalSampler, uv).xy);
}

float2 GetRandom(in float2 uv)
{
	return normalize(tex2D(randomSampler, Resolution * uv / RandomResolution).xy * 2.0f - 1.0f);
}

float SampleAmbientOcclusion(in float2 texCoord, in float3 position, in float3 normal)
{
	float3 diff = GetPosition(texCoord) - position;
	const float3 v = normalize(diff);
	const float  d = length(diff) * Scale;
	return max(0.0, dot(normal,v)) * (1.0/(1.0+d)) * Intensity;
}

float4 HighQualitySsaoPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float3 p = GetPosition(in_TexCoord);
	float3 n = GetNormal(in_TexCoord);
	float2 rand = GetRandom(in_TexCoord);

	float z = -p.z;
	float radius = SampleRadius / z;

	const float2 vecs[4] = { float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1) };

	float ao = 0.0f;
	for (int i = 0; i < 4; i++)
	{
		float2 coord1 = reflect(vecs[i], rand) * radius;
		float2 coord2 = float2(coord1.x * 0.707 - coord1.y * 0.707, coord1.x * 0.707 + coord1.y * 0.707);

		ao += SampleAmbientOcclusion(in_TexCoord + coord1 * 0.25, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord2 * 0.5, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord1 * 0.75, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord2 * 1.0, p, n);
	}

	ao /= (4 * 4);
	ao = 1 - ao;
	return ao;
}

technique SSAO
{
	Pass pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 HighQualitySsaoPS();
	}
}