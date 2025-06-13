#version 330 core

in vec2 vTexCoord;

uniform sampler2D uBrightTexture;
uniform int uHorizontal;
uniform float uBlurSize;

out vec4 fragColor;

void main()
{
    vec2 texelSize = 1.0 / textureSize(uBrightTexture, 0);
    vec2 direction = uHorizontal == 1 ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    
    vec3 color = vec3(0.0);
    float totalWeight = 0.0;
    
    const int samples = 9;
    const float weights[samples] = float[](
        0.0625, 0.125, 0.1875, 0.25, 0.3125, 0.25, 0.1875, 0.125, 0.0625
    );
    
    for (int i = 0; i < samples; i++)
    {
        float offset = (float(i) - float(samples - 1) * 0.5) * uBlurSize;
        vec2 sampleCoord = vTexCoord + direction * texelSize * offset;
        
        if (sampleCoord.x >= 0.0 && sampleCoord.x <= 1.0 && 
            sampleCoord.y >= 0.0 && sampleCoord.y <= 1.0)
        {
            vec3 sampleColor = texture(uBrightTexture, sampleCoord).rgb;
            float weight = weights[i];
            
            color += sampleColor * weight;
            totalWeight += weight;
        }
    }
    
    if (totalWeight > 0.0)
    {
        color /= totalWeight;
    }
    
    fragColor = vec4(color, 1.0);
}