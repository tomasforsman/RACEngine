// ═══════════════════════════════════════════════════════════════════════════════
// RENDERING PIPELINE POST-PROCESSING PHASE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Screen-space effects applied after main rendering is complete. This phase
// operates only on rendered frame data and applies various visual effects
// like bloom, tone mapping, and other screen-space techniques.
//
// EDUCATIONAL ASPECTS:
// - Screen-space rendering: Effects that operate on the final rendered image
// - Multi-pass rendering: Breaking complex effects into multiple simple passes
// - Framebuffer management: Efficient render target switching
// - Effect composition: Combining multiple effects in a coherent pipeline

using Rac.Rendering.VFX;
using Silk.NET.OpenGL;

namespace Rac.Rendering.Pipeline;

/// <summary>
/// Manages screen-space effects applied after main rendering.
/// 
/// RESPONSIBILITIES:
/// - Coordinating post-processing effects
/// - Managing render target switching
/// - Applying screen-space effects like bloom
/// - Final frame composition
/// 
/// RESTRICTIONS:
/// - Operates only on rendered frame data
/// - No main scene rendering
/// - Must be used after processing phase
/// - Effects are applied in specific order
/// </summary>
public class PostProcessor : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // POST-PROCESSING STATE AND RESOURCES  
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly GL _gl;
    private readonly PostProcessing? _postProcessing;
    private readonly RenderConfiguration _configuration;
    
    private bool _isFrameStarted = false;
    private bool _disposed = false;
    
    /// <summary>
    /// Creates a new post-processor.
    /// </summary>
    /// <param name="gl">OpenGL context (must be valid and current)</param>
    /// <param name="preprocessor">Preprocessor containing post-processing resources</param>
    /// <param name="configuration">Configuration defining which effects to apply</param>
    /// <exception cref="ArgumentNullException">When parameters are null</exception>
    public PostProcessor(GL gl, RenderPreprocessor preprocessor, RenderConfiguration configuration)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        ArgumentNullException.ThrowIfNull(preprocessor);
        
        _configuration = configuration;
        _postProcessing = preprocessor.PostProcessingSystem;
        
        // Validate that post-processing is available if enabled
        if (_configuration.PostProcessing.EnableBloom && _postProcessing == null)
        {
            throw new InvalidOperationException("Bloom is enabled in configuration but post-processing system is not available");
        }
    }
    
    /// <summary>
    /// Indicates whether post-processing effects are active for this frame.
    /// </summary>
    public bool IsPostProcessingActive => 
        _configuration.PostProcessing.EnableBloom && _postProcessing != null;
    
    /// <summary>
    /// Indicates whether a frame has been started and is awaiting finalization.
    /// </summary>
    public bool IsFrameStarted => _isFrameStarted;
    
    // ───────────────────────────────────────────────────────────────────────────
    // FRAME LIFECYCLE
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Begins post-processing frame capture.
    /// Must be called before any main scene rendering if post-processing is active.
    /// </summary>
    /// <exception cref="InvalidOperationException">When frame is already started</exception>
    public void BeginFrame()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostProcessor));
            
        if (_isFrameStarted)
            throw new InvalidOperationException("Frame has already been started");
        
        if (IsPostProcessingActive)
        {
            try
            {
                _postProcessing!.BeginScenePass();
                _isFrameStarted = true;
                Console.WriteLine("✓ Post-processing frame started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start post-processing frame: {ex.Message}");
                throw new InvalidOperationException($"Post-processing frame start failed: {ex.Message}", ex);
            }
        }
        else
        {
            // No post-processing active, but we still track frame state
            _isFrameStarted = true;
        }
    }
    
    /// <summary>
    /// Finalizes the frame by applying all post-processing effects.
    /// Must be called after all main scene rendering is complete.
    /// </summary>
    /// <exception cref="InvalidOperationException">When frame hasn't been started</exception>
    public void FinalizeFrame()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostProcessor));
            
        if (!_isFrameStarted)
            throw new InvalidOperationException("Frame must be started before finalization");
        
        try
        {
            if (IsPostProcessingActive)
            {
                ApplyPostProcessingEffects();
                Console.WriteLine("✓ Post-processing effects applied");
            }
            
            _isFrameStarted = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Post-processing finalization failed: {ex.Message}");
            
            // Reset frame state to prevent stuck states
            _isFrameStarted = false;
            
            // Try to recover by ensuring framebuffer is reset
            try
            {
                _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                _gl.Clear(ClearBufferMask.ColorBufferBit);
            }
            catch
            {
                // Ignore recovery errors
            }
            
            throw new InvalidOperationException($"Post-processing finalization failed: {ex.Message}", ex);
        }
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // EFFECT APPLICATION
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Applies all enabled post-processing effects in the correct order.
    /// </summary>
    private void ApplyPostProcessingEffects()
    {
        if (_postProcessing == null) return;
        
        // Apply effects based on configuration
        if (_configuration.PostProcessing.EnableBloom)
        {
            ApplyBloomEffect();
        }
        
        // Future effects would be added here in order:
        // - Tone mapping
        // - Color grading
        // - Anti-aliasing
        // - Screen-space reflections
        // etc.
    }
    
    /// <summary>
    /// Applies bloom post-processing effect with configured parameters.
    /// 
    /// BLOOM EFFECT PIPELINE:
    /// 1. Extract bright pixels above threshold
    /// 2. Apply Gaussian blur to bright areas
    /// 3. Composite blurred bloom with original scene
    /// </summary>
    private void ApplyBloomEffect()
    {
        try
        {
            // Update bloom parameters if they've changed
            UpdateBloomParameters();
            
            // Apply the bloom effect pipeline
            _postProcessing!.EndScenePassAndApplyBloom();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Bloom effect failed: {ex.Message}");
            throw new InvalidOperationException($"Bloom effect application failed: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Updates bloom effect parameters based on current configuration.
    /// </summary>
    private void UpdateBloomParameters()
    {
        // Note: This would typically update parameters in the PostProcessing system
        // if it supported runtime parameter changes. For now, parameters are
        // set during initialization.
        
        // Future enhancement: Add parameter update methods to PostProcessing
        // _postProcessing.SetBloomThreshold(_configuration.PostProcessing.BloomThreshold);
        // _postProcessing.SetBloomIntensity(_configuration.PostProcessing.BloomIntensity);
        // _postProcessing.SetBlurRadius(_configuration.PostProcessing.BlurRadius);
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // UTILITY METHODS
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Forces frame reset in case of errors or state corruption.
    /// </summary>
    public void ResetFrame()
    {
        if (_disposed) return;
        
        _isFrameStarted = false;
        
        try
        {
            // Ensure we're rendering to the screen framebuffer
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        catch
        {
            // Ignore errors during reset
        }
    }
    
    /// <summary>
    /// Updates viewport size for post-processing effects.
    /// Should be called when the window is resized.
    /// </summary>
    /// <param name="width">New viewport width</param>
    /// <param name="height">New viewport height</param>
    public void UpdateViewportSize(int width, int height)
    {
        if (_disposed) return;
        
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Viewport dimensions must be positive");
        
        // Note: Current PostProcessing implementation doesn't support runtime viewport changes
        // This method is a placeholder for future enhancement
        Console.WriteLine($"PostProcessor viewport update requested: {width}x{height}");
        Console.WriteLine("Note: Runtime viewport updates not yet supported for post-processing effects");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // RESOURCE CLEANUP
    // ───────────────────────────────────────────────────────────────────────────
    
    public void Dispose()
    {
        if (_disposed) return;
        
        // Reset any ongoing frame
        ResetFrame();
        
        // Note: We don't dispose _postProcessing here because it's owned by RenderPreprocessor
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}