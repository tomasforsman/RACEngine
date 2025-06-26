using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Systems;

public class TransformSystemTests
{
    [Fact]
    public void Constructor_RequiresWorld()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TransformSystem(null!));
    }

    [Fact]
    public void Update_RootEntityWithTransform_CreatesWorldTransform()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var entity = world.CreateEntity();
        var localTransform = new TransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 4f,
            new Vector2D<float>(2f, 3f)
        );
        world.SetComponent(entity, localTransform);

        // Act
        system.Update(0f);

        // Assert
        var worldTransforms = world.Query<WorldTransformComponent>().ToList();
        Assert.Single(worldTransforms);
        
        var worldTransform = worldTransforms[0].Component1;
        Assert.Equal(localTransform.LocalPosition, worldTransform.WorldPosition);
        Assert.Equal(localTransform.LocalRotation, worldTransform.WorldRotation);
        Assert.Equal(localTransform.LocalScale, worldTransform.WorldScale);
    }

    [Fact]
    public void Update_ParentChildHierarchy_ComputesCorrectWorldTransforms()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);

        // Create parent entity
        var parent = world.CreateEntity();
        var parentTransform = new TransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 2f, // 90 degrees
            new Vector2D<float>(1f, 1f)
        );
        world.SetComponent(parent, parentTransform);
        world.SetComponent(parent, new ParentHierarchyComponent());

        // Create child entity
        var child = world.CreateEntity();
        var childTransform = new TransformComponent(
            new Vector2D<float>(5f, 0f), // Offset from parent
            0f,
            new Vector2D<float>(2f, 2f)
        );
        world.SetComponent(child, childTransform);
        
        // Set up hierarchy relationship
        system.SetParent(parent, child);

        // Act
        system.Update(0f);

        // Assert
        var worldTransforms = world.Query<WorldTransformComponent>().ToList();
        Assert.Equal(2, worldTransforms.Count);

        // Find parent and child world transforms
        var parentWorldTransform = worldTransforms.First(wt => wt.Entity.Id == parent.Id).Component1;
        var childWorldTransform = worldTransforms.First(wt => wt.Entity.Id == child.Id).Component1;

        // Parent world transform should match local (it's a root)
        Assert.Equal(parentTransform.LocalPosition, parentWorldTransform.WorldPosition);
        Assert.Equal(parentTransform.LocalRotation, parentWorldTransform.WorldRotation);

        // Child world position should be parent position + rotated child local position
        // Child local (5,0) rotated 90° around parent = (0,5), then translated by parent (10,20) = (10,25)
        Assert.Equal(10f, childWorldTransform.WorldPosition.X, 3);
        Assert.Equal(25f, childWorldTransform.WorldPosition.Y, 3);
        
        // Child world rotation should be parent + child rotation
        Assert.Equal(MathF.PI / 2f, childWorldTransform.WorldRotation, 3);
        
        // Child world scale should be parent scale * child scale
        Assert.Equal(2f, childWorldTransform.WorldScale.X, 3);
        Assert.Equal(2f, childWorldTransform.WorldScale.Y, 3);
    }

    [Fact]
    public void Update_ThreeLevelHierarchy_ComputesCorrectWorldTransforms()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);

        // Create grandparent (root)
        var grandparent = world.CreateEntity();
        world.SetComponent(grandparent, new TransformComponent(
            new Vector2D<float>(100f, 100f),
            0f,
            Vector2D<float>.One
        ));

        // Create parent
        var parent = world.CreateEntity();
        world.SetComponent(parent, new TransformComponent(
            new Vector2D<float>(10f, 0f),
            0f,
            Vector2D<float>.One
        ));

        // Create child
        var child = world.CreateEntity();
        world.SetComponent(child, new TransformComponent(
            new Vector2D<float>(5f, 0f),
            0f,
            Vector2D<float>.One
        ));

        // Set up hierarchy: grandparent -> parent -> child
        system.SetParent(grandparent, parent);
        system.SetParent(parent, child);

        // Act
        system.Update(0f);

        // Assert
        var worldTransforms = world.Query<WorldTransformComponent>().ToList();
        Assert.Equal(3, worldTransforms.Count);

        var childWorldTransform = worldTransforms.First(wt => wt.Entity.Id == child.Id).Component1;
        
        // Child world position should be sum of all ancestors: 100 + 10 + 5 = 115
        Assert.Equal(115f, childWorldTransform.WorldPosition.X, 3);
        Assert.Equal(100f, childWorldTransform.WorldPosition.Y, 3);
    }

    [Fact]
    public void SetParent_EstablishesParentChildRelationship()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var parent = world.CreateEntity();
        var child = world.CreateEntity();

        // Act
        system.SetParent(parent, child);

        // Assert
        var parentHierarchy = world.Query<ParentHierarchyComponent>()
            .First(h => h.Entity.Id == parent.Id).Component1;
        var childHierarchy = world.Query<ParentHierarchyComponent>()
            .First(h => h.Entity.Id == child.Id).Component1;

        Assert.True(parentHierarchy.HasChild(child.Id));
        Assert.Equal(parent, childHierarchy.ParentEntity);
    }

    [Fact]
    public void SetParent_PreventsSelfParenting()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        var entity = world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => system.SetParent(entity, entity));
    }

    [Fact]
    public void SetParent_PreventsCircularReferences()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var entityA = world.CreateEntity();
        var entityB = world.CreateEntity();
        
        // Set A as parent of B
        system.SetParent(entityA, entityB);

        // Act & Assert - Try to set B as parent of A (would create cycle)
        Assert.Throws<ArgumentException>(() => system.SetParent(entityB, entityA));
    }

    [Fact]
    public void SetParent_RemovesChildFromPreviousParent()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var oldParent = world.CreateEntity();
        var newParent = world.CreateEntity();
        var child = world.CreateEntity();
        
        // Set up initial relationship
        system.SetParent(oldParent, child);

        // Act - Move child to new parent
        system.SetParent(newParent, child);

        // Assert
        var oldParentHierarchy = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == oldParent.Id);
        var newParentHierarchy = world.Query<ParentHierarchyComponent>()
            .First(h => h.Entity.Id == newParent.Id).Component1;
        var childHierarchy = world.Query<ParentHierarchyComponent>()
            .First(h => h.Entity.Id == child.Id).Component1;

        // Child should be removed from old parent
        if (oldParentHierarchy.Entity.Id != 0)
        {
            Assert.False(oldParentHierarchy.Component1.HasChild(child.Id));
        }
        
        // Child should be added to new parent
        Assert.True(newParentHierarchy.HasChild(child.Id));
        Assert.Equal(newParent, childHierarchy.ParentEntity);
    }

    [Fact]
    public void RemoveParent_MakesEntityRoot()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var parent = world.CreateEntity();
        var child = world.CreateEntity();
        
        system.SetParent(parent, child);

        // Act
        system.RemoveParent(child);

        // Assert
        var parentHierarchy = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == parent.Id);
        var childHierarchy = world.Query<ParentHierarchyComponent>()
            .First(h => h.Entity.Id == child.Id).Component1;

        // Child should be removed from parent's children
        if (parentHierarchy.Entity.Id != 0)
        {
            Assert.False(parentHierarchy.Component1.HasChild(child.Id));
        }
        
        // Child should become root
        Assert.True(childHierarchy.IsRoot);
    }

    [Fact]
    public void Update_EntitiesWithoutHierarchy_TreatedAsRoots()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        var entity = world.CreateEntity();
        var transform = new TransformComponent(new Vector2D<float>(50f, 60f));
        world.SetComponent(entity, transform);
        // Note: No ParentHierarchyComponent added

        // Act
        system.Update(0f);

        // Assert
        var worldTransforms = world.Query<WorldTransformComponent>().ToList();
        Assert.Single(worldTransforms);
        
        var worldTransform = worldTransforms[0].Component1;
        Assert.Equal(transform.LocalPosition, worldTransform.WorldPosition);
    }

    [Fact]
    public void Update_OnlyProcessesEntitiesWithTransformComponent()
    {
        // Arrange
        var world = new World();
        var system = new TransformSystem(world);
        
        // Create entity with only hierarchy component, no transform
        var entityWithoutTransform = world.CreateEntity();
        world.SetComponent(entityWithoutTransform, new ParentHierarchyComponent());
        
        // Create entity with transform component
        var entityWithTransform = world.CreateEntity();
        world.SetComponent(entityWithTransform, new TransformComponent());

        // Act
        system.Update(0f);

        // Assert
        var worldTransforms = world.Query<WorldTransformComponent>().ToList();
        Assert.Single(worldTransforms); // Only the entity with transform should have world transform
        Assert.Equal(entityWithTransform.Id, worldTransforms[0].Entity.Id);
    }
}