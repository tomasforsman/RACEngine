#version 330 core

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

out vec4 fragColor;

float smoothEdge(float distance, float radius, float softness)
{
    return 1.0 - smoothstep(radius - softness, radius + softness, distance);
}

vec3 linearToSRGB(vec3 linear)
{
    return mix(
        linear * 12.92,
        pow(linear, vec3(1.0 / 2.4)) * 1.055 - 0.055,
        step(0.0031308, linear)
    );
}

void main()
{
    vec4 baseColor = vColor;
    
    float edgeSoftness = 0.02;
    float edgeRadius = 1.0;
    
    float edgeAlpha = smoothEdge(vDistance, edgeRadius, edgeSoftness);
    
    baseColor.a *= edgeAlpha;
    
    baseColor.rgb = linearToSRGB(baseColor.rgb);
    
    const float alphaThreshold = 0.003;
    
    if (baseColor.a < alphaThreshold) {
        discard;
    }
    
    fragColor = baseColor;
}