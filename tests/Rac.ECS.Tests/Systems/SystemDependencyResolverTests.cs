using Rac.ECS.Systems;
using Xunit;
using System.Reflection;

namespace Rac.ECS.Tests.Systems;

public class SystemDependencyResolverTests
{
    [Fact]
    public void ResolveDependencies_WithNoDependencies_ReturnsOriginalOrder()
    {
        // Arrange
        var system1 = new TestInputSystem();
        var system2 = new TestMovementSystem();
        var systems = new List<ISystem> { system1, system2 };

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Same(system1, result[0]);
        Assert.Same(system2, result[1]);
    }

    [Fact]
    public void ResolveDependencies_WithSingleDependency_ReturnsCorrectOrder()
    {
        // Arrange
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem(); // Depends on TestInputSystem
        var systems = new List<ISystem> { movementSystem, inputSystem }; // Wrong order

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Same(inputSystem, result[0]);     // Should be first (no dependencies)
        Assert.Same(movementSystem, result[1]); // Should be second (depends on input)
    }

    [Fact]
    public void ResolveDependencies_WithChainedDependencies_ReturnsCorrectOrder()
    {
        // Arrange
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem();    // Depends on TestInputSystem
        var renderSystem = new TestRenderSystem();        // Depends on TestMovementSystem
        var systems = new List<ISystem> { renderSystem, inputSystem, movementSystem }; // Wrong order

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Same(inputSystem, result[0]);     // No dependencies
        Assert.Same(movementSystem, result[1]);  // Depends on input
        Assert.Same(renderSystem, result[2]);    // Depends on movement
    }

    [Fact]
    public void ResolveDependencies_WithMultipleDependencies_ReturnsCorrectOrder()
    {
        // Arrange
        var inputSystem = new TestInputSystem();
        var movementSystem = new TestMovementSystem();    // Depends on TestInputSystem
        var complexSystem = new TestComplexSystem();      // Depends on both TestInputSystem and TestMovementSystem
        var systems = new List<ISystem> { complexSystem, inputSystem, movementSystem }; // Wrong order

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Same(inputSystem, result[0]);     // No dependencies
        Assert.Same(movementSystem, result[1]);  // Depends on input
        Assert.Same(complexSystem, result[2]);   // Depends on both input and movement
    }

    [Fact]
    public void ResolveDependencies_WithMissingDependency_IgnoresMissingDependency()
    {
        // Arrange
        var movementSystem = new TestMovementSystem(); // Depends on TestInputSystem, but we don't include it
        var systems = new List<ISystem> { movementSystem };

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Single(result);
        Assert.Same(movementSystem, result[0]);
    }

    [Fact]
    public void ResolveDependencies_WithCircularDependencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemA = new TestCircularA(); // Depends on TestCircularB
        var systemB = new TestCircularB(); // Depends on TestCircularA
        var systems = new List<ISystem> { systemA, systemB };

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() => CallResolveDependencies(systems));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Circular dependency detected", exception.InnerException!.Message);
        Assert.Contains("TestCircularA", exception.InnerException.Message);
        Assert.Contains("TestCircularB", exception.InnerException.Message);
    }

    [Fact]
    public void ResolveDependencies_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var systems = new List<ISystem>();

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ResolveDependencies_WithSingleSystem_ReturnsSingleSystem()
    {
        // Arrange
        var system = new TestInputSystem();
        var systems = new List<ISystem> { system };

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Single(result);
        Assert.Same(system, result[0]);
    }

    [Fact]
    public void ResolveDependencies_WithComplexDependencyGraph_ReturnsValidTopologicalOrder()
    {
        // Create a more complex dependency graph:
        // A -> B -> D
        // A -> C -> D
        // This tests multiple paths to the same dependency
        
        // Arrange
        var systemA = new TestInputSystem();      // No dependencies
        var systemB = new TestMovementSystem();   // Depends on A (TestInputSystem)
        var systemC = new TestComplexSystem();    // Depends on A and B (TestInputSystem and TestMovementSystem)
        var systemD = new TestRenderSystem();     // Depends on B (TestMovementSystem)
        
        var systems = new List<ISystem> { systemD, systemC, systemB, systemA }; // Reverse order

        // Act
        var result = CallResolveDependencies(systems);

        // Assert
        Assert.Equal(4, result.Count);
        
        // A should be first (no dependencies)
        Assert.Same(systemA, result[0]);
        
        // B should be second (depends only on A)
        Assert.Same(systemB, result[1]);
        
        // C and D should be after A and B
        var indexC = result.IndexOf(systemC);
        var indexD = result.IndexOf(systemD);
        
        Assert.True(indexC > 1); // C should be after A and B
        Assert.True(indexD > 1); // D should be after B
    }

    /// <summary>
    /// Helper method to call the internal ResolveDependencies method using reflection.
    /// </summary>
    private static List<ISystem> CallResolveDependencies(IEnumerable<ISystem> systems)
    {
        var resolverType = typeof(SystemDependencyResolver);
        var method = resolverType.GetMethod("ResolveDependencies", 
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        
        Assert.NotNull(method);
        
        var result = method.Invoke(null, new object[] { systems });
        return (List<ISystem>)result!;
    }
}