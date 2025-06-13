#version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;

uniform float uAspect;
uniform vec4 uColor;

out vec2 vTexCoord;
out vec4 vColor;
out float vDistance;

void main()
{
    vec2 correctedPosition = aPosition;
    correctedPosition.x *= uAspect;
    
    gl_Position = vec4(correctedPosition, 0.0, 1.0);
    
    vTexCoord = aTexCoord;
    
    if (aColor.a > 0.0) {
        vColor = aColor;
    } else {
        vColor = uColor;
    }
    
    vDistance = length(aTexCoord);
}