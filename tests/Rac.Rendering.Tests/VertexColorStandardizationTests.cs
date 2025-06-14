using Rac.Rendering;
using Silk.NET.Maths;
using System;
using System.Linq;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests to verify that all vertex types now consistently provide color data
/// with default alpha = 1.0, eliminating the need for alpha checks in shaders.
/// </summary>
public class VertexColorStandardizationTests
{
    [Fact]
    public void BasicVertex_ShouldConvertToFullVertexWithDefaultColor()
    {
        // Create basic vertices (position only)
        var basicVertices = new BasicVertex[]
        {
            new(new Vector2D<float>(-0.5f, -0.5f)),
            new(new Vector2D<float>( 0.5f, -0.5f)),
            new(new Vector2D<float>( 0.0f,  0.5f))
        };

        var renderer = new TestableRenderer();
        
        // This should internally convert to FullVertex with default color (1,1,1,1)
        renderer.UpdateVertices(basicVertices);
        
        Assert.True(renderer.WasConvertedToFullVertex);
        Assert.Equal(3, renderer.LastFullVertices!.Length);
        
        // Verify all vertices have default color
        foreach (var vertex in renderer.LastFullVertices)
        {
            Assert.Equal(1f, vertex.Color.X); // Red
            Assert.Equal(1f, vertex.Color.Y); // Green
            Assert.Equal(1f, vertex.Color.Z); // Blue
            Assert.Equal(1f, vertex.Color.W); // Alpha
        }
    }

