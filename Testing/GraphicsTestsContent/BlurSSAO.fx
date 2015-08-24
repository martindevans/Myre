#include "FullScreenQuad.fxh"

texture SSAO;
sampler ssaoSampler = sampler_state
{
	Texture = (SSAO);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

texture Edges : EDGES;
sampler edgeSampler = sampler_state
{
	Texture = (Edges);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

float4 ReadSsaoPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float weight = 1 - tex2D(edgeSampler, texCoord).x;

	float4 ao = 0;
	for (int i = 0; i < 4; i++)
	{
		for (int j = 0; j < 4; j++)
		{
			float2 coord = texCoord + float2(i / Resolution.x, j / Resolution.y) * weight;
			ao += tex2D(ssaoSampler, coord);
		}
	}

	return ao / 16;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ReadSsaoPS();
	}
}