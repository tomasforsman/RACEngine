using Rac.Rendering.Shader;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests for DebugUV shader mode functionality and integration.
/// </summary>
public class DebugUVShaderTests
{
    [Fact]
    public void DebugUV_ShaderMode_ShouldBeAvailable()
    {
        // Arrange & Act
        var isAvailable = ShaderLoader.IsShaderModeAvailable(ShaderMode.DebugUV);
        
        // Assert
        Assert.True(isAvailable, "DebugUV shader mode should be available");
    }
    
    [Fact]
    public void DebugUV_FragmentShader_ShouldLoadSuccessfully()
    {
        // Arrange & Act
        var fragmentShader = ShaderLoader.LoadFragmentShader(ShaderMode.DebugUV);
        
        // Assert
        Assert.NotNull(fragmentShader);
        Assert.NotEmpty(fragmentShader);
        Assert.Contains("vTexCoord", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fragColor", fragmentShader, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void DebugUV_ShaderContent_ShouldContainUVMapping()
    {
        // Arrange & Act
        var fragmentShader = ShaderLoader.LoadFragmentShader(ShaderMode.DebugUV);
        
        // Assert
        Assert.Contains("vTexCoord", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("debugUV", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Red channel", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Green channel", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0.5", fragmentShader, StringComparison.OrdinalIgnoreCase); // Coordinate conversion
    }
    
    [Fact]
    public void DebugUV_ShouldBeIncludedInAvailableShaderModes()
    {
        // Arrange & Act
        var availableModes = ShaderLoader.DiscoverAvailableShaderModes();
        
        // Assert
        Assert.Contains(ShaderMode.DebugUV, availableModes);
    }
    
    [Fact]
    public void DebugUV_ShaderContent_ShouldContainCoordinateConversion()
    {
        // Arrange & Act
        var fragmentShader = ShaderLoader.LoadFragmentShader(ShaderMode.DebugUV);
        
        // Assert - Verify the shader converts centered coordinates to [0,1] range
        Assert.Contains("debugUV = vTexCoord + vec2(0.5, 0.5)", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("centered coordinates", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("COORDINATE SYSTEM CONVERSION", fragmentShader, StringComparison.OrdinalIgnoreCase);
    }
}