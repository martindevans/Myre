#include "EncodeNormals.fxh"
#include "GammaCorrection.fxh"
#include "FullScreenQuad.fxh"
#include "Shadows.fxh"

//#define BIAS 0.005

float3 Direction : LIGHTDIRECTION;
float3 Colour : LIGHTCOLOUR;
float3 CameraPosition : CAMERAPOSITION;
bool EnableShadows;
//float2 ShadowMapSize;

float FarClip : FARCLIP;
float4 LightNearPlane;
float LightFarClip;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float4x4 WorldView : WORLDVIEW;
float4x4 ShadowProjection;
float4x4 CameraViewToLightView;

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

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 in float3 in_FrustumRay : TEXCOORD1,
						 out float4 out_Colour : COLOR0)
{
	float4 sampledDepth = tex2D(depthSampler, in_TexCoord);
	float3 viewPosition = in_FrustumRay * sampledDepth.x;

	float4 sampledNormals = tex2D(normalSampler, in_TexCoord);
	float3 normal = DecodeNormal(sampledNormals.xy);
	float specularIntensity = sampledNormals.z;

	float4 sampledDiffuse = tex2D(diffuseSampler, in_TexCoord);
	float3 diffuse = GammaToLinear(sampledDiffuse.xyz);
	float specularPower = sampledDiffuse.w * 255;

	float3 L = -normalize(Direction);
	float3 V = normalize(CameraPosition - viewPosition);
	float3 R = normalize(reflect(-L, normal));

	float NdL = max(dot(normal, L), 0.00001);
	float RdV = max(dot(R, V), 0.00001);

	float3 light = Colour;
	if (EnableShadows)
	{	
		float4 projectedTexCoord;
		projectedTexCoord = mul(float4(viewPosition, 1), ShadowProjection);
		projectedTexCoord /= projectedTexCoord.w;
		projectedTexCoord.x = (1 + projectedTexCoord.x) / 2;
		projectedTexCoord.y = (1 - projectedTexCoord.y) / 2;

		float depth = (dot(viewPosition, LightNearPlane.xyz) + LightNearPlane.w) / LightFarClip;
		light *= CalculateShadow(depth, projectedTexCoord.xy, 9, LightFarClip);
	}

	out_Colour = float4(NdL * light * (diffuse + specularIntensity * pow(RdV, specularPower)), 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