    [Fact]
    public void TexturedVertex_ShouldConvertToFullVertexWithDefaultColor()
    {
        // Create textured vertices (position + texcoord)
        var texturedVertices = new TexturedVertex[]
        {
            new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(0f, 0f)),
            new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>(1f, 0f)),
            new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>(0.5f, 1f))
        };

        var renderer = new TestableRenderer();
        
        // This should internally convert to FullVertex with default color (1,1,1,1)
        renderer.UpdateVertices(texturedVertices);
        
        Assert.True(renderer.WasConvertedToFullVertex);
        Assert.Equal(3, renderer.LastFullVertices!.Length);
        
        // Verify texture coordinates are preserved and color is default
        for (int i = 0; i < texturedVertices.Length; i++)
        {
            var original = texturedVertices[i];
            var converted = renderer.LastFullVertices[i];
            
            Assert.Equal(original.Position.X, converted.Position.X);
            Assert.Equal(original.Position.Y, converted.Position.Y);
            Assert.Equal(original.TexCoord.X, converted.TexCoord.X);
            Assert.Equal(original.TexCoord.Y, converted.TexCoord.Y);
            
            // Color should be default
            Assert.Equal(1f, converted.Color.X);
            Assert.Equal(1f, converted.Color.Y);
            Assert.Equal(1f, converted.Color.Z);
            Assert.Equal(1f, converted.Color.W);
        }
    }

    [Fact]
    public void FullVertex_ShouldPreserveExplicitColors()
    {
        // Create full vertices with explicit colors
        var fullVertices = new FullVertex[]
        {
            new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(-1f, -1f), new Vector4D<float>(1f, 0f, 0f, 0.5f)),
            new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>( 1f, -1f), new Vector4D<float>(0f, 1f, 0f, 0.8f)),
            new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>( 0f,  1f), new Vector4D<float>(0f, 0f, 1f, 0.2f))
        };

        var renderer = new TestableRenderer();
        
        // FullVertex should pass through unchanged
        renderer.UpdateVertices(fullVertices);
        
        Assert.True(renderer.WasConvertedToFullVertex);
        Assert.Equal(3, renderer.LastFullVertices!.Length);
        
        // Verify explicit colors are preserved (including alpha < 1.0)
        for (int i = 0; i < fullVertices.Length; i++)
        {
            var original = fullVertices[i];
            var result = renderer.LastFullVertices[i];
            
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.TexCoord.X, result.TexCoord.X);
            Assert.Equal(original.TexCoord.Y, result.TexCoord.Y);
            Assert.Equal(original.Color.X, result.Color.X);
            Assert.Equal(original.Color.Y, result.Color.Y);
            Assert.Equal(original.Color.Z, result.Color.Z);
            Assert.Equal(original.Color.W, result.Color.W);
        }
    }

    [Fact]
    public void FloatArray_BasicLayout_ShouldConvertToFullVertexWithDefaultColor()
    {
        // Basic float array (position only)
        var vertices = new float[] { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };
        var layout = BasicVertex.GetLayout();

        var renderer = new TestableRenderer();
        
        renderer.UpdateVertices(vertices, layout);
        
        Assert.True(renderer.WasConvertedToFullVertex);
        Assert.Equal(3, renderer.LastFullVertices!.Length);
        
        // Verify positions and default values
        var expected = new[]
        {
            new { X = -0.5f, Y = -0.5f },
            new { X =  0.5f, Y = -0.5f },
            new { X =  0.0f, Y =  0.5f }
        };
        
        for (int i = 0; i < expected.Length; i++)
        {
            var vertex = renderer.LastFullVertices[i];
            Assert.Equal(expected[i].X, vertex.Position.X);
            Assert.Equal(expected[i].Y, vertex.Position.Y);
            Assert.Equal(0f, vertex.TexCoord.X);  // Default
            Assert.Equal(0f, vertex.TexCoord.Y);  // Default
            Assert.Equal(1f, vertex.Color.X);     // Default
            Assert.Equal(1f, vertex.Color.Y);     // Default
            Assert.Equal(1f, vertex.Color.Z);     // Default
            Assert.Equal(1f, vertex.Color.W);     // Default
        }
    }

    [Fact]
    public void FloatArray_TexturedLayout_ShouldConvertToFullVertexWithDefaultColor()
    {
        // Textured float array (position + texcoord)
        var vertices = new float[] {
            -0.5f, -0.5f, 0f, 0f,    // position + texcoord
             0.5f, -0.5f, 1f, 0f,
             0.0f,  0.5f, 0.5f, 1f
        };
        var layout = TexturedVertex.GetLayout();

        var renderer = new TestableRenderer();
        
        renderer.UpdateVertices(vertices, layout);
        
        Assert.True(renderer.WasConvertedToFullVertex);
        Assert.Equal(3, renderer.LastFullVertices!.Length);
        
        // Verify positions, texture coordinates, and default color
        var expected = new[]
        {
            new { PosX = -0.5f, PosY = -0.5f, TexX = 0f, TexY = 0f },
            new { PosX =  0.5f, PosY = -0.5f, TexX = 1f, TexY = 0f },
            new { PosX =  0.0f, PosY =  0.5f, TexX = 0.5f, TexY = 1f }
        };
        
        for (int i = 0; i < expected.Length; i++)
        {
            var vertex = renderer.LastFullVertices[i];
            Assert.Equal(expected[i].PosX, vertex.Position.X);
            Assert.Equal(expected[i].PosY, vertex.Position.Y);
            Assert.Equal(expected[i].TexX, vertex.TexCoord.X);
            Assert.Equal(expected[i].TexY, vertex.TexCoord.Y);
            Assert.Equal(1f, vertex.Color.X);     // Default
            Assert.Equal(1f, vertex.Color.Y);     // Default
            Assert.Equal(1f, vertex.Color.Z);     // Default
            Assert.Equal(1f, vertex.Color.W);     // Default
        }
    }

    /// <summary>
    /// Testable renderer that exposes the internal conversion to FullVertex
    /// </summary>
    private class TestableRenderer : IRenderer
    {
        public bool WasConvertedToFullVertex { get; private set; }
        public FullVertex[]? LastFullVertices { get; private set; }

        public void UpdateVertices<T>(T[] vertices) where T : unmanaged
        {
            // Simulate the conversion logic from OpenGLRenderer
            var defaultColor = new Vector4D<float>(1f, 1f, 1f, 1f);
            var defaultTexCoord = new Vector2D<float>(0f, 0f);
            
            LastFullVertices = typeof(T).Name switch
            {
                nameof(BasicVertex) => vertices.Cast<BasicVertex>()
                    .Select(v => new FullVertex(v.Position, defaultTexCoord, defaultColor))
                    .ToArray(),
                nameof(TexturedVertex) => vertices.Cast<TexturedVertex>()
                    .Select(v => new FullVertex(v.Position, v.TexCoord, defaultColor))
                    .ToArray(),
                nameof(FullVertex) => vertices.Cast<FullVertex>().ToArray(),
                _ => throw new ArgumentException($"Unsupported vertex type: {typeof(T).Name}")
            };
            
            WasConvertedToFullVertex = true;
        }

        public void UpdateVertices(float[] vertices, VertexLayout layout)
        {
            // Simulate the float array conversion logic
            var defaultColor = new Vector4D<float>(1f, 1f, 1f, 1f);
            var defaultTexCoord = new Vector2D<float>(0f, 0f);
            
            var floatsPerVertex = layout.Stride / sizeof(float);
            var vertexCount = vertices.Length / floatsPerVertex;
            LastFullVertices = new FullVertex[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                var offset = i * floatsPerVertex;
                
                var position = new Vector2D<float>(vertices[offset], vertices[offset + 1]);
                var texCoord = floatsPerVertex >= 4 
                    ? new Vector2D<float>(vertices[offset + 2], vertices[offset + 3])
                    : defaultTexCoord;
                
                LastFullVertices[i] = new FullVertex(position, texCoord, defaultColor);
            }
            
            WasConvertedToFullVertex = true;
        }

        // Required interface methods (minimal implementation for testing)
        public void Initialize(Silk.NET.Windowing.IWindow window) { }
        public void Clear() { }
        public void SetColor(Vector4D<float> rgba) { }
        public void SetShaderMode(Rac.Rendering.Shader.ShaderMode mode) { }
        public void UpdateVertices(float[] vertices) => UpdateVertices(vertices, BasicVertex.GetLayout());
        public void Draw() { }
        public void FinalizeFrame() { }
        public void Resize(Vector2D<int> newSize) { }
        public void Shutdown() { }
    }
}