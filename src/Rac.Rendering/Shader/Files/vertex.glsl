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
    // HYBRID CAMERA TRANSFORMATION PIPELINE WITH BACKWARD COMPATIBILITY
    // -------------------------------------------------------------------------------------
    // Provides both modern camera system and legacy aspect ratio compatibility:
    // 1. Modern camera matrix transformation for full 2D camera capabilities
    // 2. Legacy aspect ratio fallback for existing bloom and shader effects
    // 3. Automatic detection ensures seamless compatibility
    
    vec4 worldPosition = vec4(aPosition, 0.0, 1.0);
    
    // Check if camera matrix is identity (backward compatibility mode)
    // Identity matrix has 1.0 on diagonal and 0.0 elsewhere
    bool isIdentityMatrix = (
        uCameraMatrix[0][0] == 1.0 && uCameraMatrix[1][1] == 1.0 && 
        uCameraMatrix[2][2] == 1.0 && uCameraMatrix[3][3] == 1.0 &&
        uCameraMatrix[0][1] == 0.0 && uCameraMatrix[0][2] == 0.0 && uCameraMatrix[0][3] == 0.0 &&
        uCameraMatrix[1][0] == 0.0 && uCameraMatrix[1][2] == 0.0 && uCameraMatrix[1][3] == 0.0 &&
        uCameraMatrix[2][0] == 0.0 && uCameraMatrix[2][1] == 0.0 && uCameraMatrix[2][3] == 0.0 &&
        uCameraMatrix[3][0] == 0.0 && uCameraMatrix[3][1] == 0.0 && uCameraMatrix[3][2] == 0.0
    );
    
    if (isIdentityMatrix) {
        // -------------------------------------------------------------------------------------
        // LEGACY COMPATIBILITY MODE (pre-camera system)
        // -------------------------------------------------------------------------------------
        // Use original aspect ratio correction for backward compatibility with bloom effects
        vec2 correctedPosition = aPosition;
        correctedPosition.x *= uAspect;
        gl_Position = vec4(correctedPosition, 0.0, 1.0);
    } else {
        // -------------------------------------------------------------------------------------
        // MODERN CAMERA SYSTEM MODE
        // -------------------------------------------------------------------------------------
        // Apply full camera transformation matrix for 2D camera capabilities
        gl_Position = uCameraMatrix * worldPosition;
    }
    
    // -------------------------------------------------------------------------------------
    // VERTEX ATTRIBUTE PASS-THROUGH
    // -------------------------------------------------------------------------------------
    // Forward vertex attributes to fragment shader unchanged
    
    vTexCoord = aTexCoord;
    vColor = aColor;
    vDistance = length(aTexCoord);
}