#include "FullScreenQuad.fxh"

float2 SourceResolution;

texture Texture;
sampler pointSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};
sampler linearSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
};

float4 BilinearSample(float2 texCoord)
{
/*
	float offset = 0.5 / (Resolution * 2);

	float4 a = tex2D(pointSampler, texCoord + float2(-offset, -offset));
	float4 b = tex2D(pointSampler, texCoord + float2(offset, -offset));
	float4 c = tex2D(pointSampler, texCoord + float2(-offset, offset));
	float4 d = tex2D(pointSampler, texCoord + float2(offset, offset));

	return (a + b + c + d) / 4;
*/

	float2 texelPos = SourceResolution * texCoord; 
	float2 alpha = frac(texelPos); 
	float texelSize = 1.0 / SourceResolution;
	texCoord -= texelSize * alpha;
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(pointSampler, texCoord); 
	sourcevals[1] = tex2D(pointSampler, texCoord + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(pointSampler, texCoord + float2(0, texelSize)); 
	sourcevals[3] = tex2D(pointSampler, texCoord + float2(texelSize, texelSize));   
         
	return lerp(lerp(sourcevals[0], sourcevals[1], alpha.x), 
				lerp(sourcevals[2], sourcevals[3], alpha.x), 
				alpha.y);
}

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 out float4 out_Colour : COLOR0,
						 uniform bool hardwareScale)
{
	if (hardwareScale)
		out_Colour = tex2D(linearSampler, in_TexCoord);
	else
		out_Colour = BilinearSample(in_TexCoord);
}


technique Software
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 PixelShaderFunction(false);
	}
}

technique Hardware
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 PixelShaderFunction(true);
	}
}