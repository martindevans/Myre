#include "FullScreenQuad.fxh"

float4 defaultDepth = float4(1, 1, 1, 1);
float4 defaultNormals = float4(0, 0, 0, 0);
float4 defaultDiffuse = float4(0, 0, 0, 1);

void Pixel_DepthNormalsDiffuse(out float4 out_Depth : COLOR0, out float4 out_Normals : COLOR1, out float4 out_Diffuse : COLOR2)
{
	out_Depth = defaultDepth;
	out_Normals = defaultNormals;
	out_Diffuse = defaultDiffuse;
}

void Pixel_NormalsDiffuse(out float4 out_Normals : COLOR0, out float4 out_Diffuse : COLOR1)
{
	out_Normals = defaultNormals;
	out_Diffuse = defaultDiffuse;
}

technique Clear_DepthNormalsDiffuse
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 FullScreenQuadVS();
		PixelShader = compile ps_2_0 Pixel_DepthNormalsDiffuse();
	}
}

technique Clear_NormalsDiffuse
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 FullScreenQuadVS();
		PixelShader = compile ps_2_0 Pixel_NormalsDiffuse();
	}
}
