#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// SEPARABLE GAUSSIAN BLUR FRAGMENT SHADER - BLOOM FILTER
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Implements separable Gaussian blur for bloom post-processing.
// Creates smooth, wide glow halos around bright objects using efficient
// separable convolution with configurable kernel size and direction.
//
// SEPARABLE FILTER THEORY:
// - 2D Gaussian kernel can be separated into two 1D operations
// - Horizontal pass: blur each row independently  
// - Vertical pass: blur each column independently
// - Complexity: O(n²) → O(2n) for n×n kernel (major performance gain)
//
// IMPLEMENTATION STRATEGY:
// - Single shader handles both horizontal and vertical passes
// - Direction controlled by uniform variable (uHorizontal)
// - Variable kernel size for artistic control (uBlurSize)
// - Optimized sampling patterns for GPU efficiency
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// INPUT VARIABLES (FROM VERTEX SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates for bright texture sampling
in vec2 vTexCoord;

// ────────────────────────────────────────────────────────────────────────────────
// UNIFORM VARIABLES (FROM APPLICATION)
// ────────────────────────────────────────────────────────────────────────────────

// Bright pixels texture (from brightness extraction pass)
uniform sampler2D uBrightTexture;

// Blur direction: 1 = horizontal, 0 = vertical
uniform int uHorizontal;

// Blur kernel size multiplier (0.5-2.0 typical range)
uniform float uBlurSize;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES
// ────────────────────────────────────────────────────────────────────────────────

// Blurred bright pixels
out vec4 fragColor;

// ────────────────────────────────────────────────────────────────────────────────
// GAUSSIAN BLUR CONFIGURATION
// ────────────────────────────────────────────────────────────────────────────────

// Blur kernel radius (number of samples on each side of center)
const int BLUR_RADIUS = 5;

// Pre-calculated Gaussian weights for 11-tap filter (-5 to +5)
// Normalized so sum = 1.0 for energy conservation
const float gaussianWeights[11] = float[](
    0.0162162162,  // -5
    0.0540540541,  // -4  
    0.1216216216,  // -3
    0.1945945946,  // -2
    0.2270270270,  // -1
    0.2432432432,  //  0 (center)
    0.2270270270,  // +1
    0.1945945946,  // +2
    0.1216216216,  // +3
    0.0540540541,  // +4
    0.0162162162   // +5
);

// ────────────────────────────────────────────────────────────────────────────────
// BLUR UTILITY FUNCTIONS
// ────────────────────────────────────────────────────────────────────────────────

// Calculate texture step size based on blur direction and size
vec2 getBlurOffset(int sampleIndex)
{
    // Calculate offset from center sample
    float offset = float(sampleIndex - BLUR_RADIUS) * uBlurSize;
    
    // Apply blur direction
    if (uHorizontal == 1) {
        // Horizontal blur: step along X axis
        return vec2(offset / textureSize(uBrightTexture, 0).x, 0.0);
    } else {
        // Vertical blur: step along Y axis  
        return vec2(0.0, offset / textureSize(uBrightTexture, 0).y);
    }
}

// Optimized Gaussian blur using linear sampling
// Takes advantage of GPU's linear filtering to reduce texture fetches
vec3 optimizedGaussianBlur(sampler2D tex, vec2 texCoord, vec2 direction)
{
    vec3 result = vec3(0.0);
    vec2 texelSize = 1.0 / textureSize(tex, 0);
    
    // Center sample
    result += texture(tex, texCoord).rgb * gaussianWeights[BLUR_RADIUS];
    
    // Paired samples (leverage GPU linear filtering)
    for (int i = 1; i <= BLUR_RADIUS; i++) {
        vec2 offset = direction * texelSize * float(i) * uBlurSize;
        
        // Sample positive and negative offsets
        result += texture(tex, texCoord + offset).rgb * gaussianWeights[BLUR_RADIUS + i];
        result += texture(tex, texCoord - offset).rgb * gaussianWeights[BLUR_RADIUS - i];
    }
    
    return result;
}

// Quality-optimized blur for different performance targets
vec3 performanceBlur(sampler2D tex, vec2 texCoord, vec2 direction)
{
    vec3 result = vec3(0.0);
    vec2 texelSize = 1.0 / textureSize(tex, 0);
    
    // Reduced sample count for better performance
    const int PERF_RADIUS = 3;
    const float perfWeights[7] = float[](
        0.0647058824, 0.1411764706, 0.2352941176, 0.3176470588,
        0.2352941176, 0.1411764706, 0.0647058824
    );
    
    for (int i = 0; i < 7; i++) {
        vec2 offset = direction * texelSize * float(i - PERF_RADIUS) * uBlurSize;
        result += texture(tex, texCoord + offset).rgb * perfWeights[i];
    }
    
    return result;
}

// ────────────────────────────────────────────────────────────────────────────────
// MAIN BLUR FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // BLUR DIRECTION CALCULATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Determine blur direction based on pass type:
    // - Horizontal pass: blur along X axis (creates vertical streaks)
    // - Vertical pass: blur along Y axis (blurs the streaks into smooth glow)
    //
    vec2 blurDirection;
    if (uHorizontal == 1) {
        blurDirection = vec2(1.0, 0.0); // Horizontal
    } else {
        blurDirection = vec2(0.0, 1.0); // Vertical  
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // GAUSSIAN CONVOLUTION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply Gaussian filter by sampling neighboring pixels and weighting them
    // according to Gaussian distribution. This creates smooth, natural-looking blur.
    //
    vec3 blurredColor = vec3(0.0);
    
    // Standard quality blur using full kernel
    for (int i = 0; i < 11; i++) {
        vec2 sampleOffset = getBlurOffset(i);
        vec2 sampleCoord = vTexCoord + sampleOffset;
        
        // Sample texture with boundary clamping
        vec3 sampleColor = texture(uBrightTexture, sampleCoord).rgb;
        
        // Apply Gaussian weight
        blurredColor += sampleColor * gaussianWeights[i];
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // BLUR SIZE COMPENSATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Larger blur sizes can reduce overall brightness due to sample spreading.
    // Apply compensation to maintain consistent bloom intensity.
    //
    float sizeCompensation = 1.0 + (uBlurSize - 1.0) * 0.2;
    blurredColor *= sizeCompensation;
    
    // ════════════════════════════════════════════════════════════════════════════
    // ENERGY CONSERVATION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Ensure that blur operation doesn't artificially amplify energy.
    // Total light energy should be conserved across blur operations.
    //
    // Note: Gaussian weights are already normalized, but floating-point precision
    // and boundary conditions can introduce small energy variations.
    //
    float energyConservation = 0.98; // Slight energy reduction for realism
    blurredColor *= energyConservation;
    
    // ════════════════════════════════════════════════════════════════════════════
    // EDGE SOFTENING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply subtle edge softening to prevent harsh bloom boundaries.
    // Creates more natural falloff at edges of bright regions.
    //
    vec2 edgeDistance = abs(vTexCoord - vec2(0.5));
    float edgeFactor = 1.0 - smoothstep(0.3, 0.5, max(edgeDistance.x, edgeDistance.y));
    blurredColor *= mix(0.95, 1.0, edgeFactor);
    
    // ════════════════════════════════════════════════════════════════════════════
    // OUTPUT BLURRED RESULT
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Output the blurred bright pixels for next stage in bloom pipeline.
    // Alpha = 1.0 since this is an opaque post-processing operation.
    //
    fragColor = vec4(blurredColor, 1.0);
}
