#include "FullScreenQuad.fxh"

float2 TexelSize;

texture Edges : EDGES;
sampler edgeSampler = sampler_state
{
	Texture = (Edges);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
};

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MipFilter = None;
	MagFilter = Linear;
};

void EdgeDetectPS(in float2 in_TexCoord : TEXCOORD0,
				  out float4 out_Colour : COLOR0)
{
	float2 topLeft     = in_TexCoord + float2(-TexelSize.x, -TexelSize.y);
	float2 bottomRight = in_TexCoord + float2(TexelSize.x, TexelSize.y);
	float2 topRight    = in_TexCoord + float2(-TexelSize.x, TexelSize.y);
	float2 bottomLeft  = in_TexCoord + float2(TexelSize.x, -TexelSize.y);
	float2 top         = in_TexCoord + float2(0, -TexelSize.y);
	float2 bottom	   = in_TexCoord + float2(0, TexelSize.y);
	float2 left        = in_TexCoord + float2(-TexelSize.x, 0);
	float2 right       = in_TexCoord + float2(TexelSize.x, 0);


	float weight = tex2D(edgeSampler, in_TexCoord).x;

	float2 offset = in_TexCoord * (1 - weight);

	float4 sample1 = tex2D(textureSampler, topLeft * weight + offset);
	float4 sample2 = tex2D(textureSampler, topRight * weight + offset);
	float4 sample3 = tex2D(textureSampler, bottomLeft * weight + offset);
	float4 sample4 = tex2D(textureSampler, bottomRight * weight + offset);
	float4 sample5 = tex2D(textureSampler, top * weight + offset);
	float4 sample6 = tex2D(textureSampler, bottom * weight + offset);
	float4 sample7 = tex2D(textureSampler, left * weight + offset);
	float4 sample8 = tex2D(textureSampler, right * weight + offset);
	
	out_Colour = (sample1 + sample2 + sample3 + sample4 + sample5 + sample6 + sample7 + sample8) / 8;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 EdgeDetectPS();
	}
}