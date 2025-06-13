#version 330 core

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

out vec4 fragColor;

float getLuminance(vec3 color)
{
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

vec3 enhanceHDRColor(vec3 color, float enhancementFactor)
{
    float baseLuminance = getLuminance(color);
    
    float amplification = 1.0 + enhancementFactor * baseLuminance;
    
    return color * amplification;
}

float calculateBloomIntensity(float distance, vec3 color)
{
    float colorIntensity = getLuminance(color);
    
    float distanceFalloff = 1.0 / (1.0 + distance * distance * 0.5);
    
    float brightnessBoost = 1.0 + smoothstep(0.8, 1.0, colorIntensity) * 2.0;
    
    return colorIntensity * distanceFalloff * brightnessBoost;
}

vec3 applyEnergyDistribution(vec3 baseColor, float distance, float energy)
{
    float coreEnergy = smoothstep(0.4, 0.0, distance) * energy * 2.0;
    
    float midEnergy = smoothstep(0.8, 0.2, distance) * energy * 1.0;
    
    float outerEnergy = smoothstep(1.2, 0.6, distance) * energy * 0.3;
    
    float totalEnergy = coreEnergy + midEnergy + outerEnergy;
    
    return baseColor * (1.0 + totalEnergy);
}

vec3 applyColorTemperature(vec3 color, float temperature)
{
    vec3 temperatureColor = color;
    
    if (temperature > 0.0) {
        temperatureColor.r *= (1.0 + temperature * 0.3);
        temperatureColor.g *= (1.0 + temperature * 0.1);
        temperatureColor.b *= (1.0 - temperature * 0.2);
    } else {
        float coldness = -temperature;
        temperatureColor.r *= (1.0 - coldness * 0.2);
        temperatureColor.g *= (1.0 + coldness * 0.1);
        temperatureColor.b *= (1.0 + coldness * 0.4);
    }
    
    return temperatureColor;
}

void main()
{
    vec3 baseColor = vColor.rgb;
    float baseAlpha = vColor.a;
    
    float bloomIntensity = calculateBloomIntensity(vDistance, baseColor);
    
    float dynamicRange = 3.0;
    float bloomEnhancement = 2.5;
    
    vec3 hdrColor = enhanceHDRColor(baseColor, bloomEnhancement) * dynamicRange;
    
    float energy = getLuminance(hdrColor) * 1.5;
    hdrColor = applyEnergyDistribution(hdrColor, vDistance, energy);
    
    float bloomAlpha = baseAlpha * bloomIntensity * 1.2;
    
    float colorTemperatureShift = (getLuminance(baseColor) - 0.5) * 0.4;
    hdrColor = applyColorTemperature(hdrColor, colorTemperatureShift);
    
    float edgeSoftening = smoothstep(1.0, 0.8, vDistance);
    hdrColor *= edgeSoftening;
    bloomAlpha *= edgeSoftening;
    
    if (bloomAlpha < 0.005 || getLuminance(hdrColor) < 0.1) {
        discard;
    }
    
    fragColor = vec4(hdrColor, bloomAlpha);
}