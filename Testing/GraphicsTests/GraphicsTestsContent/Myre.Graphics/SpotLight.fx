#include "../EncodeNormals.fxh"
#include "../GammaCorrection.fxh"
#include "../FullScreenQuad.fxh"
#include "../Shadows.fxh"

float3 Colour;
float3 LightPosition;
float3 LightDirection;
float Angle;
float Range;
float LightFalloffFactor;
float3 CameraPosition : CAMERAPOSITION;
bool EnableProjectiveTexturing;
bool EnableShadows;

float FarClip : FARCLIP;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float4x4 WorldView : WORLDVIEW;
float4x4 CameraViewToLightProjection;
float4 LightNearPlane;
float LightFarClip;

texture Depth : GBUFFER_DEPTH;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Diffuse : GBUFFER_DIFFUSE;
sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Mask;
sampler maskSampler = sampler_state
{
	Texture = (Mask);
	MinFilter = Linear;
	MipFilter = None;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 CalculateLighting(float2 texCoord, float3 viewPosition)
{
	float3 surfaceToLight = LightPosition - viewPosition;
	float distance = length(surfaceToLight);	
	float3 L = surfaceToLight / distance;

	if (dot(L, LightDirection) > Angle)
	{
		float4 sampledNormals = tex2D(normalSampler, texCoord);
		float3 normal = DecodeNormal(sampledNormals.xy);
		float specularIntensity = sampledNormals.z;

		float4 sampledDiffuse = tex2D(diffuseSampler, texCoord);
		float3 diffuse = GammaToLinear(sampledDiffuse.xyz);
		float specularPower = sampledDiffuse.w * 255;

		float attenuation = 1 - saturate(distance / Range); //saturate(LightFalloffFactor / (distance * distance));
		attenuation *= attenuation;

		float3 V = normalize(CameraPosition - viewPosition);
		float3 R = normalize(reflect(-L, normal));

		float NdL = max(dot(normal, L), 0);
		float RdV = max(dot(R, V), 0.00001f);

		float3 light = Colour * attenuation;

		float4 projectedTexCoord = 0;
		if (EnableProjectiveTexturing || EnableShadows)
		{
			projectedTexCoord = mul(float4(viewPosition, 1), CameraViewToLightProjection);
			projectedTexCoord /= projectedTexCoord.w;
			projectedTexCoord.x = (1 + projectedTexCoord.x) / 2;
			projectedTexCoord.y = (1 - projectedTexCoord.y) / 2;
		}

		if (EnableProjectiveTexturing)
		{
			light *= tex2D(maskSampler, projectedTexCoord.xy).rgb;
		}

		if (EnableShadows)
		{
			float depth = (dot(viewPosition, LightNearPlane.xyz) + LightNearPlane.w) / LightFarClip;
			depth = length(LightPosition - viewPosition) / LightFarClip;
			light *= CalculateShadow(depth, projectedTexCoord.xy, 9, LightFarClip);
		}
		
		return float4(NdL * light * (diffuse + specularIntensity * pow(RdV, specularPower)), 1);
	}
	else
	{
		return float4(0, 0, 0, 0);
	}
}

void GeometryVS(in float3 in_Position : POSITION0,
					  out float4 out_Position : POSITION0,
					  out float3 out_PositionVS : TEXCOORD0,
					  out float4 out_PositionCS : TEXCOORD1)
{
	out_Position = mul(float4(in_Position, 1), WorldViewProjection);
	out_PositionVS = mul(float4(in_Position, 1), WorldView);
	out_PositionCS = out_Position;
}

void GeometryPS(in float3 in_PositionVS : TEXCOORD0,
						 in float4 in_PositionCS : TEXCOORD1,
						 out float4 out_Colour : COLOR0)
{
	float2 screenPos = in_PositionCS.xy / in_PositionCS.w;
	float2 texCoord;
	texCoord.x = (1 + screenPos.x) / 2 + (0.5 / Resolution.x);
	texCoord.y = (1 - screenPos.y) / 2 + (0.5 / Resolution.y);

	float4 sampledDepth = tex2D(depthSampler, texCoord);
	float3 frustumRay = in_PositionVS.xyz * (FarClip/-in_PositionVS.z);
	float3 viewPosition = frustumRay * sampledDepth.x;

	out_Colour = CalculateLighting(texCoord, viewPosition);
}

void QuadPS(in float2 in_TexCoord : TEXCOORD0,
			in float3 in_FrustumRay : TEXCOORD1,
			out float4 out_Colour : COLOR0)
{
	float4 sampledDepth = tex2D(depthSampler, in_TexCoord);
	float3 viewPosition = in_FrustumRay * sampledDepth.x;

	out_Colour = CalculateLighting(in_TexCoord, viewPosition);
}

technique Geometry
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 GeometryVS();
        PixelShader = compile ps_3_0 GeometryPS();
    }
}

technique Quad
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
		PixelShader = compile ps_3_0 QuadPS();
	}
}