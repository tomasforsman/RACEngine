using Microsoft.Extensions.DependencyInjection;
using Rac.Core.Configuration;
using Xunit;

namespace Rac.Core.Tests.Configuration;

public class ImmutableEngineConfigTests
{
    [Fact]
    public void CreateDefault_FullGame_HasCorrectSettings()
    {
        // Arrange & Act
        var config = ImmutableEngineConfig.CreateDefault(EngineProfile.FullGame);

        // Assert
        Assert.Equal(EngineProfile.FullGame, config.Profile);
        Assert.True(config.EnableGraphics);
        Assert.True(config.EnableAudio);
        Assert.True(config.EnableInput);
        Assert.True(config.EnableECS);
        Assert.False(config.EnableNetworking);
        Assert.NotNull(config.Services);
    }

    [Fact]
    public void CreateDefault_Headless_HasCorrectSettings()
    {
        // Arrange & Act
        var config = ImmutableEngineConfig.CreateDefault(EngineProfile.Headless);

        // Assert
        Assert.Equal(EngineProfile.Headless, config.Profile);
        Assert.False(config.EnableGraphics);
        Assert.False(config.EnableAudio);
        Assert.False(config.EnableInput);
        Assert.True(config.EnableECS);
        Assert.True(config.EnableNetworking);
        Assert.NotNull(config.Services);
    }

    [Fact]
    public void CreateDefault_Custom_HasCorrectSettings()
    {
        // Arrange & Act
        var config = ImmutableEngineConfig.CreateDefault(EngineProfile.Custom);

        // Assert
        Assert.Equal(EngineProfile.Custom, config.Profile);
        Assert.False(config.EnableGraphics);
        Assert.False(config.EnableAudio);
        Assert.False(config.EnableInput);
        Assert.False(config.EnableECS);
        Assert.False(config.EnableNetworking);
        Assert.NotNull(config.Services);
    }

    [Fact]
    public void CreateDefault_InvalidProfile_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            ImmutableEngineConfig.CreateDefault((EngineProfile)999));
    }

    [Fact]
    public void With_ModifiesOnlySpecifiedProperties()
    {
        // Arrange
        var originalConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.Custom);

        // Act
        var modifiedConfig = originalConfig.With(enableGraphics: true, enableAudio: true);

        // Assert
        Assert.Equal(EngineProfile.Custom, modifiedConfig.Profile); // Unchanged
        Assert.True(modifiedConfig.EnableGraphics); // Changed
        Assert.True(modifiedConfig.EnableAudio); // Changed
        Assert.False(modifiedConfig.EnableInput); // Unchanged
        Assert.False(modifiedConfig.EnableECS); // Unchanged
        Assert.False(modifiedConfig.EnableNetworking); // Unchanged
        
        // Original should be unchanged
        Assert.False(originalConfig.EnableGraphics);
        Assert.False(originalConfig.EnableAudio);
    }

    [Fact]
    public void Constructor_CopiesServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<string>(_ => "test");

        // Act
        var config = new ImmutableEngineConfig(
            EngineProfile.Custom,
            false, false, false, false, false,
            services);

        // Assert
        Assert.Single(config.Services);
        
        // Verify that modifying original services doesn't affect config
        services.AddTransient<object>(_ => new object());
        Assert.Single(config.Services); // Should still be 1
    }
}