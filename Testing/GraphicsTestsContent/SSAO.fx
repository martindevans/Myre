// adapted from a RenderMonkey implementation
// by ArKano22, http://www.gamedev.net/community/forums/topic.asp?topic_id=556187

#include "FullScreenQuad.fxh"
#include "EncodeNormals.fxh"

#define NUM_SAMPLES 16

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

texture Lighting : PREVIOUSLIGHTBUFFER;
sampler lightingSampler = sampler_state
{
	Texture = (Lighting);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float SampleRadius : SSAO_RADIUS;
float Intensity : SSAO_INTENSITY;
float Scale : SSAO_SCALE;
float FarClip : FARCLIP;
float RadiosityIntensity : SSAO_RADIOSITYINTENSITY = 0.5f;
int RandomResolution;
float2 Samples[NUM_SAMPLES];

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

float4 SampleRadiosity(in float2 texCoord, in float3 position, in float3 normal)
{
	float3 diff = GetPosition(texCoord) - position;
	const float3 v = normalize(diff);
	const float  d = length(diff) * Scale;

	float3 colour = tex2Dlod(lightingSampler, float4(texCoord, 0, 0)).rgb * RadiosityIntensity;
	float ao = max(0.0, dot(normal, v)) * (1.0 / (1.0+d)) * Intensity;
	return float4(colour * ao, ao);
}

float4 HighQualitySsaoPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float3 p = GetPosition(in_TexCoord);
	float3 n = GetNormal(in_TexCoord);
	float2 rand = GetRandom(in_TexCoord);

	float z = -p.z;
	float radius = SampleRadius / z;

	float ao = 0.0f;
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		float2 offset = reflect(Samples[i], rand) * radius;
		ao += SampleAmbientOcclusion(in_TexCoord + offset, p, n);
	}

	ao /= NUM_SAMPLES;
	ao = 1 - ao;
	return ao; //float4(0, 0, 0, ao);
}

float4 LowQualitySsaoPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	const float2 vec[8] = {
		float2(1,0),float2(-1,0),
		float2(0,1),float2(0,-1),
		float2(0.707,0),float2(-0.707,0),
		float2(0,.707),float2(0,-0.707)
	};

	in_TexCoord -= float2(3,2) / Resolution;

	float3 p = GetPosition(in_TexCoord);
	float3 n = GetNormal(in_TexCoord);
	float2 rand = GetRandom(in_TexCoord);

	float z = -p.z;
	float radius = SampleRadius / z;

	float ao = 0.0f;
	int iterations = lerp(4.0, 2.0, z / FarClip); 
	//[loop]
	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand) * radius;
		float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707, coord1.x*0.707 + coord1.y*0.707);
		ao += SampleAmbientOcclusion(in_TexCoord + coord1*0.25, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord2*0.50, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord1*0.75, p, n);
		ao += SampleAmbientOcclusion(in_TexCoord + coord2,      p, n); 
	}

	ao /= (float)(iterations * 4.0);
	ao = 1 - ao;

	return ao;// float4(0, 0, 0, ao);
}

float4 SsgiPS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	const float2 vec[8] = {
		float2(1,0),float2(-1,0),
		float2(0,1),float2(0,-1),
		float2(0.707,0),float2(-0.707,0),
		float2(0,.707),float2(0,-0.707)
	};

	float3 p = GetPosition(in_TexCoord);
	float3 n = GetNormal(in_TexCoord);
	float2 rand = GetRandom(in_TexCoord);

	float radius = SampleRadius / p.z;

	float4 ao = 0.0f;
	int iterations = lerp(4.0, 2.0, p.z / FarClip); 
	//[loop]
	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand) * radius;
		float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707, coord1.x*0.707 + coord1.y*0.707);
		ao += SampleRadiosity(in_TexCoord + coord1*0.25, p, n);
		ao += SampleRadiosity(in_TexCoord + coord2*0.50, p, n);
		ao += SampleRadiosity(in_TexCoord + coord1*0.75, p, n);
		ao += SampleRadiosity(in_TexCoord + coord2*1.00, p, n); 
	}

	ao /= (float)iterations * 4.0;

	return float4(ao.rgb * RadiosityIntensity, 1 - ao.a);
}

technique HQ_SSAO
{
	Pass pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 HighQualitySsaoPS();
	}
}

technique LQ_SSAO
{
	Pass pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 LowQualitySsaoPS();
	}
}

technique SSGI
{
	Pass pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 SsgiPS();
	}
}