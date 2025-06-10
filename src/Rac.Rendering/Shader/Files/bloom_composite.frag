#version 330 core
out vec4 FragColor;

in vec2 TexCoord;
uniform sampler2D uSceneTexture;   // Original scene
uniform sampler2D uBloomTexture;   // Blurred bloom
uniform float uBloomStrength;      // How strong the bloom effect is (e.g., 0.8)
uniform float uExposure;           // HDR exposure (e.g., 1.0)

void main()
{
    vec3 sceneColor = texture(uSceneTexture, TexCoord).rgb;
    vec3 bloomColor = texture(uBloomTexture, TexCoord).rgb;
    
    // Additive blending
    vec3 result = sceneColor + bloomColor * uBloomStrength;
    
    // Tone mapping (simple exposure)
    result = vec3(1.0) - exp(-result * uExposure);
    
    // Gamma correction
    result = pow(result, vec3(1.0 / 2.2));
    
    FragColor = vec4(result, 1.0);
}