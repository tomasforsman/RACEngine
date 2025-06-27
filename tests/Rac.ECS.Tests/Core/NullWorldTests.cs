using Rac.ECS.Components;
using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

/// <summary>
/// Tests for NullWorld implementation to ensure it provides safe no-op behavior
/// following the Null Object Pattern.
/// </summary>
public class NullWorldTests
{
    [Fact]
    public void CreateEntity_ReturnsInvalidEntity()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var entity = nullWorld.CreateEntity();

        // Assert
        Assert.Equal(0, entity.Id);
        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void SetComponent_DoesNotThrow()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = nullWorld.CreateEntity();
        var component = new TestComponent(42);

        // Act & Assert - Should not throw
        nullWorld.SetComponent(entity, component);
    }

    [Fact]
    public void GetSingleton_ThrowsInvalidOperationException()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            nullWorld.GetSingleton<TestComponent>()
        );
        Assert.Contains("NullWorld", exception.Message);
    }

    [Fact]
    public void RemoveComponent_ReturnsFalse()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = nullWorld.CreateEntity();

        // Act
        bool removed = nullWorld.RemoveComponent<TestComponent>(entity);

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void Query_SingleComponent_ReturnsEmptyEnumerable()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var results = nullWorld.Query<TestComponent>().ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Query_TwoComponents_ReturnsEmptyEnumerable()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var results = nullWorld.Query<TestComponent, TestComponent2>().ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Query_ThreeComponents_ReturnsEmptyEnumerable()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var results = nullWorld.Query<TestComponent, TestComponent2, TestComponent3>().ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void NullWorld_ProvidesSafeDefaultsForAllOperations()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = nullWorld.CreateEntity();

        // Act & Assert - All operations should be safe and not throw
        nullWorld.SetComponent(entity, new TestComponent(1));
        nullWorld.SetComponent(entity, new TestComponent2("test"));
        nullWorld.SetComponent(entity, new TestComponent3(true));

        Assert.False(nullWorld.RemoveComponent<TestComponent>(entity));
        Assert.False(nullWorld.RemoveComponent<TestComponent2>(entity));
        Assert.False(nullWorld.RemoveComponent<TestComponent3>(entity));

        Assert.Empty(nullWorld.Query<TestComponent>());
        Assert.Empty(nullWorld.Query<TestComponent, TestComponent2>());
        Assert.Empty(nullWorld.Query<TestComponent, TestComponent2, TestComponent3>());
    }

    // Test component types for testing
    private record struct TestComponent(int Value) : IComponent;
    private record struct TestComponent2(string Text) : IComponent;
    private record struct TestComponent3(bool Flag) : IComponent;
}