// http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter09.html

#include "FullScreenQuad.fxh"
#include "EncodeNormals.fxh"

float2 TexelSize;
float NormalThreshold : EDGE_NORMALTHRESHOLD;
float DepthThreshold : EDGE_DEPTHTHRESHOLD;
float NormalWeight : EDGE_NORMALWEIGHT;
float DepthWeight : EDGE_DEPTHWEIGHT;

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

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

void EdgeDetectPS(in float2 in_TexCoord : TEXCOORD0,
				  out float4 out_Colour : COLOR0)
{
	float2 topLeft     = in_TexCoord + float2(-TexelSize.x, -TexelSize.y);
	float2 bottomRight = in_TexCoord + float2(TexelSize.x, TexelSize.y);
	float2 topRight    = in_TexCoord + float2(-TexelSize.x, TexelSize.y);
	float2 bottomLeft  = in_TexCoord + float2(TexelSize.x, -TexelSize.y);

	// Normal discontinuity filter
	half3 nc = DecodeNormal(tex2D(normalSampler, in_TexCoord).xy);

	half4 nd;
	nd.x = dot(nc, (half3)DecodeNormal(tex2D(normalSampler, topLeft).xy));
	nd.y = dot(nc, (half3)DecodeNormal(tex2D(normalSampler, bottomRight).xy));
	nd.z = dot(nc, (half3)DecodeNormal(tex2D(normalSampler, topRight).xy));
	nd.w = dot(nc, (half3)DecodeNormal(tex2D(normalSampler, bottomLeft).xy));

	//nd -= NormalThreshold;
	nd = abs(nd) < NormalThreshold;
	half ne = 1 - saturate(dot(nd, NormalWeight));
	
	// Depth filter : compute gradiental difference:
	// (c-sample1)+(c-sample1_opposite)

	half dc = tex2D(depthSampler, in_TexCoord).x;

	half2 dd;
	dd.x = (half)tex2D(depthSampler, topLeft).x +
           (half)tex2D(depthSampler, bottomRight).x;

	dd.y = (half)tex2D(depthSampler, topRight).x +
           (half)tex2D(depthSampler, bottomLeft).x;

	dd = abs(2 * dc - dd);
	dd = dd > DepthThreshold; //step(dd, DepthThreshold);
	half de = 1 - saturate(dot(dd, DepthWeight));
	
    half w = 1 - ne * de;
	out_Colour = float4(w, w, w, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 EdgeDetectPS();
	}
}
