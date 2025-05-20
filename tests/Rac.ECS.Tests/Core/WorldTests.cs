using Xunit;
using Rac.ECS.Core;
using Rac.ECS.Components;

namespace Rac.ECS.Tests.Core;

public class WorldTests
{
    // Sample component types for testing
    private record struct TestComponent1(int Value) : IComponent;
    private record struct TestComponent2(string Text) : IComponent;
    private record struct TestComponent3(bool Flag) : IComponent;

    [Fact]
    public void CreateEntity_ReturnsUniqueEntities()
    {
        // Arrange
        var world = new World();

        // Act
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();

        // Assert
        Assert.NotEqual(entity1, entity2);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void SetComponent_AddsComponentToEntity()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var component = new TestComponent1(42);

        // Act
        world.SetComponent(entity, component);

        // Assert - If we can query it, it means it was added successfully
        var result = world.Query<TestComponent1>().ToList();
        Assert.Single(result);
        Assert.Equal(entity, result[0].Entity);
        Assert.Equal(component, result[0].Component1);
    }

    [Fact]
    public void SetComponent_ReplacesExistingComponent()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var initialComponent = new TestComponent1(42);
        var replacementComponent = new TestComponent1(99);

        // Act
        world.SetComponent(entity, initialComponent);
        world.SetComponent(entity, replacementComponent);

        // Assert
        var result = world.Query<TestComponent1>().ToList();
        Assert.Single(result);
        Assert.Equal(entity, result[0].Entity);
        Assert.Equal(replacementComponent, result[0].Component1);
        Assert.NotEqual(initialComponent, result[0].Component1);
    }

    [Fact]
    public void GetSingleton_ReturnsComponent_WhenExists()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var component = new TestComponent1(42);
        world.SetComponent(entity, component);

        // Act
        var result = world.GetSingleton<TestComponent1>();

        // Assert
        Assert.Equal(component, result);
    }

    [Fact]
    public void GetSingleton_ThrowsException_WhenNoComponentExists()
    {
        // Arrange
        var world = new World();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => world.GetSingleton<TestComponent1>());
        Assert.Contains("No singleton component", exception.Message);
    }

    [Fact]
    public void Query_SingleComponent_ReturnsEntitiesWithComponent()
    {
        // Arrange
        var world = new World();
        
        // Create entities with components
        var entity1 = world.CreateEntity();
        world.SetComponent(entity1, new TestComponent1(1));
        
        var entity2 = world.CreateEntity();
        world.SetComponent(entity2, new TestComponent1(2));
        
        var entity3 = world.CreateEntity();
        world.SetComponent(entity3, new TestComponent2("Text"));

        // Act
        var results = world.Query<TestComponent1>().ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Entity.Id == entity1.Id && r.Component1.Value == 1);
        Assert.Contains(results, r => r.Entity.Id == entity2.Id && r.Component1.Value == 2);
    }

    [Fact]
    public void Query_TwoComponents_ReturnsEntitiesWithBothComponents()
    {
        // Arrange
        var world = new World();
        
        // Entity with both components
        var entity1 = world.CreateEntity();
        world.SetComponent(entity1, new TestComponent1(1));
        world.SetComponent(entity1, new TestComponent2("Entity1"));
        
        // Entity with only one component
        var entity2 = world.CreateEntity();
        world.SetComponent(entity2, new TestComponent1(2));
        
        // Entity with different components
        var entity3 = world.CreateEntity();
        world.SetComponent(entity3, new TestComponent1(3));
        world.SetComponent(entity3, new TestComponent3(true));

        // Act
        var results = world.Query<TestComponent1, TestComponent2>().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1, results[0].Entity);
        Assert.Equal(1, results[0].Component1.Value);
        Assert.Equal("Entity1", results[0].Component2.Text);
    }

    [Fact]
    public void Query_ThreeComponents_ReturnsEntitiesWithAllThreeComponents()
    {
        // Arrange
        var world = new World();
        
        // Entity with all three components
        var entity1 = world.CreateEntity();
        world.SetComponent(entity1, new TestComponent1(1));
        world.SetComponent(entity1, new TestComponent2("Entity1"));
        world.SetComponent(entity1, new TestComponent3(true));
        
        // Entity with only two components
        var entity2 = world.CreateEntity();
        world.SetComponent(entity2, new TestComponent1(2));
        world.SetComponent(entity2, new TestComponent2("Entity2"));

        // Act
        var results = world.Query<TestComponent1, TestComponent2, TestComponent3>().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1, results[0].Entity);
        Assert.Equal(1, results[0].Component1.Value);
        Assert.Equal("Entity1", results[0].Component2.Text);
        Assert.True(results[0].Component3.Flag);
    }

    [Fact]
    public void RemoveComponent_RemovesComponentFromEntity()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new TestComponent1(42));

        // Act
        var removed = world.RemoveComponent<TestComponent1>(entity);

        // Assert
        Assert.True(removed);
        Assert.Empty(world.Query<TestComponent1>());
    }

    [Fact]
    public void RemoveComponent_ReturnsFalse_WhenComponentDoesNotExist()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var removed = world.RemoveComponent<TestComponent1>(entity);

        // Assert
        Assert.False(removed);
    }
}