// Functions from http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping

float MinExposure : HDR_MINEXPOSURE;
float MaxExposure : HDR_MAXEXPOSURE;

// Applies the filmic curve from John Hable's presentation
float3 ToneMapFilmicALU(float3 color)
{
    color = max(0, color - 0.004f);
    color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f)+ 0.06f);

    // result has 1/2.2 baked in
    //return pow(color, 2.2f);
	return color;
}

// Determines the color based on exposure settings
float3 CalcExposedColor(float3 color, float avgLuminance, float threshold)
{
    avgLuminance = max(avgLuminance, 0.001f);

    float keyValue = 1.03f - (2.0f / (2 + log10(avgLuminance + 1)));

    float linearExposure = (keyValue / avgLuminance);
    float exposure = clamp(log2(max(linearExposure, 0.0001f)), MinExposure, MaxExposure);

	exposure -= threshold;
    return exp2(exposure) * color;
}