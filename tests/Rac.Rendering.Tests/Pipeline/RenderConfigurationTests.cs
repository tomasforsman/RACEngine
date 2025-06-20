using Rac.Rendering.Pipeline;
using Silk.NET.Maths;
using Xunit;

namespace Rac.Rendering.Tests.Pipeline;

public class RenderConfigurationTests
{
    [Fact]
    public void RenderConfiguration_DefaultConstructor_CreatesValidConfiguration()
    {
        // Arrange
        var viewportSize = new Vector2D<int>(1920, 1080);
        
        // Act
        var config = new RenderConfiguration(viewportSize);
        
        // Assert
        Assert.Equal(viewportSize, config.ViewportSize);
        Assert.Equal(new Vector4D<float>(0f, 0f, 0f, 1f), config.ClearColor);
        Assert.False(config.PostProcessing.EnableBloom);
        Assert.True(config.Camera.UseOrthographic);
        Assert.Equal(1, config.Quality.MsaaSamples);
    }
    
    [Fact]
    public void RenderConfiguration_InvalidViewportSize_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new RenderConfiguration(new Vector2D<int>(0, 100)));
        Assert.Throws<ArgumentException>(() => new RenderConfiguration(new Vector2D<int>(100, 0)));
        Assert.Throws<ArgumentException>(() => new RenderConfiguration(new Vector2D<int>(-10, 100)));
    }
    
    [Fact]
    public void RenderConfigurationBuilder_BuildsCorrectConfiguration()
    {
        // Arrange
        var viewportSize = new Vector2D<int>(800, 600);
        var clearColor = new Vector4D<float>(0.2f, 0.3f, 0.4f, 1f);
        var postProcessing = new PostProcessingConfiguration { EnableBloom = true, BloomThreshold = 0.8f };
        
        // Act
        var config = RenderConfiguration.Create(viewportSize)
            .WithClearColor(clearColor)
            .WithPostProcessing(postProcessing)
            .Build();
        
        // Assert
        Assert.Equal(viewportSize, config.ViewportSize);
        Assert.Equal(clearColor, config.ClearColor);
        Assert.True(config.PostProcessing.EnableBloom);
        Assert.Equal(0.8f, config.PostProcessing.BloomThreshold);
    }
    
    [Fact]
    public void PostProcessingConfiguration_DefaultValues_AreCorrect()
    {
        // Act
        var config = new PostProcessingConfiguration();
        
        // Assert
        Assert.False(config.EnableBloom);
        Assert.Equal(1.0f, config.BloomThreshold);
        Assert.Equal(1.0f, config.BloomIntensity);
        Assert.Equal(2.0f, config.BlurRadius);
    }
    
    [Fact]
    public void CameraConfiguration_DefaultValues_AreCorrect()
    {
        // Act
        var config = new CameraConfiguration();
        
        // Assert
        Assert.Equal(45f, config.FieldOfView);
        Assert.Equal(0.1f, config.NearPlane);
        Assert.Equal(1000f, config.FarPlane);
        Assert.True(config.UseOrthographic);
    }
    
    [Fact]
    public void QualityConfiguration_DefaultValues_AreCorrect()
    {
        // Act
        var config = new QualityConfiguration();
        
        // Assert
        Assert.Equal(1, config.MsaaSamples);
        Assert.True(config.EnableAnisotropicFiltering);
        Assert.Equal(16f, config.MaxAnisotropy);
        Assert.True(config.VSync);
    }
}