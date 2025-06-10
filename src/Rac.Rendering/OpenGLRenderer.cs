// File: src/Engine/Rendering/OpenGLRenderer.cs

using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <inheritdoc />
public class OpenGLRenderer : IRenderer
{
    private const string VertexShaderSource =
        @"#version 330 core
layout(location = 0) in vec2 position;
uniform float uAspect;
void main()
{
    gl_Position = vec4(position.x * uAspect, position.y, 0.0, 1.0);
}";

    private const string FragmentShaderSource =
        @"#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    fragColor = uColor;
}";

    private const string SoftGlowFragmentShaderSource =
        @"#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    // Soft glow effect with moderate brightness boost and slight desaturation
    vec3 baseColor = uColor.rgb;
    
    // Boost brightness moderately
    vec3 glowColor = baseColor * 1.3;
    
    // Slight desaturation for softer appearance
    float luminance = dot(glowColor, vec3(0.299, 0.587, 0.114));
    glowColor = mix(glowColor, vec3(luminance), 0.1);
    
    // Clamp to prevent overflow
    glowColor = min(glowColor, vec3(1.0));
    
    fragColor = vec4(glowColor, uColor.a);
}";

    private const string BloomFragmentShaderSource =
        @"#version 330 core
out vec4 fragColor;
uniform vec4 uColor;
void main()
{
    // Enhanced bloom effect with strong brightness boost and color saturation
    vec3 baseColor = uColor.rgb;
    
    // Strong brightness boost for bloom
    vec3 bloomColor = baseColor * 2.2;
    
    // Increase color saturation for more vibrant bloom
    float luminance = dot(bloomColor, vec3(0.299, 0.587, 0.114));
    bloomColor = mix(vec3(luminance), bloomColor, 1.4);
    
    // Slight shift towards white for authentic bloom look
    bloomColor = mix(bloomColor, vec3(1.0), 0.1);
    
    // Clamp to prevent overflow
    bloomColor = min(bloomColor, vec3(1.0));
    
    fragColor = vec4(bloomColor, uColor.a * 1.1);
}";

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

    private uint _vao;
    private uint _vbo;
    private uint _vertexCount;

    public void Initialize(IWindow window)
    {
        _gl = GL.GetApi(window);
        _gl.ClearColor(0f, 0f, 0f, 1f);

        // Create all shader programs
        _normalShader = new ShaderProgram(_gl, VertexShaderSource, FragmentShaderSource);
        _softGlowShader = new ShaderProgram(_gl, VertexShaderSource, SoftGlowFragmentShaderSource);
        _bloomShader = new ShaderProgram(_gl, VertexShaderSource, BloomFragmentShaderSource);

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

        Resize(window.Size);
    }

    public void Clear()
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        
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
        
        // Configure blending based on shader mode
        if (mode == ShaderMode.Normal)
        {
            _gl.Disable(EnableCap.Blend);
        }
        else
        {
            // Enable additive blending for glow effects
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }
        
        // Update current shader and program handle
        _shader = mode switch
        {
            ShaderMode.Normal => _normalShader,
            ShaderMode.SoftGlow => _softGlowShader,
            ShaderMode.Bloom => _bloomShader,
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

    public void Resize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        _aspectRatio = newSize.Y / (float)newSize.X;
    }

    public void Shutdown()
    {
        _normalShader.Dispose();
        _softGlowShader.Dispose();
        _bloomShader.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}
