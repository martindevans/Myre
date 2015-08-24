#include "FullScreenQuad.fxh"

float TimeDelta : TIMEDELTA;
float AdaptionRate : HDR_ADAPTIONRATE;

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

texture PreviousAdaption;
sampler previousAdaptionSampler = sampler_state
{
	Texture = (PreviousAdaption);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

float4 ExtractLuminancePS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float3 colour = tex2D(textureSampler, in_TexCoord).rgb;
	float luminance = max(dot(colour, float3(0.299f, 0.587f, 0.114f)), 0.0001f);
	luminance = log2(luminance);
	return float4(luminance, luminance, luminance, 1);
}

float4 ReadLuminancePS() : COLOR0
{
	return exp2(tex2Dlod(textureSampler, float4(0.5, 0.5, 0, 11)).x);
}

float4 AdaptLuminancePS() : COLOR0 //in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
    float lastLuminance = tex2D(previousAdaptionSampler, float2(0.5, 0.5)).x;
    float currentLuminance = tex2D(textureSampler, float2(0.5, 0.5)).x;
    
    // Adapt the luminance using Pattanaik's technique    
    float adaptedLum = lastLuminance + (currentLuminance - lastLuminance) * (1 - exp(-TimeDelta * AdaptionRate));
	
    return float4(adaptedLum, adaptedLum, adaptedLum, 1);
}

float4 CopyPS() : COLOR0
{
	return tex2D(textureSampler, float2(0.5, 0.5)).x;
}

technique ExtractLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ExtractLuminancePS();
	}
}

technique ReadLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 ReadLuminancePS();
	}
}

technique AdaptLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 AdaptLuminancePS();
	}
}

technique Copy
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 CopyPS();
	}
}