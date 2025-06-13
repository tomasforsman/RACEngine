using Rac.Core.Logger;
using Rac.Engine;
using Xunit;

namespace Rac.Engine.Tests;

/// <summary>
/// Tests for the IEngineFacade interface and its implementations.
/// These tests focus on interface compliance and basic functionality without requiring complex mocks.
/// </summary>
public class EngineFacadeTests
{
    /// <summary>
    /// Verifies that the original EngineFacade implements IEngineFacade correctly.
    /// </summary>
    [Fact]
    public void EngineFacade_ImplementsIEngineFacade()
    {
        // Assert - Type should implement the interface
        Assert.True(typeof(EngineFacade).IsAssignableTo(typeof(IEngineFacade)));
    }

    /// <summary>
    /// Verifies that ModularEngineFacade implements IEngineFacade correctly.
    /// </summary>
    [Fact]
    public void ModularEngineFacade_ImplementsIEngineFacade()
    {
        // Assert - Type should implement the interface
        Assert.True(typeof(ModularEngineFacade).IsAssignableTo(typeof(IEngineFacade)));
    }

    /// <summary>
    /// Verifies that ModularEngineFacade throws when constructed with null dependencies.
    /// </summary>
    [Fact]
    public void ModularEngineFacade_Constructor_ThrowsOnNullDependencies()
    {
        // Arrange
        var logger = new SerilogLogger();

        // Act & Assert - Should throw on any null dependency
        Assert.Throws<ArgumentNullException>(() => new ModularEngineFacade(null!, null!, null!, logger));
        Assert.Throws<ArgumentNullException>(() => new ModularEngineFacade(null!, null!, null!, null!));
    }

    /// <summary>
    /// Verifies that the IEngineFacade interface has all required members.
    /// </summary>
    [Fact]
    public void IEngineFacade_HasRequiredMembers()
    {
        // Arrange
        var interfaceType = typeof(IEngineFacade);

        // Act & Assert - Check for required properties
        Assert.NotNull(interfaceType.GetProperty("World"));
        Assert.NotNull(interfaceType.GetProperty("Systems"));
        Assert.NotNull(interfaceType.GetProperty("Renderer"));

        // Check for required events
        Assert.NotNull(interfaceType.GetEvent("LoadEvent"));
        Assert.NotNull(interfaceType.GetEvent("UpdateEvent"));
        Assert.NotNull(interfaceType.GetEvent("RenderEvent"));
        Assert.NotNull(interfaceType.GetEvent("KeyEvent"));

        // Check for required methods
        Assert.NotNull(interfaceType.GetMethod("AddSystem"));
        Assert.NotNull(interfaceType.GetMethod("Run"));
    }

    /// <summary>
    /// Verifies that both implementations can be cast to the interface.
    /// </summary>
    [Fact]
    public void BothImplementations_AreAssignableToInterface()
    {
        // Assert - Both types should be assignable to IEngineFacade
        Assert.True(typeof(EngineFacade).IsAssignableTo(typeof(IEngineFacade)));
        Assert.True(typeof(ModularEngineFacade).IsAssignableTo(typeof(IEngineFacade)));
    }

    /// <summary>
    /// Verifies that the SerilogLogger implements ILogger correctly.
    /// </summary>
    [Fact]
    public void SerilogLogger_ImplementsILogger()
    {
        // Arrange & Act
        var logger = new SerilogLogger();

        // Assert
        Assert.IsAssignableFrom<ILogger>(logger);

        // Should not throw when calling methods
        logger.LogDebug("test");
        logger.LogInfo("test");
        logger.LogWarning("test");
        logger.LogError("test");
    }

    /// <summary>
    /// Verifies that the ILogger interface has the expected methods.
    /// </summary>
    [Fact]
    public void ILogger_HasRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(ILogger);

        // Act & Assert - Check for required methods
        Assert.NotNull(interfaceType.GetMethod("LogDebug"));
        Assert.NotNull(interfaceType.GetMethod("LogInfo"));
        Assert.NotNull(interfaceType.GetMethod("LogWarning"));
        Assert.NotNull(interfaceType.GetMethod("LogError"));
    }
}