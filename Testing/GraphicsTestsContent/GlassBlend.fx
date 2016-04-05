#include "GBufferHeader.fxh"

uniform float Opacity : OPACITY;
uniform float Scattering : SCATTERING;
uniform float Attenuation : ATTENUATION;

uniform texture DiffuseMap;
uniform texture NormalMap;
uniform texture SpecularMap;

uniform float2 Resolution : RESOLUTION;
uniform float NearClip : NEARCLIP;

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

texture TransparencyLightbuffer : TRANSPARENCY_LIGHTBUFFER;
sampler transparencyLightbufferSampler = sampler_state
{
	Texture = (TransparencyLightbuffer);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Lightbuffer : LIGHTBUFFER;
sampler lightbufferSampler = sampler_state
{
	Texture = (Lightbuffer);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Scatter light from the front side of the object to nearby pixels
float3 Scatter(float2 texCoord, float scattering)
{
	float2 pixel = 1 / Resolution;

	const float offsetsLength = 8;

	//Sample kernel, XY is offset and Z is weight (1 / length)
	const float3 offsets[] = {
		float3(-1, -1, 0.7071),
		float3(-1, 0, 1),
		float3(-1, 1, 0.7071),
		float3(0, -1, 1),
		float3(0, 1, 1),
		float3(1, -1, 0.7071),
		float3(1, 0, 1),
		float3(1, 1, 0.7071),
	};

	float3 total = float3(0, 0, 0);
	float count = 0;

	for (int i = 0; i < offsetsLength; i++)
	{
		float2 coord = offsets[i].xy + texCoord;

		float3 col = tex2D(transparencyLightbufferSampler, coord).rgb;
		float alpha = tex2D(normalSampler, coord).rgb;

		alpha = min(1, alpha);

		total += col * alpha;
		count += alpha * pow(abs(scattering), offsets[i].z);
	}

	return total;
}

// Transmit light from the back of the object to nearby pixels
float3 Transmit(float2 texCoord, float thickness, float scattering, float attenuation)
{
	float2 pixel = 1 / Resolution;

	//Sample a pattern around the point like this:
	//
	//     o    
	//   i   i  
	// o   c   o
	//   i   i  
	//     o    
	//
	// Keeping separate totals for i and o
	float3 totalo = tex2D(lightbufferSampler, texCoord + float2(-2, 0) * pixel).rgb
					+ tex2D(lightbufferSampler, texCoord + float2(2, 0) * pixel).rgb
					+ tex2D(lightbufferSampler, texCoord + float2(0, 2) * pixel).rgb
					+ tex2D(lightbufferSampler, texCoord + float2(0, -2) * pixel).rgb;
	totalo /= 4;
	float3 totali = tex2D(lightbufferSampler, texCoord + float2(-1, 1) * pixel).rgb
				  + tex2D(lightbufferSampler, texCoord + float2(1, 1) * pixel).rgb
				  + tex2D(lightbufferSampler, texCoord + float2(-1, -1) * pixel).rgb
				  + tex2D(lightbufferSampler, texCoord + float2(-1, -1) * pixel).rgb;
	totali /= 4;

	float3 center = tex2D(lightbufferSampler, texCoord).rgb;

	//Attenuate by thickness and distance
	float attn = pow(clamp(1 - attenuation, 0, 1), thickness);
	center *= attn;
	totali *= attn;
	totalo *= attn;

	// Now we have average colour for surrounding pixels. Near (totali) and far (totalo)
	// Calculate colour of pixel by blending with respect to scattering
	if (scattering < 0.5)
	{
		return lerp(center, totali, scattering);
	}
	else
	{
		return lerp(totali, totalo, scattering - 0.5);
	}
}

float4 FoggyGlassPixelShaderFunction(in DefaultVertexShaderOutput input) : COLOR0
{
	//Calculate screen space position of this pixel
	float2 screenPos = input.PositionCS.xy / input.PositionCS.w;
	float2 texCoord = float2(
		(1 + screenPos.x) / 2 + (0.5 / Resolution.x),
		(1 - screenPos.y) / 2 + (0.5 / Resolution.y)
	);

	//Sample normals, and clip on alpha stencil
	float4 sampledNormals = tex2D(normalSampler, texCoord);
	if (sampledNormals.a == 0)
		clip(-1);

	//Calculate the thickness of the object at this pixel
	float backDepth = input.Depth;
	float frontDepth = tex2D(depthSampler, texCoord);
	float thickness = (backDepth - frontDepth) * (FarClip - NearClip);

	float3 trans = Scatter(texCoord, Scattering) + Transmit(texCoord, thickness, Scattering, Attenuation);
	float3 opaqe = tex2D(transparencyLightbufferSampler, texCoord).rgb;

	return float4(lerp(trans, opaqe, Opacity), 1);
}

technique FoggyGlass
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 FoggyGlassPixelShaderFunction();
	}
}

technique FoggyGlassAnimated
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 AnimatedVertexShaderFunction();
		PixelShader = compile ps_3_0 FoggyGlassPixelShaderFunction();
	}
}