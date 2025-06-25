// ═══════════════════════════════════════════════════════════════════════════════
// RENDERING PIPELINE PREPROCESSING PHASE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Asset loading, validation, and GPU resource compilation. This phase handles
// all expensive operations that happen once during initialization, before any
// rendering can occur.
//
// EDUCATIONAL ASPECTS:
// - Resource management: Centralized asset loading and validation
// - OpenGL state initialization: VAO/VBO setup, shader compilation
// - Error handling: Comprehensive validation with meaningful error messages
// - Performance optimization: All expensive operations happen here, not during rendering

using Rac.Rendering.Shader;
using Rac.Rendering.VFX;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace Rac.Rendering.Pipeline;

/// <summary>
/// Manages asset loading, validation, and GPU resource compilation.
/// 
/// RESPONSIBILITIES:
/// - Shader compilation and validation
/// - Vertex buffer object creation and setup
/// - Post-processing framebuffer initialization
/// - Asset dependency validation
/// - GPU capability validation
/// 
/// RESTRICTIONS:
/// - No per-frame operations
/// - No rendering commands
/// - Must be completed before processing phase
/// </summary>
public class RenderPreprocessor : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PREPROCESSING STATE AND RESOURCES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly GL _gl;
    private bool _isPreprocessed = false;
    private bool _disposed = false;
    
    // Compiled shader programs
    private readonly Dictionary<ShaderMode, ShaderProgram> _shaders = new();
    private readonly Dictionary<ShaderMode, ShaderUniforms> _uniforms = new();
    
    // OpenGL resources
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    
    // Post-processing resources
    private PostProcessing? _postProcessing;
    
    /// <summary>
    /// Encapsulates uniform variable locations for performance optimization
    /// </summary>
    public class ShaderUniforms
    {
        public int AspectLocation { get; init; }
        public int ColorLocation { get; init; }
        public int CameraMatrixLocation { get; init; }

        public ShaderUniforms(GL gl, uint programHandle)
        {
            AspectLocation = gl.GetUniformLocation(programHandle, "uAspect");
            ColorLocation = gl.GetUniformLocation(programHandle, "uColor");
            CameraMatrixLocation = gl.GetUniformLocation(programHandle, "uCameraMatrix");
        }
    }
    
    /// <summary>
    /// Creates a new render preprocessor.
    /// </summary>
    /// <param name="gl">OpenGL context (must be valid and current)</param>
    /// <exception cref="ArgumentNullException">When GL context is null</exception>
    public RenderPreprocessor(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
    }
    
    /// <summary>
    /// Indicates whether preprocessing has been completed successfully.
    /// </summary>
    public bool IsPreprocessed => _isPreprocessed;
    
    /// <summary>
    /// Provides access to compiled shader programs.
    /// </summary>
    public IReadOnlyDictionary<ShaderMode, ShaderProgram> Shaders => _shaders;
    
    /// <summary>
    /// Provides access to shader uniform locations.
    /// </summary>
    public IReadOnlyDictionary<ShaderMode, ShaderUniforms> Uniforms => _uniforms;
    
    /// <summary>
    /// Provides access to vertex array object.
    /// </summary>
    public uint VertexArrayObject => _vao;
    
    /// <summary>
    /// Provides access to vertex buffer object.
    /// </summary>
    public uint VertexBufferObject => _vbo;
    
    /// <summary>
    /// Provides access to element buffer object.
    /// </summary>
    public uint ElementBufferObject => _ebo;
    
    /// <summary>
    /// Provides access to post-processing system.
    /// </summary>
    public PostProcessing? PostProcessingSystem => _postProcessing;
    
    /// <summary>
    /// Performs complete preprocessing pipeline with comprehensive validation.
    /// </summary>
    /// <param name="configuration">Rendering configuration to preprocess</param>
    /// <exception cref="InvalidOperationException">When preprocessing fails or is called multiple times</exception>
    /// <exception cref="ArgumentNullException">When configuration is null</exception>
    public void Preprocess(RenderConfiguration configuration)
    {
        if (_isPreprocessed)
            throw new InvalidOperationException("Preprocessing has already been completed");
            
        if (_disposed)
            throw new ObjectDisposedException(nameof(RenderPreprocessor));
        
        try
        {
            Console.WriteLine("Starting render preprocessing...");
            
            // Step 1: Validate OpenGL context and capabilities
            ValidateOpenGLContext();
            ValidateOpenGLCapabilities(configuration);
            
            // Step 2: Load and compile all shaders
            InitializeShaderSystem();
            
            // Step 3: Set up vertex buffer objects
            SetupVertexBuffers();
            
            // Step 4: Initialize post-processing if enabled
            if (configuration.PostProcessing.EnableBloom)
            {
                InitializePostProcessing(configuration);
            }
            
            _isPreprocessed = true;
            Console.WriteLine("✓ Render preprocessing completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Preprocessing failed: {ex.Message}");
            
            // Clean up any partially created resources
            DisposeResources();
            throw new InvalidOperationException($"Render preprocessing failed: {ex.Message}", ex);
        }
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // VALIDATION METHODS
    // ───────────────────────────────────────────────────────────────────────────
    
    private void ValidateOpenGLContext()
    {
        Console.WriteLine("Validating OpenGL context...");
        
        var error = _gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new InvalidOperationException($"OpenGL context has existing error state: {error}");
        }
        
        Console.WriteLine("✓ OpenGL context validated");
    }
    
    private void ValidateOpenGLCapabilities(RenderConfiguration configuration)
    {
        Console.WriteLine("Validating OpenGL capabilities...");
        
        // Basic validation without version string parsing for now
        Console.WriteLine("OpenGL context is available and responsive");
        
        // Validate post-processing requirements
        if (configuration.PostProcessing.EnableBloom)
        {
            // Check framebuffer object support
            if (!_gl.IsExtensionPresent("GL_ARB_framebuffer_object"))
            {
                throw new InvalidOperationException("Post-processing requires GL_ARB_framebuffer_object extension");
            }
        }
        
        Console.WriteLine("✓ OpenGL capabilities validated");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // SHADER SYSTEM INITIALIZATION
    // ───────────────────────────────────────────────────────────────────────────
    
    private void InitializeShaderSystem()
    {
        Console.WriteLine("Initializing shader system...");
        
        var vertexShaderSource = ShaderLoader.LoadVertexShader();
        var loadedCount = 0;
        var failedCount = 0;

        foreach (ShaderMode mode in Enum.GetValues<ShaderMode>())
        {
            try
            {
                Console.WriteLine($"Loading shader mode: {mode}");
                var fragmentSource = ShaderLoader.LoadFragmentShader(mode);
                Console.WriteLine($"  Fragment shader loaded ({fragmentSource.Length} chars)");
                
                var shaderProgram = new ShaderProgram(_gl, vertexShaderSource, fragmentSource);
                Console.WriteLine($"  Shader program compiled successfully");
                
                var uniforms = new ShaderUniforms(_gl, shaderProgram.Handle);
                Console.WriteLine($"  Uniforms located: aspect={uniforms.AspectLocation}, color={uniforms.ColorLocation}, camera={uniforms.CameraMatrixLocation}");

                _shaders[mode] = shaderProgram;
                _uniforms[mode] = uniforms;

                loadedCount++;
                Console.WriteLine($"✅ Loaded shader mode: {mode}");
            }
            catch (Exception ex)
            {
                failedCount++;
                Console.WriteLine($"⚠️ Failed to load shader mode {mode}:");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        if (!_shaders.ContainsKey(ShaderMode.Normal))
        {
            throw new InvalidOperationException("Critical error: Normal shader mode is required but failed to load");
        }

        Console.WriteLine($"✓ Shader system initialized: {loadedCount} loaded, {failedCount} failed");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // VERTEX BUFFER SETUP
    // ───────────────────────────────────────────────────────────────────────────
    
    private void SetupVertexBuffers()
    {
        Console.WriteLine("Setting up vertex buffers...");
        
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // Unbind for safety
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        
        Console.WriteLine("✓ Vertex buffers initialized");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // POST-PROCESSING INITIALIZATION
    // ───────────────────────────────────────────────────────────────────────────
    
    private void InitializePostProcessing(RenderConfiguration configuration)
    {
        Console.WriteLine("Initializing post-processing system...");
        
        // Validate required shader files exist
        ValidatePostProcessingShaders();
        
        // Create and initialize PostProcessing instance
        _postProcessing = new PostProcessing(_gl);
        _postProcessing.Initialize(configuration.ViewportSize.X, configuration.ViewportSize.Y);
        
        Console.WriteLine("✓ Post-processing system initialized");
    }
    
    private void ValidatePostProcessingShaders()
    {
        var requiredShaders = new[]
        {
            "fullscreen_quad.vert",
            "brightness_extract.frag", 
            "gaussian_blur.frag",
            "bloom_composite.frag"
        };

        Console.WriteLine("Validating post-processing shaders...");
        
        foreach (var shaderFile in requiredShaders)
        {
            try
            {
                // This will throw if shader doesn't exist or can't be loaded
                ShaderLoader.LoadShaderFile(shaderFile);
                Console.WriteLine($"✓ Validated shader: {shaderFile}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Required post-processing shader '{shaderFile}' is missing or invalid: {ex.Message}");
            }
        }
        
        Console.WriteLine("✓ All post-processing shaders validated");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // RESOURCE CLEANUP
    // ───────────────────────────────────────────────────────────────────────────
    
    private void DisposeResources()
    {
        // Dispose shader programs
        foreach (var shader in _shaders.Values)
        {
            shader?.Dispose();
        }
        _shaders.Clear();
        _uniforms.Clear();
        
        // Dispose OpenGL resources
        if (_vao != 0)
        {
            _gl.DeleteVertexArray(_vao);
            _vao = 0;
        }
        
        if (_vbo != 0)
        {
            _gl.DeleteBuffer(_vbo);
            _vbo = 0;
        }
        
        if (_ebo != 0)
        {
            _gl.DeleteBuffer(_ebo);
            _ebo = 0;
        }
        
        // Dispose post-processing
        _postProcessing?.Dispose();
        _postProcessing = null;
        
        _isPreprocessed = false;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        DisposeResources();
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}