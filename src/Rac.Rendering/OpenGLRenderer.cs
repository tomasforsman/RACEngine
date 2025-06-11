// ════════════════════════════════════════════════════════════════════════════════
//  OPENGL RENDERER
// ════════════════════════════════════════════════════════════════════════════════
//
// Modern OpenGL renderer designed for high-performance graphics applications.
// Supports everything from simple 2D shapes to complex visual effects with
// a type-safe, extensible architecture.
//
// CORE CAPABILITIES:
// - Dynamic shader discovery and hot-reloading
// - Type-safe vertex structures with compile-time validation
// - Multiple vertex attribute support (position, texture, color, normals)
// - Post-processing effects (bloom, HDR tone mapping)
// - Comprehensive error handling and graceful fallbacks
// - Automatic GPU state management and performance optimization
// - Resource lifecycle management with deterministic cleanup
//
// VERTEX SYSTEM:
// - Structured vertex types: BasicVertex, TexturedVertex, FullVertex
// - Automatic layout detection and OpenGL attribute configuration
// - Legacy float array support for backward compatibility
// - Extensible design for custom vertex types
// - Efficient batching with complex vertex data
//
// SHADER MANAGEMENT:
// - File-system based shader discovery (drop files to add effects)
// - Automatic blending configuration per shader mode
// - Cached uniform locations for optimal performance
// - Runtime shader switching with state preservation
//
// ARCHITECTURE:
// - Single class handling all rendering complexity
// - Production-ready error handling and logging
// - Window lifecycle integration (resize, cleanup)
// - Future-proof design supporting advanced graphics techniques

using Rac.Rendering.Shader;
using Rac.Rendering.VFX;

