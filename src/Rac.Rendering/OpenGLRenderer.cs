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

	private int _aspectLocation;
	private float _aspectRatio;
	private int _colorLocation;
	private GL _gl = null!;
	private uint _programHandle;
	private ShaderProgram _shader = null!;

	private uint _vao;
	private uint _vbo;
	private uint _vertexCount;

	public void Initialize(IWindow window)
	{
		_gl = GL.GetApi(window);
		_gl.ClearColor(0f, 0f, 0f, 1f);

		_shader = new ShaderProgram(_gl, VertexShaderSource, FragmentShaderSource);
		_programHandle = _shader.Handle;

		_aspectLocation = _gl.GetUniformLocation(_programHandle, "uAspect");
		_colorLocation = _gl.GetUniformLocation(_programHandle, "uColor");

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
	}

	public void SetColor(Vector4D<float> rgba)
	{
		_gl.Uniform4(_colorLocation, rgba.X, rgba.Y, rgba.Z, rgba.W);
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
		_shader.Dispose();
		_gl.DeleteBuffer(_vbo);
		_gl.DeleteVertexArray(_vao);
	}
}