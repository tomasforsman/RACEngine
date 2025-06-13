#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// BRIGHTNESS EXTRACTION FRAGMENT SHADER - BLOOM THRESHOLD
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Extracts bright pixels from HDR scene for bloom post-processing.
// Implements perceptual luminance thresholding with smooth transitions and
// intensity amplification for enhanced glow effects.
//
// ALGORITHM OVERVIEW:
// 1. Sample scene color from HDR framebuffer
// 2. Calculate perceptual luminance using ITU-R BT.709 coefficients
// 3. Apply threshold with smooth falloff for natural transitions
// 4. Amplify brightness for enhanced bloom contribution
//
// THRESHOLD BEHAVIOR:
// - Pixels below threshold → black (no bloom contribution)
// - Pixels above threshold → amplified color for bloom effect
// - Smooth transition zone → gradual falloff (prevents hard edges)
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates for scene texture sampling
in vec2 vTexCoord;

// ────────────────────────────────────────────────────────────────────────────────
// UNIFORM VARIABLES (FROM APPLICATION)
// ────────────────────────────────────────────────────────────────────────────────

// HDR scene texture (RGB16F format with values potentially > 1.0)
uniform sampler2D uSceneTexture;

// Brightness threshold for bloom extraction (typically 0.8-1.2)
uniform float uThreshold;

// Intensity amplification for extracted bright pixels (typically 1.0-3.0)
uniform float uIntensity;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// Extracted bright pixels (black for non-bright areas)
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// LUMINANCE AND BRIGHTNESS UTILITY FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Calculate perceptual luminance using ITU-R BT.709 standard
// These coefficients account for human eye sensitivity to different colors
float getLuminance(vec3 color)
{
    return dot(color, vec3(0.2126, 0.7152, 0.0722));
}

// Enhanced threshold function with smooth transitions
// Prevents harsh cutoffs that cause visual artifacts in bloom
float smoothThreshold(float luminance, float threshold, float softness)
{
    // Calculate distance from threshold
    float distance = luminance - threshold;
    
    // Apply smooth transition
    return smoothstep(-softness, softness, distance);
}

// Adaptive brightness enhancement based on original luminance
// Brighter pixels get more amplification for dramatic effect
float calculateBrightnessMultiplier(float luminance, float baseIntensity)
{
    // Base intensity for all bright pixels
    float intensity = baseIntensity;
    
    // Additional amplification for very bright pixels
    float extraAmplification = smoothstep(1.0, 2.0, luminance) * 0.5;
    
    return intensity + extraAmplification;
}

// Color temperature adjustment for realistic bloom behavior
// Very bright lights often shift toward blue-white spectrum
vec3 adjustColorTemperature(vec3 color, float luminance)
{
    // Calculate temperature shift based on brightness
    float temperatureShift = smoothstep(1.5, 3.0, luminance) * 0.1;
    
    // Adjust color channels
    vec3 adjusted = color;
    adjusted.r *= (1.0 - temperatureShift * 0.05); // Slight red reduction
    adjusted.b *= (1.0 + temperatureShift * 0.15); // Blue enhancement
    
    return adjusted;
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN BRIGHTNESS EXTRACTION FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // SCENE TEXTURE SAMPLING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Sample the HDR scene texture at current fragment's texture coordinate.
    // HDR textures can contain values > 1.0, representing very bright lights.
    //
    vec3 sceneColor = texture(uSceneTexture, vTexCoord).rgb;
    
    // ════════════════════════════════════════════════════════════════════════════
    // PERCEPTUAL LUMINANCE CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Calculate how bright this pixel appears to human vision.
    // Uses ITU-R BT.709 coefficients that account for eye sensitivity.
    //
    // LUMINANCE INTERPRETATION:
    // - 0.0 = black
    // - 1.0 = standard white
    // - >1.0 = over-bright (HDR values)
    //
    float luminance = getLuminance(sceneColor);
    
    // ════════════════════════════════════════════════════════════════════════════
    // THRESHOLD APPLICATION WITH SMOOTH TRANSITIONS
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply brightness threshold with smooth falloff to prevent hard edges.
    // This creates more natural-looking bloom transitions.
    //
    const float thresholdSoftness = 0.1; // Transition zone width
    float thresholdMask = smoothThreshold(luminance, uThreshold, thresholdSoftness);
    
    // Early discard for performance (skip fully black pixels)
    if (thresholdMask < 0.001) {
        fragColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // BRIGHTNESS AMPLIFICATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Amplify extracted bright pixels for enhanced bloom effect.
    // Very bright pixels get additional amplification for dramatic impact.
    //
    float brightnessMultiplier = calculateBrightnessMultiplier(luminance, uIntensity);
    vec3 amplifiedColor = sceneColor * brightnessMultiplier;
    
    // ════════════════════════════════════════════════════════════════════════════
    // COLOR TEMPERATURE ENHANCEMENT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle color temperature adjustment for realistic light behavior.
    // Very bright lights often shift toward blue-white spectrum.
    //
    amplifiedColor = adjustColorTemperature(amplifiedColor, luminance);
    
    // ════════════════════════════════════════════════════════════════════════════
    // THRESHOLD MASK APPLICATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply the threshold mask to gradually fade bright areas.
    // This creates smooth transitions at bloom boundaries.
    //
    vec3 extractedColor = amplifiedColor * thresholdMask;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ENERGY CONSERVATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Ensure total energy remains reasonable to prevent bloom from overwhelming
    // the scene. Apply subtle energy scaling based on overall brightness.
    //
    float energyScale = 1.0 / (1.0 + luminance * 0.1);
    extractedColor *= energyScale;
    
    // ════════════════════════════════════════════════════════════════════════════
    // OUTPUT BRIGHT PIXELS
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Output the extracted bright pixels for blur processing.
    // Alpha = 1.0 since this is an opaque post-processing operation.
    //
    fragColor = vec4(extractedColor, 1.0);
}
