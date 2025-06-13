#version 330 core

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

out vec4 fragColor;

float smoothFalloff(float distance, float coreRadius, float edgeRadius, float power)
{
    if (distance <= coreRadius) {
        return 1.0;
    }
    
    if (distance <= edgeRadius) {
        float t = (distance - coreRadius) / (edgeRadius - coreRadius);
        return pow(1.0 - t, power);
    }
    
    return 0.0;
}

float enhancedSmoothstep(float edge0, float edge1, float x, float curve)
{
    float t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return pow(t, curve) * (3.0 - 2.0 * pow(t, curve));
}

vec3 atmosphericScattering(vec3 color, float distance, float density)
{
    float scattering = exp(-distance * density);
    return color * scattering;
}

float calculateSoftEdge(float distance)
{
    const float coreRadius = 0.2;
    const float midRadius = 0.6;
    const float edgeRadius = 1.0;
    const float softRadius = 1.2;
    
    if (distance <= coreRadius) {
        return 1.0;
    } else if (distance <= midRadius) {
        float t = (distance - coreRadius) / (midRadius - coreRadius);
        return 1.0 - smoothstep(0.0, 1.0, t) * 0.3;
    } else if (distance <= edgeRadius) {
        float t = (distance - midRadius) / (edgeRadius - midRadius);
        return 0.7 * (1.0 - smoothstep(0.0, 1.0, t));
    } else if (distance <= softRadius) {
        float t = (distance - edgeRadius) / (softRadius - edgeRadius);
        return 0.7 * (1.0 - smoothstep(0.0, 1.0, t * t));
    } else {
        return 0.0;
    }
}

void main()
{
    float softAlpha = calculateSoftEdge(vDistance);
    
    if (softAlpha < 0.002) {
        discard;
    }
    
    float intensityBoost = 1.0 + (1.0 - vDistance) * 0.5;
    vec3 enhancedColor = vColor.rgb * intensityBoost;
    
    float atmosphericDensity = 0.1;
    enhancedColor = atmosphericScattering(enhancedColor, vDistance, atmosphericDensity);
    
    float energyConservation = 0.8;
    enhancedColor *= energyConservation;
    
    float finalAlpha = vColor.a * softAlpha;
    
    fragColor = vec4(enhancedColor, finalAlpha);
}