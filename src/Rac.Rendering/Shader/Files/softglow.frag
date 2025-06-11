#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// SOFT GLOW FRAGMENT SHADER - ADDITIVE PARTICLE EFFECTS
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Specialized fragment shader for particle systems and soft light effects.
// Generates smooth radial gradients with distance-based transparency for seamless
// additive blending and atmospheric effects.
//
// RENDERING CHARACTERISTICS:
// - Radial gradient alpha falloff for smooth particle edges
// - Distance-based transparency for depth-like effects
// - Additive blending compatibility for light accumulation
// - Smooth interpolation functions for visual quality
//
// USE CASES:
// - Particle systems (fire, smoke, sparks, magic effects)
// - Light corona and atmospheric scattering simulation
// - UI element highlighting and emphasis effects
// - Soft shadow and ambient lighting approximation
//
// BLENDING RECOMMENDATION:
// - Source: GL_SRC_ALPHA
// - Destination: GL_ONE (additive blending)
// - Results in natural light accumulation behavior
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates (used as local particle coordinates)
in vec2 vTexCoord;

// Final vertex color (base particle color and alpha)
in vec4 vColor;

// Distance from center (pre-calculated for performance)
in float vDistance;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// Final fragment color optimized for additive blending
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// SOFT GLOW UTILITY FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Smooth radial falloff function with configurable parameters
// Creates natural-looking particle edges with customizable softness
float radialFalloff(float distance, float coreRadius, float edgeRadius, float power)
{
    // Core region: full intensity
    if (distance <= coreRadius) {
        return 1.0;
    }
    
    // Edge region: smooth falloff
    if (distance <= edgeRadius) {
        float t = (distance - coreRadius) / (edgeRadius - coreRadius);
        return pow(1.0 - t, power);
    }
    
    // Outside region: no contribution
    return 0.0;
}

// Enhanced smoothstep with adjustable curve shape
// Provides more artistic control over falloff characteristics
float enhancedSmoothstep(float edge0, float edge1, float x, float curve)
{
    float t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return pow(t, curve) * (3.0 - 2.0 * pow(t, curve));
}

// Atmospheric scattering approximation for realistic light behavior
// Simulates how light intensity decreases with distance through atmosphere
vec3 atmosphericScattering(vec3 color, float distance, float density)
{
    float scattering = exp(-distance * density);
    return color * scattering;
}

// Soft edge calculation for natural particle boundaries
// Eliminates harsh cutoffs that break immersion in particle effects
float calculateSoftEdge(float distance)
{
    // Multi-stage falloff for natural particle appearance
    const float coreRadius = 0.2;    // Solid center region
    const float midRadius = 0.6;     // Primary transition zone  
    const float edgeRadius = 1.0;    // Outer boundary
    const float softRadius = 1.2;    // Soft edge extension
    
    if (distance <= coreRadius) {
        // Core: full intensity
        return 1.0;
    } else if (distance <= midRadius) {
        // Primary transition: gentle curve
        float t = (distance - coreRadius) / (midRadius - coreRadius);
        return 1.0 - smoothstep(0.0, 1.0, t) * 0.3;
    } else if (distance <= edgeRadius) {
        // Edge transition: steeper falloff
        float t = (distance - midRadius) / (edgeRadius - midRadius);
        return 0.7 * (1.0 - smoothstep(0.0, 1.0, t));
    } else if (distance <= softRadius) {
        // Soft edge: very gentle fade to zero
        float t = (distance - edgeRadius) / (softRadius - edgeRadius);
        return 0.7 * (1.0 - smoothstep(0.0, 1.0, t * t));
    } else {
        // Outside: no contribution
        return 0.0;
    }
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN FRAGMENT FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // DISTANCE-BASED ALPHA CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Create smooth radial falloff from particle center to edges.
    // This is the defining characteristic of soft glow effects.
    //
    // FALLOFF CHARACTERISTICS:
    // - Smooth gradients prevent visible banding
    // - Natural light-like behavior
    // - Configurable for different particle types
    //
    float softAlpha = calculateSoftEdge(vDistance);
    
    // Early discard for performance optimization
    if (softAlpha < 0.002) {
        discard;
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // COLOR INTENSITY MODULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Enhance color intensity at particle center for more dramatic effects.
    // This simulates how bright light sources appear brighter at their core.
    //
    float intensityBoost = 1.0 + (1.0 - vDistance) * 0.5; // 50% boost at center
    vec3 enhancedColor = vColor.rgb * intensityBoost;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ATMOSPHERIC EFFECTS
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle atmospheric scattering for more realistic particle behavior.
    // This makes particles feel more integrated with their environment.
    //
    float atmosphericDensity = 0.1; // Light atmospheric effect
    enhancedColor = atmosphericScattering(enhancedColor, vDistance, atmosphericDensity);
    
    // ════════════════════════════════════════════════════════════════════════════
    // ENERGY CONSERVATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Ensure that total light energy remains reasonable for additive blending.
    // Prevents particle accumulation from overwhelming the scene.
    //
    float energyConservation = 0.8; // Slight energy reduction
    enhancedColor *= energyConservation;
    
    // ════════════════════════════════════════════════════════════════════════════
    // FINAL ALPHA COMPOSITION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Combine base vertex alpha with calculated soft falloff.
    // This allows for both per-particle and distance-based transparency.
    //
    float finalAlpha = vColor.a * softAlpha;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ADDITIVE BLENDING OPTIMIZATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Pre-multiply alpha for proper additive blending behavior.
    // This ensures particles accumulate naturally when overlapping.
    //
    // BLENDING EQUATION:
    // Result = Source * SourceAlpha + Destination * 1.0
    // Pre-multiplying optimizes this calculation on the GPU.
    //
    enhancedColor *= finalAlpha;
    
    // ════════════════════════════════════════════════════════════════════════════
    // OUTPUT FOR ADDITIVE BLENDING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Output color optimized for additive blending mode.
    // Alpha channel contains the pre-multiplied contribution.
    //
    fragColor = vec4(enhancedColor, finalAlpha);
}