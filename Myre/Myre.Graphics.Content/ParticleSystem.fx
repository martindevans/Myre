// Based upon Partile3D sample on AppHub.
// Modified to support more flexibility, to fit into the Myre.Graphics engine, and to support soft particles.

float4x4 World : WORLD;
float4x4 ViewProjection : VIEWPROJECTION;
float4x4 Projection : PROJECTION;
float4x4 InverseProjection : INVERSEPROJECTION;
float FarClip : FARCLIP;
float3 CameraPosition : CAMERAPOSITION;

texture Depth : GBUFFER_DEPTH;

texture Texture;
float Lifetime;
float EndVelocity;
float EndScale;
float3 Gravity;
float Time;
float2 ViewportScale;

sampler textureSampler = sampler_state
{
	Texture = (Texture);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
};

sampler depthSampler = sampler_state
{
	Texture = (Depth);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

// Vertex shader helper for computing the position of a particle.
float4 ComputeParticlePosition(float3 position, float3 velocity, float age, float normalisedAge, float size, uniform bool offsetZBySize)
{
    float startVelocity = length(velocity);

    // Work out how fast the particle should be moving at the end of its life,
    // by applying a constant scaling factor to its starting velocity.
    float endVelocity = startVelocity * EndVelocity;
    
    // Our particles have constant acceleration, so given a starting velocity
    // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
    // The particle position is the sum of this velocity over the range 0 to T.
    // To compute the position directly, we must integrate the velocity
    // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

    float velocityIntegral = startVelocity * normalisedAge +
                             (endVelocity - startVelocity) * normalisedAge *
                                                             normalisedAge / 2;
     
    position += normalize(velocity) * velocityIntegral * Lifetime;
    
    // Apply the gravitational force.
    position += Gravity * age * normalisedAge;

	/*
	if (offsetZBySize)
	{
		float3 toCamera = CameraPosition - position;
		position += normalize(toCamera) * size;
	}
	*/
    
    // Apply the camera view and projection transforms.
    return mul(mul(float4(position, 1), World), ViewProjection);
}


// Vertex shader helper for computing the size of a particle.
float ComputeParticleSize(float startSize, float normalisedAge)
{    
    // Compute the actual size based on the age of the particle.
    float size = lerp(startSize, startSize * EndScale, normalisedAge);
    
    return size;
}


// Vertex shader helper for computing the colour of a particle.
float4 ComputeParticleColour(float4 startColour, float4 endColour, float normalisedAge)
{
    // Apply a random factor to make each particle a slightly different color.
    float4 colour = lerp(startColour, endColour, normalisedAge);
    
    // Fade the alpha based on the age of the particle. This curve is hard coded
    // to make the particle fade in fairly quickly, then fade out more slowly:
    // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
    // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
    // will reach all the way up to fully solid.
    
    colour *= normalisedAge * (1-normalisedAge) * (1-normalisedAge) * 6.7;
   
    return colour;
}


// Vertex shader helper for computing the rotation of a particle.
float2x2 ComputeParticleRotation(float angularVelocity, float age)
{    
    float rotation = angularVelocity * age;

    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);
    
    return float2x2(c, -s, s, c);
}

void VertexShaderFunction(in float2 corner : POSITION0,
						  in float3 position : POSITION1,
						  in float4 velocity : NORMAL0,
						  in float4 startColour : COLOR0,
						  in float4 endColour : COLOR1,
						  in float2 scales : TEXCOORD0,
						  in float  startTime : TEXCOORD1,
						  out float4 out_Position : POSITION0,
						  out float4 out_Colour : COLOR0,
						  out float2 out_TexCoord : TEXCOORD0,
						  out float4 out_PositionCS : TEXCOORD1,
						  out float2 out_Depth : TEXCOORD2,
						  uniform bool soft)
{
    // Compute the age of the particle.
    float age = Time - startTime;
    
    // Apply a random factor to make different particles age at different rates.
    age *= scales.y;
    
    // Normalize the age into the range zero to one.
    float normalisedAge = saturate(age / Lifetime);

    float size = ComputeParticleSize(scales.x, normalisedAge);
    float2x2 rotation = ComputeParticleRotation(velocity.w, age);

	// Compute the particle position, size, color, and rotation.
    out_Position = ComputeParticlePosition(position, velocity.xyz, age, normalisedAge, size, soft);

	float2 offset = mul(corner, rotation) * size * Projection._m11 * ViewportScale;
    out_Position.xy += offset;

	out_PositionCS = out_Position;

	float4 viewPosition = mul(out_Position, InverseProjection);
	out_Depth = float2(-viewPosition.z, size / 2) / FarClip;
    
    out_Colour = ComputeParticleColour(startColour, endColour, normalisedAge);
    out_TexCoord = (corner + 1) / 2;
}

float4 PixelShaderFunction(in float4 colour   : COLOR0,
						   in float2 texCoord : TEXCOORD0,
						   in float4 positionCS : TEXCOORD1,
						   in float2 depth : TEXCOORD2,
						   uniform bool soft) : COLOR0
{
	if (soft)
	{
		positionCS /= positionCS.w;
		float2 depthTexCoord = float2(positionCS.x / 2 + 0.5, -positionCS.y / 2 + 0.5);
		float sceneDepth = tex2D(depthSampler, depthTexCoord).r;
		float depthDelta = sceneDepth - depth.x;

		colour *= saturate(depthDelta / depth.y);
	}

    return tex2D(textureSampler, texCoord) * colour;

	
	//positionCS /= positionCS.w;
	//float2 depthTexCoord = float2(positionCS.x / 2 + 0.5, -positionCS.y / 2 + 0.5);
	//float sceneDepth = tex2D(depthSampler, depthTexCoord).r - depth.x;
	//return saturate((sceneDepth - depth.x) / depth.y);
	//return float4(sceneDepth, sceneDepth, sceneDepth, 1);
	//return float4(depthTexCoord, 1, 1);
	//return depth.x;
	//return sceneDepth - depth.x;
}

technique Hard
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction(false);
        PixelShader = compile ps_3_0 PixelShaderFunction(false);
    }
}

technique Soft
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction(true);
		PixelSHader = compile ps_3_0 PixelShaderFunction(true);
	}
}
