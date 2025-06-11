#version 330 core

// ════════════════════════════════════════════════════════════════════════════════
// UNIVERSAL VERTEX SHADER FOR RACENGINE
// ════════════════════════════════════════════════════════════════════════════════
//
// PURPOSE: Transform vertices from model space to screen space with aspect ratio 
// correction while supporting multiple vertex formats (BasicVertex, TexturedVertex, 
// FullVertex) through optional attributes.
//
// DESIGN PRINCIPLES:
// - Single shader handles all vertex types through optional attributes
// - Aspect ratio correction for consistent 2D rendering across screen sizes
// - Efficient attribute passthrough for fragment shader variety
// - GLSL 330 core for broad compatibility
//
// COORDINATE SYSTEM:
// - Input: Model space coordinates (typically -1 to 1 range)
// - Output: Clip space coordinates with aspect ratio applied
// - Y-axis: Standard OpenGL convention (up is positive)
//
// ════════════════════════════════════════════════════════════════════════════════

// ────────────────────────────────────────────────────────────────────────────────
// VERTEX ATTRIBUTES
// ────────────────────────────────────────────────────────────────────────────────

// Required: Position attribute (present in all vertex types)
layout(location = 0) in vec2 aPosition;

// Optional: Texture coordinates (TexturedVertex, FullVertex)
layout(location = 1) in vec2 aTexCoord;

// Optional: Per-vertex color (FullVertex only)
layout(location = 2) in vec4 aColor;

// ────────────────────────────────────────────────────────────────────────────────
// UNIFORMS
// ────────────────────────────────────────────────────────────────────────────────

// Aspect ratio correction for consistent 2D rendering
uniform float uAspect;

// Global color tint (used when per-vertex colors are not available)
uniform vec4 uColor;

// ────────────────────────────────────────────────────────────────────────────────
// OUTPUT VARIABLES (TO FRAGMENT SHADER)
// ────────────────────────────────────────────────────────────────────────────────

// Interpolated texture coordinates for gradient/distance calculations
out vec2 vTexCoord;

// Final vertex color (either per-vertex or global)
out vec4 vColor;

// Distance from center (for radial effects like soft glow)
out float vDistance;

// ────────────────────────────────────────────────────────────────────────────────
// MAIN TRANSFORMATION FUNCTION
// ────────────────────────────────────────────────────────────────────────────────

void main()
{
    // ════════════════════════════════════════════════════════════════════════════
    // POSITION TRANSFORMATION WITH ASPECT RATIO CORRECTION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Apply aspect ratio correction to prevent stretching on non-square viewports.
    // This ensures circles remain circular and squares remain square regardless
    // of window dimensions.
    //
    // ASPECT RATIO CALCULATION:
    // - aspect = viewport_height / viewport_width  
    // - Values > 1.0 indicate tall viewports (portrait)
    // - Values < 1.0 indicate wide viewports (landscape)
    //
    vec2 correctedPosition = aPosition;
    correctedPosition.x *= uAspect;
    
    // Transform to clip space coordinates
    gl_Position = vec4(correctedPosition, 0.0, 1.0);
    
    // ════════════════════════════════════════════════════════════════════════════
    // TEXTURE COORDINATE HANDLING
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Pass through texture coordinates for fragment shader use.
    // These are used for:
    // - Distance calculations in soft glow effects
    // - UV mapping for textured rendering
    // - Gradient generation in visual effects
    //
    vTexCoord = aTexCoord;
    
    // ════════════════════════════════════════════════════════════════════════════
    // COLOR ATTRIBUTE RESOLUTION
    // ════════════════════════════════════════════════════════════════════════════
    //
    // PRIORITY SYSTEM:
    // 1. Per-vertex colors (FullVertex) - highest priority
    // 2. Global uniform color (BasicVertex, TexturedVertex) - fallback
    //
    // This allows mixing of colored and non-colored geometry in the same
    // rendering pipeline while maintaining efficiency.
    //
    
    // Check if per-vertex color is provided (FullVertex format)
    // Note: In OpenGL, attributes not provided default to (0,0,0,1) for vec4
    // We detect this by checking if the alpha component suggests a valid color
    if (aColor.a > 0.0) {
        vColor = aColor;
    } else {
        vColor = uColor;
    }
    
    // ════════════════════════════════════════════════════════════════════════════
    // DISTANCE CALCULATION FOR RADIAL EFFECTS
    // ════════════════════════════════════════════════════════════════════════════
    //
    // Calculate distance from center for soft glow and particle effects.
    // Uses texture coordinates as they represent the local coordinate system
    // of the primitive being rendered.
    //
    // DISTANCE INTERPRETATION:
    // - 0.0 = center of primitive
    // - 1.0 = edge of unit circle
    // - >1.0 = outside unit circle (for soft edges)
    //
    vDistance = length(aTexCoord);
}