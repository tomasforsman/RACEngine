// ═══════════════════════════════════════════════════════════════════════════════
// CAMERA MANAGER SERVICE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Central service managing dual-camera system for game world and UI rendering.
// Provides lifecycle management, coordinate transformations, and rendering
// coordination between game world camera and UI overlay camera.
//
// DUAL CAMERA ARCHITECTURE:
// - Game Camera: World-space rendering with transformations (position, zoom, rotation)
// - UI Camera: Screen-space rendering with 1:1 pixel mapping for UI elements
// - Coordinate Services: Transformation utilities for input handling and positioning
//
// RENDERING COORDINATION:
// - Multi-pass rendering: Game world first, UI overlay second
// - Matrix management: Automatic updates on viewport changes
// - Performance optimization: Cached transformations and dirty flagging
//
// INTEGRATION POINTS:
// - Window resize handling: Updates both camera projections
// - Input transformation: Screen coordinates to appropriate world space
// - Renderer integration: Provides camera matrices to shader system

using Silk.NET.Maths;

namespace Rac.Rendering.Camera;

/// <summary>
/// Service managing dual-camera system for comprehensive 2D rendering.
/// 
/// ARCHITECTURE OVERVIEW:
/// - Game Camera: Handles world-space transformations for game objects
/// - UI Camera: Provides fixed screen-space coordinates for UI elements
/// - Coordinate Services: Bidirectional transformations for input handling
/// - Lifecycle Management: Automatic updates and resource coordination
/// 
/// RENDERING PIPELINE INTEGRATION:
/// - Provides camera matrices to renderer for multi-pass rendering
/// - Handles viewport changes and matrix recalculation
/// - Optimizes performance through cached transformations
/// 
/// INPUT HANDLING SUPPORT:
/// - Mouse coordinate transformation for game world interaction
/// - UI coordinate mapping for interface element handling
/// - Automatic selection of appropriate coordinate space
/// </summary>
public interface ICameraManager
{
    /// <summary>
    /// Game world camera for rendering game objects with transformations.
    /// Supports position, zoom, and rotation for interactive world navigation.
    /// </summary>
    GameCamera GameCamera { get; }
    
    /// <summary>
    /// UI overlay camera for rendering interface elements in screen space.
    /// Provides 1:1 pixel mapping independent of game camera transformations.
    /// </summary>
    UICamera UICamera { get; }
    
    /// <summary>
    /// Updates both cameras when viewport dimensions change.
    /// Called automatically on window resize events.
    /// </summary>
    /// <param name="viewportWidth">New viewport width in pixels</param>
    /// <param name="viewportHeight">New viewport height in pixels</param>
    void UpdateViewport(int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms screen coordinates to game world coordinates.
    /// Essential for mouse interaction, click handling, and object selection.
    /// </summary>
    /// <param name="screenPosition">Screen coordinates (pixels from top-left)</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>Game world coordinates</returns>
    Vector2D<float> ScreenToGameWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms game world coordinates to screen coordinates.
    /// Useful for positioning UI elements relative to game objects.
    /// </summary>
    /// <param name="worldPosition">Game world coordinates</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>Screen coordinates (pixels from top-left)</returns>
    Vector2D<float> GameWorldToScreen(Vector2D<float> worldPosition, int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms screen coordinates to UI world coordinates.
    /// For UI element positioning and interface interaction handling.
    /// </summary>
    /// <param name="screenPosition">Screen coordinates (pixels from top-left)</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>UI world coordinates (center-origin)</returns>
    Vector2D<float> ScreenToUIWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight);
    
    /// <summary>
    /// Transforms UI world coordinates to screen coordinates.
    /// For converting UI element positions to screen pixel coordinates.
    /// </summary>
    /// <param name="uiPosition">UI world coordinates (center-origin)</param>
    /// <param name="viewportWidth">Viewport width in pixels</param>
    /// <param name="viewportHeight">Viewport height in pixels</param>
    /// <returns>Screen coordinates (pixels from top-left)</returns>
    Vector2D<float> UIWorldToScreen(Vector2D<float> uiPosition, int viewportWidth, int viewportHeight);
}

/// <summary>
/// Default implementation of camera manager providing dual-camera system.
/// 
/// PERFORMANCE CHARACTERISTICS:
/// - Lazy matrix calculation: Updates only when viewport changes or camera properties change
/// - Cached transformations: Avoids redundant matrix computations
/// - Minimal memory allocation: Reuses matrix structures
/// 
/// THREAD SAFETY:
/// - Not thread-safe: Intended for single-threaded rendering pipeline
/// - Camera updates should occur on main/render thread only
/// </summary>
public class CameraManager : ICameraManager
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DUAL CAMERA SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════
    
    public GameCamera GameCamera { get; } = new();
    public UICamera UICamera { get; } = new();
    
    private int _viewportWidth = 1;
    private int _viewportHeight = 1;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // VIEWPORT MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════
    
    public void UpdateViewport(int viewportWidth, int viewportHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0)
        {
            throw new ArgumentException("Viewport dimensions must be positive");
        }
        
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        
        // Update both cameras with new viewport dimensions
        GameCamera.UpdateMatrices(viewportWidth, viewportHeight);
        UICamera.UpdateMatrices(viewportWidth, viewportHeight);
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME WORLD COORDINATE TRANSFORMATIONS
    // ═══════════════════════════════════════════════════════════════════════════
    
    public Vector2D<float> ScreenToGameWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight)
    {
        return GameCamera.ScreenToWorld(screenPosition, viewportWidth, viewportHeight);
    }
    
    public Vector2D<float> GameWorldToScreen(Vector2D<float> worldPosition, int viewportWidth, int viewportHeight)
    {
        return GameCamera.WorldToScreen(worldPosition, viewportWidth, viewportHeight);
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // UI COORDINATE TRANSFORMATIONS
    // ═══════════════════════════════════════════════════════════════════════════
    
    public Vector2D<float> ScreenToUIWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight)
    {
        return UICamera.ScreenToWorld(screenPosition, viewportWidth, viewportHeight);
    }
    
    public Vector2D<float> UIWorldToScreen(Vector2D<float> uiPosition, int viewportWidth, int viewportHeight)
    {
        return UICamera.WorldToScreen(uiPosition, viewportWidth, viewportHeight);
    }
}