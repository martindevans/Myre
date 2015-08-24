#include "FullScreenQuad.fxh"
#define NUM_SAMPLES 11

float Weights[NUM_SAMPLES];
float Offsets[NUM_SAMPLES];

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
};

void GaussianPS(in float2 in_TexCoord : TEXCOORD0,
				out float4 out_Colour : COLOR0,
				uniform float2 direction)
{
	float2 texelSize = direction / Resolution;

	float3 total = 0;
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		float2 offset = Offsets[i] * texelSize;
		total += tex2D(textureSampler, in_TexCoord + offset).rgb * Weights[i];
	}

	out_Colour = float4(total, 1);
}

technique BlurHorizontal
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 GaussianPS(float2(1, 0));
	}
}

technique BlurVertical
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 GaussianPS(float2(0, 1));
	}
}