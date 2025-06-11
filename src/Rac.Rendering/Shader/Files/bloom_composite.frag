#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// BLOOM COMPOSITE FRAGMENT SHADER - FINAL HDR COMPOSITION
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Final stage of bloom post-processing pipeline. Combines original HDR
// scene with blurred bloom contribution and applies tone mapping for display.
// Implements sophisticated HDR to LDR conversion with exposure controls.
//
// COMPOSITION ALGORITHM:
// 1. Sample original scene and blurred bloom textures
// 2. Blend using additive/screen blending with artistic controls
// 3. Apply HDR tone mapping for display-ready output
// 4. Perform gamma correction for proper color space
//
// TONE MAPPING OPTIONS:
// - Reinhard: Simple and fast, good for general use
// - ACES: Film industry standard, cinematic look
// - Linear: Simple exposure scaling
// - Uncharted 2: Popular in gaming, good contrast
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates for texture sampling
in vec2 vTexCoord;

// ────────────────────────────────────────────────────────────────────────────────
// UNIFORM VARIABLES (FROM APPLICATION)
// ────────────────────────────────────────────────────────────────────────────────

// Original HDR scene texture (before bloom extraction)
uniform sampler2D uSceneTexture;

// Blurred bloom texture (from Gaussian blur passes)
uniform sampler2D uBloomTexture;

// Bloom contribution strength (0.0-2.0+ typical range)
uniform float uBloomStrength;

// HDR exposure compensation (0.1-3.0+ typical range)
uniform float uExposure;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// Final tone-mapped LDR result ready for display
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// TONE MAPPING OPERATORS
// ────────────────────────────────────────────────────────────────────────────────

// Reinhard tone mapping - simple and effective
// Maps infinite range to [0,1] with natural rolloff
vec3 reinhardToneMapping(vec3 hdrColor, float exposure)
{
    // Apply exposure
    vec3 exposed = hdrColor * exposure;
    
    // Reinhard formula: x / (1 + x)
    return exposed / (1.0 + exposed);
}

// ACES (Academy Color Encoding System) tone mapping
// Industry standard for film, provides cinematic look
vec3 acesToneMapping(vec3 hdrColor, float exposure)
{
    // Apply exposure
    vec3 exposed = hdrColor * exposure;
    
    // ACES constants
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    
    // ACES formula
    return clamp((exposed * (a * exposed + b)) / (exposed * (c * exposed + d) + e), 0.0, 1.0);
}

// Uncharted 2 tone mapping - popular in gaming
// Good contrast and color preservation
vec3 uncharted2ToneMapping(vec3 hdrColor, float exposure)
{
    // Uncharted 2 curve function
    auto tonemap = [](vec3 x) -> vec3 {
        const float A = 0.15; // Shoulder strength
        const float B = 0.50; // Linear strength  
        const float C = 0.10; // Linear angle
        const float D = 0.20; // Toe strength
        const float E = 0.02; // Toe numerator
        const float F = 0.30; // Toe denominator
        
        return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
    };
    
    vec3 exposed = hdrColor * exposure;
    vec3 mapped = tonemap(exposed);
    
    // White point normalization
    const vec3 whitePoint = vec3(11.2);
    vec3 whiteScale = vec3(1.0) / tonemap(whitePoint);
    
    return mapped * whiteScale;
}

// Linear tone mapping - simple exposure scaling with clamp
vec3 linearToneMapping(vec3 hdrColor, float exposure)
{
    return clamp(hdrColor * exposure, 0.0, 1.0);
}

// ────────────────────────────────────────────────────────────────────────────────
// COLOR SPACE AND GAMMA FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Convert from linear RGB to sRGB color space (gamma correction)
vec3 linearToSRGB(vec3 linear)
{
    return mix(
        linear * 12.92,
        pow(linear, vec3(1.0 / 2.4)) * 1.055 - 0.055,
        step(0.0031308, linear)
    );
}

