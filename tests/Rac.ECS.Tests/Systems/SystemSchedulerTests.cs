using Rac.ECS.Core;
using Rac.ECS.Systems;
using Xunit;

namespace Rac.ECS.Tests.Systems;

public class SystemSchedulerTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Add_WithWorld_CallsInitializeOnSystem()
    {
        // Arrange
        var world = new World();
        var scheduler = new SystemScheduler(world);
        var system = new TestInputSystem();

        // Act
        scheduler.Add(system);

        // Assert
        Assert.True(system.InitializeCalled);
        Assert.Same(world, system.ReceivedWorld);
    }

    [Fact]
    public void Remove_WithWorld_CallsShutdownOnSystem()
    {
        // Arrange
        var world = new World();
        var scheduler = new SystemScheduler(world);
        var system = new TestInputSystem();
        scheduler.Add(system);

        // Act
        var removed = scheduler.Remove(system);

        // Assert
        Assert.True(removed);
        Assert.True(system.ShutdownCalled);
    }

    [Fact]
    public void Clear_WithWorld_CallsShutdownOnAllSystems()
    {
        // Arrange
        var world = new World();
        var scheduler = new SystemScheduler(world);
        var system1 = new TestInputSystem();
        var system2 = new TestMovementSystem();
        scheduler.Add(system1);
        scheduler.Add(system2);

        // Act
        scheduler.Clear();

        // Assert
        Assert.True(system1.ShutdownCalled);
        Assert.True(system2.ShutdownCalled);
        Assert.Equal(0, scheduler.Count);
    }

    [Fact]
    public void AddSystems_WithWorld_InitializesAllSystems()
    {
        // Arrange
        var world = new World();
        var scheduler = new SystemScheduler(world);
        var system1 = new TestInputSystem();
        var system2 = new TestMovementSystem();

        // Act
        scheduler.AddSystems(system1, system2);

        // Assert
        Assert.True(system1.InitializeCalled);
        Assert.True(system2.InitializeCalled);
        Assert.Equal(2, scheduler.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCY RESOLUTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Update_WithDependencies_ExecutesSystemsInCorrectOrder()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem();
        var renderSystem = new TestRenderSystem();

        // Add systems in reverse dependency order to test sorting
        scheduler.Add(renderSystem);     // Depends on movement
        scheduler.Add(movementSystem);   // Depends on input
        scheduler.Add(inputSystem);      // No dependencies

        // Act
        scheduler.Update(0.016f);

        // Assert - All systems should have been called once
        Assert.Equal(1, inputSystem.UpdateCallCount);
        Assert.Equal(1, movementSystem.UpdateCallCount);
        Assert.Equal(1, renderSystem.UpdateCallCount);

        // Verify execution order through GetExecutionOrder
        var executionOrder = scheduler.GetExecutionOrder();
        Assert.Equal(3, executionOrder.Count);
        Assert.Same(inputSystem, executionOrder[0]);      // No dependencies, runs first
        Assert.Same(movementSystem, executionOrder[1]);   // Depends on input
        Assert.Same(renderSystem, executionOrder[2]);     // Depends on movement
    }

    [Fact]
    public void Add_WithComplexDependencies_ResolvesCorrectOrder()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem();
        var complexSystem = new TestComplexSystem(); // Depends on both input and movement

        // Act
        scheduler.Add(complexSystem);
        scheduler.Add(inputSystem);
        scheduler.Add(movementSystem);

        // Assert
        var executionOrder = scheduler.GetExecutionOrder();
        Assert.Equal(3, executionOrder.Count);
        
        // Input should run first (no dependencies)
        Assert.Same(inputSystem, executionOrder[0]);
        
        // Movement should run second (depends on input)
        Assert.Same(movementSystem, executionOrder[1]);
        
        // Complex should run last (depends on both input and movement)
        Assert.Same(complexSystem, executionOrder[2]);
    }

    [Fact]
    public void Add_WithCircularDependencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var systemA = new TestCircularA();
        var systemB = new TestCircularB();

        // Act & Assert
        scheduler.Add(systemA);
        
        var exception = Assert.Throws<InvalidOperationException>(() => scheduler.Add(systemB));
        Assert.Contains("Circular dependency detected", exception.Message);
    }

    [Fact]
    public void Add_WithMissingDependency_IgnoresMissingDependency()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var movementSystem = new TestMovementSystem(); // Depends on InputSystem, but we won't add it

        // Act - Should not throw
        scheduler.Add(movementSystem);

        // Assert
        var executionOrder = scheduler.GetExecutionOrder();
        Assert.Single(executionOrder);
        Assert.Same(movementSystem, executionOrder[0]);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDGE CASES AND ERROR HANDLING
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_WithNullWorld_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SystemScheduler(null!));
    }

    [Fact]
    public void Add_WithNullSystem_ThrowsArgumentNullException()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => scheduler.Add(null!));
    }

    [Fact]
    public void AddSystems_WithNullArray_ThrowsArgumentNullException()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => scheduler.AddSystems(null!));
    }

    [Fact]
    public void AddSystems_WithNullSystemInArray_ThrowsArgumentNullException()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var validSystem = new TestInputSystem();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => scheduler.AddSystems(validSystem, null!));
    }

    [Fact]
    public void Remove_WithNullSystem_ReturnsFalse()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());

        // Act
        var result = scheduler.Remove(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Remove_WithNonExistentSystem_ReturnsFalse()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var system = new TestInputSystem();

        // Act
        var result = scheduler.Remove(system);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetRegisteredSystems_ReturnsSystemsInRegistrationOrder()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var system1 = new TestInputSystem();
        var system2 = new TestMovementSystem();
        var system3 = new TestRenderSystem();

        // Act
        scheduler.Add(system3); // Add in non-dependency order
        scheduler.Add(system1);
        scheduler.Add(system2);

        // Assert
        var registered = scheduler.GetRegisteredSystems();
        Assert.Equal(3, registered.Count);
        Assert.Same(system3, registered[0]); // Registration order preserved
        Assert.Same(system1, registered[1]);
        Assert.Same(system2, registered[2]);
    }

    [Fact]
    public void Update_WithEmptyScheduler_DoesNotThrow()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());

        // Act & Assert - Should not throw
        scheduler.Update(0.016f);
    }

    [Fact]
    public void Update_WithSpecificSystems_BypassesDependencyResolution()
    {
        // Arrange
        var scheduler = new SystemScheduler(new World());
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem();

        // Add systems to scheduler (they have dependencies)
        scheduler.Add(movementSystem);
        scheduler.Add(inputSystem);

        // Act - Update with explicit order (reverse of dependency order)
        scheduler.Update(0.016f, new ISystem[] { movementSystem, inputSystem });

        // Assert - Both should be called even in wrong order
        Assert.Equal(1, inputSystem.UpdateCallCount);
        Assert.Equal(1, movementSystem.UpdateCallCount);
    }
}