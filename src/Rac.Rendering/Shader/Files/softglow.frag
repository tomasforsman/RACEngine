#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    // Soft glow effect with moderate brightness boost and slight desaturation
    vec3 baseColor = uColor.rgb;
    
    // Boost brightness moderately
    vec3 glowColor = baseColor * 1.3;
    
    // Slight desaturation for softer appearance
    float luminance = dot(glowColor, vec3(0.299, 0.587, 0.114));
    glowColor = mix(glowColor, vec3(luminance), 0.1);
    
    // Clamp to prevent overflow
    glowColor = min(glowColor, vec3(1.0));
    
    fragColor = vec4(glowColor, uColor.a);
}