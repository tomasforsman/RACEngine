using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

/// <summary>
/// Tests for IWorld interface to ensure concrete implementations are compatible
/// and interface contracts are properly maintained.
/// </summary>
public class IWorldTests
{
    [Fact]
    public void World_ImplementsIWorld()
    {
        // Arrange & Act
        var world = new World();

        // Assert
        Assert.IsAssignableFrom<IWorld>(world);
    }

    [Fact]
    public void NullWorld_ImplementsIWorld()
    {
        // Arrange & Act
        var nullWorld = new NullWorld();

        // Assert
        Assert.IsAssignableFrom<IWorld>(nullWorld);
    }

    [Fact]
    public void IWorld_CanBeUsedPolymorphically()
    {
        // Arrange
        IWorld[] worlds = { new World(), new NullWorld() };

        // Act & Assert - Should not throw
        foreach (var world in worlds)
        {
            var entity = world.CreateEntity();
            Assert.NotNull(entity);
            
            // Interface provides consistent API regardless of implementation
            Assert.IsType<Entity>(entity);
        }
    }

    [Fact]
    public void IWorld_HasAllRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IWorld);

        // Act & Assert - Check for required methods
        Assert.NotNull(interfaceType.GetMethod("CreateEntity", Type.EmptyTypes)); // No parameters
        Assert.NotNull(interfaceType.GetMethod("CreateEntity", new[] { typeof(string) })); // String parameter
        Assert.NotNull(interfaceType.GetMethod("SetComponent"));
        Assert.NotNull(interfaceType.GetMethod("GetSingleton"));
        Assert.NotNull(interfaceType.GetMethod("RemoveComponent"));
        
        // Check for query methods
        var queryMethods = interfaceType.GetMethods().Where(m => m.Name == "Query").ToArray();
        Assert.Equal(3, queryMethods.Length); // Single, double, and triple component queries
    }
}