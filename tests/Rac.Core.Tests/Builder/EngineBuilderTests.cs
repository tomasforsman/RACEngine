using Microsoft.Extensions.DependencyInjection;
using Rac.Core.Builder;
using Rac.Core.Configuration;
using Xunit;

namespace Rac.Core.Tests.Builder;

public class EngineBuilderTests
{
    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "test";
    }

    public class AlternativeTestService : ITestService
    {
        public string GetValue() => "alternative";
    }

    [Fact]
    public void Create_FullGame_ReturnsBuilderWithCorrectProfile()
    {
        // Act
        var builder = EngineBuilder.Create(EngineProfile.FullGame);
        var config = builder.Build();

        // Assert
        Assert.Equal(EngineProfile.FullGame, config.Profile);
        Assert.True(config.EnableGraphics);
        Assert.True(config.EnableAudio);
        Assert.True(config.EnableInput);
        Assert.True(config.EnableECS);
        Assert.False(config.EnableNetworking);
    }

    [Fact]
    public void Create_Headless_ReturnsBuilderWithCorrectProfile()
    {
        // Act
        var builder = EngineBuilder.Create(EngineProfile.Headless);
        var config = builder.Build();

        // Assert
        Assert.Equal(EngineProfile.Headless, config.Profile);
        Assert.False(config.EnableGraphics);
        Assert.False(config.EnableAudio);
        Assert.False(config.EnableInput);
        Assert.True(config.EnableECS);
        Assert.True(config.EnableNetworking);
    }

    [Fact]
    public void WithGraphics_ReturnsNewBuilderWithModifiedConfig()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = originalBuilder.WithGraphics(true);
        var originalConfig = originalBuilder.Build();
        var newConfig = newBuilder.Build();

        // Assert
        Assert.False(originalConfig.EnableGraphics); // Original unchanged
        Assert.True(newConfig.EnableGraphics); // New has change
        Assert.NotSame(originalBuilder, newBuilder); // Different instances
    }

    [Fact]
    public void WithAudio_ReturnsNewBuilderWithModifiedConfig()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = originalBuilder.WithAudio(true);
        var newConfig = newBuilder.Build();

        // Assert
        Assert.True(newConfig.EnableAudio);
    }

    [Fact]
    public void WithInput_ReturnsNewBuilderWithModifiedConfig()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = originalBuilder.WithInput(true);
        var newConfig = newBuilder.Build();

        // Assert
        Assert.True(newConfig.EnableInput);
    }

    [Fact]
    public void WithECS_ReturnsNewBuilderWithModifiedConfig()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = originalBuilder.WithECS(true);
        var newConfig = newBuilder.Build();

        // Assert
        Assert.True(newConfig.EnableECS);
    }

    [Fact]
    public void WithNetworking_ReturnsNewBuilderWithModifiedConfig()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = originalBuilder.WithNetworking(true);
        var newConfig = newBuilder.Build();

        // Assert
        Assert.True(newConfig.EnableNetworking);
    }

    [Fact]
    public void AddService_WithImplementationType_AddsServiceCorrectly()
    {
        // Arrange
        var builder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = builder.AddService<ITestService, TestService>();
        var config = newBuilder.Build();

        // Assert
        Assert.Single(config.Services);
        var service = config.Services.First();
        Assert.Equal(typeof(ITestService), service.ServiceType);
        Assert.Equal(typeof(TestService), service.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, service.Lifetime);
    }

    [Fact]
    public void AddService_WithFactory_AddsServiceCorrectly()
    {
        // Arrange
        var builder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = builder.AddService<ITestService>(_ => new TestService());
        var config = newBuilder.Build();

        // Assert
        Assert.Single(config.Services);
        var service = config.Services.First();
        Assert.Equal(typeof(ITestService), service.ServiceType);
        Assert.NotNull(service.ImplementationFactory);
        Assert.Equal(ServiceLifetime.Transient, service.Lifetime);
    }

    [Fact]
    public void AddSingleton_AddsServiceWithSingletonLifetime()
    {
        // Arrange
        var builder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = builder.AddSingleton<ITestService, TestService>();
        var config = newBuilder.Build();

        // Assert
        var service = config.Services.First();
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    [Fact]
    public void AddScoped_AddsServiceWithScopedLifetime()
    {
        // Arrange
        var builder = EngineBuilder.Create(EngineProfile.Custom);

        // Act
        var newBuilder = builder.AddScoped<ITestService, TestService>();
        var config = newBuilder.Build();

        // Assert
        var service = config.Services.First();
        Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
    }

    [Fact]
    public void ChainedCalls_WorkCorrectly()
    {
        // Act
        var config = EngineBuilder.Create(EngineProfile.Custom)
            .WithGraphics(true)
            .WithAudio(true)
            .AddSingleton<ITestService, TestService>()
            .AddTransient<string>(_ => "test")
            .Build();

        // Assert
        Assert.True(config.EnableGraphics);
        Assert.True(config.EnableAudio);
        Assert.Equal(2, config.Services.Count);
    }

    [Fact]
    public void BuildServiceProvider_CreatesValidServiceProvider()
    {
        // Arrange
        var builder = EngineBuilder.Create(EngineProfile.Custom)
            .AddSingleton<ITestService, TestService>();

        // Act
        var serviceProvider = builder.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider);
        var testService = serviceProvider.GetService<ITestService>();
        Assert.NotNull(testService);
        Assert.Equal("test", testService.GetValue());
    }

    [Fact]
    public void ImmutabilityTest_OriginalBuilderUnchangedAfterOperations()
    {
        // Arrange
        var originalBuilder = EngineBuilder.Create(EngineProfile.Custom);
        var originalConfig = originalBuilder.Build();

        // Act - perform various operations
        var newBuilder = originalBuilder
            .WithGraphics(true)
            .WithAudio(true)
            .AddSingleton<ITestService, TestService>();

        var newConfig = newBuilder.Build();
        var originalConfigAfter = originalBuilder.Build();

        // Assert - original is unchanged
        Assert.False(originalConfigAfter.EnableGraphics);
        Assert.False(originalConfigAfter.EnableAudio);
        Assert.Empty(originalConfigAfter.Services);
        
        // New config has changes
        Assert.True(newConfig.EnableGraphics);
        Assert.True(newConfig.EnableAudio);
        Assert.Single(newConfig.Services);
    }

    [Fact]
    public void FromConfig_CreatesBuilderFromExistingConfig()
    {
        // Arrange
        var originalConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.FullGame);

        // Act
        var builder = EngineBuilder.FromConfig(originalConfig);
        var config = builder.Build();

        // Assert
        Assert.Equal(EngineProfile.FullGame, config.Profile);
        Assert.True(config.EnableGraphics);
        Assert.True(config.EnableAudio);
        Assert.True(config.EnableInput);
        Assert.True(config.EnableECS);
    }
}