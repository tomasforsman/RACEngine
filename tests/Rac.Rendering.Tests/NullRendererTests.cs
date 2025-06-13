using Rac.Rendering;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Xunit;

namespace Rac.Rendering.Tests;

public class NullRendererTests
{
    [Fact]
    public void NullRenderer_ImplementsIRenderer()
    {
        // Verify NullRenderer implements IRenderer interface
        Assert.Contains(typeof(IRenderer), typeof(NullRenderer).GetInterfaces());
    }

    [Fact]
    public void NullRenderer_AllMethodsCanBeCalledWithoutExceptions()
    {
        // Arrange
        var renderer = new NullRenderer();
        var vertices = new float[] { 0f, 0f, 1f, 0f, 0.5f, 1f };
        var basicVertices = new BasicVertex[] { new() { Position = new Vector2D<float>(0, 0) } };
        var layout = BasicVertex.GetLayout();

        // Act & Assert - all methods should complete without throwing exceptions
        renderer.Initialize(null!); // Null window is acceptable for null renderer
        renderer.Clear();
        renderer.SetColor(new Vector4D<float>(1, 0, 0, 1));
        renderer.SetShaderMode(ShaderMode.Normal);
        renderer.UpdateVertices(vertices);
        renderer.UpdateVertices(basicVertices);
        renderer.UpdateVertices(vertices, layout);
        renderer.Draw();
        renderer.FinalizeFrame();
        renderer.Resize(new Vector2D<int>(800, 600));
        renderer.Shutdown();
    }

    [Fact]
    public void NullRenderer_CanBeInstantiatedMultipleTimes()
    {
        // Verify multiple instances can be created and used
        var renderer1 = new NullRenderer();
        var renderer2 = new NullRenderer();
        
        // Both should work independently
        renderer1.Clear();
        renderer2.Clear();
        
        Assert.NotSame(renderer1, renderer2);
    }
}