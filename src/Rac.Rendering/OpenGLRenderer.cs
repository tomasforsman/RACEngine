// ═══════════════════════════════════════════════════════════════════════════════
// PHASE-BASED OPENGL RENDERER ORCHESTRATOR
// ═══════════════════════════════════════════════════════════════════════════════
//
// This is the new phase-based OpenGL renderer that orchestrates the four distinct
// rendering phases while maintaining backward compatibility with the existing IRenderer
// interface. This class enforces proper phase ordering and validates prerequisites.
//
// EDUCATIONAL ASPECTS:
// - Orchestrator pattern: Coordinates multiple specialized components
// - Phase isolation: Ensures clear separation of concerns
// - Dependency injection: Phases depend only on earlier phases
// - State validation: Prevents operations in wrong phase

using Rac.Rendering.Camera;
using Rac.Rendering.Pipeline;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
/// Phase-based OpenGL renderer that orchestrates distinct rendering phases.
/// 
/// PHASE ARCHITECTURE:
/// 1. Configuration: Pure data structures (no GPU interaction)
/// 2. Preprocessing: Asset loading, shader compilation, GPU setup
/// 3. Processing: Fast GPU rendering operations  
/// 4. Post-Processing: Screen-space effects
/// 
/// DESIGN BENEFITS:
/// - Clear separation of concerns
/// - Tool-friendly phase boundaries
/// - Predictable operation ordering
/// - Performance optimization through phase isolation
/// - Extensibility through focused components
/// </summary>
public class OpenGLRenderer : IRenderer, IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PHASE ORCHESTRATION STATE
    // ═══════════════════════════════════════════════════════════════════════════
    
    private GL _gl = null!;
    private bool _isInitialized = false;
    private bool _disposed = false;
    
    // Phase components
    private RenderConfiguration _configuration;
    private RenderPreprocessor? _preprocessor;
    private RenderProcessor? _processor;
    private PostProcessor? _postProcessor;
    
    // Current frame state
    private bool _isFrameActive = false;
    
    /// <summary>
    /// Provides access to the current render configuration.
    /// </summary>
    public RenderConfiguration Configuration => _configuration;
    
    /// <summary>
    /// Indicates whether all phases have been properly initialized.
    /// </summary>
    public bool IsFullyInitialized => _isInitialized && _preprocessor?.IsPreprocessed == true;
    
    // ───────────────────────────────────────────────────────────────────────────
    // PHASE 1: INITIALIZATION AND CONFIGURATION
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Initializes the renderer with window context and default configuration.
    /// This triggers the preprocessing phase automatically for backward compatibility.
    /// </summary>
    public void Initialize(IWindow window)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Renderer is already initialized");
            
        Console.WriteLine("Initializing phase-based OpenGL renderer...");
        
        // Get OpenGL context
        _gl = GL.GetApi(window);
        _gl.ClearColor(0f, 0f, 0f, 1f);
        
        // Create default configuration based on window
        _configuration = new RenderConfiguration(window.Size);
        
        // Automatically proceed to preprocessing for backward compatibility
        InitializePreprocessing();
        InitializeProcessing();
        InitializePostProcessing();
        
        _isInitialized = true;
        Console.WriteLine("✓ Phase-based renderer initialization complete");
    }
    
    /// <summary>
    /// Updates the render configuration and triggers reprocessing if needed.
    /// </summary>
    /// <param name="newConfiguration">New configuration to apply</param>
    public void UpdateConfiguration(RenderConfiguration newConfiguration)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Renderer must be initialized before updating configuration");
            
        var needsReprocessing = RequiresReprocessing(_configuration, newConfiguration);
        _configuration = newConfiguration;
        
        if (needsReprocessing)
        {
            Console.WriteLine("Configuration change requires reprocessing...");
            ReinitializePhases();
        }
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // PHASE 2: PREPROCESSING
    // ───────────────────────────────────────────────────────────────────────────
    
    private void InitializePreprocessing()
    {
        Console.WriteLine("Initializing preprocessing phase...");
        
        _preprocessor?.Dispose();
        _preprocessor = new RenderPreprocessor(_gl);
        _preprocessor.Preprocess(_configuration);
        
        Console.WriteLine("✓ Preprocessing phase complete");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // PHASE 3: PROCESSING SETUP
    // ───────────────────────────────────────────────────────────────────────────
    
    private void InitializeProcessing()
    {
        Console.WriteLine("Initializing processing phase...");
        
        if (_preprocessor == null || !_preprocessor.IsPreprocessed)
            throw new InvalidOperationException("Preprocessing must complete before processing setup");
            
        _processor = new RenderProcessor(_gl, _preprocessor);
        
        Console.WriteLine("✓ Processing phase ready");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // PHASE 4: POST-PROCESSING SETUP
    // ───────────────────────────────────────────────────────────────────────────
    
    private void InitializePostProcessing()
    {
        Console.WriteLine("Initializing post-processing phase...");
        
        if (_preprocessor == null)
            throw new InvalidOperationException("Preprocessing must complete before post-processing setup");
            
        _postProcessor?.Dispose();
        _postProcessor = new PostProcessor(_gl, _preprocessor, _configuration);
        
        Console.WriteLine("✓ Post-processing phase ready");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // IRENDERER INTERFACE IMPLEMENTATION
    // ───────────────────────────────────────────────────────────────────────────
    
    public void Clear()
    {
        ValidateRenderingState();
        
        // Begin post-processing frame if needed
        if (_postProcessor?.IsPostProcessingActive == true)
        {
            _postProcessor.BeginFrame();
            _isFrameActive = true;
        }
        
        // Clear with configured color
        _processor!.Clear(_configuration.ClearColor);
    }
    
    public void SetColor(Vector4D<float> rgba)
    {
        ValidateRenderingState();
        _processor!.SetColor(rgba);
    }
    
    public void SetCameraMatrix(Matrix4X4<float> cameraMatrix)
    {
        ValidateRenderingState();
        _processor!.SetCameraMatrix(cameraMatrix);
    }
    
    public void SetActiveCamera(ICamera camera)
    {
        ValidateRenderingState();
        _processor!.SetActiveCamera(camera, _configuration.ViewportSize.X, _configuration.ViewportSize.Y);
    }
    
    public void SetShaderMode(ShaderMode mode)
    {
        ValidateRenderingState();
        _processor!.SetShaderMode(mode);
    }
    
    public void SetPrimitiveType(PrimitiveType primitiveType)
    {
        ValidateRenderingState();
        _processor!.SetPrimitiveType(primitiveType);
    }
    
    public void UpdateVertices(float[] vertices)
    {
        ValidateRenderingState();
        _processor!.UpdateVertices(vertices);
    }
    
    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        ValidateRenderingState();
        _processor!.UpdateVertices(vertices);
    }
    
    public void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        ValidateRenderingState();
        _processor!.UpdateVertices(vertices, layout);
    }
    
    public void Draw()
    {
        ValidateRenderingState();
        _processor!.Draw();
    }
    
    /// <summary>
    /// Draw with index buffer for complex geometry.
    /// </summary>
    public void DrawIndexed(uint[] indices)
    {
        ValidateRenderingState();
        _processor!.DrawIndexed(indices);
    }
    
    /// <summary>
    /// Sets a custom uniform value in the current shader.
    /// </summary>
    public void SetUniform(string name, float value)
    {
        // Note: Custom uniform support would require extending RenderProcessor
        // For now, log a warning that this feature is not yet implemented
        Console.WriteLine($"Warning: SetUniform({name}, {value}) not yet supported in phase-based renderer");
    }
    
    /// <summary>
    /// Sets a custom uniform value in the current shader.
    /// </summary>
    public void SetUniform(string name, Vector2D<float> value)
    {
        Console.WriteLine($"Warning: SetUniform({name}, Vector2D) not yet supported in phase-based renderer");
    }
    
    /// <summary>
    /// Sets a custom uniform value in the current shader.
    /// </summary>
    public void SetUniform(string name, Vector4D<float> value)
    {
        Console.WriteLine($"Warning: SetUniform({name}, Vector4D) not yet supported in phase-based renderer");
    }
    
    public void FinalizeFrame()
    {
        ValidateRenderingState();
        
        try
        {
            // Apply post-processing effects if active
            if (_isFrameActive && _postProcessor?.IsPostProcessingActive == true)
            {
                _postProcessor.FinalizeFrame();
            }
            
            _isFrameActive = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Frame finalization failed: {ex.Message}");
            
            // Reset frame state to prevent stuck states
            _isFrameActive = false;
            _postProcessor?.ResetFrame();
            
            throw;
        }
    }
    
    public void Resize(Vector2D<int> newSize)
    {
        if (!_isInitialized) return;
        
        Console.WriteLine($"Resizing renderer to {newSize.X}x{newSize.Y}");
        
        // Update viewport
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        
        // Update aspect ratio in processor
        var aspectRatio = newSize.Y / (float)newSize.X;
        _processor?.SetAspectRatio(aspectRatio);
        
        // Update configuration with new size
        var newConfig = _configuration with { ViewportSize = newSize };
        UpdateConfiguration(newConfig);
    }
    
    public void Shutdown()
    {
        Dispose();
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // INTERNAL VALIDATION AND UTILITIES
    // ───────────────────────────────────────────────────────────────────────────
    
    private void ValidateRenderingState()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenGLRenderer));
            
        if (!IsFullyInitialized)
            throw new InvalidOperationException("Renderer is not fully initialized. Call Initialize() first.");
            
        if (_processor == null)
            throw new InvalidOperationException("Processing phase is not ready");
    }
    
    private static bool RequiresReprocessing(RenderConfiguration oldConfig, RenderConfiguration newConfig)
    {
        // Changes that require full reprocessing
        return oldConfig.ViewportSize != newConfig.ViewportSize ||
               oldConfig.PostProcessing.EnableBloom != newConfig.PostProcessing.EnableBloom ||
               oldConfig.Quality.MsaaSamples != newConfig.Quality.MsaaSamples;
    }
    
    private void ReinitializePhases()
    {
        try
        {
            // Dispose existing phases in reverse order
            _postProcessor?.Dispose();
            _processor = null;
            _preprocessor?.Dispose();
            
            // Reinitialize phases
            InitializePreprocessing();
            InitializeProcessing();
            InitializePostProcessing();
            
            Console.WriteLine("✓ Phase reinitialization complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Phase reinitialization failed: {ex.Message}");
            throw new InvalidOperationException($"Failed to reinitialize rendering phases: {ex.Message}", ex);
        }
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // RESOURCE CLEANUP
    // ───────────────────────────────────────────────────────────────────────────
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Console.WriteLine("Disposing phase-based renderer...");
        
        // Reset frame state
        _isFrameActive = false;
        
        // Dispose phases in reverse order
        _postProcessor?.Dispose();
        _postProcessor = null;
        
        _processor = null; // No disposal needed, it's just a coordinator
        
        _preprocessor?.Dispose();
        _preprocessor = null;
        
        _isInitialized = false;
        _disposed = true;
        
        GC.SuppressFinalize(this);
        Console.WriteLine("✓ Phase-based renderer disposed");
    }
}