using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Core;

public class EntityHierarchyExtensionsTests
{
    [Fact]
    public void SetParent_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);
        var parent = new Entity(2);
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(new World());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.SetParent(null!, parent, transformSystem));
    }

    [Fact]
    public void SetParent_RequiresValidTransformSystem()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var parent = world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.SetParent(world, parent, null!));
    }

    [Fact]
    public void SetParent_EstablishesHierarchyRelationship()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();

        // Act
        child.SetParent(world, parent, transformSystem);

        // Assert
        Assert.Equal(parent, child.GetParent(world));
        Assert.Contains(child, parent.GetChildren(world));
    }

    [Fact]
    public void RemoveParent_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(new World());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.RemoveParent(null!, transformSystem));
    }

    [Fact]
    public void RemoveParent_RequiresValidTransformSystem()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.RemoveParent(world, null!));
    }

    [Fact]
    public void RemoveParent_MakesEntityRoot()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();
        
        child.SetParent(world, parent, transformSystem);

        // Act
        child.RemoveParent(world, transformSystem);

        // Assert
        Assert.Null(child.GetParent(world));
        Assert.True(child.IsRoot(world));
        Assert.DoesNotContain(child, parent.GetChildren(world));
    }

    [Fact]
    public void AddChild_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);
        var child = new Entity(2);
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(new World());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.AddChild(null!, child, transformSystem));
    }

    [Fact]
    public void AddChild_RequiresValidTransformSystem()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var child = world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.AddChild(world, child, null!));
    }

    [Fact]
    public void AddChild_EstablishesHierarchyRelationship()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();

        // Act
        parent.AddChild(world, child, transformSystem);

        // Assert
        Assert.Equal(parent, child.GetParent(world));
        Assert.Contains(child, parent.GetChildren(world));
    }

    [Fact]
    public void GetParent_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetParent(null!));
    }

    [Fact]
    public void GetParent_ReturnsNullForRootEntity()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var parent = entity.GetParent(world);

        // Assert
        Assert.Null(parent);
    }

    [Fact]
    public void GetParent_ReturnsParentForChildEntity()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();
        
        child.SetParent(world, parent, transformSystem);

        // Act
        var retrievedParent = child.GetParent(world);

        // Assert
        Assert.Equal(parent, retrievedParent);
    }

    [Fact]
    public void GetChildren_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetChildren(null!).ToList());
    }

    [Fact]
    public void GetChildren_ReturnsEmptyForEntityWithoutChildren()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var children = entity.GetChildren(world);

        // Assert
        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_ReturnsAllDirectChildren()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child1 = world.CreateEntity();
        var child2 = world.CreateEntity();
        var child3 = world.CreateEntity();

        parent.AddChild(world, child1, transformSystem);
        parent.AddChild(world, child2, transformSystem);
        parent.AddChild(world, child3, transformSystem);

        // Act
        var children = parent.GetChildren(world).ToList();

        // Assert
        Assert.Equal(3, children.Count);
        Assert.Contains(child1, children);
        Assert.Contains(child2, children);
        Assert.Contains(child3, children);
    }

    [Fact]
    public void IsRoot_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.IsRoot(null!));
    }

    [Fact]
    public void IsRoot_ReturnsTrueForEntityWithoutParent()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act & Assert
        Assert.True(entity.IsRoot(world));
    }

    [Fact]
    public void IsRoot_ReturnsFalseForEntityWithParent()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();
        
        child.SetParent(world, parent, transformSystem);

        // Act & Assert
        Assert.False(child.IsRoot(world));
    }

    [Fact]
    public void IsLeaf_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.IsLeaf(null!));
    }

    [Fact]
    public void IsLeaf_ReturnsTrueForEntityWithoutChildren()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act & Assert
        Assert.True(entity.IsLeaf(world));
    }

    [Fact]
    public void IsLeaf_ReturnsFalseForEntityWithChildren()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child = world.CreateEntity();
        
        parent.AddChild(world, child, transformSystem);

        // Act & Assert
        Assert.False(parent.IsLeaf(world));
    }

    [Fact]
    public void GetChildCount_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetChildCount(null!));
    }

    [Fact]
    public void GetChildCount_ReturnsZeroForEntityWithoutChildren()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var count = entity.GetChildCount(world);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void GetChildCount_ReturnsCorrectCountForEntityWithChildren()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);
        var parent = world.CreateEntity();
        var child1 = world.CreateEntity();
        var child2 = world.CreateEntity();

        parent.AddChild(world, child1, transformSystem);
        parent.AddChild(world, child2, transformSystem);

        // Act
        var count = parent.GetChildCount(world);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void SetLocalTransform_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);
        var position = new Vector2D<float>(1f, 2f);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.SetLocalTransform(null!, position));
    }

    [Fact]
    public void SetLocalTransform_SetsTransformComponent()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var position = new Vector2D<float>(10f, 20f);
        var rotation = MathF.PI / 4f;
        var scale = new Vector2D<float>(2f, 3f);

        // Act
        entity.SetLocalTransform(world, position, rotation, scale);

        // Assert
        var transform = entity.GetLocalTransform(world);
        Assert.NotNull(transform);
        Assert.Equal(position, transform.Value.LocalPosition);
        Assert.Equal(rotation, transform.Value.LocalRotation);
        Assert.Equal(scale, transform.Value.LocalScale);
    }

    [Fact]
    public void SetLocalTransform_UsesDefaultsForOptionalParameters()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var position = new Vector2D<float>(10f, 20f);

        // Act
        entity.SetLocalTransform(world, position);

        // Assert
        var transform = entity.GetLocalTransform(world);
        Assert.NotNull(transform);
        Assert.Equal(position, transform.Value.LocalPosition);
        Assert.Equal(0f, transform.Value.LocalRotation);
        Assert.Equal(Vector2D<float>.One, transform.Value.LocalScale);
    }

    [Fact]
    public void GetLocalTransform_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetLocalTransform(null!));
    }

    [Fact]
    public void GetLocalTransform_ReturnsNullForEntityWithoutTransform()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var transform = entity.GetLocalTransform(world);

        // Assert
        Assert.Null(transform);
    }

    [Fact]
    public void GetWorldTransform_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetWorldTransform(null!));
    }

    [Fact]
    public void GetWorldTransform_ReturnsNullForEntityWithoutWorldTransform()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var worldTransform = entity.GetWorldTransform(world);

        // Assert
        Assert.Null(worldTransform);
    }

    [Fact]
    public void GetWorldPosition_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetWorldPosition(null!));
    }

    [Fact]
    public void GetWorldPosition_ReturnsZeroForEntityWithoutWorldTransform()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var position = entity.GetWorldPosition(world);

        // Assert
        Assert.Equal(Vector2D<float>.Zero, position);
    }

    [Fact]
    public void GetWorldRotation_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetWorldRotation(null!));
    }

    [Fact]
    public void GetWorldRotation_ReturnsZeroForEntityWithoutWorldTransform()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var rotation = entity.GetWorldRotation(world);

        // Assert
        Assert.Equal(0f, rotation);
    }

    [Fact]
    public void GetWorldScale_RequiresValidWorld()
    {
        // Arrange
        var entity = new Entity(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.GetWorldScale(null!));
    }

    [Fact]
    public void GetWorldScale_ReturnsOneForEntityWithoutWorldTransform()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var scale = entity.GetWorldScale(world);

        // Assert
        Assert.Equal(Vector2D<float>.One, scale);
    }

    [Fact]
    public void IntegrationTest_CompleteHierarchyWorkflow()
    {
        // Arrange
        var world = new World();
        var transformSystem = new TransformSystem();
        transformSystem.Initialize(world);

        var character = world.CreateEntity();
        var weapon = world.CreateEntity();
        var scope = world.CreateEntity();

        // Set up transforms
        character.SetLocalTransform(world, new Vector2D<float>(100f, 200f));
        weapon.SetLocalTransform(world, new Vector2D<float>(10f, 0f));
        scope.SetLocalTransform(world, new Vector2D<float>(5f, 2f));

        // Set up hierarchy: character -> weapon -> scope
        character.AddChild(world, weapon, transformSystem);
        weapon.AddChild(world, scope, transformSystem);

        // Update transforms
        transformSystem.Update(0f);

        // Act & Assert - Test hierarchy queries
        Assert.True(character.IsRoot(world));
        Assert.False(character.IsLeaf(world));
        Assert.Equal(1, character.GetChildCount(world));

        Assert.False(weapon.IsRoot(world));
        Assert.False(weapon.IsLeaf(world));
        Assert.Equal(character, weapon.GetParent(world));

        Assert.False(scope.IsRoot(world));
        Assert.True(scope.IsLeaf(world));
        Assert.Equal(weapon, scope.GetParent(world));

        // Test world transforms
        var characterWorldPos = character.GetWorldPosition(world);
        var weaponWorldPos = weapon.GetWorldPosition(world);
        var scopeWorldPos = scope.GetWorldPosition(world);

        Assert.Equal(new Vector2D<float>(100f, 200f), characterWorldPos);
        Assert.Equal(new Vector2D<float>(110f, 200f), weaponWorldPos);
        Assert.Equal(new Vector2D<float>(115f, 202f), scopeWorldPos);
    }
}