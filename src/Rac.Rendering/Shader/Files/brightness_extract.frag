#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
uniform sampler2D uSceneTexture;
uniform float uThreshold; // Brightness threshold (e.g., 0.8)
uniform float uIntensity; // Bloom intensity multiplier

void main()
{
    vec3 color = texture(uSceneTexture, TexCoord).rgb;
    
    // Calculate brightness using luminance
    float brightness = dot(color, vec3(0.2126, 0.7152, 0.0722));
    
    // Extract bright areas above threshold
    if (brightness > uThreshold) {
        // Scale the extracted color by intensity
        FragColor = vec4(color * uIntensity, 1.0);
    } else {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}