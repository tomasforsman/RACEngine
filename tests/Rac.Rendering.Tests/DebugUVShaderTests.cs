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
        Assert.Contains("vTexCoord.x", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("vTexCoord.y", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Red channel", fragmentShader, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Green channel", fragmentShader, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void DebugUV_ShouldBeIncludedInAvailableShaderModes()
    {
        // Arrange & Act
        var availableModes = ShaderLoader.DiscoverAvailableShaderModes();
        
        // Assert
        Assert.Contains(ShaderMode.DebugUV, availableModes);
    }
}