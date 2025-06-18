// ═══════════════════════════════════════════════════════════════════════════════
// CAMERA SYSTEM INTERFACE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Core abstraction for 2D camera system enabling game world transformations and
// UI overlay rendering. Provides matrix-based transformations for modern OpenGL
// rendering pipeline with view and projection matrix separation.
//
// CAMERA TYPES:
// - Game Camera: World-space transformations (position, zoom, rotation)
// - UI Camera: Screen-space mapping with 1:1 pixel correspondence
//
// COORDINATE SYSTEMS:
// - World Space: Game objects exist in arbitrary coordinate system
// - View Space: World transformed by camera view matrix
// - Clip Space: View transformed by projection matrix (-1 to +1 NDC)
// - Screen Space: Pixel coordinates for UI elements
//
// MATHEMATICAL FOUNDATION:
// - View Matrix: Inverse of camera transform (position, rotation, scale)
// - Projection Matrix: Orthographic projection for 2D rendering
// - Combined Matrix: Projection * View for vertex shader transformation

using Silk.NET.Maths;

namespace Rac.Rendering.Camera;

/// <summary>
/// Core camera interface providing matrix-based transformations for 2D rendering.
/// 
/// DESIGN PRINCIPLES:
/// - Matrix-based transformations for GPU efficiency
/// - Separation of view and projection concerns
/// - Support for both world-space and screen-space rendering
/// - Coordinate transformation utilities for input handling
/// 
/// USAGE PATTERNS:
/// - Game cameras manipulate view matrix (position, zoom, rotation)
/// - UI cameras use identity view with screen-space projection
/// - Combined matrices passed as uniforms to vertex shaders
/// </summary>
public interface ICamera
{
    /// <summary>
    /// View matrix transforming world coordinates to camera-relative coordinates.
    /// For game cameras: includes position, rotation, and zoom transformations.
    /// For UI cameras: typically identity matrix for direct screen mapping.
    /// </summary>
    Matrix4X4<float> ViewMatrix { get; }
    
    /// <summary>
    /// Projection matrix transforming camera coordinates to normalized device coordinates.
    /// For 2D rendering: orthographic projection maintaining aspect ratio.
    /// Range: Maps world/screen space to [-1, +1] NDC for OpenGL rendering.
    /// </summary>
    Matrix4X4<float> ProjectionMatrix { get; }
    
    /// <summary>
    /// Combined transformation matrix (Projection * View) for vertex shader efficiency.
    /// Eliminates per-vertex matrix multiplication in GPU, improving performance.
    /// Updated automatically when view or projection matrices change.
    /// </summary>
    Matrix4X4<float> CombinedMatrix { get; }
    
    /// <summary>
    /// Updates camera matrices based on viewport dimensions and camera parameters.
    /// Called on window resize and camera property changes.
    /// </summary>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    void UpdateMatrices(int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms world coordinates to screen pixel coordinates.
    /// Essential for mouse input handling and UI positioning relative to world objects.
    /// </summary>
    /// <param name="worldPosition">Position in world coordinate system</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>Screen coordinates in pixel space (origin at top-left)</returns>
    Vector2D<float> WorldToScreen(Vector2D<float> worldPosition, int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms screen pixel coordinates to world coordinates.
    /// Critical for mouse picking, click handling, and screen-to-world interaction.
    /// </summary>
    /// <param name="screenPosition">Position in screen coordinate system (pixels from top-left)</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>World coordinates transformed by inverse camera matrices</returns>
    Vector2D<float> ScreenToWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight);
}