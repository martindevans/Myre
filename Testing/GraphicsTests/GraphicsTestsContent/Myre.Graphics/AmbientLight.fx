#include "../EncodeNormals.fxh"
#include "../GammaCorrection.fxh"
#include "../FullScreenQuad.fxh"

float3 Up;
float3 SkyColour;
float3 GroundColour;

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

texture Diffuse : GBUFFER_DIFFUSE;
sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture SSAO : SSAO;
sampler ssaoSampler = sampler_state
{
	Texture = (SSAO);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

float ReadSsao(float2 texCoord)
{
	//return tex2D(ssaoSampler, texCoord).a;
    
	float ao = 0;

	//for (int i = 0; i < 4; i++)
	//{
	//	for (int j = 0; j < 4; j++)
	//	{
	//		float2 coord = texCoord + float2(i / Resolution.x, j / Resolution.y);
	//		ao += tex2D(ssaoSampler, coord);
	//	}
	//}
	//return ao.a / 16;

	float2 halfTexel = 0.5 / Resolution;

	ao = tex2D(ssaoSampler, texCoord + halfTexel * float2(-1, -1)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(1, -1)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(3, -1)).a

	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(-1, 1)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(1, 1)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(3, 1)).a

	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(-1, 3)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(1, 3)).a
	   + tex2D(ssaoSampler, texCoord + halfTexel * float2(3, 3)).a;

	return ao / 9;
}

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 in float3 in_FrustumRay : TEXCOORD1,
						 out float4 out_Colour : COLOR0,
						 uniform bool enableSsao)
{
	float4 sampledNormals = tex2D(normalSampler, in_TexCoord);
	float3 normal = DecodeNormal(sampledNormals.xy);

	float alpha = dot(Up, normal) / 2 + 0.5;
	float3 colour = lerp(GroundColour, SkyColour, alpha);

	if (enableSsao)
	{
		float ssao = ReadSsao(in_TexCoord);
		colour *= ssao;
		//colour += ssao.rgb;
	}

	float3 diffuse = tex2D(diffuseSampler, in_TexCoord).rgb;
	colour *= GammaToLinear(diffuse);

	out_Colour = float4(colour, 1);
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction(false);
    }
}

technique AmbientSSAO
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction(true);
    }
}
