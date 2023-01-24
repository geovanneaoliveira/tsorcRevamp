matrix WorldViewProjection;
texture noiseTexture;
sampler textureSampler = sampler_state
{
    Texture = (noiseTexture);
    AddressU = wrap;
    AddressV = wrap;
};

float fadeOut;
float time;
float4 shaderColor;
float4 secondaryColor;
float length;
float speed;
float2 samplePointOffset1;
float2 samplePointOffset2;

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    //return float4(0.8, 1, 0.4, 1);
    float2 uv = input.TextureCoordinates;
    //float pixelSize = 0.0005;
    //uv.x = uv.x - fmod(uv.x, pixelSize);
    //uv.y = uv.y - fmod(uv.y, pixelSize  * 50);
    
    //Calculate how close the current pixel is to the center line of the screen
    float intensity = 1.0 - abs(uv.y - 0.5);
    
    //Raise it to a high exponent, resulting in sharply increased intensity at the center that trails off smoothly
    //Higher number = more narrow and compressed trail
    intensity = pow(intensity, 3.0);
    
    //Flat doubling to incrase the total intensity
    intensity *= 1;
    
    //This controls where the front of the bolt starts to curve
    float inflectionPoint = 0.9;
    //inflectionPoint *= fadeOut;
    
    //Make it fade out towards the end
    float start = 0.80;
    float end = 0.15;
    float yStart = 0.7;
    
    //Make it fade in towards the start
    if (uv.x > start)
    {
        intensity = lerp(0.0, intensity, pow((1.0 - uv.x) / (1 - start), 0.115));
    }
    if (uv.x < end)
    {
        intensity = lerp(0.0, intensity, pow((uv.x) / (end), 4));
    }
    float yEnd = 1 - yStart;
    if (uv.y > yStart)
    {
        intensity = lerp(0.0, intensity, pow((1.0 - uv.y) / (1 - yStart), 3));
    }
    if (uv.y < yEnd)
    {
        intensity = lerp(0.0, intensity, pow((uv.y) / (yEnd), 3));
    }
    
    float trailProgress = (1 - uv.x * uv.x);
    trailProgress = (trailProgress * 0.33) + 0.66;
    float4 trailColor = lerp(shaderColor * 2, secondaryColor * 5, trailProgress);
    //trailColor = secondaryColor * 5;
    
    intensity = pow(intensity, 5.0);
    intensity = intensity * uv.x;
    
    intensity *= fadeOut * fadeOut;
    intensity *= 2;

    //Pick where to sample the texture used for the flowing effect
    float2 samplePoint1 = uv;
    
    //Zoom in on the noise texture, then shift it over time to make it appear to be flowing    
    //samplePoint /= 70;
    samplePoint1.x = (samplePoint1.x) * length / 100; // ;
    samplePoint1.x = samplePoint1.x + time / 2;
    
    //Compress it vertically
    samplePoint1.y = (samplePoint1.y / 1);
    samplePoint1 /= 4;
    
    float2 samplePoint2 = samplePoint1;
    samplePoint1 += samplePointOffset1;
    samplePoint2 += samplePointOffset2;
    samplePoint2.x = samplePoint2.x + time / 2;

    //Get the noise texture at that point
    float sampleIntensity = tex2D(textureSampler, samplePoint1).r;
    sampleIntensity *= tex2D(textureSampler, samplePoint2).r;
    
    sampleIntensity = sampleIntensity;
    //Mix it with the laser color
    float4 noiseColor = float4(1.0, 1.0, 1.0, 1.0);
    noiseColor.r = sampleIntensity * trailColor.r;
    noiseColor.b = sampleIntensity * trailColor.b;
    noiseColor.g = sampleIntensity * trailColor.g;
    
    //return noiseColor;
    //
    //Mix it with 'intensity' to make it more intense near the center
    //float4 effectColor = pow(noiseColor, 0.85) * pow(intensity, 2) * 1.0;
    
    //Not the vibe i'm going for here, but looks cool as hell and will be useful later:
    if (intensity > 1)
    {
        intensity = 1;
    }
    
    return pow(noiseColor, 1) * 13.0 * intensity * pow(shaderColor, 2.5);
}


technique InterstellarVessel
{
    pass InterstellarVesselPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 MainPS();
    }
};