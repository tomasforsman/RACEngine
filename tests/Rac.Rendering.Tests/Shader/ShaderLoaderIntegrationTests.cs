using Rac.Rendering.Shader;
using Xunit;

namespace Rac.Rendering.Tests.Shader;

public class ShaderLoaderIntegrationTests
{
    [Fact]
    public void ShaderLoader_CanDiscoverAllRequiredShaderModes()
    {
        // Test that shader discovery finds all required shader modes
        var availableModes = ShaderLoader.DiscoverAvailableShaderModes();
        
        // Should find all three required shader modes
        Assert.Contains(ShaderMode.Normal, availableModes);
        Assert.Contains(ShaderMode.SoftGlow, availableModes);
        Assert.Contains(ShaderMode.Bloom, availableModes);
    }

    [Fact]
    public void ShaderLoader_CanValidateShaderDirectory()
    {
        // Test that shader directory validation works
        var status = ShaderLoader.ValidateShaderDirectory();
        
        // Directory should exist and be ready
        Assert.True(status.Exists);
        Assert.True(status.HasVertexShader);
        Assert.True(status.FragmentShaderCount >= 3); // Normal, SoftGlow, Bloom
        Assert.Equal("Ready", status.Status);
    }

    [Fact]
    public void ShaderLoader_CanLoadVertexShader()
    {
        // Test that vertex shader can be loaded
        var vertexSource = ShaderLoader.LoadVertexShader();
        
        Assert.NotNull(vertexSource);
        Assert.NotEmpty(vertexSource);
        Assert.Contains("#version 330 core", vertexSource);
        Assert.Contains("gl_Position", vertexSource);
    }

    [Theory]
    [InlineData(ShaderMode.Normal)]
    [InlineData(ShaderMode.SoftGlow)]
    [InlineData(ShaderMode.Bloom)]
    public void ShaderLoader_CanLoadAllFragmentShaders(ShaderMode mode)
    {
        // Test that all fragment shaders can be loaded
        var fragmentSource = ShaderLoader.LoadFragmentShader(mode);
        
        Assert.NotNull(fragmentSource);
        Assert.NotEmpty(fragmentSource);
        Assert.Contains("#version 330 core", fragmentSource);
        Assert.Contains("fragColor", fragmentSource);
    }

    [Theory]
    [InlineData(ShaderMode.Normal)]
    [InlineData(ShaderMode.SoftGlow)]
    [InlineData(ShaderMode.Bloom)]
    public void ShaderLoader_ReportsAllShaderModesAvailable(ShaderMode mode)
    {
        // Test that all shader modes are reported as available
        var isAvailable = ShaderLoader.IsShaderModeAvailable(mode);
        Assert.True(isAvailable);
    }

    [Fact]
    public void ShaderLoader_GeneratesCompleteAvailabilityReport()
    {
        // Test that shader availability report includes all modes
        var report = ShaderLoader.GetShaderAvailabilityReport();
        
        // Should have entries for all shader modes
        Assert.True(report.ContainsKey(ShaderMode.Normal));
        Assert.True(report.ContainsKey(ShaderMode.SoftGlow));
        Assert.True(report.ContainsKey(ShaderMode.Bloom));
        
        // All shaders should be available
        Assert.True(report[ShaderMode.Normal].IsAvailable);
        Assert.True(report[ShaderMode.SoftGlow].IsAvailable);
        Assert.True(report[ShaderMode.Bloom].IsAvailable);
    }

    [Theory]
    [InlineData("fullscreen_quad.vert")]
    [InlineData("brightness_extract.frag")]
    [InlineData("gaussian_blur.frag")]
    [InlineData("bloom_composite.frag")]
    public void ShaderLoader_CanLoadPostProcessingShaders(string filename)
    {
        // Test that all post-processing shaders can be loaded
        var shaderSource = ShaderLoader.LoadShaderFromFile(filename);
        
        Assert.NotNull(shaderSource);
        Assert.NotEmpty(shaderSource);
        Assert.Contains("#version 330 core", shaderSource);
        
        // Verify shader type-specific content
        if (filename.EndsWith(".vert"))
        {
            Assert.Contains("gl_Position", shaderSource);
        }
        else if (filename.EndsWith(".frag"))
        {
            Assert.Contains("fragColor", shaderSource);
        }
    }
}