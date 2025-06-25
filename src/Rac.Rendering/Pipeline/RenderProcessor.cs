// ═══════════════════════════════════════════════════════════════════════════════
// RENDERING PIPELINE PROCESSING PHASE
// ═══════════════════════════════════════════════════════════════════════════════
//
// Fast GPU rendering operations that happen every frame. This phase handles
// all the actual rendering commands without any asset loading, compilation,
// or file I/O operations.
//
// EDUCATIONAL ASPECTS:
// - GPU state management: Efficient handling of OpenGL state changes
// - Vertex data streaming: Dynamic vertex buffer updates
// - Draw call optimization: Minimizing state changes between draws
// - Memory management: Efficient vertex data handling

using Rac.Rendering.Camera;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Rac.Rendering.Pipeline;

/// <summary>
/// Handles fast GPU rendering operations during the main render loop.
/// 
/// RESPONSIBILITIES:
/// - Setting render state (shaders, uniforms, blending)
/// - Uploading vertex data to GPU
/// - Issuing draw calls
/// - Managing camera transformations
/// 
/// RESTRICTIONS:
/// - No asset loading or compilation
/// - No file I/O operations
/// - Must be used after preprocessing is complete
/// - Designed for per-frame performance
/// </summary>
public class RenderProcessor
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROCESSING STATE AND DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly GL _gl;
    private readonly RenderPreprocessor _preprocessor;
    
    // Current rendering state
    private ShaderMode _currentShaderMode = ShaderMode.Normal;
    private Vector4D<float> _currentColor = new(1f, 1f, 1f, 1f);
    private Matrix4X4<float> _currentCameraMatrix = Matrix4X4<float>.Identity;
    private PrimitiveType _currentPrimitiveType = PrimitiveType.Triangles;
    private float _aspectRatio = 1f;
    
    // Vertex data state
    private uint _vertexCount;
    private VertexLayout? _currentLayout;
    
    /// <summary>
    /// Creates a new render processor.
    /// </summary>
    /// <param name="gl">OpenGL context (must be valid and current)</param>
    /// <param name="preprocessor">Preprocessor that has completed successfully</param>
    /// <exception cref="ArgumentNullException">When parameters are null</exception>
    /// <exception cref="InvalidOperationException">When preprocessor hasn't completed</exception>
    public RenderProcessor(GL gl, RenderPreprocessor preprocessor)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _preprocessor = preprocessor ?? throw new ArgumentNullException(nameof(preprocessor));
        
        if (!_preprocessor.IsPreprocessed)
            throw new InvalidOperationException("Preprocessor must complete before creating processor");
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // FRAME LIFECYCLE OPERATIONS
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Clears the framebuffer and prepares for a new frame.
    /// </summary>
    /// <param name="clearColor">Color to clear the framebuffer with</param>
    public void Clear(Vector4D<float> clearColor)
    {
        _gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    
    /// <summary>
    /// Issues a draw call with the current vertex data and state.
    /// </summary>
    public void Draw()
    {
        if (_vertexCount == 0)
        {
            Console.WriteLine("Warning: Draw called with no vertex data");
            return;
        }
        
        // Ensure we have a valid shader
        var currentShader = GetCurrentShader();
        if (currentShader == null)
        {
            Console.WriteLine($"Warning: No shader available for mode {_currentShaderMode}");
            return;
        }
        
        // Update uniforms with current state
        UpdateShaderUniforms();
        
        // Bind vertex array and issue draw call
        _gl.BindVertexArray(_preprocessor.VertexArrayObject);
        _gl.DrawArrays(_currentPrimitiveType, 0, _vertexCount);
    }
    
    /// <summary>
    /// Draw with index buffer for complex geometry.
    /// </summary>
    /// <param name="indices">Index array for indexed drawing</param>
    public void DrawIndexed(uint[] indices)
    {
        if (indices.Length == 0)
        {
            Console.WriteLine("Warning: DrawIndexed called with empty indices");
            return;
        }
        
        // Upload indices to element buffer
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _preprocessor.ElementBufferObject);
        _gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.DynamicDraw);
        
        // Ensure we have a valid shader
        var currentShader = GetCurrentShader();
        if (currentShader == null)
        {
            Console.WriteLine($"Warning: No shader available for mode {_currentShaderMode}");
            return;
        }
        
        // Update uniforms and draw
        UpdateShaderUniforms();
        _gl.BindVertexArray(_preprocessor.VertexArrayObject);
        var indexCount = (uint)indices.Length;
        var elementType = DrawElementsType.UnsignedInt;
        _gl.DrawElements(_currentPrimitiveType, indexCount, elementType, 0);
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // STATE MANAGEMENT
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Sets the current rendering color.
    /// </summary>
    public void SetColor(Vector4D<float> color)
    {
        _currentColor = color;
    }
    
    /// <summary>
    /// Sets the camera transformation matrix.
    /// </summary>
    public void SetCameraMatrix(Matrix4X4<float> cameraMatrix)
    {
        _currentCameraMatrix = cameraMatrix;
    }
    
    /// <summary>
    /// Sets the active camera for subsequent rendering operations.
    /// </summary>
    public void SetActiveCamera(ICamera camera, int viewportWidth, int viewportHeight)
    {
        ArgumentNullException.ThrowIfNull(camera);
        
        camera.UpdateMatrices(viewportWidth, viewportHeight);
        _currentCameraMatrix = camera.CombinedMatrix;
        _aspectRatio = viewportHeight / (float)viewportWidth;
    }
    
    /// <summary>
    /// Switches active shader mode with state management.
    /// </summary>
    public void SetShaderMode(ShaderMode mode)
    {
        if (_currentShaderMode == mode) return;
        
        _currentShaderMode = mode;
        
        // Configure blending for the new mode
        ConfigureBlendingForMode(mode);
        
        // Use the shader program
        var shader = GetCurrentShader();
        shader?.Use();
    }
    
    /// <summary>
    /// Sets the primitive type for subsequent draw calls.
    /// </summary>
    public void SetPrimitiveType(PrimitiveType primitiveType)
    {
        _currentPrimitiveType = primitiveType;
    }
    
    /// <summary>
    /// Updates the aspect ratio (typically called on window resize).
    /// </summary>
    public void SetAspectRatio(float aspectRatio)
    {
        _aspectRatio = aspectRatio;
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // VERTEX DATA MANAGEMENT
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Uploads vertex positions (2D) into the VBO.
    /// </summary>
    public void UpdateVertices(float[] vertices)
    {
        if (vertices.Length % 2 != 0)
        {
            throw new ArgumentException("Vertex array length must be even for 2D positions", nameof(vertices));
        }
        
        _vertexCount = (uint)(vertices.Length / 2);
        _currentLayout = null; // Legacy mode
        
        // Upload to GPU
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _preprocessor.VertexBufferObject);
        _gl.BufferData<float>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
        
        // Configure vertex attributes for simple 2D positions
        _gl.BindVertexArray(_preprocessor.VertexArrayObject);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
    }
    
    /// <summary>
    /// Upload vertex data with automatic layout detection and type safety.
    /// </summary>
    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        if (vertices.Length == 0)
        {
            _vertexCount = 0;
            return;
        }
        
        // Get layout information from vertex type
        var layout = GetVertexLayout<T>();
        _currentLayout = layout;
        _vertexCount = (uint)vertices.Length;
        
        // Upload to GPU
        unsafe
        {
            fixed (T* ptr = vertices)
            {
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _preprocessor.VertexBufferObject);
                _gl.BufferData<T>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
            }
        }
        
        // Configure vertex attributes
        ConfigureVertexAttributes(layout);
    }
    
    /// <summary>
    /// Upload raw float array with explicit layout specification.
    /// </summary>
    public void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        
        if (vertices.Length % (layout.Stride / sizeof(float)) != 0)
        {
            throw new ArgumentException($"Vertex array length must be divisible by stride ({layout.Stride / sizeof(float)})", nameof(vertices));
        }
        
        _vertexCount = (uint)(vertices.Length / (layout.Stride / sizeof(float)));
        _currentLayout = layout;
        
        // Upload to GPU
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _preprocessor.VertexBufferObject);
        _gl.BufferData<float>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
        
        // Configure vertex attributes
        ConfigureVertexAttributes(layout);
    }
    
    // ───────────────────────────────────────────────────────────────────────────
    // INTERNAL HELPER METHODS
    // ───────────────────────────────────────────────────────────────────────────
    
    private ShaderProgram? GetCurrentShader()
    {
        _preprocessor.Shaders.TryGetValue(_currentShaderMode, out var shader);
        return shader;
    }
    
    private void UpdateShaderUniforms()
    {
        if (!_preprocessor.Uniforms.TryGetValue(_currentShaderMode, out var uniforms))
            return;
        
        // Update aspect ratio uniform
        if (uniforms.AspectLocation >= 0)
            _gl.Uniform1(uniforms.AspectLocation, _aspectRatio);
        
        // Update color uniform
        if (uniforms.ColorLocation >= 0)
            _gl.Uniform4(uniforms.ColorLocation, _currentColor.X, _currentColor.Y, _currentColor.Z, _currentColor.W);
        
        // Update camera matrix uniform
        if (uniforms.CameraMatrixLocation >= 0)
        {
            var matrixArray = new float[]
            {
                _currentCameraMatrix.M11, _currentCameraMatrix.M12, _currentCameraMatrix.M13, _currentCameraMatrix.M14,
                _currentCameraMatrix.M21, _currentCameraMatrix.M22, _currentCameraMatrix.M23, _currentCameraMatrix.M24,
                _currentCameraMatrix.M31, _currentCameraMatrix.M32, _currentCameraMatrix.M33, _currentCameraMatrix.M34,
                _currentCameraMatrix.M41, _currentCameraMatrix.M42, _currentCameraMatrix.M43, _currentCameraMatrix.M44
            };
            _gl.UniformMatrix4(uniforms.CameraMatrixLocation, 1, false, matrixArray);
        }
    }
    
    private void ConfigureBlendingForMode(ShaderMode mode)
    {
        switch (mode)
        {
            case ShaderMode.Normal:
                _gl.Disable(EnableCap.Blend);
                break;
            case ShaderMode.SoftGlow:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                break;
            case ShaderMode.Bloom:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                break;
            case ShaderMode.DebugUV:
                // Disable blending for clear UV coordinate visualization
                _gl.Disable(EnableCap.Blend);
                break;
            default:
                _gl.Disable(EnableCap.Blend);
                break;
        }
    }
    
    private VertexLayout GetVertexLayout<T>() where T : unmanaged
    {
        // Use reflection to get layout from type
        var type = typeof(T);
        var method = type.GetMethod("GetLayout", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        if (method?.Invoke(null, null) is VertexLayout layout)
            return layout;
        
        throw new InvalidOperationException($"Vertex type {type.Name} must implement a static GetLayout() method");
    }
    
    private unsafe void ConfigureVertexAttributes(VertexLayout layout)
    {
        _gl.BindVertexArray(_preprocessor.VertexArrayObject);
        
        // Disable all attributes first
        for (uint i = 0; i < 16; i++)
        {
            _gl.DisableVertexAttribArray(i);
        }
        
        // Configure attributes based on layout
        foreach (var attribute in layout.Attributes)
        {
            _gl.EnableVertexAttribArray(attribute.Index);
            _gl.VertexAttribPointer(
                attribute.Index,
                attribute.Size,
                attribute.Type,
                attribute.Normalized,
                (uint)layout.Stride,
                (void*)attribute.Offset
            );
        }
    }
}