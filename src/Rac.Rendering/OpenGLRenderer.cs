// File: src/Engine/Rendering/OpenGLRenderer.cs

using Rac.Rendering.Shader;
using Rac.Rendering.VFX;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <inheritdoc />
public class OpenGLRenderer : IRenderer
{


    private int _aspectLocation;
    private float _aspectRatio;
    private int _colorLocation;
    private GL _gl = null!;
    private uint _programHandle;
    private ShaderProgram _shader = null!;

    // Multiple shader support
    private ShaderProgram _normalShader = null!;
    private ShaderProgram _softGlowShader = null!;
    private ShaderProgram _bloomShader = null!;
    private ShaderMode _currentShaderMode = ShaderMode.Normal;
    
    // Uniform locations for each shader
    private int _normalAspectLocation;
    private int _normalColorLocation;
    private int _softGlowAspectLocation;
    private int _softGlowColorLocation;
    private int _bloomAspectLocation;
    private int _bloomColorLocation;

    // Post-processing
    private PostProcessing? _postProcessing;
    private bool _isBloomActive = false;

    private uint _vao;
    private uint _vbo;
    private uint _vertexCount;

    public void Initialize(IWindow window)
    {
        _gl = GL.GetApi(window);
        _gl.ClearColor(0f, 0f, 0f, 1f);

        // Create all shader programs using ShaderLoader
        var vertexShaderSource = ShaderLoader.LoadVertexShader();
        _normalShader = new ShaderProgram(_gl, vertexShaderSource, ShaderLoader.LoadFragmentShader(ShaderMode.Normal));
        _softGlowShader = new ShaderProgram(_gl, vertexShaderSource, ShaderLoader.LoadFragmentShader(ShaderMode.SoftGlow));
        _bloomShader = new ShaderProgram(_gl, vertexShaderSource, ShaderLoader.LoadFragmentShader(ShaderMode.Bloom));

        // Set initial shader and program handle
        _shader = _normalShader;
        _programHandle = _shader.Handle;

        // Get uniform locations for all shaders
        _normalAspectLocation = _gl.GetUniformLocation(_normalShader.Handle, "uAspect");
        _normalColorLocation = _gl.GetUniformLocation(_normalShader.Handle, "uColor");
        _softGlowAspectLocation = _gl.GetUniformLocation(_softGlowShader.Handle, "uAspect");
        _softGlowColorLocation = _gl.GetUniformLocation(_softGlowShader.Handle, "uColor");
        _bloomAspectLocation = _gl.GetUniformLocation(_bloomShader.Handle, "uAspect");
        _bloomColorLocation = _gl.GetUniformLocation(_bloomShader.Handle, "uColor");

        // Set legacy uniform locations for backward compatibility
        _aspectLocation = _normalAspectLocation;
        _colorLocation = _normalColorLocation;

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(
            /* index     */0,
            /* size      */2,
            /* type      */VertexAttribPointerType.Float,
            /* normalized*/false,
            /* stride    */2 * sizeof(float),
            /* pointer   */IntPtr.Zero
        );

        // Initialize post-processing
        _postProcessing = new PostProcessing(_gl);
        _postProcessing.Initialize(window.Size.X, window.Size.Y);

        Resize(window.Size);
    }

    public void Clear()
    {
        // If bloom is active, begin scene pass to render to framebuffer
        if (_isBloomActive && _postProcessing != null)
        {
            _postProcessing.BeginScenePass();
        }
        else
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }
        
        // Reset to normal shader mode at the start of each frame
        if (_currentShaderMode != ShaderMode.Normal)
        {
            SetShaderMode(ShaderMode.Normal);
        }
    }

    public void SetColor(Vector4D<float> rgba)
    {
        // Use the appropriate color location based on current shader mode
        var colorLocation = _currentShaderMode switch
        {
            ShaderMode.Normal => _normalColorLocation,
            ShaderMode.SoftGlow => _softGlowColorLocation,
            ShaderMode.Bloom => _bloomColorLocation,
            _ => _normalColorLocation
        };
        
        _gl.Uniform4(colorLocation, rgba.X, rgba.Y, rgba.Z, rgba.W);
    }

    public void SetShaderMode(ShaderMode mode)
    {
        _currentShaderMode = mode;
        
        // Determine if bloom post-processing should be active
        _isBloomActive = (mode == ShaderMode.Bloom);
        
        // Configure blending based on shader mode
        if (mode == ShaderMode.Normal || _isBloomActive)
        {
            _gl.Disable(EnableCap.Blend);
        }
        else
        {
            // Enable additive blending for SoftGlow effects
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }
        
        // Update current shader and program handle
        // For bloom mode, we use the simplified bloom shader for scene rendering
        _shader = mode switch
        {
            ShaderMode.Normal => _normalShader,
            ShaderMode.SoftGlow => _softGlowShader,
            ShaderMode.Bloom => _bloomShader, // This now uses the simplified version
            _ => _normalShader
        };
        
        _programHandle = _shader.Handle;
        
        // Update legacy uniform locations for backward compatibility
        _aspectLocation = mode switch
        {
            ShaderMode.Normal => _normalAspectLocation,
            ShaderMode.SoftGlow => _softGlowAspectLocation,
            ShaderMode.Bloom => _bloomAspectLocation,
            _ => _normalAspectLocation
        };
        
        _colorLocation = mode switch
        {
            ShaderMode.Normal => _normalColorLocation,
            ShaderMode.SoftGlow => _softGlowColorLocation,
            ShaderMode.Bloom => _bloomColorLocation,
            _ => _normalColorLocation
        };
    }

    public unsafe void UpdateVertices(float[] vertices)
    {
        _vertexCount = (uint)(vertices.Length / 2);
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

    public void Draw()
    {
        _gl.UseProgram(_programHandle);
        _gl.Uniform1(_aspectLocation, _aspectRatio);
        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
    }

    /// <summary>
    /// Finalizes the frame, applying bloom post-processing if active.
    /// </summary>
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
        
        // Resize post-processing framebuffers
        _postProcessing?.Resize(newSize.X, newSize.Y);
    }

    public void Shutdown()
    {
        _normalShader.Dispose();
        _softGlowShader.Dispose();
        _bloomShader.Dispose();
        _postProcessing?.Shutdown();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}
