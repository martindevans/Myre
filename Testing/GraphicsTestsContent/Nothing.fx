float4x4 WorldViewProjection : WORLDVIEWPROJECTION;

void VertexShaderFunction(in float3 in_Position : POSITION0,
						  out float4 out_Position : POSITION0)
{
	out_Position = mul(float4(in_Position, 1), WorldViewProjection);
}

float4 PixelShaderFunction() : COLOR0
{
    return float4(1, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}