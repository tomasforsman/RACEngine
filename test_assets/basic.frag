#version 330 core

in vec2 texCoord;
out vec4 FragColor;

uniform vec4 uColor;
uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, texCoord) * uColor;
}