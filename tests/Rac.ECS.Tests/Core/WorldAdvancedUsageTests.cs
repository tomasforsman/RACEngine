using Rac.ECS.Components;
using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

/// <summary>
/// Tests to verify that direct World instantiation and advanced usage scenarios
/// continue to work alongside the new IWorld interface.
/// </summary>
public class WorldAdvancedUsageTests
{
    [Fact]
    public void DirectWorldInstantiation_StillWorks()
    {
        // Arrange & Act - Direct instantiation should still work
        var world = new World();

        // Assert
        Assert.NotNull(world);
        Assert.IsType<World>(world);
        Assert.IsAssignableFrom<IWorld>(world);
    }

    [Fact]
    public void AdvancedScenario_ConcreteWorldWithSystems()
    {
        // Arrange - Advanced scenario where systems might need concrete World
        var world = new World();
        var entity = world.CreateEntity();
        
        // Act - Advanced operations that might require concrete World features
        world.SetComponent(entity, new TestComponent(42));
        var result = world.Query<TestComponent>().FirstOrDefault();

        // Assert
        Assert.NotEqual(default, result);
        Assert.Equal(entity.Id, result.Entity.Id);
        Assert.Equal(42, result.Component1.Value);
    }

    [Fact]
    public void PolymorphicUsage_IWorldAndConcreteWorld()
    {
        // Arrange - Both interface and concrete usage should work
        IWorld iworld = new World();
        World concreteWorld = new World();

        // Act - Both should behave identically for common operations
        var entity1 = iworld.CreateEntity();
        var entity2 = concreteWorld.CreateEntity();

        iworld.SetComponent(entity1, new TestComponent(1));
        concreteWorld.SetComponent(entity2, new TestComponent(2));

        // Assert
        Assert.Single(iworld.Query<TestComponent>());
        Assert.Single(concreteWorld.Query<TestComponent>());
    }

    [Fact]
    public void InterfaceBasedSystem_AcceptsConcreteWorld()
    {
        // Arrange - Systems accepting IWorld should work with concrete World
        var concreteWorld = new World();
        
        // Act - This simulates system construction with concrete World
        IWorld worldInterface = concreteWorld;
        var entity = worldInterface.CreateEntity();
        worldInterface.SetComponent(entity, new TestComponent(123));

        // Assert - Should work seamlessly
        var results = concreteWorld.Query<TestComponent>().ToList();
        Assert.Single(results);
        Assert.Equal(123, results[0].Component1.Value);
    }

    // Test component for testing
    private record struct TestComponent(int Value) : IComponent;
}