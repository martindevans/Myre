#ifndef SkinningHeaderFxh
#define SkinningHeaderFxh

uniform int WeightsPerVertex : WEIGHTS_PER_VERTEX = 4;

#define SKINNED_EFFECT_MAX_BONES   60
float4x4 Bones[SKINNED_EFFECT_MAX_BONES] : BONES;

float4x3 CalculateSkinMatrix(int4 indices, float4 weights, uniform int boneCount)
{
    float4x3 skinning = 0;

	//return (float4x3)Bones[indices[0]];

    for (int i = 0; i < boneCount; i++)
        skinning += ((float4x3)Bones[indices[i]]) * weights[i];

	return skinning;
}

float4 SkinTransformPosition(float4 position, float4x3 skinTransform)
{
	position.xyz = mul(position, skinTransform);
	return position;
}

float3 SkinTransformNormal(float3 normal, float4x3 skinTransform)
{
	return mul(normal, (float3x3)skinTransform);
}

#endif