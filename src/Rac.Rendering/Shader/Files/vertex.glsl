#version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;

// -------------------------------------------------------------------------------------
// CAMERA SYSTEM UNIFORMS
// -------------------------------------------------------------------------------------
// Camera matrices for modern 2D transformation pipeline:
// - uCameraMatrix: Combined view-projection matrix for efficient vertex transformation
// - uAspect: Legacy aspect ratio correction for backward compatibility
// - uColor: Global color modifier for rendering effects

uniform mat4 uCameraMatrix;  // Combined camera transformation matrix
uniform float uAspect;       // Legacy aspect ratio (maintained for compatibility)
uniform vec4 uColor;         // Global color modifier

out vec2 vTexCoord;
out vec4 vColor;
out float vDistance;

void main()
{
    // -------------------------------------------------------------------------------------
    // MODERN CAMERA TRANSFORMATION PIPELINE
    // -------------------------------------------------------------------------------------
    // Transform vertex position through camera matrix system:
    // 1. World coordinates -> Camera view space (position, rotation, zoom)
    // 2. Camera space -> Clip space (orthographic projection)
    // 3. Result: Proper 2D camera transformations with zoom, pan, rotate support
    
    vec4 worldPosition = vec4(aPosition, 0.0, 1.0);
    gl_Position = uCameraMatrix * worldPosition;
    
    // -------------------------------------------------------------------------------------
    // VERTEX ATTRIBUTE PASS-THROUGH
    // -------------------------------------------------------------------------------------
    // Forward vertex attributes to fragment shader unchanged
    
    vTexCoord = aTexCoord;
    vColor = aColor;
    vDistance = length(aTexCoord);
}