#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    // Keep boids simple and bright for proper bloom extraction
    // The bloom effect is now handled by post-processing
    fragColor = uColor;
}