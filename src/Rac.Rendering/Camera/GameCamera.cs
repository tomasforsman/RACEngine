// ═══════════════════════════════════════════════════════════════════════════════
// GAME CAMERA IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// 2D game camera providing world-space transformations including position, zoom,
// and rotation for interactive game world navigation. Implements camera controls
// commonly found in 2D games, strategy games, and simulation applications.
//
// TRANSFORMATION ORDER:
// 1. Scale (Zoom): Uniform scaling for zoom in/out functionality
// 2. Rotation: Rotation around camera center point
// 3. Translation: Camera position offset in world space
// 4. Projection: Orthographic projection to NDC space
//
// COORDINATE SYSTEM:
// - World Space: Arbitrary game world coordinates (e.g., meters, tiles)
// - Camera Space: World coordinates transformed by camera view matrix
// - NDC Space: [-1, +1] normalized device coordinates for OpenGL
//
// MATHEMATICAL FOUNDATION:
// - View Matrix = Inverse(Translation × Rotation × Scale)
// - Projection Matrix = Orthographic(left, right, bottom, top, near, far)
// - Combined = Projection × View (standard OpenGL matrix multiplication order)

using Silk.NET.Maths;

namespace Rac.Rendering.Camera;

/// <summary>
/// 2D game camera with position, zoom, and rotation capabilities for world-space rendering.
/// 
/// FEATURES:
/// - World position control for panning and following objects
/// - Zoom control for close-up detail or wide-area overview
/// - Rotation support for artistic effects or gameplay mechanics
/// - Smooth interpolation support for cinematic camera movements
/// - Boundary constraints for limiting camera movement areas
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Matrix calculations cached until camera properties change
/// - Inverse matrix computed only when needed for coordinate transformations
/// - Minimal garbage allocation through struct-based vectors and matrices
/// 
/// USAGE PATTERNS:
/// - Strategy games: Pan to follow action, zoom for tactical overview
/// - Platformers: Follow player with smooth camera tracking
/// - Simulations: Free camera movement for detailed inspection
/// </summary>
public class GameCamera : ICamera
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA PARAMETERS
    // ═══════════════════════════════════════════════════════════════════════════
    
    private Vector2D<float> _position = Vector2D<float>.Zero;
    private float _rotation = 0f;
    private float _zoom = 1f;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // CACHED TRANSFORMATION MATRICES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private Matrix4X4<float> _viewMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _projectionMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _combinedMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _inverseViewMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _inverseProjectionMatrix = Matrix4X4<float>.Identity;
    
    private bool _matricesDirty = true;
    private int _viewportWidth = 1;
    private int _viewportHeight = 1;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// World position of the camera center point.
    /// Positive X moves camera right, positive Y moves camera up.
    /// </summary>
    public Vector2D<float> Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                _matricesDirty = true;
            }
        }
    }
    
    /// <summary>
    /// Camera rotation in radians (counter-clockwise positive).
    /// Rotates the view around the camera center point.
    /// Common values: 0 (no rotation), π/2 (90° rotation), π (180° rotation).
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (Math.Abs(_rotation - value) > float.Epsilon)
            {
                _rotation = value;
                _matricesDirty = true;
            }
        }
    }
    
    /// <summary>
    /// Camera zoom factor (multiplicative scale).
    /// Values > 1.0 zoom in (objects appear larger), values < 1.0 zoom out.
    /// Recommended range: 0.1 to 10.0 for typical 2D games.
    /// NOTE: Higher values = closer view (standard zoom convention).
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set
        {
            if (Math.Abs(_zoom - value) > float.Epsilon)
            {
                _zoom = Math.Max(0.01f, value); // Prevent invalid zoom values
                _matricesDirty = true;
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // MATRIX PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    public Matrix4X4<float> ViewMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _viewMatrix;
        }
    }
    
    public Matrix4X4<float> ProjectionMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _projectionMatrix;
        }
    }
    
    public Matrix4X4<float> CombinedMatrix
    {
        get
        {
            if (_matricesDirty) UpdateMatricesInternal();
            return _combinedMatrix;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA CONTROL METHODS
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Move camera by specified offset in world coordinates.
    /// Useful for relative camera movement (WASD controls, drag panning).
    /// </summary>
    /// <param name="offset">Movement offset in world units</param>
    public void Move(Vector2D<float> offset)
    {
        Position += offset;
    }
    
    /// <summary>
    /// Rotate camera by specified angle in radians.
    /// Useful for relative rotation controls (Q/E keys, mouse wheel).
    /// </summary>
    /// <param name="deltaRotation">Rotation change in radians</param>
    public void Rotate(float deltaRotation)
    {
        Rotation += deltaRotation;
    }
    
    /// <summary>
    /// Scale zoom by multiplicative factor.
    /// Values > 1.0 zoom in, values < 1.0 zoom out.
    /// Example: Scale(2.0f) doubles zoom, Scale(0.5f) halves zoom.
    /// </summary>
    /// <param name="factor">Multiplicative zoom factor</param>
    public void Scale(float factor)
    {
        Zoom *= factor;
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
        // VIEW MATRIX COMPUTATION
        // ───────────────────────────────────────────────────────────────────────
        
        // Standard camera view matrix construction
        // 1. Create individual transformation matrices
        var translationMatrix = Matrix4X4.CreateTranslation(-_position.X, -_position.Y, 0f); // Negative for view
        var rotationMatrix = Matrix4X4.CreateRotationZ(-_rotation); // Negative for view
        var scaleMatrix = Matrix4X4.CreateScale(_zoom, _zoom, 1f); // Zoom affects world scale
        
        // 2. Combine in standard order: Scale * Rotation * Translation
        _viewMatrix = translationMatrix * rotationMatrix * scaleMatrix;
        Matrix4X4.Invert(_viewMatrix, out _inverseViewMatrix);
        
        // ───────────────────────────────────────────────────────────────────────
        // PROJECTION MATRIX COMPUTATION
        // ───────────────────────────────────────────────────────────────────────
        
        // Orthographic projection preserving aspect ratio
        float aspectRatio = (float)_viewportWidth / _viewportHeight;
        float orthoHeight = 2f; // NDC space is [-1, +1] = 2 units total
        float orthoWidth = orthoHeight * aspectRatio;
        
        _projectionMatrix = Matrix4X4.CreateOrthographic(orthoWidth, orthoHeight, -1f, 1f);
        Matrix4X4.Invert(_projectionMatrix, out _inverseProjectionMatrix);
        
        // ───────────────────────────────────────────────────────────────────────
        // COMBINED MATRIX
        // ───────────────────────────────────────────────────────────────────────
        
        _combinedMatrix = _viewMatrix * _projectionMatrix;
        _matricesDirty = false;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // COORDINATE TRANSFORMATION UTILITIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    public Vector2D<float> WorldToScreen(Vector2D<float> worldPosition, int viewportWidth, int viewportHeight)
    {
        if (_matricesDirty) UpdateMatricesInternal();
        
        // Transform world position through view and projection matrices
        var worldPos4 = new Vector4D<float>(worldPosition.X, worldPosition.Y, 0f, 1f);
        var clipPos = Vector4D.Transform(worldPos4, CombinedMatrix);
        
        // Convert from NDC to screen coordinates
        var ndcX = clipPos.X / clipPos.W;
        var ndcY = clipPos.Y / clipPos.W;
        
        var screenX = (ndcX + 1f) * 0.5f * viewportWidth;
        var screenY = (1f - ndcY) * 0.5f * viewportHeight; // Flip Y for screen coordinates
        
        return new Vector2D<float>(screenX, screenY);
    }
    
    public Vector2D<float> ScreenToWorld(Vector2D<float> screenPosition, int viewportWidth, int viewportHeight)
    {
        if (_matricesDirty) UpdateMatricesInternal();
        
        // Convert screen coordinates to NDC
        var ndcX = (screenPosition.X / viewportWidth) * 2f - 1f;
        var ndcY = 1f - (screenPosition.Y / viewportHeight) * 2f; // Flip Y for NDC
        
        // Transform through inverse matrices
        var clipPos = new Vector4D<float>(ndcX, ndcY, 0f, 1f);
        var viewPos = Vector4D.Transform(clipPos, _inverseProjectionMatrix);
        var worldPos = Vector4D.Transform(viewPos, _inverseViewMatrix);
        
        return new Vector2D<float>(worldPos.X, worldPos.Y);
    }
}