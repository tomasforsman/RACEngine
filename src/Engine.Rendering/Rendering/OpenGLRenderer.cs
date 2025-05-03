using Engine.Rendering.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;

namespace Engine.Rendering
{
    public class OpenGLRenderer : IRenderer
    {
        private GL _gl;
        private uint _shaderProgram;
        private uint _vbo, _vao;
        private uint _vertexCount;
        private ShaderProgram _shader;

        public void Initialize(IWindow window)
        {
            _gl = GL.GetApi(window);
            _gl.ClearColor(0, 0, 0, 1);

            // compile shaders
            _shader = new ShaderProgram(_gl, vertexSrc, fragmentSrc);
            _shaderProgram = _shader.Handle;

            // setup VBO/VAO
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Render(double delta)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.UseProgram(_shaderProgram);
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        }

        public void Resize(Vector2D<int> newSize)
        {
            _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        }

        public void Shutdown()
        {
            _shader.Dispose();
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteVertexArray(_vao);
        }

        // New method to update vertex data dynamically
        public unsafe void UpdateVertices(float[] vertices)
        {
            _vertexCount = (uint)(vertices.Length / 2);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (float* v = vertices)
            {
                nuint size = (nuint)(vertices.Length * sizeof(float));
                _gl.BufferData(BufferTargetARB.ArrayBuffer, size, v, BufferUsageARB.DynamicDraw);
            }
        }

        // GLSL shaders (fixed syntax)
        private const string vertexSrc = @"#version 330 core
            layout(location = 0) in vec2 position;
            void main()
            {
                gl_Position = vec4(position, 0, 1);
            }
        ";

        private const string fragmentSrc = @"#version 330 core
            out vec4 color;
            void main()
            {
                color = vec4(1);
            }
        ";
    }
}
