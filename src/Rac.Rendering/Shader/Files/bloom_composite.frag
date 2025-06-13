#version 330 core

in vec2 vTexCoord;

uniform sampler2D uSceneTexture;
uniform sampler2D uBloomTexture;
uniform float uBloomStrength;
uniform float uExposure;

out vec4 fragColor;

vec3 reinhardToneMapping(vec3 color, float exposure)
{
    color *= exposure;
    return color / (1.0 + color);
}

vec3 acesToneMapping(vec3 color, float exposure)
{
    color *= exposure;
    
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    
    return clamp((color * (a * color + b)) / (color * (c * color + d) + e), 0.0, 1.0);
}

vec3 gammaCorrection(vec3 color, float gamma)
{
    return pow(color, vec3(1.0 / gamma));
}

void main()
{
    vec3 sceneColor = texture(uSceneTexture, vTexCoord).rgb;
    vec3 bloomColor = texture(uBloomTexture, vTexCoord).rgb;
    
    vec3 combinedColor = sceneColor + bloomColor * uBloomStrength;
    
    vec3 toneMappedColor = acesToneMapping(combinedColor, uExposure);
    
    vec3 finalColor = gammaCorrection(toneMappedColor, 2.2);
    
    finalColor = clamp(finalColor, 0.0, 1.0);

    fragColor = vec4(finalColor, 1.0);
}