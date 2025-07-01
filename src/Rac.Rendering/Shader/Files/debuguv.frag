#version 330 core

// ===============================================================================
// UV COORDINATE DEBUGGING FRAGMENT SHADER
// ===============================================================================
//
// EDUCATIONAL PURPOSE:
// This shader provides visual debugging capabilities for texture coordinate mapping,
// enabling developers to quickly identify UV mapping issues in rendered geometry.
//
// VISUALIZATION STRATEGY:
// - Convert centered coordinates (+/-0.5) to [0,1] range for proper color mapping
// - U coordinate (horizontal) -> Red channel intensity
// - V coordinate (vertical) -> Green channel intensity  
// - Blue channel -> Constant low value for contrast
// - Alpha channel -> Full opacity for clear visibility
//
// COORDINATE INTERPRETATION:
// - Black (0,0) -> Bottom-left corner (original vTexCoord = -0.5, -0.5)
// - Red (1,0) -> Bottom-right corner (original vTexCoord = +0.5, -0.5)
// - Green (0,1) -> Top-left corner (original vTexCoord = -0.5, +0.5)
// - Yellow (1,1) -> Top-right corner (original vTexCoord = +0.5, +0.5)
//
// GRAPHICS THEORY REFERENCE:
// UV coordinates represent 2D parametric surface mapping, commonly used in:
// - Texture atlas mapping
// - Procedural pattern generation
// - Surface parameterization for complex geometry
// - Real-time shader effects and animations
//
// ===============================================================================

in vec2 vTexCoord;
in vec4 vColor;
in float vDistance;

out vec4 fragColor;

void main()
{
    // ---------------------------------------------------------------------------
    // COORDINATE SYSTEM CONVERSION FOR VISUALIZATION
    // ---------------------------------------------------------------------------
    //
    // RACEngine uses centered coordinates around (0,0) for procedural effects,
    // but UV debugging requires [0,1] range for proper color visualization.
    // Convert centered coordinates to [0,1] range for debugging display.
    //
    // Mathematical transformation:
    // Input: vTexCoord in [-0.5, 0.5] (centered around origin)
    // Output: debugUV in [0,1] (standard UV visualization range)
    // Formula: debugUV = vTexCoord + 0.5
    
    vec2 debugUV = vTexCoord + vec2(0.5, 0.5);
    
    // ---------------------------------------------------------------------------
    // UV COORDINATE TO COLOR MAPPING
    // ---------------------------------------------------------------------------
    //
    // Map converted UV coordinates to RGB color channels for immediate
    // visual feedback about UV coordinate distribution across the geometry.
    //
    // Mathematical mapping:
    // U in [0,1] -> Red channel in [0,1]
    // V in [0,1] -> Green channel in [0,1]
    // Blue = 0.2 (constant for contrast)
    // Alpha = 1.0 (full opacity)
    //
    // VISUALIZATION INTERPRETATION:
    // - (0,0) Black: Bottom-left corner of geometry (original vTexCoord = -0.5, -0.5)
    // - (1,0) Red: Bottom-right corner of geometry (original vTexCoord = +0.5, -0.5)
    // - (0,1) Green: Top-left corner of geometry (original vTexCoord = -0.5, +0.5)
    // - (1,1) Yellow: Top-right corner of geometry (original vTexCoord = +0.5, +0.5)
    
    vec3 debugColor = vec3(
        debugUV.x,          // U coordinate -> Red channel
        debugUV.y,          // V coordinate -> Green channel
        0.2                 // Low blue for visual contrast
    );
    
    // ---------------------------------------------------------------------------
    // COLOR SPACE CONSIDERATIONS
    // ---------------------------------------------------------------------------
    //
    // Output in linear color space for accurate debugging visualization.
    // Post-processing pipeline will handle gamma correction if enabled.
    
    fragColor = vec4(debugColor, 1.0);
}
