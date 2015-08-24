#include "FullScreenQuad.fxh"

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	return float4(0, 0, 0, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}