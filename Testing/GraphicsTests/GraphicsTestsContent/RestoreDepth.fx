#include "FullScreenQuad.fxh"

float4x4 Projection : PROJECTION;
float farClip : FARCLIP;

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

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 in float3 in_FrustumRay : TEXCOORD1,
						 out float4 out_Colour : COLOR0,
						 out float out_Depth : DEPTH)
{
	float4 sampledDepth = tex2D(depthSampler, in_TexCoord);
	float3 viewPosition = in_FrustumRay * sampledDepth.x;

	float4 projectedPosition = mul(float4(viewPosition, 1), Projection);
	
	out_Colour = float4(0, 0, 0, 1);
	out_Depth = projectedPosition.z / projectedPosition.w;
	//projectedPosition /= projectedPosition.w;
	//out_Colour = float4(projectedPosition.xy / 2 + 0.5, projectedPosition.z, 1); //float4(viewPosition.xy, -viewPosition.z / farClip, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
