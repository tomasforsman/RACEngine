using Xunit;
using Rac.Rendering.Shader;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests for DebugUV shader fallback mechanism to ensure the shader is always available.
/// </summary>
public class DebugUVFallbackTests
{
    [Fact]
    public void DebugUV_ShouldAlwaysBeDiscoverable()
    {
        // Arrange & Act
        var availableShaders = ShaderLoader.DiscoverAvailableShaderModes();
        
        // Assert
        Assert.Contains(ShaderMode.DebugUV, availableShaders);
    }
    
    [Fact]
    public void DebugUV_ShouldAlwaysBeAvailable()
    {
        // Arrange & Act
        var isAvailable = ShaderLoader.IsShaderModeAvailable(ShaderMode.DebugUV);
        
        // Assert
        Assert.True(isAvailable, "DebugUV shader should always be available");
    }
    
    [Fact]
    public void DebugUV_ShaderContent_ShouldBeValid()
    {
        // Arrange & Act
        var fragmentShader = ShaderLoader.LoadFragmentShader(ShaderMode.DebugUV);
        
        // Assert
        Assert.NotNull(fragmentShader);
        Assert.NotEmpty(fragmentShader);
        Assert.Contains("#version", fragmentShader);
        Assert.Contains("in vec2 vTexCoord", fragmentShader);
        Assert.Contains("in vec4 vColor", fragmentShader);
        Assert.Contains("in float vDistance", fragmentShader);
        Assert.Contains("out vec4 fragColor", fragmentShader);
        Assert.Contains("void main()", fragmentShader);
    }
    
    [Fact]
    public void DebugUV_AvailabilityReport_ShouldShowAsAvailable()
    {
        // Arrange & Act
        var report = ShaderLoader.GetShaderAvailabilityReport();
        
        // Assert
        Assert.True(report.ContainsKey(ShaderMode.DebugUV), "DebugUV should be in availability report");
        
        var debugUVInfo = report[ShaderMode.DebugUV];
        Assert.True(debugUVInfo.Exists, "DebugUV shader file should exist");
        Assert.True(debugUVInfo.IsAvailable, "DebugUV shader should be available");
        Assert.Equal("debuguv.frag", debugUVInfo.Filename);
    }
    
    [Theory]
    [InlineData(ShaderMode.Normal)]
    [InlineData(ShaderMode.SoftGlow)]
    [InlineData(ShaderMode.Bloom)]
    [InlineData(ShaderMode.DebugUV)]
    public void AllShaderModes_ShouldBeAvailable(ShaderMode mode)
    {
        // Arrange & Act
        var isAvailable = ShaderLoader.IsShaderModeAvailable(mode);
        
        // Assert
        Assert.True(isAvailable, $"Shader mode {mode} should be available");
    }
}