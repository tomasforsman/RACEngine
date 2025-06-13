#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// BLOOM FRAGMENT SHADER - HDR BRIGHT RENDERING
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: High Dynamic Range (HDR) fragment shader for bright light sources and
// bloom-capable rendering. Outputs color values exceeding display range (>1.0)
// for post-processing pipeline integration.
//
// RENDERING CHARACTERISTICS:
// - HDR color output with values exceeding 1.0
// - Enhanced brightness amplification for glow effects
// - Linear color space rendering for post-processing
// - Optimized for bloom extraction and composition
//
// USE CASES:
// - Bright light sources (sun, lamps, lasers, energy effects)
// - Neon signs and glowing UI elements
// - Magic spells and supernatural effects
// - Fire, explosions, and high-energy phenomena
//
// POST-PROCESSING INTEGRATION:
// - Brightness extraction identifies bloomable regions
// - Gaussian blur creates glow halo effect
// - Final composition blends with main scene
// - Tone mapping converts HDR to display range
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates (for radial bloom effects)
in vec2 vTexCoord;

// Final vertex color (base HDR color)
in vec4 vColor;

// Distance from center (for radial intensity variation)
in float vDistance;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// HDR fragment color (values can exceed 1.0)
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// HDR AND BLOOM UTILITY FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Perceptual luminance calculation following ITU-R BT.709 standard
// Returns brightness as perceived by human vision
float getLuminance(vec3 color)
{
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

// HDR color enhancement for bloom-worthy brightness levels
// Amplifies colors based on their base brightness for dramatic effect
vec3 enhanceHDRColor(vec3 color, float enhancementFactor)
{
    float baseLuminance = getLuminance(color);
    
    // Amplify bright colors more than dark ones
    float amplification = 1.0 + enhancementFactor * baseLuminance;
    
    return color * amplification;
}

// Bloom intensity calculation based on distance and color properties
// Creates realistic light falloff for various light source types
float calculateBloomIntensity(float distance, vec3 color)
{
    // Base intensity from color brightness
    float colorIntensity = getLuminance(color);
    
    // Distance-based falloff (inverse square law approximation)
    float distanceFalloff = 1.0 / (1.0 + distance * distance * 0.5);
    
    // Enhance intensity for very bright colors
    float brightnessBoost = 1.0 + smoothstep(0.8, 1.0, colorIntensity) * 2.0;
    
    return colorIntensity * distanceFalloff * brightnessBoost;
}

// Energy-based color enhancement for realistic light behavior
// Simulates how bright lights affect their surrounding area
vec3 applyEnergyDistribution(vec3 baseColor, float distance, float energy)
{
    // Core energy concentration
    float coreEnergy = smoothstep(0.4, 0.0, distance) * energy * 2.0;
    
    // Mid-range energy distribution  
    float midEnergy = smoothstep(0.8, 0.2, distance) * energy * 1.0;
    
    // Outer energy halo
    float outerEnergy = smoothstep(1.2, 0.6, distance) * energy * 0.3;
    
    float totalEnergy = 1.0 + coreEnergy + midEnergy + outerEnergy;
    
    return baseColor * totalEnergy;
}

// Advanced HDR tone mapping for preview purposes
// Provides artist feedback while maintaining HDR values for post-processing
vec3 previewToneMap(vec3 hdrColor, float exposure)
{
    // Reinhard tone mapping for smooth highlights
    vec3 mapped = hdrColor / (hdrColor + vec3(1.0));
    
    // Apply exposure adjustment
    mapped *= exposure;
    
    return mapped;
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN FRAGMENT FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // BASE HDR COLOR CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Start with vertex color and prepare for HDR enhancement.
    // Base colors should be in linear space for proper HDR calculations.
    //
    vec3 baseColor = vColor.rgb;
    float baseAlpha = vColor.a;
    
    // ════════════════════════════════════════════════════════════════════════════
    // BLOOM INTENSITY CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Calculate how much this fragment should contribute to bloom effect.
    // This determines the brightness multiplier for HDR output.
    //
    float bloomIntensity = calculateBloomIntensity(vDistance, baseColor);
    
    // ════════════════════════════════════════════════════════════════════════════
    // ENERGY DISTRIBUTION MODELING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply realistic energy distribution based on distance from light center.
    // This creates natural-looking bright light sources.
    //
    float lightEnergy = 3.0; // Base energy multiplier for bloom effect
    vec3 energyEnhancedColor = applyEnergyDistribution(baseColor, vDistance, lightEnergy);
    
    // ════════════════════════════════════════════════════════════════════════════
    // HDR COLOR ENHANCEMENT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Amplify colors to HDR range for dramatic bloom effects.
    // Values exceeding 1.0 will be captured by post-processing pipeline.
    //
    float hdrEnhancement = 2.5; // Amplification factor for HDR output
    vec3 hdrColor = enhanceHDRColor(energyEnhancedColor, hdrEnhancement);
    
    // ════════════════════════════════════════════════════════════════════════════
    // BRIGHTNESS AMPLIFICATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply additional brightness boost based on bloom intensity.
    // This creates the "over-bright" effect essential for bloom.
    //
    float brightnessMultiplier = 1.0 + bloomIntensity * 4.0; // Up to 5x brightness
    hdrColor *= brightnessMultiplier;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ALPHA CHANNEL PROCESSING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Maintain alpha channel for proper blending while allowing HDR colors.
    // Alpha determines how much the bloom effect affects the final image.
    //
    float bloomAlpha = baseAlpha;
    
    // Enhance alpha based on bloom intensity for stronger glow
    bloomAlpha *= (1.0 + bloomIntensity * 0.5);
    
    // Clamp alpha to valid range while allowing HDR colors
    bloomAlpha = clamp(bloomAlpha, 0.0, 1.0);
    
    // ════════════════════════════════════════════════════════════════════════════
    // COLOR TEMPERATURE ADJUSTMENT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle color temperature shifts for more realistic light behavior.
    // Bright lights often shift toward blue-white (higher color temperature).
    //
    float colorTemperatureShift = smoothstep(1.0, 5.0, brightnessMultiplier) * 0.1;
    hdrColor.r *= (1.0 - colorTemperatureShift * 0.1); // Slight red reduction
    hdrColor.b *= (1.0 + colorTemperatureShift * 0.2); // Blue enhancement
    
    // ════════════════════════════════════════════════════════════════════════════
    // EDGE SOFTENING FOR NATURAL BLOOM
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply gentle edge softening to prevent harsh bloom boundaries.
    // This creates more natural-looking light scattering effects.
    //
    float edgeSoftening = smoothstep(1.0, 0.8, vDistance);
    hdrColor *= edgeSoftening;
    bloomAlpha *= edgeSoftening;
    
    // ════════════════════════════════════════════════════════════════════════════
    // EARLY DISCARD FOR PERFORMANCE
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Discard fragments that won't contribute meaningfully to bloom effect.
    // This improves performance by reducing post-processing workload.
    //
    if (bloomAlpha < 0.005 || getLuminance(hdrColor) < 0.1) {
        discard;
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // HDR OUTPUT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Output HDR color values for post-processing pipeline.
    // Colors can exceed 1.0 and will be processed by:
    // 1. Brightness extraction (threshold + intensity amplification)
    // 2. Gaussian blur (glow halo generation)
    // 3. Bloom composition (additive blending with main scene)
    // 4. Tone mapping (HDR to LDR conversion for display)
    //
    fragColor = vec4(hdrColor, bloomAlpha);
}
