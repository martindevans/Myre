// Lambert Azimuthal Equal-Area projection
// http://aras-p.info/texts/CompactNormalStorage.html

float2 EncodeNormal(float3 n)
{
    float f = sqrt(8*n.z+8);
    return n.xy / f + 0.5;
}

float3 DecodeNormal(float2 enc)
{
    float2 fenc = enc*4-2;
    float f = dot(fenc,fenc);
    float g = sqrt(1-f/4);

    float3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
}