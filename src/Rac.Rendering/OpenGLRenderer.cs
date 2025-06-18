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

using System;
using System.Collections.Generic;
using System.Linq;

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
    // CAMERA MATRIX SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════
    
    private Matrix4X4<float> _currentCameraMatrix = Matrix4X4<float>.Identity;

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
        public int CameraMatrixLocation { get; init; }

        public ShaderUniforms(GL gl, uint programHandle)
        {
            AspectLocation = gl.GetUniformLocation(programHandle, "uAspect");
            ColorLocation = gl.GetUniformLocation(programHandle, "uColor");
            CameraMatrixLocation = gl.GetUniformLocation(programHandle, "uCameraMatrix");
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
    /// Convert BasicVertex array to FullVertex array with default color (1,1,1,1)
    /// </summary>
    private FullVertex[] ConvertBasicToFull(BasicVertex[] vertices)
    {
        var defaultColor = new Vector4D<float>(1f, 1f, 1f, 1f);
        var defaultTexCoord = new Vector2D<float>(0f, 0f);

        return vertices.Select(v => new FullVertex(v.Position, defaultTexCoord, defaultColor)).ToArray();
    }

    /// <summary>
    /// Convert TexturedVertex array to FullVertex array with default color (1,1,1,1)
    /// </summary>
    private FullVertex[] ConvertTexturedToFull(TexturedVertex[] vertices)
    {
        var defaultColor = new Vector4D<float>(1f, 1f, 1f, 1f);

        return vertices.Select(v => new FullVertex(v.Position, v.TexCoord, defaultColor)).ToArray();
    }

    /// <summary>
    /// Convert float array to FullVertex array with default color (1,1,1,1) based on layout
    /// </summary>
    private FullVertex[] ConvertFloatArrayToFull(float[] vertices, VertexLayout layout)
    {
        var defaultColor = new Vector4D<float>(1f, 1f, 1f, 1f);
        var defaultTexCoord = new Vector2D<float>(0f, 0f);

        var floatsPerVertex = layout.Stride / sizeof(float);
        var vertexCount = vertices.Length / floatsPerVertex;
        var result = new FullVertex[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            var offset = i * floatsPerVertex;

            // Position is always first (2 floats)
            var position = new Vector2D<float>(vertices[offset], vertices[offset + 1]);

            // TexCoord depends on layout
            var texCoord = floatsPerVertex >= 4
                ? new Vector2D<float>(vertices[offset + 2], vertices[offset + 3])
                : defaultTexCoord;

            // Color always defaults to (1,1,1,1) for float arrays
            result[i] = new FullVertex(position, texCoord, defaultColor);
        }

        return result;
    }

    /// <summary>
    /// Upload vertex data with automatic layout detection and type safety.
    /// Always converts to FullVertex format with default color (1,1,1,1) if not provided.
    /// </summary>
    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        // Always use FullVertex layout to ensure color data is present
        SetVertexLayout(FullVertex.GetLayout());

        // Convert all vertex types to FullVertex format with default color
        var fullVertices = typeof(T).Name switch
        {
            nameof(BasicVertex) => ConvertBasicToFull(vertices.Cast<BasicVertex>().ToArray()),
            nameof(TexturedVertex) => ConvertTexturedToFull(vertices.Cast<TexturedVertex>().ToArray()),
            nameof(FullVertex) => vertices.Cast<FullVertex>().ToArray(),
            _ => throw new ArgumentException($"Unsupported vertex type: {typeof(T).Name}")
        };

        _vertexCount = (uint)fullVertices.Length;
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<FullVertex>(BufferTargetARB.ArrayBuffer, fullVertices, BufferUsageARB.DynamicDraw);
    }

    /// <summary>
    /// Upload raw float array with automatic basic layout (legacy compatibility).
    /// Automatically converts to FullVertex format with default color (1,1,1,1).
    /// </summary>
    public unsafe void UpdateVertices(float[] vertices)
    {
        UpdateVertices(vertices, BasicVertex.GetLayout());
    }

    /// <summary>
    /// Upload raw float array with explicit layout specification.
    /// Automatically converts to FullVertex format with default color (1,1,1,1).
    /// </summary>
    public unsafe void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        // Always use FullVertex layout to ensure color data is present
        SetVertexLayout(FullVertex.GetLayout());

        // Convert float array to FullVertex format based on the provided layout
        var fullVertices = ConvertFloatArrayToFull(vertices, layout);

        _vertexCount = (uint)fullVertices.Length;
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<FullVertex>(BufferTargetARB.ArrayBuffer, fullVertices, BufferUsageARB.DynamicDraw);
    }

    public void Clear()
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // DEFERRED POST-PROCESSING INITIALIZATION AND CLEANUP
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // PostProcessing must be initialized at a safe point in the render pipeline,
        // not during active rendering. Clear() is called at the start of each frame
        // before any drawing, making it the ideal place for deferred initialization.
        //
        // Similarly, when bloom is disabled, we need to clean up PostProcessing 
        // resources to prevent continued bloom rendering and resource leaks.

        // CLEANUP: Dispose PostProcessing when bloom is turned off
        if (!_isBloomActive && _postProcessing != null)
        {
            _postProcessing.Dispose();
            _postProcessing = null;
            
            // Ensure we're rendering to default framebuffer when bloom is disabled
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        // INITIALIZATION: Create PostProcessing when bloom is turned on
        if (_isBloomActive && _postProcessing == null)
        {
            EnsurePostProcessingInitialized();

            // If initialization failed, disable bloom
            if (_postProcessing == null)
            {
                _isBloomActive = false;
                _currentShaderMode = ShaderMode.Normal;
                if (_shaders.ContainsKey(ShaderMode.Normal))
                {
                    _currentShader = _shaders[ShaderMode.Normal];
                    _currentUniforms = _uniforms[ShaderMode.Normal];
                }
            }
        }

        if (_isBloomActive && _postProcessing != null)
        {
            _postProcessing.BeginScenePass();
        }
        else
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }
    }

    /// <summary>
    /// Sets the current rendering color with full HDR (High Dynamic Range) support.
    ///
    /// HDR COLOR USAGE:
    /// - Standard colors: 0.0-1.0 range for normal rendering modes
    /// - HDR colors: Values > 1.0 for dramatic bloom effects when bloom mode is active
    /// - Example HDR colors for bloom:
    ///   * Red bloom: (2.5f, 0.3f, 0.3f, 1.0f) - Creates intense red glow
    ///   * White bloom: (2.0f, 2.0f, 2.0f, 1.0f) - Creates bright white glow
    ///   * Blue bloom: (0.3f, 0.3f, 2.5f, 1.0f) - Creates intense blue glow
    ///
    /// BACKWARD COMPATIBILITY:
    /// - Non-bloom shaders automatically handle HDR colors through tone mapping
    /// - Standard LDR colors (0.0-1.0) work identically in all shader modes
    /// - Alpha channel should typically remain 1.0 for bloom effects
    /// </summary>
    /// <param name="rgba">RGBA color vector. RGB components can exceed 1.0 for HDR bloom effects.</param>
    public void SetColor(Vector4D<float> rgba)
    {
        _currentColor = rgba;

        // HDR colors (values > 1.0) are preserved and passed directly to shaders
        // The bloom shader will process these through enhanceHDRColor() and dynamic range scaling
        // Non-bloom shaders handle HDR values through their respective tone mapping functions
        if (_currentUniforms != null && _currentUniforms.ColorLocation >= 0)
        {
            _gl.Uniform4(_currentUniforms.ColorLocation, rgba.X, rgba.Y, rgba.Z, rgba.W);
        }
    }

    /// <summary>
    /// Set camera transformation matrix for vertex transformations.
    /// Enables 2D camera system with position, zoom, and rotation support.
    /// </summary>
    /// <param name="cameraMatrix">Combined view-projection matrix from camera system</param>
    public void SetCameraMatrix(Matrix4X4<float> cameraMatrix)
    {
        _currentCameraMatrix = cameraMatrix;

        // Upload camera matrix to current shader if available
        if (_currentUniforms != null && _currentUniforms.CameraMatrixLocation >= 0)
        {
            // Convert Matrix4X4<float> to float array for OpenGL
            var matrixArray = new float[]
            {
                cameraMatrix.M11, cameraMatrix.M12, cameraMatrix.M13, cameraMatrix.M14,
                cameraMatrix.M21, cameraMatrix.M22, cameraMatrix.M23, cameraMatrix.M24,
                cameraMatrix.M31, cameraMatrix.M32, cameraMatrix.M33, cameraMatrix.M34,
                cameraMatrix.M41, cameraMatrix.M42, cameraMatrix.M43, cameraMatrix.M44
            };
            _gl.UniformMatrix4(_currentUniforms.CameraMatrixLocation, 1, false, matrixArray);
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
    /// Switches active shader mode with comprehensive state management.
    ///
    /// DEFERRED INITIALIZATION PATTERN:
    /// PostProcessing initialization is deferred to Clear() to prevent crashes.
    /// This method may be called during active rendering (e.g., from DrawSpecies),
    /// and creating framebuffers mid-render can cause OpenGL state conflicts.
    /// </summary>
    public void SetShaderMode(ShaderMode mode)
    {
        // Safety check: Don't allow bloom mode if window size is not set (renderer not fully initialized)
        if (mode == ShaderMode.Bloom && (_windowSize.X <= 0 || _windowSize.Y <= 0))
        {
            mode = ShaderMode.Normal;
        }

        if (!_shaders.ContainsKey(mode))
        {
            mode = ShaderMode.Normal;
        }

        // CRITICAL FIX: Defer PostProcessing initialization to prevent crashes
        // We only set the mode here, actual initialization happens at frame boundary
        _currentShaderMode = mode;
        _currentShader = _shaders[mode];
        _currentUniforms = _uniforms[mode];

        bool wasBloomActive = _isBloomActive;
        _isBloomActive = (mode == ShaderMode.Bloom);

        // Mark that we need to initialize PostProcessing, but don't do it now
        // This prevents crashes from initializing during active rendering
        if (_isBloomActive && !wasBloomActive && _postProcessing == null)
        {
            // PostProcessing will be initialized in Clear() at the start of next frame
        }

        ConfigureBlendingForMode(mode);

        // Activate shader and set uniforms with validation
        if (_currentShader != null && _currentUniforms != null)
        {
            _gl.UseProgram(_currentShader.Handle);

            if (_currentUniforms.AspectLocation >= 0)
                _gl.Uniform1(_currentUniforms.AspectLocation, _aspectRatio);

            if (_currentUniforms.ColorLocation >= 0)
                _gl.Uniform4(_currentUniforms.ColorLocation, _currentColor.X, _currentColor.Y, _currentColor.Z, _currentColor.W);

            if (_currentUniforms.CameraMatrixLocation >= 0)
            {
                var matrixArray = new float[]
                {
                    _currentCameraMatrix.M11, _currentCameraMatrix.M12, _currentCameraMatrix.M13, _currentCameraMatrix.M14,
                    _currentCameraMatrix.M21, _currentCameraMatrix.M22, _currentCameraMatrix.M23, _currentCameraMatrix.M24,
                    _currentCameraMatrix.M31, _currentCameraMatrix.M32, _currentCameraMatrix.M33, _currentCameraMatrix.M34,
                    _currentCameraMatrix.M41, _currentCameraMatrix.M42, _currentCameraMatrix.M43, _currentCameraMatrix.M44
                };
                _gl.UniformMatrix4(_currentUniforms.CameraMatrixLocation, 1, false, matrixArray);
            }
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
        
        // CRITICAL FIX: Use null pointer instead of "in System.IntPtr.Zero" to prevent crashes.
        // Taking the address of IntPtr.Zero can cause undefined behavior similar to issue #69.
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, null);
        }
    }

    /// <summary>
    /// Validates that all required post-processing shader files exist and are readable,
    /// and that the OpenGL context supports the required version and extensions
    /// </summary>
    /// <returns>True if all required shaders and OpenGL capabilities are available, false otherwise</returns>
    private bool ValidatePostProcessingShaders()
    {
        // Step 1: Validate OpenGL version and extensions
        Console.WriteLine("Validating OpenGL capabilities for post-processing...");

        if (!ValidateOpenGLVersion())
        {
            Console.WriteLine("Error: OpenGL version validation failed");
            return false;
        }

        if (!ValidateOpenGLExtensions())
        {
            Console.WriteLine("Error: Required OpenGL extensions not available");
            return false;
        }

        Console.WriteLine("✓ OpenGL capabilities validated successfully");

        // Step 2: Validate shader files
        var requiredShaders = new[]
        {
            "fullscreen_quad.vert",
            "brightness_extract.frag",
            "gaussian_blur.frag",
            "bloom_composite.frag"
        };

        Console.WriteLine("Validating post-processing shader files...");

        foreach (var shaderFile in requiredShaders)
        {
            try
            {
                var shaderSource = Shader.ShaderLoader.LoadShaderFromFile(shaderFile);
                if (string.IsNullOrEmpty(shaderSource))
                {
                    Console.WriteLine($"Error: Shader file '{shaderFile}' is empty or invalid");
                    return false;
                }
                Console.WriteLine($"✓ Shader file '{shaderFile}' validated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Failed to load shader file '{shaderFile}': {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("✓ All post-processing requirements validated successfully");
        return true;
    }

    /// <summary>
    /// Validates that the OpenGL version is 3.3 or higher, which is required for post-processing
    /// </summary>
    /// <returns>True if OpenGL version is supported, false otherwise</returns>
    private bool ValidateOpenGLVersion()
    {
        try
        {
            string versionString;
            unsafe
            {
                var versionPtr = _gl.GetString(StringName.Version);
                versionString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)versionPtr) ?? "";
            }

            if (string.IsNullOrEmpty(versionString))
            {
                Console.WriteLine("Error: Unable to query OpenGL version");
                return false;
            }

            Console.WriteLine($"OpenGL Version: {versionString}");

            // Parse version string - format is typically "MAJOR.MINOR.PATCH ..."
            var parts = versionString.Split(' ')[0].Split('.');
            if (parts.Length < 2)
            {
                Console.WriteLine("Error: Invalid OpenGL version format");
                return false;
            }

            if (!int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
            {
                Console.WriteLine("Error: Unable to parse OpenGL version numbers");
                return false;
            }

            // Check for OpenGL 3.3 or higher
            var version = new Version(major, minor);
            var requiredVersion = new Version(3, 3);

            if (version < requiredVersion)
            {
                Console.WriteLine($"Error: Post-processing requires OpenGL {requiredVersion} or higher, but only {version} is available");
                Console.WriteLine("Please update your graphics drivers or use a newer graphics card that supports OpenGL 3.3+");
                return false;
            }

            Console.WriteLine($"✓ OpenGL version {version} meets minimum requirement ({requiredVersion})");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to validate OpenGL version: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validates that the required OpenGL extensions are available for post-processing
    /// </summary>
    /// <returns>True if all required extensions are supported, false otherwise</returns>
    private bool ValidateOpenGLExtensions()
    {
        var requiredExtensions = new[]
        {
            "GL_ARB_framebuffer_object",
            "GL_ARB_texture_float"
        };

        try
        {
            // Get number of extensions
            _gl.GetInteger(GetPName.NumExtensions, out var numExtensions);
            var availableExtensions = new HashSet<string>();

            // Collect all available extensions
            for (var i = 0; i < numExtensions; i++)
            {
                string extension;
                unsafe
                {
                    var extensionPtr = _gl.GetString(StringName.Extensions, (uint)i);
                    extension = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)extensionPtr) ?? "";
                }

                if (!string.IsNullOrEmpty(extension))
                {
                    availableExtensions.Add(extension);
                }
            }

            Console.WriteLine($"Found {availableExtensions.Count} OpenGL extensions");

            // Check each required extension
            foreach (var requiredExtension in requiredExtensions)
            {
                if (availableExtensions.Contains(requiredExtension))
                {
                    Console.WriteLine($"✓ Extension {requiredExtension} is available");
                }
                else
                {
                    Console.WriteLine($"Error: Required extension {requiredExtension} is not available");
                    Console.WriteLine("This extension is needed for framebuffer objects and floating-point textures in post-processing");
                    Console.WriteLine("Please update your graphics drivers or use a newer graphics card");
                    return false;
                }
            }

            Console.WriteLine("✓ All required OpenGL extensions are available");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to validate OpenGL extensions: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initializes the post-processing system with comprehensive error handling and diagnostics
    /// </summary>
    /// <returns>True if initialization succeeded, false otherwise</returns>
    private bool InitializePostProcessing()
    {
        Console.WriteLine("Initializing post-processing system...");

        // Step 1: Validate OpenGL context state
        Console.WriteLine("Validating OpenGL context state...");
        try
        {
            var error = _gl.GetError();
            if (error != GLEnum.NoError)
            {
                Console.WriteLine($"Error: OpenGL context has existing error state: {error}");
                return false;
            }
            Console.WriteLine("✓ OpenGL context state is clean");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to check OpenGL context state: {ex.Message}");
            return false;
        }

        // Step 2: Validate required shader files
        if (!ValidatePostProcessingShaders())
        {
            Console.WriteLine("Error: Post-processing shader validation failed");
            return false;
        }

        // Step 3: Create and initialize PostProcessing instance
        Console.WriteLine("Creating PostProcessing instance...");
        try
        {
            _postProcessing = new PostProcessing(_gl);
            Console.WriteLine("✓ PostProcessing instance created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to create PostProcessing instance: {ex.Message}");
            _postProcessing = null;
            return false;
        }

        // Step 4: Initialize the post-processing pipeline
        Console.WriteLine($"Initializing post-processing pipeline (resolution: {_windowSize.X}x{_windowSize.Y})...");
        try
        {
            _postProcessing.Initialize(_windowSize.X, _windowSize.Y);
            Console.WriteLine("✓ Post-processing pipeline initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to initialize post-processing pipeline: {ex.Message}");
            _postProcessing?.Dispose();
            _postProcessing = null;
            return false;
        }
    }

    /// <summary>
    /// Ensures post-processing system is initialized when bloom effects are needed
    /// </summary>
    private void EnsurePostProcessingInitialized()
    {
        if (_postProcessing == null)
        {
            Console.WriteLine("Post-processing system not initialized, attempting initialization...");

            if (!InitializePostProcessing())
            {
                Console.WriteLine("Warning: Post-processing initialization failed - falling back to non-bloom rendering");
                _isBloomActive = false;
                _postProcessing = null;
            }
            else
            {
                Console.WriteLine("✓ Post-processing system ready for bloom effects");
            }
        }
    }

    public void FinalizeFrame()
    {
        try
        {
            if (_isBloomActive && _postProcessing != null)
            {
                _postProcessing.EndScenePassAndApplyBloom();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OpenGLRenderer.FinalizeFrame: Failed during frame finalization: {ex.Message}");

            // Disable bloom to prevent further crashes and enable graceful fallback
            _isBloomActive = false;
            _postProcessing?.Dispose();
            _postProcessing = null;

            // Instead of throwing, try to recover by clearing any OpenGL errors
            // and ensuring the framebuffer is reset to screen (framebuffer 0)
            try
            {
                // Clear any accumulated OpenGL errors
                while (_gl.GetError() != GLEnum.NoError) { }
                
                // Ensure we're rendering to the default framebuffer (screen)
                _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                _gl.Viewport(0, 0, (uint)_windowSize.X, (uint)_windowSize.Y);
                
                Console.WriteLine("✓ Gracefully recovered from bloom failure - continuing with normal rendering");
            }
            catch (Exception recoveryEx)
            {
                Console.WriteLine($"❌ Failed to recover from bloom failure: {recoveryEx.Message}");
                // Only throw if recovery also fails
                throw;
            }
        }
    }

    public void Resize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        _aspectRatio = newSize.Y / (float)newSize.X;
        _windowSize = newSize;

        // Update uniforms in current shader
        if (_currentUniforms != null)
        {
            if (_currentUniforms.AspectLocation >= 0)
            {
                _gl.Uniform1(_currentUniforms.AspectLocation, _aspectRatio);
            }

            if (_currentUniforms.CameraMatrixLocation >= 0)
            {
                var matrixArray = new float[]
                {
                    _currentCameraMatrix.M11, _currentCameraMatrix.M12, _currentCameraMatrix.M13, _currentCameraMatrix.M14,
                    _currentCameraMatrix.M21, _currentCameraMatrix.M22, _currentCameraMatrix.M23, _currentCameraMatrix.M24,
                    _currentCameraMatrix.M31, _currentCameraMatrix.M32, _currentCameraMatrix.M33, _currentCameraMatrix.M34,
                    _currentCameraMatrix.M41, _currentCameraMatrix.M42, _currentCameraMatrix.M43, _currentCameraMatrix.M44
                };
                _gl.UniformMatrix4(_currentUniforms.CameraMatrixLocation, 1, false, matrixArray);
            }
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
