using Rac.Rendering;
using Rac.Rendering.Pipeline;
using Silk.NET.Maths;
using Xunit;

namespace Rac.Rendering.Tests.Pipeline;

public class PipelineArchitectureTests
{
    [Fact]
    public void OpenGLRenderer_HasPipelineArchitecture()
    {
        // Arrange & Act
        var renderer = new OpenGLRenderer();
        
        // Assert - Verify the renderer exposes the required properties
        Assert.NotNull(renderer);
        Assert.False(renderer.IsFullyInitialized); // Should not be initialized yet
        
        // Verify configuration is accessible
        var config = renderer.Configuration;
        // RenderConfiguration is a value type, so just verify it has expected default properties
        Assert.Equal(default(Vector2D<int>), config.ViewportSize);
    }
    
    [Fact]
    public void RenderConfiguration_SupportsPostProcessingConfiguration()
    {
        // Arrange
        var viewportSize = new Silk.NET.Maths.Vector2D<int>(1920, 1080);
        
        // Act
        var config = RenderConfiguration.Create(viewportSize)
            .WithPostProcessing(new PostProcessingConfiguration { EnableBloom = true })
            .Build();
        
        // Assert
        Assert.True(config.PostProcessing.EnableBloom);
        Assert.Equal(viewportSize, config.ViewportSize);
    }
    
    [Fact]
    public void RenderConfiguration_Builder_AllowsMethodChaining()
    {
        // Arrange
        var viewportSize = new Silk.NET.Maths.Vector2D<int>(800, 600);
        var clearColor = new Silk.NET.Maths.Vector4D<float>(0.1f, 0.2f, 0.3f, 1f);
        
        // Act
        var config = RenderConfiguration.Create(viewportSize)
            .WithClearColor(clearColor)
            .WithCamera(new CameraConfiguration { UseOrthographic = false })
            .WithQuality(new QualityConfiguration { MsaaSamples = 4 })
            .Build();
        
        // Assert
        Assert.Equal(clearColor, config.ClearColor);
        Assert.False(config.Camera.UseOrthographic);
        Assert.Equal(4, config.Quality.MsaaSamples);
    }
    
    [Fact] 
    public void OpenGLRenderer_ProvidesBackwardCompatibility()
    {
        // Arrange
        var renderer = new OpenGLRenderer();
        
        // Act & Assert - Verify IRenderer interface methods are available
        Assert.NotNull(renderer);
        
        // Verify key IRenderer methods exist (simplified test)
        var rendererType = typeof(OpenGLRenderer);
        
        // Check non-generic methods
        Assert.NotNull(rendererType.GetMethod("Initialize"));
        Assert.NotNull(rendererType.GetMethod("Clear"));
        Assert.NotNull(rendererType.GetMethod("SetColor"));
        Assert.NotNull(rendererType.GetMethod("SetCameraMatrix"));
        Assert.NotNull(rendererType.GetMethod("SetActiveCamera"));
        Assert.NotNull(rendererType.GetMethod("SetShaderMode"));
        Assert.NotNull(rendererType.GetMethod("SetPrimitiveType"));
        Assert.NotNull(rendererType.GetMethod("Draw"));
        Assert.NotNull(rendererType.GetMethod("FinalizeFrame"));
        Assert.NotNull(rendererType.GetMethod("Resize"));
        Assert.NotNull(rendererType.GetMethod("Shutdown"));
        
        // Check UpdateVertices overloads (including generic)
        var updateVerticesMethods = rendererType.GetMethods().Where(m => m.Name == "UpdateVertices").ToArray();
        Assert.True(updateVerticesMethods.Length >= 2, "Should have multiple UpdateVertices overloads");
        
        // Verify IRenderer interface implementation
        Assert.True(typeof(IRenderer).IsAssignableFrom(rendererType), "OpenGLRenderer should implement IRenderer");
    }
}