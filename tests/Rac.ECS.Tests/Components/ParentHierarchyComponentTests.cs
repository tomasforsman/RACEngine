using Rac.ECS.Components;
using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Components;

public class ParentHierarchyComponentTests
{
    [Fact]
    public void DefaultConstructor_CreatesRootWithNoChildren()
    {
        // Act
        var hierarchy = new ParentHierarchyComponent();

        // Assert
        Assert.True(hierarchy.IsRoot);
        Assert.True(hierarchy.IsLeaf);
        Assert.Equal(0, hierarchy.ChildCount);
        Assert.False(hierarchy.ParentEntity.IsAlive);
    }

    [Fact]
    public void ParentConstructor_CreatesChildWithSpecifiedParent()
    {
        // Arrange
        var parent = new Entity(42);

        // Act
        var hierarchy = new ParentHierarchyComponent(parent);

        // Assert
        Assert.False(hierarchy.IsRoot);
        Assert.True(hierarchy.IsLeaf);
        Assert.Equal(parent, hierarchy.ParentEntity);
        Assert.Equal(0, hierarchy.ChildCount);
    }

    [Fact]
    public void HasChild_ReturnsTrueForExistingChild()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();
        var childId = 123;
        hierarchy = hierarchy.WithChild(childId);

        // Act & Assert
        Assert.True(hierarchy.HasChild(childId));
        Assert.False(hierarchy.HasChild(999)); // Non-existent child
    }

    [Fact]
    public void WithChild_AddsChildToHierarchy()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();
        var childId = 123;

        // Act
        var updated = hierarchy.WithChild(childId);

        // Assert
        Assert.True(updated.HasChild(childId));
        Assert.Equal(1, updated.ChildCount);
        Assert.False(updated.IsLeaf);
    }

    [Fact]
    public void WithChild_AllowsMultipleChildren()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();

        // Act
        var updated = hierarchy
            .WithChild(100)
            .WithChild(200)
            .WithChild(300);

        // Assert
        Assert.Equal(3, updated.ChildCount);
        Assert.True(updated.HasChild(100));
        Assert.True(updated.HasChild(200));
        Assert.True(updated.HasChild(300));
    }

    [Fact]
    public void WithChild_DoesNotDuplicateExistingChild()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();
        var childId = 123;

        // Act
        var updated = hierarchy
            .WithChild(childId)
            .WithChild(childId); // Add same child twice

        // Assert
        Assert.Equal(1, updated.ChildCount); // Should still be 1
        Assert.True(updated.HasChild(childId));
    }

    [Fact]
    public void WithoutChild_RemovesChildFromHierarchy()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent()
            .WithChild(100)
            .WithChild(200)
            .WithChild(300);

        // Act
        var updated = hierarchy.WithoutChild(200);

        // Assert
        Assert.Equal(2, updated.ChildCount);
        Assert.True(updated.HasChild(100));
        Assert.False(updated.HasChild(200));
        Assert.True(updated.HasChild(300));
    }

    [Fact]
    public void WithoutChild_HandlesNonExistentChildGracefully()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent()
            .WithChild(100);

        // Act
        var updated = hierarchy.WithoutChild(999); // Non-existent child

        // Assert
        Assert.Equal(1, updated.ChildCount);
        Assert.True(updated.HasChild(100));
    }

    [Fact]
    public void WithParent_UpdatesParentEntity()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();
        var newParent = new Entity(42);

        // Act
        var updated = hierarchy.WithParent(newParent);

        // Assert
        Assert.Equal(newParent, updated.ParentEntity);
        Assert.False(updated.IsRoot);
    }

    [Fact]
    public void AsRoot_RemovesParentRelationship()
    {
        // Arrange
        var parent = new Entity(42);
        var hierarchy = new ParentHierarchyComponent(parent);

        // Act
        var updated = hierarchy.AsRoot();

        // Assert
        Assert.True(updated.IsRoot);
        Assert.False(updated.ParentEntity.IsAlive);
    }

    [Fact]
    public void WithChildren_AddsMultipleChildrenAtOnce()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();
        var childIds = new[] { 100, 200, 300 };

        // Act
        var updated = hierarchy.WithChildren(childIds);

        // Assert
        Assert.Equal(3, updated.ChildCount);
        foreach (var childId in childIds)
        {
            Assert.True(updated.HasChild(childId));
        }
    }

    [Fact]
    public void WithChildren_MergesWithExistingChildren()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent()
            .WithChild(50);
        var newChildIds = new[] { 100, 200 };

        // Act
        var updated = hierarchy.WithChildren(newChildIds);

        // Assert
        Assert.Equal(3, updated.ChildCount);
        Assert.True(updated.HasChild(50));
        Assert.True(updated.HasChild(100));
        Assert.True(updated.HasChild(200));
    }

    [Fact]
    public void WithoutAllChildren_RemovesAllChildren()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent()
            .WithChild(100)
            .WithChild(200)
            .WithChild(300);

        // Act
        var updated = hierarchy.WithoutAllChildren();

        // Assert
        Assert.Equal(0, updated.ChildCount);
        Assert.True(updated.IsLeaf);
    }

    [Fact]
    public void IsLeafProperty_ReflectsChildrenState()
    {
        // Arrange
        var hierarchy = new ParentHierarchyComponent();

        // Act & Assert
        Assert.True(hierarchy.IsLeaf); // No children initially

        var withChild = hierarchy.WithChild(100);
        Assert.False(withChild.IsLeaf); // Has children

        var withoutChildren = withChild.WithoutAllChildren();
        Assert.True(withoutChildren.IsLeaf); // No children again
    }
}