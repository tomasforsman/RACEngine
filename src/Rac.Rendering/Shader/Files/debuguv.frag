#version 330 core

// ═══════════════════════════════════════════════════════════════════════════════
// UV COORDINATE DEBUGGING FRAGMENT SHADER
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL PURPOSE:
// This shader provides visual debugging capabilities for texture coordinate mapping,
// enabling developers to quickly identify UV mapping issues in rendered geometry.
//
// VISUALIZATION STRATEGY:
// - U coordinate (horizontal) → Red channel intensity
// - V coordinate (vertical) → Green channel intensity  
// - Blue channel → Constant low value for contrast
// - Alpha channel → Full opacity for clear visibility
//
// COORDINATE INTERPRETATION:
// - (0,0) → Black (bottom-left in standard UV space)
// - (1,0) → Red (bottom-right)
// - (0,1) → Green (top-left)
// - (1,1) → Yellow (top-right, Red + Green)
//
// GRAPHICS THEORY REFERENCE:
// UV coordinates represent 2D parametric surface mapping, commonly used in:
// - Texture atlas mapping
// - Procedural pattern generation
// - Surface parameterization for complex geometry
// - Real-time shader effects and animations
//
// ═══════════════════════════════════════════════════════════════════════════════

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

out vec4 fragColor;

void main()
{
    // ───────────────────────────────────────────────────────────────────────────
    // UV COORDINATE TO COLOR MAPPING
    // ───────────────────────────────────────────────────────────────────────────
    //
    // Map texture coordinates directly to RGB color channels for immediate
    // visual feedback about UV coordinate distribution across the geometry.
    //
    // Mathematical mapping:
    // U ∈ [0,1] → Red channel ∈ [0,1]
    // V ∈ [0,1] → Green channel ∈ [0,1]
    // Blue = 0.2 (constant for contrast)
    // Alpha = 1.0 (full opacity)
    
    vec3 debugColor = vec3(
        vTexCoord.x,        // U coordinate → Red channel
        vTexCoord.y,        // V coordinate → Green channel
        0.2                 // Low blue for visual contrast
    );
    
    // ───────────────────────────────────────────────────────────────────────────
    // COLOR SPACE CONSIDERATIONS
    // ───────────────────────────────────────────────────────────────────────────
    //
    // Output in linear color space for accurate debugging visualization.
    // Post-processing pipeline will handle gamma correction if enabled.
    
    fragColor = vec4(debugColor, 1.0);
}