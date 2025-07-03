using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Core;

public class ContainerExtensionsTests
{
    private readonly World _world;
    private readonly TransformSystem _transformSystem;

    public ContainerExtensionsTests()
    {
        _world = new World();
        _transformSystem = new TransformSystem();
        _transformSystem.Initialize(_world);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLACEIN TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void PlaceIn_WithValidContainer_EstablishesParentChildRelationship()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));

        // Act
        item.PlaceIn(container, _world, _transformSystem);

        // Assert
        var itemQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == item.Id);
        var containerQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == container.Id);
        
        Assert.NotEqual(0, itemQuery.Entity.Id);
        Assert.NotEqual(0, containerQuery.Entity.Id);
        Assert.Equal(container, itemQuery.Component1.ParentEntity);
        Assert.True(containerQuery.Component1.HasChild(item.Id));
    }

    [Fact]
    public void PlaceIn_WithNonContainer_ThrowsArgumentException()
    {
        // Arrange
        var item = _world.CreateEntity();
        var notContainer = _world.CreateEntity(); // No ContainerComponent

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            item.PlaceIn(notContainer, _world, _transformSystem));
        
        Assert.Contains("is not a container", exception.Message);
        Assert.Equal("container", exception.ParamName);
    }

    [Fact]
    public void PlaceIn_WithPosition_SetsLocalPosition()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));
        var position = new Vector2D<float>(10f, 20f);

        // Act
        item.PlaceIn(container, _world, _transformSystem, position);

        // Assert
        var transformQuery = _world.Query<TransformComponent>()
            .FirstOrDefault(t => t.Entity.Id == item.Id);
        
        Assert.NotEqual(0, transformQuery.Entity.Id);
        Assert.Equal(position, transformQuery.Component1.LocalPosition);
    }

    [Fact]
    public void PlaceIn_WithNullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            item.PlaceIn(container, null!, _transformSystem));
    }

    [Fact]
    public void PlaceIn_WithNullTransformSystem_ThrowsArgumentNullException()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            item.PlaceIn(container, _world, null!));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ATTACHTO TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void AttachTo_WithAnyEntity_EstablishesParentChildRelationship()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity(); // No ContainerComponent required
        var position = new Vector2D<float>(5f, 10f);

        // Act
        item.AttachTo(target, position, _world, _transformSystem);

        // Assert
        var itemQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == item.Id);
        var targetQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == target.Id);
        
        Assert.NotEqual(0, itemQuery.Entity.Id);
        Assert.NotEqual(0, targetQuery.Entity.Id);
        Assert.Equal(target, itemQuery.Component1.ParentEntity);
        Assert.True(targetQuery.Component1.HasChild(item.Id));
    }

    [Fact]
    public void AttachTo_SetsLocalPosition()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity();
        var position = new Vector2D<float>(15f, 25f);

        // Act
        item.AttachTo(target, position, _world, _transformSystem);

        // Assert
        var transformQuery = _world.Query<TransformComponent>()
            .FirstOrDefault(t => t.Entity.Id == item.Id);
        
        Assert.NotEqual(0, transformQuery.Entity.Id);
        Assert.Equal(position, transformQuery.Component1.LocalPosition);
    }

    [Fact]
    public void AttachTo_WithNullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity();
        var position = Vector2D<float>.Zero;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            item.AttachTo(target, position, null!, _transformSystem));
    }

    [Fact]
    public void AttachTo_WithNullTransformSystem_ThrowsArgumentNullException()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity();
        var position = Vector2D<float>.Zero;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            item.AttachTo(target, position, _world, null!));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LOADTOWORLD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LoadToWorld_RemovesFromParent()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));
        
        // First place item in container
        item.PlaceIn(container, _world, _transformSystem);

        // Act
        item.LoadToWorld(_world, _transformSystem);

        // Assert
        var itemQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == item.Id);
        
        Assert.NotEqual(0, itemQuery.Entity.Id);
        Assert.True(itemQuery.Component1.IsRoot);
    }

    [Fact]
    public void LoadToWorld_WithPosition_SetsWorldPosition()
    {
        // Arrange
        var item = _world.CreateEntity();
        var worldPosition = new Vector2D<float>(100f, 200f);

        // Act
        item.LoadToWorld(_world, _transformSystem, worldPosition);

        // Assert
        var transformQuery = _world.Query<TransformComponent>()
            .FirstOrDefault(t => t.Entity.Id == item.Id);
        
        Assert.NotEqual(0, transformQuery.Entity.Id);
        Assert.Equal(worldPosition, transformQuery.Component1.LocalPosition);
    }

    [Fact]
    public void LoadToWorld_WithNullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        var item = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            item.LoadToWorld(null!, _transformSystem));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REMOVEFROM TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void RemoveFrom_RemovesFromParentAndResetsPosition()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));
        
        // First place item in container with position
        item.PlaceIn(container, _world, _transformSystem, new Vector2D<float>(50f, 75f));

        // Act
        item.RemoveFrom(_world, _transformSystem);

        // Assert
        var itemQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == item.Id);
        var transformQuery = _world.Query<TransformComponent>()
            .FirstOrDefault(t => t.Entity.Id == item.Id);
        
        Assert.NotEqual(0, itemQuery.Entity.Id);
        Assert.NotEqual(0, transformQuery.Entity.Id);
        Assert.True(itemQuery.Component1.IsRoot);
        Assert.Equal(Vector2D<float>.Zero, transformQuery.Component1.LocalPosition);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY OPERATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void IsInContainer_WithItemInContainer_ReturnsTrue()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));
        item.PlaceIn(container, _world, _transformSystem);

        // Act & Assert
        Assert.True(item.IsInContainer(_world));
    }

    [Fact]
    public void IsInContainer_WithItemAttachedToNonContainer_ReturnsFalse()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity(); // No ContainerComponent
        item.AttachTo(target, Vector2D<float>.Zero, _world, _transformSystem);

        // Act & Assert
        Assert.False(item.IsInContainer(_world));
    }

    [Fact]
    public void IsInContainer_WithRootEntity_ReturnsFalse()
    {
        // Arrange
        var item = _world.CreateEntity();

        // Act & Assert
        Assert.False(item.IsInContainer(_world));
    }

    [Fact]
    public void GetContainer_WithItemInContainer_ReturnsContainer()
    {
        // Arrange
        var item = _world.CreateEntity();
        var container = _world.CreateEntity();
        _world.SetComponent(container, new ContainerComponent("TestContainer"));
        item.PlaceIn(container, _world, _transformSystem);

        // Act
        var result = item.GetContainer(_world);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(container, result.Value);
    }

    [Fact]
    public void GetContainer_WithItemAttachedToNonContainer_ReturnsNull()
    {
        // Arrange
        var item = _world.CreateEntity();
        var target = _world.CreateEntity(); // No ContainerComponent
        item.AttachTo(target, Vector2D<float>.Zero, _world, _transformSystem);

        // Act
        var result = item.GetContainer(_world);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetContainer_WithRootEntity_ReturnsNull()
    {
        // Arrange
        var item = _world.CreateEntity();

        // Act
        var result = item.GetContainer(_world);

        // Assert
        Assert.Null(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void ComplexScenario_InventoryWithMultipleItems()
    {
        // Arrange
        var backpack = _world.CreateEntity();
        _world.SetComponent(backpack, new ContainerComponent("PlayerBackpack"));
        
        var sword = _world.CreateEntity();
        var potion = _world.CreateEntity();
        var key = _world.CreateEntity();

        // Act - Place multiple items in backpack
        sword.PlaceIn(backpack, _world, _transformSystem, new Vector2D<float>(0f, 0f));
        potion.PlaceIn(backpack, _world, _transformSystem, new Vector2D<float>(1f, 0f));
        key.PlaceIn(backpack, _world, _transformSystem, new Vector2D<float>(2f, 0f));

        // Assert
        Assert.True(sword.IsInContainer(_world));
        Assert.True(potion.IsInContainer(_world));
        Assert.True(key.IsInContainer(_world));
        
        Assert.Equal(backpack, sword.GetContainer(_world));
        Assert.Equal(backpack, potion.GetContainer(_world));
        Assert.Equal(backpack, key.GetContainer(_world));

        var backpackQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == backpack.Id);
        Assert.NotEqual(0, backpackQuery.Entity.Id);
        Assert.Equal(3, backpackQuery.Component1.ChildCount);
    }

    [Fact]
    public void ComplexScenario_WeaponWithAttachments()
    {
        // Arrange
        var rifle = _world.CreateEntity();
        var scope = _world.CreateEntity();
        var silencer = _world.CreateEntity();

        // Act - Attach components to rifle
        scope.AttachTo(rifle, new Vector2D<float>(0f, 0.1f), _world, _transformSystem);
        silencer.AttachTo(rifle, new Vector2D<float>(0.5f, 0f), _world, _transformSystem);

        // Assert
        Assert.False(scope.IsInContainer(_world)); // Attached, not contained
        Assert.False(silencer.IsInContainer(_world)); // Attached, not contained
        
        Assert.Null(scope.GetContainer(_world)); // Not in container
        Assert.Null(silencer.GetContainer(_world)); // Not in container

        var rifleQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == rifle.Id);
        Assert.NotEqual(0, rifleQuery.Entity.Id);
        Assert.Equal(2, rifleQuery.Component1.ChildCount);
    }
}