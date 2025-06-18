// ═══════════════════════════════════════════════════════════════════════════════
// UI CAMERA IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// Screen-space camera for UI element rendering with 1:1 pixel mapping.
// Provides direct pixel coordinate mapping without world transformations,
// ensuring UI elements remain fixed in screen space regardless of game camera.
//
// COORDINATE MAPPING:
// - Screen coordinates map directly to world coordinates
// - Origin at center of screen (0,0)
// - X-axis: -width/2 to +width/2
// - Y-axis: -height/2 to +height/2 (Y-up convention)
//
// USE CASES:
// - HUD elements (health bars, minimap, menus)
// - Debug overlays and development tools
// - UI panels and dialog boxes
// - Text rendering and notifications
//
// MATHEMATICAL FOUNDATION:
// - Identity view matrix (no world transformation)
// - Orthographic projection mapping pixel space to NDC
// - 1:1 correspondence between screen pixels and world units

using Silk.NET.Maths;

namespace Rac.Rendering.Camera;

/// <summary>
/// Screen-space camera providing 1:1 pixel mapping for UI element rendering.
/// 
/// DESIGN PRINCIPLES:
/// - Direct pixel-to-world coordinate mapping
/// - No camera transformations (identity view matrix)
/// - Consistent UI positioning regardless of game camera state
/// - Simplified coordinate system for UI development
/// 
/// COORDINATE SYSTEM:
/// - Origin at screen center (0, 0)
/// - Positive X extends right, positive Y extends up
/// - Screen bounds: [-width/2, +width/2] × [-height/2, +height/2]
/// - Direct pixel correspondence: 1 world unit = 1 screen pixel
/// 
/// USAGE PATTERNS:
/// - HUD rendering: Position elements relative to screen edges
/// - UI overlays: Absolute positioning independent of game world
/// - Text rendering: Pixel-perfect text placement and sizing
/// </summary>
public class UICamera : ICamera
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CACHED TRANSFORMATION MATRICES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private Matrix4X4<float> _viewMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _projectionMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _combinedMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _inverseProjectionMatrix = Matrix4X4<float>.Identity;
    
    private bool _matricesDirty = true;
    private int _viewportWidth = 1;
    private int _viewportHeight = 1;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // MATRIX PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Identity view matrix - no world transformations applied.
    /// UI elements render directly in screen space without camera influence.
    /// </summary>
    public Matrix4X4<float> ViewMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _viewMatrix;
        }
    }
    
    /// <summary>
    /// Orthographic projection mapping screen pixel space to NDC.
    /// Maps viewport dimensions directly to [-1, +1] normalized device coordinates.
    /// </summary>
    public Matrix4X4<float> ProjectionMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _projectionMatrix;
        }
    }
    
    /// <summary>
    /// Combined transformation matrix (Projection * View).
    /// Since view is identity, this equals the projection matrix.
    /// </summary>
    public Matrix4X4<float> CombinedMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _combinedMatrix;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // MATRIX COMPUTATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    public void UpdateMatrices(int viewportWidth, int viewportHeight)
    {
        if (_viewportWidth != viewportWidth || _viewportHeight != viewportHeight)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _matricesDirty = true;
        }
        
        if (_matricesDirty) UpdateMatricesInternal();
    }
    
    private void UpdateMatricesInternal()
    {
        // ───────────────────────────────────────────────────────────────────────
        // VIEW MATRIX: Identity (no transformations)
        // ───────────────────────────────────────────────────────────────────────
        
        _viewMatrix = Matrix4X4<float>.Identity;
        
        // ───────────────────────────────────────────────────────────────────────
        // PROJECTION MATRIX: Orthographic mapping screen space to NDC
        // ───────────────────────────────────────────────────────────────────────
        
        // Map screen pixel coordinates to world coordinates
        // Screen center (width/2, height/2) maps to world origin (0, 0)
        float left = -_viewportWidth * 0.5f;
        float right = _viewportWidth * 0.5f;
        float bottom = -_viewportHeight * 0.5f;
        float top = _viewportHeight * 0.5f;
        
        _projectionMatrix = Matrix4X4.CreateOrthographicOffCenter(left, right, bottom, top, -1f, 1f);
        Matrix4X4.Invert(_projectionMatrix, out _inverseProjectionMatrix);
        
        // ───────────────────────────────────────────────────────────────────────
        // COMBINED MATRIX: Projection * View (equals projection since view is identity)
        // ───────────────────────────────────────────────────────────────────────
        
        _combinedMatrix = _viewMatrix * _projectionMatrix;
        _matricesDirty = false;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // COORDINATE TRANSFORMATION UTILITIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Transforms world coordinates to screen pixel coordinates.
    /// For UI camera: world coordinates are already in screen space.
    /// </summary>
    /// <param name="worldPosition">Position in UI world coordinate system</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>Screen coordinates in pixel space</returns>
    public Vector2D<float> WorldToScreen(Vector2D<float> worldPosition, int viewportWidth, int viewportHeight)
    {
        // UI world coordinates map directly to screen space
        // Convert from center-origin to top-left origin screen coordinates
        var screenX = worldPosition.X + viewportWidth * 0.5f;
        var screenY = -worldPosition.Y + viewportHeight * 0.5f; // Flip Y-axis
        
        return new Vector2D<float>(screenX, screenY);
    }
    
    /// <summary>
    /// Transforms screen pixel coordinates to world coordinates.
    /// For UI camera: screen coordinates map directly to world space.
    /// </summary>
    /// <param name="screenPosition">Position in screen coordinate system</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>UI world coordinates</returns>
    public Vector2D<float> ScreenToWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight)
    {
        // Convert from top-left origin screen coordinates to center-origin world coordinates
        var worldX = screenPosition.X - viewportWidth * 0.5f;
        var worldY = -(screenPosition.Y - viewportHeight * 0.5f); // Flip Y-axis
        
        return new Vector2D<float>(worldX, worldY);
    }
}