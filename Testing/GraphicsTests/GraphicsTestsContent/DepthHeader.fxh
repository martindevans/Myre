float CalculateDepth(float4 viewPosition, float farClip) {
	return -viewPosition.z / farClip;
}