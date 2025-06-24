using Rac.Rendering;
using Rac.Rendering.Camera;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests demonstrating that IRenderer now supports type-safe vertex operations
/// that were previously only available through OpenGLRenderer directly.
/// </summary>
public class VertexTypeSafetyTests
{
    [Fact]
    public void IRenderer_CanAcceptTypedVertices_BasicVertex()
    {
        // This test demonstrates that we can now use type-safe vertices through the interface
        // Previously, this would only work if you cast to OpenGLRenderer
        
        // Create typed vertices - this provides compile-time type safety
        var vertices = new BasicVertex[]
        {
            new(new Vector2D<float>(-0.5f, -0.5f)),
            new(new Vector2D<float>( 0.5f, -0.5f)),
            new(new Vector2D<float>( 0.0f,  0.5f))
        };

        // Mock renderer that implements IRenderer interface methods
        var renderer = new MockRenderer();
        
        // This should compile and call the generic method through the interface
        renderer.UpdateVertices(vertices);
        
        Assert.True(renderer.GenericUpdateVerticesCalled);
        Assert.Equal(typeof(BasicVertex), renderer.LastVertexType);
        Assert.Equal(3, renderer.LastVertexCount);
    }

    [Fact]
    public void IRenderer_CanAcceptTypedVertices_TexturedVertex()
    {
        // Test with a different vertex type to ensure generics work properly
        var vertices = new TexturedVertex[]
        {
            new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(0f, 0f)),
            new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>(1f, 0f)),
            new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>(0.5f, 1f))
        };

        var renderer = new MockRenderer();
        
        renderer.UpdateVertices(vertices);
        
        Assert.True(renderer.GenericUpdateVerticesCalled);
        Assert.Equal(typeof(TexturedVertex), renderer.LastVertexType);
        Assert.Equal(3, renderer.LastVertexCount);
    }

    [Fact]
    public void IRenderer_CanAcceptFloatArrayWithLayout()
    {
        // Test the explicit layout overload through the interface
        var vertices = new float[] { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };
        var layout = BasicVertex.GetLayout();

        var renderer = new MockRenderer();
        
        renderer.UpdateVertices(vertices, layout);
        
        Assert.True(renderer.LayoutUpdateVerticesCalled);
        Assert.Equal(6, renderer.LastFloatVertices!.Length);
        Assert.Equal(layout, renderer.LastLayout);
    }

    [Fact]
    public void IRenderer_StillSupportsBasicFloatArray()
    {
        // Ensure float array support is maintained
        var vertices = new float[] { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };

        var renderer = new MockRenderer();
        
        renderer.UpdateVertices(vertices);
        
        Assert.True(renderer.BasicUpdateVerticesCalled);
        Assert.Equal(6, renderer.LastFloatVertices!.Length);
    }

    /// <summary>
    /// Mock implementation of IRenderer to test interface method calls
    /// </summary>
    private class MockRenderer : IRenderer
    {
        public bool BasicUpdateVerticesCalled { get; private set; }
        public bool GenericUpdateVerticesCalled { get; private set; }
        public bool LayoutUpdateVerticesCalled { get; private set; }
        
        public float[]? LastFloatVertices { get; private set; }
        public VertexLayout? LastLayout { get; private set; }
        public Type? LastVertexType { get; private set; }
        public int LastVertexCount { get; private set; }

        public void UpdateVertices(float[] vertices)
        {
            BasicUpdateVerticesCalled = true;
            LastFloatVertices = vertices;
        }

        public void UpdateVertices<T>(T[] vertices) where T : unmanaged
        {
            GenericUpdateVerticesCalled = true;
            LastVertexType = typeof(T);
            LastVertexCount = vertices.Length;
        }

        public void UpdateVertices(float[] vertices, VertexLayout layout)
        {
            LayoutUpdateVerticesCalled = true;
            LastFloatVertices = vertices;
            LastLayout = layout;
        }

        // Required interface methods (minimal implementation for testing)
        public void Initialize(Silk.NET.Windowing.IWindow window) { }
        public void Clear() { }
        public void SetColor(Vector4D<float> rgba) { }
        public void SetCameraMatrix(Matrix4X4<float> cameraMatrix) { }
        public void SetActiveCamera(ICamera camera) { }
        public void SetShaderMode(Rac.Rendering.Shader.ShaderMode mode) { }
        public void SetPrimitiveType(PrimitiveType primitiveType) { }
        public void Draw() { }
        public void FinalizeFrame() { }
        public void Resize(Vector2D<int> newSize) { }
        public void Shutdown() { }
    }
}