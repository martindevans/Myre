float4 Colour : COLOUR;

void VertexShaderFunction(inout float4 position : POSITION0)
{
}

float4 PixelShaderFunction() : COLOR0
{
	return Colour;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