// Simplified gamma correction (approximation)
vec3 gammaCorrect(vec3 color, float gamma)
{
    return pow(color, vec3(1.0 / gamma));
}

// ────────────────────────────────────────────────────────────────────────────────
// BLOOM BLENDING FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Additive blending - simple and effective for most bloom
vec3 additiveBlend(vec3 scene, vec3 bloom, float strength)
{
    return scene + (bloom * strength);
}

// Screen blending - prevents over-brightening
vec3 screenBlend(vec3 scene, vec3 bloom, float strength)
{
    vec3 bloomContrib = bloom * strength;
    return scene + bloomContrib - (scene * bloomContrib);
}

// Soft light blending - more subtle bloom integration
vec3 softLightBlend(vec3 scene, vec3 bloom, float strength)
{
    vec3 bloomContrib = bloom * strength;
    return mix(scene, 
               mix(2.0 * scene * bloomContrib, 
                   1.0 - 2.0 * (1.0 - scene) * (1.0 - bloomContrib), 
                   step(0.5, bloomContrib)), 
               strength);
}

// ────────────────────────────────────────────────────────────────────────────────
// ARTISTIC ENHANCEMENT FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Color grading adjustment for cinematic look
vec3 colorGrade(vec3 color)
{
    // Subtle color adjustments for cinematic look
    color.r *= 1.02; // Slight red enhancement
    color.g *= 0.98; // Slight green reduction  
    color.b *= 1.01; // Slight blue enhancement
    
    return color;
}

// Contrast enhancement
vec3 enhanceContrast(vec3 color, float contrast)
{
    return ((color - 0.5) * contrast) + 0.5;
}

// Saturation adjustment
vec3 adjustSaturation(vec3 color, float saturation)
{
    float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
    return mix(vec3(luminance), color, saturation);
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN COMPOSITION FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // TEXTURE SAMPLING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Sample both the original HDR scene and the blurred bloom contribution.
    // These will be combined using various blending techniques.
    //
    vec3 sceneColor = texture(uSceneTexture, vTexCoord).rgb;
    vec3 bloomColor = texture(uBloomTexture, vTexCoord).rgb;
    
    // ════════════════════════════════════════════════════════════════════════════
    // BLOOM BLENDING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Combine scene and bloom using screen blending for natural light accumulation.
    // Screen blending prevents over-brightening while maintaining bloom effect.
    //
    vec3 blendedColor = screenBlend(sceneColor, bloomColor, uBloomStrength);
    
    // Optional: Add subtle additive component for extra glow
    blendedColor += bloomColor * uBloomStrength * 0.1;
    
    // ════════════════════════════════════════════════════════════════════════════
    // HDR TO LDR TONE MAPPING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Convert HDR values (potentially > 1.0) to display-ready LDR range [0,1].
    // Using ACES tone mapping for cinematic quality.
    //
    vec3 toneMappedColor = acesToneMapping(blendedColor, uExposure);
    
    // ════════════════════════════════════════════════════════════════════════════
    // ARTISTIC ENHANCEMENTS
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle artistic adjustments for enhanced visual appeal.
    // These adjustments are kept minimal to maintain natural appearance.
    //
    toneMappedColor = colorGrade(toneMappedColor);
    toneMappedColor = enhanceContrast(toneMappedColor, 1.05); // 5% contrast boost
    toneMappedColor = adjustSaturation(toneMappedColor, 1.02); // 2% saturation boost
    
    // ════════════════════════════════════════════════════════════════════════════
    // GAMMA CORRECTION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Convert from linear color space to sRGB for proper display.
    // This ensures colors appear correctly on standard monitors.
    //
    vec3 finalColor = linearToSRGB(toneMappedColor);
    
    // ════════════════════════════════════════════════════════════════════════════
    // FINAL CLAMPING AND OUTPUT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Ensure output is in valid range and handle any edge cases.
    // Alpha = 1.0 since this is the final opaque result.
    //
    finalColor = clamp(finalColor, 0.0, 1.0);
    
    fragColor = vec4(finalColor, 1.0);
}