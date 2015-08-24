float CalculateDepth(float4 viewPosition, float farClip) {
	return -viewPosition.z / farClip;
}

void ZTest(float zBufferDepth, float pixelDepth) {
	clip(zBufferDepth - pixelDepth);
}

float3 ReconstructWorldPosition(float3 viewRay, float depth, float4x4 InverseView) {
	float3 viewPosition = viewRay * depth;

	//Calculate world position of this pixel
	return mul(float4(viewPosition, 1), InverseView).xyz;
}