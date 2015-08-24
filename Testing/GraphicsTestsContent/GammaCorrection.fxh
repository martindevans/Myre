float3 LinearToGamma(float3 colour)
{
    return pow(abs(colour), 1/2.2f);
}

float3 GammaToLinear(float3 colour)
{
    return pow(abs(colour), 2.2f);
}