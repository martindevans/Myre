#define BIAS 0//0.001

float2 ShadowMapSize;

texture ShadowMap;
sampler shadowSampler = sampler_state
{
	Texture = (ShadowMap);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};


float CalculateShadow(float depth, float2 texCoord, uniform int sqrtSamples, uniform float biasScale)
{
    float shadow = 0.0f;
    
    float radius = (sqrtSamples - 1.0f) / 2;
    for (float y = -radius; y <= radius; y++)
    {
        for (float x = -radius; x <= radius; x++)
        {
            float2 offset = 0;
            offset = float2(x, y);
            offset /= ShadowMapSize;
            float2 sampleCoord = texCoord + offset;
            float sampleDepth = tex2D(shadowSampler, sampleCoord).x;
            float sample = (depth <= sampleDepth + BIAS * biasScale);
            
            float xWeight = 1;
            float yWeight = 1;
            
            if (x == -radius)
                xWeight = 1 - frac(texCoord.x * ShadowMapSize.x);
            else if (x == radius)
                xWeight = frac(texCoord.x * ShadowMapSize.x);
            
            if (y == -radius)
                yWeight = 1 - frac(texCoord.y * ShadowMapSize.y);
            else if (y == radius)
                yWeight = frac(texCoord.y * ShadowMapSize.y);

            shadow += sample * xWeight * yWeight;
        }
    }
    
    shadow /= (sqrtSamples * sqrtSamples);
    
    return shadow;
}


/*
// Calculates the shadow term using PCF with edge tap smoothing
float CalculateShadow(float fLightDepth, float2 vTexCoord, int iSqrtSamples)
{
    float fShadowTerm = 0.0f;  
        
    float fRadius = (iSqrtSamples - 1.0f) / 2;        
    for (float y = -fRadius; y <= fRadius; y++)
    {
        for (float x = -fRadius; x <= fRadius; x++)
        {
            float2 vOffset = 0;
            vOffset = float2(x, y);                
            vOffset /= ShadowMapSize;
            float2 vSamplePoint = vTexCoord + vOffset;            
            float fDepth = tex2D(shadowSampler, vSamplePoint).x;
            float fSample = (fLightDepth <= fDepth + BIAS);
            
            // Edge tap smoothing
            float xWeight = 1;
            float yWeight = 1;
            
            if (x == -fRadius)
                xWeight = 1 - frac(vTexCoord.x * ShadowMapSize.x);
            else if (x == fRadius)
                xWeight = frac(vTexCoord.x * ShadowMapSize.x);
                
            if (y == -fRadius)
                yWeight = 1 - frac(vTexCoord.y * ShadowMapSize.y);
            else if (y == fRadius)
                yWeight = frac(vTexCoord.y * ShadowMapSize.y);
                
            fShadowTerm += fSample * xWeight * yWeight;
        }                                            
    }        
    
    fShadowTerm /= (iSqrtSamples * iSqrtSamples);
    
    return fShadowTerm;
}
*/