using Silk.NET.OpenGL;

namespace Rac.Rendering.Shader;

public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private bool _disposed;

    public uint Handle { get; private set; }

    public ShaderProgram(GL gl, string vertSrc, string fragSrc)
    {
        _gl = gl;

        uint vs = 0;
        uint fs = 0;

        try
        {
            vs = Compile(ShaderType.VertexShader, vertSrc);
            fs = Compile(ShaderType.FragmentShader, fragSrc);

            Handle = _gl.CreateProgram();
            
            _gl.AttachShader(Handle, vs);
            _gl.AttachShader(Handle, fs);
            _gl.LinkProgram(Handle);

            _gl.GetProgram(Handle, GLEnum.LinkStatus, out var success);
            if (success == 0)
            {
                string infoLog = _gl.GetProgramInfoLog(Handle);
                throw new InvalidOperationException($"Shader linking failed: {infoLog}");
            }
        }
        finally
        {
            if (vs != 0) _gl.DeleteShader(vs);
            if (fs != 0) _gl.DeleteShader(fs);
        }
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    private uint Compile(ShaderType type, string src)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, src);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, GLEnum.CompileStatus, out var success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            _gl.DeleteShader(shader);
            throw new InvalidOperationException($"Shader compilation failed ({type}): {infoLog}");
        }

        return shader;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteProgram(Handle);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}