#ifndef GBufferHeaderFxh
#define GBufferHeaderFxh

#include "SkinningHeader.fxh"
#include "EncodeNormals.fxh"
#include "DepthHeader.fxh"

uniform float4x4 WorldView : WORLDVIEW;
uniform float4x4 Projection : PROJECTION;
uniform float FarClip : FARCLIP;

struct DefaultVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Binormal : BINORMAL;
	float3 Tangent : TANGENT;
};

struct DefaultVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	float3x3 TangentToView : TEXCOORD2;
	float4 PositionCS : COLOR1;
};

DefaultVertexShaderOutput DefaultVertexShaderFunction(DefaultVertexShaderInput input)
{
	DefaultVertexShaderOutput output;

	float4 viewPosition = mul(input.Position, WorldView);

	output.Position = mul(viewPosition, Projection);
	output.PositionCS = output.Position;
	output.Depth = CalculateDepth(viewPosition, FarClip);
	output.TexCoord = input.TexCoord;

	output.TangentToView[0] = mul(input.Tangent, WorldView);
	output.TangentToView[1] = mul(input.Binormal, WorldView);
	output.TangentToView[2] = mul(input.Normal, WorldView);

	return output;
}

struct AnimatedVertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Binormal : BINORMAL;
	float3 Tangent : TANGENT;

	float4 Indices  : BLENDINDICES0;
	float4 Weights  : BLENDWEIGHT0;
};

DefaultVertexShaderOutput AnimatedVertexShaderFunction(AnimatedVertexShaderInput input)
{
	//Apply animation effects and convert data into a form the default vertex shader can understand
	DefaultVertexShaderInput postAnimation;
	float4x3 skinning = CalculateSkinMatrix(input.Indices, input.Weights, WeightsPerVertex);
	postAnimation.Position = SkinTransformPosition(input.Position, skinning);
	postAnimation.TexCoord = input.TexCoord;
	postAnimation.Tangent = SkinTransformNormal(input.Tangent, skinning);
	postAnimation.Binormal = SkinTransformNormal(input.Binormal, skinning);
	postAnimation.Normal = SkinTransformNormal(input.Normal, skinning);

	//Do normal vertex shader stuff
	return DefaultVertexShaderFunction(postAnimation);
}

#endif