using Rac.ECS.Components;
using Xunit;

namespace Rac.ECS.Tests.Components;

public class ContainerComponentTests
{
    [Fact]
    public void DefaultConstructor_CreatesEmptyLoadedNonPersistentContainer()
    {
        // Act
        var container = new ContainerComponent();

        // Assert
        Assert.Equal(string.Empty, container.ContainerName);
        Assert.True(container.IsLoaded);
        Assert.False(container.IsPersistent);
    }

    [Fact]
    public void ConstructorWithName_SetsNameAndDefaults()
    {
        // Arrange
        var name = "TestContainer";

        // Act
        var container = new ContainerComponent(name);

        // Assert
        Assert.Equal(name, container.ContainerName);
        Assert.True(container.IsLoaded);
        Assert.False(container.IsPersistent);
    }

    [Fact]
    public void FullConstructor_SetsAllProperties()
    {
        // Arrange
        var name = "PersistentContainer";
        var isLoaded = false;
        var isPersistent = true;

        // Act
        var container = new ContainerComponent(name, isLoaded, isPersistent);

        // Assert
        Assert.Equal(name, container.ContainerName);
        Assert.Equal(isLoaded, container.IsLoaded);
        Assert.Equal(isPersistent, container.IsPersistent);
    }

    [Fact]
    public void WithLoaded_UpdatesLoadedState()
    {
        // Arrange
        var container = new ContainerComponent("Test", true, false);

        // Act
        var updated = container.WithLoaded(false);

        // Assert
        Assert.Equal("Test", updated.ContainerName);
        Assert.False(updated.IsLoaded);
        Assert.False(updated.IsPersistent);
    }

    [Fact]
    public void WithPersistent_UpdatesPersistentState()
    {
        // Arrange
        var container = new ContainerComponent("Test", true, false);

        // Act
        var updated = container.WithPersistent(true);

        // Assert
        Assert.Equal("Test", updated.ContainerName);
        Assert.True(updated.IsLoaded);
        Assert.True(updated.IsPersistent);
    }

    [Fact]
    public void WithName_UpdatesName()
    {
        // Arrange
        var container = new ContainerComponent("OldName", true, false);

        // Act
        var updated = container.WithName("NewName");

        // Assert
        Assert.Equal("NewName", updated.ContainerName);
        Assert.True(updated.IsLoaded);
        Assert.False(updated.IsPersistent);
    }

    [Fact]
    public void WithName_HandlesNullName()
    {
        // Arrange
        var container = new ContainerComponent("Test");

        // Act
        var updated = container.WithName(null);

        // Assert
        Assert.Equal(string.Empty, updated.ContainerName);
    }

    [Fact]
    public void Record_StructEquality()
    {
        // Arrange
        var container1 = new ContainerComponent("Test", true, false);
        var container2 = new ContainerComponent("Test", true, false);
        var container3 = new ContainerComponent("Different", true, false);

        // Assert
        Assert.Equal(container1, container2);
        Assert.NotEqual(container1, container3);
    }

    [Fact]
    public void Record_StructImmutability()
    {
        // Arrange
        var original = new ContainerComponent("Test", true, false);

        // Act
        var modified = original.WithName("Modified");

        // Assert
        Assert.Equal("Test", original.ContainerName);
        Assert.Equal("Modified", modified.ContainerName);
        Assert.NotEqual(original, modified);
    }
}