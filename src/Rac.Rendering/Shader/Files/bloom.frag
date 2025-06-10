#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    // Enhanced bloom effect with strong brightness boost and color saturation
    vec3 baseColor = uColor.rgb;
    
    // Strong brightness boost for bloom
    vec3 bloomColor = baseColor * 2.2;
    
    // Increase color saturation for more vibrant bloom
    float luminance = dot(bloomColor, vec3(0.299, 0.587, 0.114));
    bloomColor = mix(vec3(luminance), bloomColor, 1.4);
    
    // Slight shift towards white for authentic bloom look
    bloomColor = mix(bloomColor, vec3(1.0), 0.1);
    
    // Clamp to prevent overflow
    bloomColor = min(bloomColor, vec3(1.0));
    
    fragColor = vec4(bloomColor, uColor.a * 1.1);
}