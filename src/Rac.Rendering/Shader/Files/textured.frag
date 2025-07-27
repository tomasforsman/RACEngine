#version 330 core

// ===============================================================================
// BASIC TEXTURED RENDERING FRAGMENT SHADER
// ===============================================================================
//
// EDUCATIONAL PURPOSE:
// This shader demonstrates basic texture sampling and blending with vertex colors.
// It provides the fundamental building block for textured geometry rendering.
//
// RENDERING STRATEGY:
// - Sample texture at UV coordinates provided by vertex shader
// - Multiply texture color with vertex color for tinting support
// - Apply global color uniform for additional effects
// - Preserve alpha channel for transparency support
//
// USE CASES:
// - Basic sprite rendering
// - Textured UI elements  
// - Simple textured geometry
// - Asset demonstration and debugging
//
// ===============================================================================

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

uniform sampler2D uTexture;  // Main texture sampler
uniform vec4 uColor;         // Global color modifier

out vec4 fragColor;

void main()
{
    // ---------------------------------------------------------------------------
    // TEXTURE SAMPLING
    // ---------------------------------------------------------------------------
    // Sample the texture at the interpolated UV coordinates
    vec4 textureColor = texture(uTexture, vTexCoord);
    
    // ---------------------------------------------------------------------------
    // COLOR COMPOSITION
    // ---------------------------------------------------------------------------
    // Combine texture color with vertex color and global color modifier
    // This allows for tinting, vertex-based coloring, and global effects
    vec4 finalColor = textureColor * vColor * uColor;
    
    // ---------------------------------------------------------------------------
    // ALPHA TESTING
    // ---------------------------------------------------------------------------
    // Discard fragments with very low alpha to prevent z-fighting
    // and improve rendering performance for transparent textures
    if (finalColor.a < 0.01) {
        discard;
    }
    
    fragColor = finalColor;
}