#version 330 core

in vec2 vTexCoord;

uniform sampler2D uSceneTexture;
uniform float uThreshold;
uniform float uIntensity;

out vec4 fragColor;

float getLuminance(vec3 color)
{
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

float smoothThreshold(float luminance, float threshold, float softness)
{
    return smoothstep(threshold - softness, threshold + softness, luminance);
}

float calculateBrightnessMultiplier(float luminance, float intensity)
{
    float baseMultiplier = intensity;
    float brightnessBoost = smoothstep(0.8, 1.5, luminance) * 1.5;
    return baseMultiplier * (1.0 + brightnessBoost);
}

vec3 adjustColorTemperature(vec3 color, float luminance)
{
    float temperatureShift = luminance * 0.1;
    vec3 adjusted = color;
    adjusted.b += temperatureShift;
    adjusted.r += temperatureShift * 0.5;
    return adjusted;
}

void main()
{
    vec3 sceneColor = texture(uSceneTexture, vTexCoord).rgb;
    
    float luminance = getLuminance(sceneColor);
    
    float thresholdSoftness = 0.1;
    float thresholdMask = smoothThreshold(luminance, uThreshold, thresholdSoftness);
    
    float brightnessMultiplier = calculateBrightnessMultiplier(luminance, uIntensity);
    vec3 amplifiedColor = sceneColor * brightnessMultiplier;
    
    amplifiedColor = adjustColorTemperature(amplifiedColor, luminance);
    
    vec3 extractedColor = amplifiedColor * thresholdMask;
    
    float energyScale = 1.0 / (1.0 + luminance * 0.1);
    extractedColor *= energyScale;
    
    fragColor = vec4(extractedColor, 1.0);
}