float2 Resolution : RESOLUTION;
float3 FrustumCorners[4] : FARFRUSTUMCORNERS;

void FullScreenQuadFrustumCornerVS(in float3 in_Position : POSITION0,
								   in float2 in_TexCoord : TEXCOORD0,
								   in float  in_CornerIndex : TEXCOORD1,
								   out float4 out_Position : POSITION0,
								   out float2 out_TexCoord : TEXCOORD0,
								   out float3 out_FrustumCorner : TEXCOORD1)
{
    out_Position.x = in_Position.x - (1 / Resolution.x);
	out_Position.y = in_Position.y + (1 / Resolution.y);
	out_Position.z = in_Position.z;
	out_Position.w = 1;

	out_TexCoord = in_TexCoord;// + (0.5 / Resolution);
	
	out_FrustumCorner = FrustumCorners[in_CornerIndex];
}

void FullScreenQuadVS(in float3 in_Position : POSITION0,
					  in float2 in_TexCoord : TEXCOORD0,
					  out float4 out_Position : POSITION0,
					  out float2 out_TexCoord : TEXCOORD0)
{
    out_Position.x = in_Position.x - (1 / Resolution.x);
	out_Position.y = in_Position.y + (1 / Resolution.y);
	out_Position.z = in_Position.z;
	out_Position.w = 1;

	out_TexCoord = in_TexCoord;// + (0.5 / Resolution);
}