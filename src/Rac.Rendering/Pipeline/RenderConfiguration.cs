// ═══════════════════════════════════════════════════════════════════════════════
// RENDERING PIPELINE CONFIGURATION PHASE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Pure data structures defining rendering behavior without any GPU interaction,
// file I/O, or asset loading. This phase represents immutable configuration
// that drives the preprocessing and processing phases.
//
// EDUCATIONAL ASPECTS:
// - Separation of concerns: Configuration is pure data
// - Immutable design patterns for thread-safe configuration
// - Builder pattern for complex configuration construction
// - Validation patterns for ensuring configuration consistency

using Rac.Rendering.Camera;
using Rac.Rendering.Shader;
using Silk.NET.Maths;

namespace Rac.Rendering.Pipeline;

/// <summary>
/// Immutable configuration for the rendering pipeline.
/// 
/// DESIGN PRINCIPLES:
/// - Pure data structure with no behavior
/// - Immutable after construction for thread safety
/// - No GPU interaction, file I/O, or asset loading
/// - Validation happens at construction time
/// 
/// CONFIGURATION SCOPE:
/// - Camera settings and viewport configuration
/// - Post-processing effect parameters
/// - Quality settings and performance hints
/// - Layer definitions and rendering order
/// </summary>
public readonly record struct RenderConfiguration
{
    /// <summary>Camera configuration for view and projection matrices</summary>
    public CameraConfiguration Camera { get; init; }
    
    /// <summary>Post-processing effect configuration</summary>
    public PostProcessingConfiguration PostProcessing { get; init; }
    
    /// <summary>Quality and performance settings</summary>
    public QualityConfiguration Quality { get; init; }
    
    /// <summary>Viewport dimensions in pixels</summary>
    public Vector2D<int> ViewportSize { get; init; }
    
    /// <summary>Default clear color for the framebuffer</summary>
    public Vector4D<float> ClearColor { get; init; }
    
    /// <summary>
    /// Creates a new render configuration with validation.
    /// </summary>
    /// <param name="viewportSize">Viewport dimensions (must be positive)</param>
    /// <exception cref="ArgumentException">When viewport size is invalid</exception>
    public RenderConfiguration(Vector2D<int> viewportSize)
    {
        if (viewportSize.X <= 0 || viewportSize.Y <= 0)
            throw new ArgumentException("Viewport size must be positive", nameof(viewportSize));
            
        ViewportSize = viewportSize;
        Camera = new CameraConfiguration();
        PostProcessing = new PostProcessingConfiguration();
        Quality = new QualityConfiguration();
        ClearColor = new Vector4D<float>(0f, 0f, 0f, 1f);
    }
    
    /// <summary>
    /// Creates a configuration builder for complex setup scenarios.
    /// </summary>
    public static RenderConfigurationBuilder Create(Vector2D<int> viewportSize) =>
        new(viewportSize);
}

/// <summary>
/// Camera-specific configuration parameters.
/// </summary>
public readonly record struct CameraConfiguration
{
    /// <summary>Field of view for perspective cameras</summary>
    public float FieldOfView { get; init; }
    
    /// <summary>Near clipping plane distance</summary>
    public float NearPlane { get; init; }
    
    /// <summary>Far clipping plane distance</summary>
    public float FarPlane { get; init; }
    
    /// <summary>Whether to use orthographic projection</summary>
    public bool UseOrthographic { get; init; }
    
    public CameraConfiguration()
    {
        FieldOfView = 45f;
        NearPlane = 0.1f;
        FarPlane = 1000f;
        UseOrthographic = true;
    }
}

/// <summary>
/// Post-processing effect configuration.
/// </summary>
public readonly record struct PostProcessingConfiguration
{
    /// <summary>Enable bloom post-processing effect</summary>
    public bool EnableBloom { get; init; }
    
    /// <summary>Brightness threshold for bloom extraction</summary>
    public float BloomThreshold { get; init; }
    
    /// <summary>Bloom effect intensity multiplier</summary>
    public float BloomIntensity { get; init; }
    
    /// <summary>Gaussian blur radius for bloom effect</summary>
    public float BlurRadius { get; init; }
    
    public PostProcessingConfiguration()
    {
        EnableBloom = false;
        BloomThreshold = 1.0f;
        BloomIntensity = 1.0f;
        BlurRadius = 2.0f;
    }
}

/// <summary>
/// Quality and performance configuration.
/// </summary>
public readonly record struct QualityConfiguration
{
    /// <summary>MSAA sample count (1 = disabled, 2/4/8 = enabled)</summary>
    public int MsaaSamples { get; init; }
    
    /// <summary>Enable anisotropic filtering for textures</summary>
    public bool EnableAnisotropicFiltering { get; init; }
    
    /// <summary>Maximum anisotropy level</summary>
    public float MaxAnisotropy { get; init; }
    
    /// <summary>V-Sync enabled</summary>
    public bool VSync { get; init; }
    
    public QualityConfiguration()
    {
        MsaaSamples = 1;
        EnableAnisotropicFiltering = true;
        MaxAnisotropy = 16f;
        VSync = true;
    }
}

/// <summary>
/// Builder pattern for complex render configuration construction.
/// </summary>
public class RenderConfigurationBuilder
{
    private Vector2D<int> _viewportSize;
    private CameraConfiguration _camera = new();
    private PostProcessingConfiguration _postProcessing = new();
    private QualityConfiguration _quality = new();
    private Vector4D<float> _clearColor = new(0f, 0f, 0f, 1f);
    
    internal RenderConfigurationBuilder(Vector2D<int> viewportSize)
    {
        if (viewportSize.X <= 0 || viewportSize.Y <= 0)
            throw new ArgumentException("Viewport size must be positive", nameof(viewportSize));
        _viewportSize = viewportSize;
    }
    
    /// <summary>Configures camera settings</summary>
    public RenderConfigurationBuilder WithCamera(CameraConfiguration camera)
    {
        _camera = camera;
        return this;
    }
    
    /// <summary>Configures post-processing effects</summary>
    public RenderConfigurationBuilder WithPostProcessing(PostProcessingConfiguration postProcessing)
    {
        _postProcessing = postProcessing;
        return this;
    }
    
    /// <summary>Configures quality settings</summary>
    public RenderConfigurationBuilder WithQuality(QualityConfiguration quality)
    {
        _quality = quality;
        return this;
    }
    
    /// <summary>Sets the clear color</summary>
    public RenderConfigurationBuilder WithClearColor(Vector4D<float> clearColor)
    {
        _clearColor = clearColor;
        return this;
    }
    
    /// <summary>Updates viewport size</summary>
    public RenderConfigurationBuilder WithViewportSize(Vector2D<int> viewportSize)
    {
        if (viewportSize.X <= 0 || viewportSize.Y <= 0)
            throw new ArgumentException("Viewport size must be positive", nameof(viewportSize));
        _viewportSize = viewportSize;
        return this;
    }
    
    /// <summary>Builds the final immutable configuration</summary>
    public RenderConfiguration Build() => new()
    {
        ViewportSize = _viewportSize,
        Camera = _camera,
        PostProcessing = _postProcessing,
        Quality = _quality,
        ClearColor = _clearColor
    };
}