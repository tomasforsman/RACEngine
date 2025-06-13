#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// FULLSCREEN QUAD VERTEX SHADER - POST-PROCESSING GEOMETRY
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Positions vertices for fullscreen post-processing effects.
// Creates a screen-aligned quad covering the entire viewport for pixel-level
// processing operations like bloom, blur, and tone mapping.
//
// GEOMETRY STRUCTURE:
// - Two triangles forming a quad covering normalized device coordinates [-1,+1]
// - UV coordinates [0,1] for texture sampling
// - No model/view/projection matrices needed (already in clip space)
//
// RENDERING PIPELINE POSITION:
// Scene → Offscreen Framebuffer → Post-processing Quad → Screen
// This shader handles the "Post-processing Quad" stage
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// VERTEX ATTRIBUTES
// ────────────────────────────────────────────────────────────────────────────────

// Vertex position in normalized device coordinates [-1, +1]
// Forms a quad covering the entire screen
layout(location = 0) in vec2 aPosition;

// Texture coordinates for sampling framebuffer textures
// Range [0, 1] mapping to screen coverage
layout(location = 1) in vec2 aTexCoord;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES (TO FRAGMENT SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates for fragment shader sampling
out vec2 vTexCoord;

// ────────────────────────────────────────────────────────────────────────────────
// MAIN TRANSFORMATION FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // POSITION PROCESSING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Input positions are already in normalized device coordinates [-1, +1].
    // No transformation needed - pass through directly to clip space.
    //
    // COORDINATE SYSTEM:
    // - (-1, -1) = bottom-left corner of screen
    // - (+1, +1) = top-right corner of screen
    // - Z = 0.0 (flat against near plane)
    // - W = 1.0 (no perspective division needed)
    //
    gl_Position = vec4(aPosition, 0.0, 1.0);
    
    // ════════════════════════════════════════════════════════════════════════════
    // TEXTURE COORDINATE PASSTHROUGH
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Pass texture coordinates to fragment shader for framebuffer sampling.
    // These coordinates map the fullscreen quad to texture space:
    // - (0, 0) = bottom-left of texture
    // - (1, 1) = top-right of texture
    //
    // GPU hardware automatically interpolates these values across the quad surface,
    // providing each fragment with its corresponding texture coordinate.
    //
    vTexCoord = aTexCoord;
}
