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
        var createEntityMethods = interfaceType.GetMethods().Where(m => m.Name == "CreateEntity");
        Assert.Contains(createEntityMethods, m => m.GetParameters().Length == 0);
        Assert.Contains(createEntityMethods, m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
        
        Assert.NotNull(interfaceType.GetMethod("DestroyEntity"));
        Assert.NotNull(interfaceType.GetProperty("EntityCount"));
        Assert.NotNull(interfaceType.GetMethod("GetEntitiesWithTag"));
        Assert.NotNull(interfaceType.GetMethod("FindEntityByName"));
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
        var originalCreateEntityMethods = originalType.GetMethods().Where(m => m.Name == "CreateEntity");
        var modularCreateEntityMethods = modularType.GetMethods().Where(m => m.Name == "CreateEntity");
        
        Assert.Contains(originalCreateEntityMethods, m => m.GetParameters().Length == 0);
        Assert.Contains(originalCreateEntityMethods, m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
        Assert.NotNull(originalType.GetMethod("DestroyEntity"));
        Assert.NotNull(originalType.GetProperty("EntityCount"));
        Assert.NotNull(originalType.GetMethod("GetEntitiesWithTag"));
        Assert.NotNull(originalType.GetMethod("FindEntityByName"));
        
        Assert.Contains(modularCreateEntityMethods, m => m.GetParameters().Length == 0);
        Assert.Contains(modularCreateEntityMethods, m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
        Assert.NotNull(modularType.GetMethod("DestroyEntity"));
        Assert.NotNull(modularType.GetProperty("EntityCount"));
        Assert.NotNull(modularType.GetMethod("GetEntitiesWithTag"));
        Assert.NotNull(modularType.GetMethod("FindEntityByName"));
    }

    [Fact]
    public void ConvenienceMethods_HaveCorrectSignatures()
    {
        // Arrange
        var interfaceType = typeof(IEngineFacade);

        // Act
        var createEntityMethods = interfaceType.GetMethods().Where(m => m.Name == "CreateEntity").ToArray();
        var destroyEntityMethod = interfaceType.GetMethod("DestroyEntity");
        var entityCountProperty = interfaceType.GetProperty("EntityCount");
        var getEntitiesWithTagMethod = interfaceType.GetMethod("GetEntitiesWithTag");
        var findEntityByNameMethod = interfaceType.GetMethod("FindEntityByName");

        // Assert - Check method signatures
        Assert.Equal(2, createEntityMethods.Length);
        
        var parameterlessCreate = createEntityMethods.First(m => m.GetParameters().Length == 0);
        var namedCreate = createEntityMethods.First(m => m.GetParameters().Length == 1);
        
        Assert.Equal(typeof(Entity), parameterlessCreate.ReturnType);
        Assert.Empty(parameterlessCreate.GetParameters());
        
        Assert.Equal(typeof(Entity), namedCreate.ReturnType);
        Assert.Single(namedCreate.GetParameters());
        Assert.Equal(typeof(string), namedCreate.GetParameters()[0].ParameterType);

        Assert.NotNull(destroyEntityMethod);
        Assert.Equal(typeof(void), destroyEntityMethod.ReturnType);
        Assert.Single(destroyEntityMethod.GetParameters());
        Assert.Equal(typeof(Entity), destroyEntityMethod.GetParameters()[0].ParameterType);

        Assert.NotNull(entityCountProperty);
        Assert.Equal(typeof(int), entityCountProperty.PropertyType);
        Assert.True(entityCountProperty.CanRead);
        Assert.False(entityCountProperty.CanWrite); // Should be read-only
        
        Assert.NotNull(getEntitiesWithTagMethod);
        Assert.Equal(typeof(IEnumerable<Entity>), getEntitiesWithTagMethod.ReturnType);
        Assert.Single(getEntitiesWithTagMethod.GetParameters());
        Assert.Equal(typeof(string), getEntitiesWithTagMethod.GetParameters()[0].ParameterType);
        
        Assert.NotNull(findEntityByNameMethod);
        Assert.Equal(typeof(Entity?), findEntityByNameMethod.ReturnType);
        Assert.Single(findEntityByNameMethod.GetParameters());
        Assert.Equal(typeof(string), findEntityByNameMethod.GetParameters()[0].ParameterType);
    }
}