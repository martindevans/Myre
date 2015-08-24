#include "FullScreenQuad.fxh"

void PixelShaderFunction(out float4 out_Depth : COLOR0,
						 out float4 out_Normals : COLOR1,
						 out float4 out_Diffuse : COLOR2)
{
	out_Depth = float4(1, 1, 1, 1);
	out_Normals = float4(0, 0, 0, 1);
	out_Diffuse = float4(0, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 FullScreenQuadVS();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
