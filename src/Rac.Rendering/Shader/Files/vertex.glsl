#version 330 core
layout(location = 0) in vec2 position;
uniform float uAspect;
void main()
{
    gl_Position = vec4(position.x * uAspect, position.y, 0.0, 1.0);
}