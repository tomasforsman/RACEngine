#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// NORMAL FRAGMENT SHADER - STANDARD OPAQUE RENDERING
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: High-performance fragment shader for standard 2D geometry rendering.
// Optimized for solid shapes, UI elements, and basic textured geometry without
// special effects.
//
// RENDERING CHARACTERISTICS:
// - Solid color output with alpha blending support
// - Optimized for high polygon throughput
// - Standard sRGB color space handling  
// - Clean edges with analytical anti-aliasing
//
// USE CASES:
// - UI elements and interface components
// - Solid geometry and basic shapes
// - Text rendering and icon display
// - Background elements and simple sprites
//
// PERFORMANCE PROFILE:
// - Minimal texture fetches for maximum ALU utilization
// - Branch-free execution path for GPU efficiency
// - Optimized for mobile and integrated graphics
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates (for distance calculations)
in vec2 vTexCoord;

// Final vertex color (either per-vertex or global uniform)
in vec4 vColor;

// Distance from center (for optional edge softening)
in float vDistance;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// Final fragment color (LDR - Low Dynamic Range)
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// UTILITY FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Analytical anti-aliasing function for smooth edges
// Returns smooth falloff for values near 0, sharp cutoff elsewhere
float smoothEdge(float distance, float radius, float softness)
{
    return 1.0 - smoothstep(radius - softness, radius + softness, distance);
}

// Gamma correction for proper color space handling
// Converts from linear to sRGB color space
vec3 linearToSRGB(vec3 linear)
{
    return mix(
        linear * 12.92,
        pow(linear, vec3(1.0 / 2.4)) * 1.055 - 0.055,
        step(0.0031308, linear)
    );
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN FRAGMENT FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // BASE COLOR CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Start with the interpolated vertex color, which comes from either:
    // - Per-vertex color attributes (FullVertex)
    // - Global uniform color (BasicVertex, TexturedVertex)
    //
    vec4 baseColor = vColor;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ANALYTICAL ANTI-ALIASING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle edge softening for shapes that use texture coordinates
    // to define their boundaries. This provides cleaner edges than pure
    // geometric anti-aliasing, especially for UI elements.
    //
    // ANTI-ALIASING STRATEGY:
    // - Use distance field for smooth falloff
    // - Apply only near edges to maintain sharp appearance
    // - Preserve alpha channel for proper blending
    //
    float edgeSoftness = 0.02; // Subtle softening for crisp appearance
    float edgeRadius = 1.0;    // Unit circle boundary
    
    // Calculate edge alpha for smooth boundaries
    float edgeAlpha = smoothEdge(vDistance, edgeRadius, edgeSoftness);
    
    // Apply edge softening to alpha channel only
    baseColor.a *= edgeAlpha;
    
    // ════════════════════════════════════════════════════════════════════════════
    // COLOR SPACE CONVERSION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Convert from linear RGB to sRGB for proper display.
    // This ensures colors appear as intended on standard monitors.
    //
    // LINEAR VS SRGB:
    // - Linear: mathematically correct for lighting calculations
    // - sRGB: standard display color space with gamma correction
    // - Conversion necessary for proper color appearance
    //
    baseColor.rgb = linearToSRGB(baseColor.rgb);
    
    // ════════════════════════════════════════════════════════════════════════════
    // ALPHA HANDLING FOR PERFORMANCE
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Early alpha test for improved performance on fragment-heavy scenes.
    // Discards nearly transparent fragments to reduce overdraw.
    //
    // PERFORMANCE BENEFITS:
    // - Reduces memory bandwidth for transparent areas
    // - Improves depth testing efficiency
    // - Allows GPU to skip expensive blending operations
    //
    const float alphaThreshold = 0.003; // ~1/256 - minimal visible alpha
    
    if (baseColor.a < alphaThreshold) {
        discard;
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // FINAL OUTPUT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Output the final color in sRGB color space with proper alpha for blending.
    // This color will be alpha-blended with the framebuffer contents.
    //
    fragColor = baseColor;
}
