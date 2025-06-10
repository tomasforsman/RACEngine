#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
uniform sampler2D uBrightTexture;
uniform bool uHorizontal; // true for horizontal pass, false for vertical
uniform float uBlurSize; // Blur radius (e.g., 2.0)

// Gaussian blur weights for 9-tap kernel
const float weights[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main()
{
    vec2 texelSize = 1.0 / textureSize(uBrightTexture, 0);
    vec3 result = texture(uBrightTexture, TexCoord).rgb * weights[0];
    
    if (uHorizontal) {
        // Horizontal blur
        for (int i = 1; i < 5; ++i) {
            result += texture(uBrightTexture, TexCoord + vec2(texelSize.x * i * uBlurSize, 0.0)).rgb * weights[i];
            result += texture(uBrightTexture, TexCoord - vec2(texelSize.x * i * uBlurSize, 0.0)).rgb * weights[i];
        }
    } else {
        // Vertical blur
        for (int i = 1; i < 5; ++i) {
            result += texture(uBrightTexture, TexCoord + vec2(0.0, texelSize.y * i * uBlurSize)).rgb * weights[i];
            result += texture(uBrightTexture, TexCoord - vec2(0.0, texelSize.y * i * uBlurSize)).rgb * weights[i];
        }
    }
    
    FragColor = vec4(result, 1.0);
}