using Silk.NET.OpenGL;

namespace Rac.Rendering.Shader
{
    public class ShaderProgram : IDisposable
    {
        public uint Handle { get; }
        private GL _gl;

        public ShaderProgram(GL gl, string vertSrc, string fragSrc)
        {
            _gl = gl;
            uint vs = Compile(ShaderType.VertexShader,   vertSrc);
            uint fs = Compile(ShaderType.FragmentShader, fragSrc);

            Handle = _gl.CreateProgram();
            _gl.AttachShader(Handle, vs);
            _gl.AttachShader(Handle, fs);
            _gl.LinkProgram(Handle);

            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);
        }

        private uint Compile(ShaderType type, string src)
        {
            uint s = _gl.CreateShader(type);
            _gl.ShaderSource(s, src);
            _gl.CompileShader(s);
            _gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
            if (ok == 0)
            {
                string log = _gl.GetShaderInfoLog(s);
                throw new Exception($"Shader compile error: {log}");
            }
            return s;
        }

        public void Dispose()
        {
            _gl.DeleteProgram(Handle);
        }
    }
}
