#include "../EncodeNormals.fxh"
#include "../DepthHeader.fxh"

uniform float4x4 InvWorld : INVERSEWORLD;
uniform float4x4 View : VIEW;
uniform float4x4 InvView : INVERSEVIEW;
uniform float4x4 Projection : PROJECTION;
uniform float4x4 WorldView : WORLDVIEW;
uniform float FarClip : FARCLIP;
uniform float2 Resolution : RESOLUTION;

uniform float3 DecalDirection;
uniform float DecalDirectionClip;
uniform float AngleFadeWidth = 0.17;

uniform float4 DecalColor;

uniform texture Depth : GBUFFER_DEPTH;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

uniform texture Diffuse;
sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MinFilter = Anisotropic;
	MipFilter = Linear;
	MagFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
};

uniform texture Normal;
sampler normalSampler = sampler_state
{
	Texture = (Normal);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct DefaultVertexShaderInput
{
	float4 Position : POSITION0;
};

struct DefaultVertexShaderOutput
{
	float4 Position : POSITION0;
	float Depth : TEXCOORD0;
	float4 PositionCS : TEXCOORD1;
	float4 PositionVS : TEXCOORD2;
};

DefaultVertexShaderOutput DefaultVertexShaderFunction(DefaultVertexShaderInput input)
{
	DefaultVertexShaderOutput output;

	float4 viewPosition = mul(input.Position, WorldView);

	output.Position = mul(viewPosition, Projection);
	output.Depth = CalculateDepth(viewPosition, FarClip);
	output.PositionCS = output.Position;
	output.PositionVS = viewPosition;

	return output;
}

float3 CalculatePixelWorldPosition(in float4 positionCS, in float4 positionVS, in float depth, uniform bool zTest)
{
	//Sample the depth buffer
	float2 screenPos = positionCS.xy / positionCS.w;
		float2 texCoord = float2(
		(1 + screenPos.x) / 2 + (0.5 / Resolution.x),
		(1 - screenPos.y) / 2 + (0.5 / Resolution.y)
	);
	float4 sampledDepth = tex2D(depthSampler, texCoord);

	//Z clip this pixel
	if (zTest)
		ZTest(sampledDepth, depth);

	//Calculate view position of this pixel
	float3 frustumRay = positionVS.xyz * (FarClip / -positionVS.z);
	return ReconstructWorldPosition(frustumRay, sampledDepth.x, InvView);
}

float3 ClipPixelNormal(float3 worldPosition, out float3 tangent, out float3 binormal, out float alpha)
{
	//Calculate the surface normal of this pixel from the change in world position
	float3 ddxWp = ddx(worldPosition);
	float3 ddyWp = ddy(worldPosition);
	float3 normal = normalize(cross(ddyWp, ddxWp));

	//Calculate angle between surface normal and decal direction
	float angle = acos(dot(-normal, DecalDirection));

	//If difference is negative that means angle is bigger than allowed
	float difference = DecalDirectionClip - angle;
	clip(difference);

	//Calculate alpha fade based on normal difference (angle in 0->AngleFadeWidth range fades in alpha, anything greater saturates to 1)
	alpha = saturate(difference / AngleFadeWidth);

	//Calculate tangent space basis
	binormal = normalize(ddxWp);
	tangent = normalize(ddyWp);

	return normal;
}

float2 CalculateDecalTexCoord(float3 worldPosition)
{
	float4 objectPosition = mul(float4(worldPosition, 1), InvWorld);
	clip(0.5 - abs(objectPosition.xyz));

	return objectPosition.xz + 0.5;
}

float4 DiffuseValue(float2 decalTexCoord, float alpha)
{
	return tex2D(diffuseSampler, decalTexCoord) * DecalColor * float4(1, 1, 1, alpha);
}

float4 NormalValue(float2 decalTexCoord, float3 pixelNormal, float3 pixelTangent, float3 pixelBinormal)
{
	//Sample normal value from normal map
	float4 normalSample = tex2D(normalSampler, decalTexCoord);
	float3 normal = normalize(normalSample.xyz * 2 - 1);

	//Construct a tangent space to view space conversion
	float3x3 tangentToView;
	tangentToView[0] = mul(pixelTangent, View);
	tangentToView[1] = mul(pixelBinormal, View);
	tangentToView[2] = mul(pixelNormal, View);
	
	//Convert normal into view space
	normal = mul(normal, tangentToView);

	//Write out normal value with alpha preserved (this will be used in the decal mixing stage later)
	return float4(EncodeNormal(normal), 0, normalSample.a);
}

void PixelShaderFunction(uniform bool outputNormals, uniform bool outputDiffuse, uniform bool zTest,
	in DefaultVertexShaderOutput input,
	out float4 out_normal : COLOR0,
	out float4 out_diffuse : COLOR1)
{
	float3 worldPosition = CalculatePixelWorldPosition(input.PositionCS, input.PositionVS, input.Depth, zTest);
	float2 decalTexCoord = CalculateDecalTexCoord(worldPosition);

	float3 pixelTangent;
	float3 pixelBinormal;
	float alpha;
	float3 pixelNormal = ClipPixelNormal(worldPosition, pixelTangent, pixelBinormal, alpha);

	if (outputDiffuse)
		out_diffuse = DiffuseValue(decalTexCoord, alpha);
	else
		out_diffuse = float4(0, 0, 0, 0);

	if (outputNormals)
		out_normal = NormalValue(decalTexCoord, pixelNormal, pixelTangent, pixelBinormal);
	else
		out_normal = float4(0, 0, 0, 0);
}

technique ZTestedDecalDiffuseNormal
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(true, true, true);
    }
}

technique ZTestedDecalDiffuse
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(false, true, true);
	}
}

technique ZTestedDecalNormal
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(true, false, true);
	}
}

technique DecalDiffuseNormal
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(true, true, false);
	}
}

technique DecalDiffuse
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(false, true, false);
	}
}

technique DecalNormal
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 DefaultVertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(true, false, false);
	}
}