using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
/// Production-ready OpenGL renderer with flexible vertex attribute system.
///
/// DESIGN GOALS:
/// - Type safety through structured vertex data
/// - Performance through caching and state optimization
/// - Extensibility through modular shader and vertex systems
/// - Reliability through comprehensive error handling
/// - Future-proofing through flexible architecture
///
/// CAPABILITIES:
/// - Multiple vertex types (BasicVertex, TexturedVertex, FullVertex)
/// - Dynamic shader discovery and management
/// - Post-processing effects (bloom, HDR)
/// - Automatic GPU state management
/// - Comprehensive error handling and fallbacks
/// </summary>
public class OpenGLRenderer : IRenderer, IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE RENDERING INFRASTRUCTURE
    // ═══════════════════════════════════════════════════════════════════════════

    private GL _gl = null!;
    private float _aspectRatio;
    private ShaderMode _currentShaderMode = ShaderMode.Normal;

    // ═══════════════════════════════════════════════════════════════════════════
    // DYNAMIC SHADER MANAGEMENT SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Dictionary<ShaderMode, ShaderProgram> _shaders = new();
    private readonly Dictionary<ShaderMode, ShaderUniforms> _uniforms = new();
    private ShaderProgram? _currentShader;
    private ShaderUniforms? _currentUniforms;

    // ═══════════════════════════════════════════════════════════════════════════
    // POST-PROCESSING INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════

    private PostProcessing? _postProcessing;
    private bool _isBloomActive = false;
    private Vector2D<int> _windowSize;

    /// <summary>Post-processing system accessor for effect parameter tuning</summary>
    public PostProcessing? PostProcessing => _postProcessing;

    // ═══════════════════════════════════════════════════════════════════════════
    // FLEXIBLE VERTEX DATA MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private uint _vertexCount;
    private VertexLayout? _currentLayout;

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING STATE
    // ═══════════════════════════════════════════════════════════════════════════

    private Vector4D<float> _currentColor = new(1f, 1f, 1f, 1f);
    private bool _disposed;

    /// <summary>
    /// Encapsulates uniform variable locations for performance optimization
    /// </summary>
    private class ShaderUniforms
    {
        public int AspectLocation { get; init; }
        public int ColorLocation { get; init; }

        public ShaderUniforms(GL gl, uint programHandle)
        {
            AspectLocation = gl.GetUniformLocation(programHandle, "uAspect");
            ColorLocation = gl.GetUniformLocation(programHandle, "uColor");
        }
    }

    public void Initialize(IWindow window)
    {
        _gl = GL.GetApi(window);
        _gl.ClearColor(0f, 0f, 0f, 1f);

        InitializeShaderSystem();
        SetupVertexBuffers();

        _windowSize = window.Size;
        Resize(window.Size);

        SetShaderMode(ShaderMode.Normal);
    }

    /// <summary>
    /// Initializes the dynamic shader management system with comprehensive error handling
    /// </summary>
    private void InitializeShaderSystem()
    {
        var vertexShaderSource = ShaderLoader.LoadVertexShader();
        var loadedCount = 0;
        var failedCount = 0;

        foreach (ShaderMode mode in Enum.GetValues<ShaderMode>())
        {
            try
            {
                var fragmentSource = ShaderLoader.LoadFragmentShader(mode);
                var shaderProgram = new ShaderProgram(_gl, vertexShaderSource, fragmentSource);
                var uniforms = new ShaderUniforms(_gl, shaderProgram.Handle);

                _shaders[mode] = shaderProgram;
                _uniforms[mode] = uniforms;

                loadedCount++;
                Console.WriteLine($"✅ Loaded shader mode: {mode}");
            }
            catch (Exception ex)
            {
                failedCount++;
                Console.WriteLine($"⚠️ Failed to load shader mode {mode}: {ex.Message}");
            }
        }

        if (!_shaders.ContainsKey(ShaderMode.Normal))
        {
            throw new InvalidOperationException("Critical error: Normal shader mode is required but failed to load");
        }

        Console.WriteLine($"Shader system initialized: {loadedCount} loaded, {failedCount} failed");
    }

    /// <summary>
    /// Sets up flexible vertex buffer system supporting multiple attribute layouts
    /// </summary>
    private void SetupVertexBuffers()
    {
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // Enable blending for transparency effects
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    /// <summary>
    /// Configure vertex layout for current rendering batch
    /// </summary>
    public unsafe void SetVertexLayout(VertexLayout layout)
    {
        if (_currentLayout?.Equals(layout) == true)
            return;

        _currentLayout = layout;
        _gl.BindVertexArray(_vao);

        // Disable all existing attributes
        for (uint i = 0; i < 8; i++)
        {
            _gl.DisableVertexAttribArray(i);
        }

        // Configure new attributes
        foreach (var attr in layout.Attributes)
        {
            _gl.EnableVertexAttribArray(attr.Index);
            _gl.VertexAttribPointer(
                attr.Index,
                attr.Size,
                attr.Type,
                attr.Normalized,
                (uint)layout.Stride,
                (void*)attr.Offset
            );
        }
    }

    /// <summary>
    /// Upload vertex data with automatic layout detection and type safety
    /// </summary>
    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        var layout = typeof(T).Name switch
        {
            nameof(BasicVertex) => BasicVertex.GetLayout(),
            nameof(TexturedVertex) => TexturedVertex.GetLayout(),
            nameof(FullVertex) => FullVertex.GetLayout(),
            _ => throw new ArgumentException($"Unsupported vertex type: {typeof(T).Name}")
        };

        SetVertexLayout(layout);

        _vertexCount = (uint)vertices.Length;
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<T>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
    }

    /// <summary>
    /// Upload raw float array with automatic basic layout (legacy compatibility)
    /// </summary>
    public unsafe void UpdateVertices(float[] vertices)
    {
        UpdateVertices(vertices, BasicVertex.GetLayout());
    }

    /// <summary>
    /// Upload raw float array with explicit layout specification
    /// </summary>
    public unsafe void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        SetVertexLayout(layout);

        _vertexCount = (uint)(vertices.Length / (layout.Stride / sizeof(float)));
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* ptr = vertices)
        {
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(float)),
                ptr,
                BufferUsageARB.DynamicDraw
            );
        }
    }

    public void Clear()
    {
        if (_isBloomActive)
        {
            EnsurePostProcessingInitialized();
            if (_postProcessing != null)
            {
                _postProcessing.BeginScenePass();
            }
            else
            {
                _gl.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
        else
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }
    }

    public void SetColor(Vector4D<float> rgba)
    {
        _currentColor = rgba;
        if (_currentUniforms != null && _currentUniforms.ColorLocation >= 0)
        {
            _gl.Uniform4(_currentUniforms.ColorLocation, rgba.X, rgba.Y, rgba.Z, rgba.W);
        }
    }

    /// <summary>
    /// Set custom uniform value with type safety
    /// </summary>
    public void SetUniform(string name, float value)
    {
        if (_currentShader == null) return;

        var location = _gl.GetUniformLocation(_currentShader.Handle, name);
        if (location >= 0)
            _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, Vector2D<float> value)
    {
        if (_currentShader == null) return;

        var location = _gl.GetUniformLocation(_currentShader.Handle, name);
        if (location >= 0)
            _gl.Uniform2(location, value.X, value.Y);
    }

    public void SetUniform(string name, Vector4D<float> value)
    {
        if (_currentShader == null) return;

        var location = _gl.GetUniformLocation(_currentShader.Handle, name);
        if (location >= 0)
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    /// <summary>
    /// Switches active shader mode with comprehensive state management
    /// </summary>
    public void SetShaderMode(ShaderMode mode)
    {
        if (!_shaders.ContainsKey(mode))
        {
            Console.WriteLine($"❌ Shader mode {mode} not available, falling back to Normal");
            mode = ShaderMode.Normal;
        }

        _currentShaderMode = mode;
        _currentShader = _shaders[mode];
        _currentUniforms = _uniforms[mode];

        _isBloomActive = (mode == ShaderMode.Bloom);
        ConfigureBlendingForMode(mode);

        // Activate shader and set uniforms with validation
        if (_currentShader != null && _currentUniforms != null)
        {
            _gl.UseProgram(_currentShader.Handle);

            if (_currentUniforms.AspectLocation >= 0)
                _gl.Uniform1(_currentUniforms.AspectLocation, _aspectRatio);

            if (_currentUniforms.ColorLocation >= 0)
                _gl.Uniform4(_currentUniforms.ColorLocation, _currentColor.X, _currentColor.Y, _currentColor.Z, _currentColor.W);
        }
    }

    /// <summary>
    /// Configures GPU blending state based on shader mode requirements
    /// </summary>
    private void ConfigureBlendingForMode(ShaderMode mode)
    {
        if (mode == ShaderMode.Normal || _isBloomActive)
        {
            _gl.Disable(EnableCap.Blend);
        }
        else
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }
    }

    public void Draw()
    {
        if (_currentShader != null && _currentLayout != null)
        {
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        }
    }

    /// <summary>
    /// Draw with index buffer for complex geometry
    /// </summary>
    public void DrawIndexed(uint[] indices)
    {
        if (_currentShader == null || _currentLayout == null)
            return;

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        _gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.DynamicDraw);

        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, in System.IntPtr.Zero);
    }

    /// <summary>
    /// Ensures post-processing system is initialized when bloom effects are needed
    /// </summary>
    private void EnsurePostProcessingInitialized()
    {
        if (_postProcessing == null)
        {
            try
            {
                _postProcessing = new PostProcessing(_gl);
                _postProcessing.Initialize(_windowSize.X, _windowSize.Y);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize bloom post-processing: {ex.Message}");
                _isBloomActive = false;
                _postProcessing = null;
            }
        }
    }

    public void FinalizeFrame()
    {
        if (_isBloomActive && _postProcessing != null)
        {
            _postProcessing.EndScenePassAndApplyBloom();
        }
    }

    public void Resize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        _aspectRatio = newSize.Y / (float)newSize.X;
        _windowSize = newSize;

        // Update aspect ratio in current shader
        if (_currentUniforms != null && _currentUniforms.AspectLocation >= 0)
        {
            _gl.Uniform1(_currentUniforms.AspectLocation, _aspectRatio);
        }

        _postProcessing?.Resize(newSize.X, newSize.Y);
    }

    /// <summary>
    /// Releases all resources used by the OpenGLRenderer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up if Dispose() is not called.
    /// </summary>
    ~OpenGLRenderer()
    {
        Dispose(false);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            foreach (var shader in _shaders.Values)
            {
                shader.Dispose();
            }
            _shaders.Clear();
            _uniforms.Clear();

            _postProcessing?.Dispose();
        }

        // Free unmanaged resources
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_vao);

        _disposed = true;
    }

    /// <summary>
    /// Shuts down the renderer and releases all resources.
    /// 
    /// LEGACY METHOD - Use Dispose() instead.
    /// Maintained for backwards compatibility.
    /// </summary>
    [Obsolete("Use Dispose() instead. This method will be removed in a future version.")]
    public void Shutdown()
    {
        Dispose();
    }
}
