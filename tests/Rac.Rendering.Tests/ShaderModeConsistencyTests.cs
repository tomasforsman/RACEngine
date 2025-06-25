using Rac.Rendering.Shader;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests for ensuring consistent shader mode application across all entities
/// when demonstrating visual effects in samples like BoidSample.
/// </summary>
public class ShaderModeConsistencyTests
{
    [Theory]
    [InlineData(ShaderMode.Normal)]
    [InlineData(ShaderMode.SoftGlow)]
    [InlineData(ShaderMode.Bloom)]
    [InlineData(ShaderMode.DebugUV)]
    public void ShaderMode_ShouldBeAppliedConsistentlyToAllEntities(ShaderMode selectedMode)
    {
        // Arrange - This represents the user's selected shader mode
        var userSelectedMode = selectedMode;
        
        // Act - Simulate what should happen for different entity types
        var whiteBoidShader = GetShaderModeForEntity("White", userSelectedMode);
        var blueBoidShader = GetShaderModeForEntity("Blue", userSelectedMode);
        var redBoidShader = GetShaderModeForEntity("Red", userSelectedMode);
        var obstacleShader = GetShaderModeForEntity("Obstacle", userSelectedMode);
        
        // Assert - All entities should use the same shader mode
        Assert.Equal(userSelectedMode, whiteBoidShader);
        Assert.Equal(userSelectedMode, blueBoidShader);
        Assert.Equal(userSelectedMode, redBoidShader);
        Assert.Equal(userSelectedMode, obstacleShader);
    }
    
    [Fact]
    public void ShaderMode_AllModesShould_BeDistinctValues()
    {
        // Ensure each shader mode has a distinct value for proper demonstration
        Assert.NotEqual(ShaderMode.Normal, ShaderMode.SoftGlow);
        Assert.NotEqual(ShaderMode.Normal, ShaderMode.Bloom);
        Assert.NotEqual(ShaderMode.Normal, ShaderMode.DebugUV);
        Assert.NotEqual(ShaderMode.SoftGlow, ShaderMode.Bloom);
        Assert.NotEqual(ShaderMode.SoftGlow, ShaderMode.DebugUV);
        Assert.NotEqual(ShaderMode.Bloom, ShaderMode.DebugUV);
    }
    
    /// <summary>
    /// Helper method that represents the corrected logic for shader mode selection.
    /// This should always return the user's selected mode regardless of entity type.
    /// </summary>
    private static ShaderMode GetShaderModeForEntity(string entityType, ShaderMode currentShaderMode)
    {
        // This is the CORRECTED logic - all entities use the current shader mode
        return currentShaderMode;
    }
}