#include "SkinningHeader.fxh"

float4x4 WorldView : WORLDVIEW;
float4x4 Projection : PROJECTION;
float FarClip : FARCLIP;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 PositionCS : POSITION0;
	float Depth : TEXCOORD0;
};

VertexShaderOutput ViewLengthVS(VertexShaderInput input)
{
    float4 viewPosition = mul(input.Position, WorldView);
	float4 clipPosition = mul(viewPosition, Projection);
    
	VertexShaderOutput output;
	output.PositionCS = clipPosition;
	output.Depth = length(viewPosition) / FarClip;
	return output;
}

VertexShaderOutput ViewZVS(VertexShaderInput input)
{
    float4 viewPosition = mul(input.Position, WorldView);
	float4 clipPosition = mul(viewPosition, Projection);
    
	VertexShaderOutput output;
	output.PositionCS = clipPosition;
	output.Depth = -viewPosition.z / FarClip;
	return output;
}

struct AnimatedVertexShaderInput
{
    float4 Position : POSITION0;

	float4 Indices  : BLENDINDICES0;
    float4 Weights  : BLENDWEIGHT0;
};

VertexShaderInput Animate(AnimatedVertexShaderInput input)
{
    VertexShaderInput postAnimation;
	float4x3 skinning = CalculateSkinMatrix(input.Indices, input.Weights, WeightsPerVertex);
	postAnimation.Position = SkinTransformPosition(input.Position, skinning);

	return postAnimation;
}

VertexShaderOutput AnimatedViewLengthVS(AnimatedVertexShaderInput input)
{
	return ViewLengthVS(Animate(input));
}

VertexShaderOutput AnimatedViewZVS(AnimatedVertexShaderInput input)
{
	return ViewZVS(Animate(input));
}

void PS(in float in_Depth : TEXCOORD0,
		out float4 out_Colour : COLOR0)
{
	out_Colour = float4(in_Depth, 0, 0, 1);
}

technique ViewLength
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ViewLengthVS();
        PixelShader = compile ps_3_0 PS();
    }
}

technique ViewZ
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ViewZVS();
        PixelShader = compile ps_3_0 PS();
    }
}

technique AnimatedViewLength
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 AnimatedViewLengthVS();
        PixelShader = compile ps_3_0 PS();
    }
}

technique AnimatedViewZ
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 AnimatedViewZVS();
        PixelShader = compile ps_3_0 PS();
    }
}