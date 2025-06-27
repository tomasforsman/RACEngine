using Rac.ECS.Core;
using Rac.Engine;
using Xunit;

namespace Rac.Engine.Tests;

/// <summary>
/// Tests for the new IWorld integration and facade convenience methods.
/// These tests focus on the interface changes and convenience functionality.
/// </summary>
public class IWorldFacadeTests
{
    [Fact]
    public void IEngineFacade_ExposesIWorldInterface()
    {
        // Assert - Interface should expose IWorld not concrete World
        var worldProperty = typeof(IEngineFacade).GetProperty("World");
        Assert.NotNull(worldProperty);
        Assert.Equal(typeof(IWorld), worldProperty.PropertyType);
    }

    [Fact]
    public void IEngineFacade_HasConvenienceMethods()
    {
        // Arrange
        var interfaceType = typeof(IEngineFacade);

        // Act & Assert - Check for required convenience methods
        Assert.NotNull(interfaceType.GetMethod("CreateEntity"));
        Assert.NotNull(interfaceType.GetMethod("DestroyEntity"));
        Assert.NotNull(interfaceType.GetProperty("EntityCount"));
    }

    [Fact]
    public void BothFacadeImplementations_ProvideConsistentIWorldAccess()
    {
        // Assert - Both implementations should expose IWorld consistently
        var originalType = typeof(EngineFacade);
        var modularType = typeof(ModularEngineFacade);
        
        var originalWorldProperty = originalType.GetProperty("World");
        var modularWorldProperty = modularType.GetProperty("World");
        
        Assert.NotNull(originalWorldProperty);
        Assert.NotNull(modularWorldProperty);
        Assert.Equal(typeof(IWorld), originalWorldProperty.PropertyType);
        Assert.Equal(typeof(IWorld), modularWorldProperty.PropertyType);
    }

    [Fact]
    public void BothFacadeImplementations_HaveConvenienceMethods()
    {
        // Arrange
        var originalType = typeof(EngineFacade);
        var modularType = typeof(ModularEngineFacade);

        // Act & Assert - Both should have convenience methods
        Assert.NotNull(originalType.GetMethod("CreateEntity"));
        Assert.NotNull(originalType.GetMethod("DestroyEntity"));
        Assert.NotNull(originalType.GetProperty("EntityCount"));
        
        Assert.NotNull(modularType.GetMethod("CreateEntity"));
        Assert.NotNull(modularType.GetMethod("DestroyEntity"));
        Assert.NotNull(modularType.GetProperty("EntityCount"));
    }

    [Fact]
    public void ConvenienceMethods_HaveCorrectSignatures()
    {
        // Arrange
        var interfaceType = typeof(IEngineFacade);

        // Act
        var createEntityMethod = interfaceType.GetMethod("CreateEntity");
        var destroyEntityMethod = interfaceType.GetMethod("DestroyEntity");
        var entityCountProperty = interfaceType.GetProperty("EntityCount");

        // Assert - Check method signatures
        Assert.NotNull(createEntityMethod);
        Assert.Equal(typeof(Entity), createEntityMethod.ReturnType);
        Assert.Empty(createEntityMethod.GetParameters());

        Assert.NotNull(destroyEntityMethod);
        Assert.Equal(typeof(void), destroyEntityMethod.ReturnType);
        Assert.Single(destroyEntityMethod.GetParameters());
        Assert.Equal(typeof(Entity), destroyEntityMethod.GetParameters()[0].ParameterType);

        Assert.NotNull(entityCountProperty);
        Assert.Equal(typeof(int), entityCountProperty.PropertyType);
        Assert.True(entityCountProperty.CanRead);
        Assert.False(entityCountProperty.CanWrite); // Should be read-only
    }
}