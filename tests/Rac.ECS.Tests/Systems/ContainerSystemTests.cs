using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Systems;

public class ContainerSystemTests
{
    private readonly World _world;
    private readonly ContainerSystem _containerSystem;
    private readonly TransformSystem _transformSystem;

    public ContainerSystemTests()
    {
        _world = new World();
        _containerSystem = new ContainerSystem();
        _transformSystem = new TransformSystem();
        
        _containerSystem.Initialize(_world);
        _transformSystem.Initialize(_world);
        _containerSystem.SetTransformSystem(_transformSystem);
    }

    // Helper method to get component using query system
    private T GetComponentByQuery<T>(Entity entity) where T : struct, IComponent
    {
        var query = _world.Query<T>()
            .FirstOrDefault(q => q.Entity.Id == entity.Id);
        
        if (query.Entity.Id == 0)
            throw new InvalidOperationException($"Entity {entity.Id} does not have component {typeof(T).Name}");
            
        return query.Component1;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM LIFECYCLE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Initialize_WithValidWorld_Succeeds()
    {
        // Arrange
        var world = new World();
        var system = new ContainerSystem();

        // Act & Assert - Should not throw
        system.Initialize(world);
    }

    [Fact]
    public void Initialize_WithNullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        var system = new ContainerSystem();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => system.Initialize(null!));
    }

    [Fact]
    public void Update_WithoutErrors_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        _containerSystem.Update(0.016f);
    }

    [Fact]
    public void Shutdown_WithValidWorld_Succeeds()
    {
        // Act & Assert - Should not throw
        _containerSystem.Shutdown(_world);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER CREATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CreateContainer_WithName_CreatesValidContainer()
    {
        // Arrange
        var name = "TestContainer";

        // Act
        var container = _containerSystem.CreateContainer(name);

        // Assert
        Assert.True(container.IsAlive);
        Assert.True(_world.HasComponent<ContainerComponent>(container));
        Assert.True(_world.HasComponent<NameComponent>(container));
        Assert.True(_world.HasComponent<TransformComponent>(container));
        Assert.True(_world.HasComponent<ParentHierarchyComponent>(container));

        var containerComp = GetComponentByQuery<ContainerComponent>(container);
        Assert.Equal(name, containerComp.ContainerName);
        Assert.True(containerComp.IsLoaded);
        Assert.False(containerComp.IsPersistent);
    }

    [Fact]
    public void CreateContainer_WithAllParameters_CreatesValidContainer()
    {
        // Arrange
        var name = "PersistentContainer";
        var position = new Vector2D<float>(100f, 200f);
        var isLoaded = false;
        var isPersistent = true;

        // Act
        var container = _containerSystem.CreateContainer(name, position, isLoaded, isPersistent);

        // Assert
        var containerComp = GetComponentByQuery<ContainerComponent>(container);
        var nameComp = GetComponentByQuery<NameComponent>(container);
        var transformComp = GetComponentByQuery<TransformComponent>(container);

        Assert.Equal(name, containerComp.ContainerName);
        Assert.Equal(isLoaded, containerComp.IsLoaded);
        Assert.Equal(isPersistent, containerComp.IsPersistent);
        Assert.Equal(name, nameComp.Name);
        Assert.Equal(position, transformComp.LocalPosition);
    }

    [Fact]
    public void CreateContainer_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.CreateContainer(string.Empty));
    }

    [Fact]
    public void CreateContainer_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.CreateContainer(null!));
    }

    [Fact]
    public void CreateContainer_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var system = new ContainerSystem();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            system.CreateContainer("Test"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER DESTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DestroyContainer_WithValidContainer_RemovesComponents()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");

        // Act
        _containerSystem.DestroyContainer(container);

        // Assert
        Assert.False(_world.HasComponent<ContainerComponent>(container));
        Assert.False(_world.HasComponent<ParentHierarchyComponent>(container));
        Assert.False(_world.HasComponent<TransformComponent>(container));
        Assert.False(_world.HasComponent<NameComponent>(container));
    }

    [Fact]
    public void DestroyContainer_WithNonContainer_ThrowsArgumentException()
    {
        // Arrange
        var notContainer = _world.CreateEntity();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            _containerSystem.DestroyContainer(notContainer));
        
        Assert.Contains("is not a container", exception.Message);
    }

    [Fact]
    public void DestroyContainer_WithContainedEntities_DestroysThem()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");
        var item1 = _world.CreateEntity();
        var item2 = _world.CreateEntity();
        
        item1.PlaceIn(container, _world, _transformSystem);
        item2.PlaceIn(container, _world, _transformSystem);

        // Act
        _containerSystem.DestroyContainer(container, destroyContainedEntities: true);

        // Assert
        Assert.False(_world.HasComponent<ContainerComponent>(container));
        Assert.False(_world.HasComponent<ParentHierarchyComponent>(item1));
        Assert.False(_world.HasComponent<ParentHierarchyComponent>(item2));
    }

    [Fact]
    public void DestroyContainer_WithoutDestroyingContents_MovesToWorld()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");
        var item = _world.CreateEntity();
        
        item.PlaceIn(container, _world, _transformSystem);

        // Act
        _containerSystem.DestroyContainer(container, destroyContainedEntities: false);

        // Assert
        Assert.False(_world.HasComponent<ContainerComponent>(container));
        
        // Item should still exist and be a root entity
        Assert.True(_world.HasComponent<ParentHierarchyComponent>(item));
        var itemHierarchy = GetComponentByQuery<ParentHierarchyComponent>(item);
        Assert.True(itemHierarchy.IsRoot);
    }

    [Fact]
    public void DestroyContainer_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var system = new ContainerSystem();
        var container = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            system.DestroyContainer(container));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER STATE MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void SetContainerLoaded_WithValidContainer_UpdatesState()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");

        // Act
        _containerSystem.SetContainerLoaded(container, false);

        // Assert
        var containerComp = GetComponentByQuery<ContainerComponent>(container);
        Assert.False(containerComp.IsLoaded);
    }

    [Fact]
    public void SetContainerLoaded_WithNonContainer_ThrowsArgumentException()
    {
        // Arrange
        var notContainer = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.SetContainerLoaded(notContainer, false));
    }

    [Fact]
    public void SetContainerPersistent_WithValidContainer_UpdatesState()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");

        // Act
        _containerSystem.SetContainerPersistent(container, true);

        // Assert
        var containerComp = GetComponentByQuery<ContainerComponent>(container);
        Assert.True(containerComp.IsPersistent);
    }

    [Fact]
    public void SetContainerPersistent_WithNonContainer_ThrowsArgumentException()
    {
        // Arrange
        var notContainer = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.SetContainerPersistent(notContainer, true));
    }

    [Fact]
    public void RenameContainer_WithValidContainer_UpdatesName()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("OldName");
        var newName = "NewName";

        // Act
        _containerSystem.RenameContainer(container, newName);

        // Assert
        var containerComp = GetComponentByQuery<ContainerComponent>(container);
        var nameComp = GetComponentByQuery<NameComponent>(container);
        
        Assert.Equal(newName, containerComp.ContainerName);
        Assert.Equal(newName, nameComp.Name);
    }

    [Fact]
    public void RenameContainer_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var container = _containerSystem.CreateContainer("TestContainer");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.RenameContainer(container, string.Empty));
    }

    [Fact]
    public void RenameContainer_WithNonContainer_ThrowsArgumentException()
    {
        // Arrange
        var notContainer = _world.CreateEntity();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _containerSystem.RenameContainer(notContainer, "NewName"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void ComplexScenario_InventoryManagement()
    {
        // Arrange - Create containers and items
        var playerBackpack = _containerSystem.CreateContainer("PlayerBackpack");
        var treasureChest = _containerSystem.CreateContainer("TreasureChest", Vector2D<float>.Zero, true, true);
        
        var sword = _world.CreateEntity();
        var potion = _world.CreateEntity();
        var key = _world.CreateEntity();

        // Act - Place items in containers
        sword.PlaceIn(playerBackpack, _world, _transformSystem);
        potion.PlaceIn(treasureChest, _world, _transformSystem);
        key.PlaceIn(playerBackpack, _world, _transformSystem);

        // Move potion from treasure chest to backpack
        potion.RemoveFrom(_world, _transformSystem);
        potion.PlaceIn(playerBackpack, _world, _transformSystem);

        // Assert - Verify final state
        Assert.True(sword.IsInContainer(_world));
        Assert.True(potion.IsInContainer(_world));
        Assert.True(key.IsInContainer(_world));
        
        Assert.Equal(playerBackpack, sword.GetContainer(_world));
        Assert.Equal(playerBackpack, potion.GetContainer(_world));
        Assert.Equal(playerBackpack, key.GetContainer(_world));

        var backpackHierarchy = GetComponentByQuery<ParentHierarchyComponent>(playerBackpack);
        var chestHierarchy = GetComponentByQuery<ParentHierarchyComponent>(treasureChest);
        
        Assert.Equal(3, backpackHierarchy.ChildCount);
        Assert.Equal(0, chestHierarchy.ChildCount);
    }

    [Fact]
    public void ComplexScenario_NestedContainers()
    {
        // Arrange - Create nested container structure
        var room = _containerSystem.CreateContainer("Room");
        var chest = _containerSystem.CreateContainer("Chest");
        var backpack = _containerSystem.CreateContainer("Backpack");
        
        var sword = _world.CreateEntity();

        // Act - Create nested structure: room -> chest -> backpack -> sword
        chest.PlaceIn(room, _world, _transformSystem);
        backpack.PlaceIn(chest, _world, _transformSystem);
        sword.PlaceIn(backpack, _world, _transformSystem);

        // Assert - Verify hierarchy
        Assert.True(chest.IsInContainer(_world));
        Assert.True(backpack.IsInContainer(_world));
        Assert.True(sword.IsInContainer(_world));
        
        Assert.Equal(room, chest.GetContainer(_world));
        Assert.Equal(chest, backpack.GetContainer(_world));
        Assert.Equal(backpack, sword.GetContainer(_world));

        // Destroy middle container and verify cleanup
        _containerSystem.DestroyContainer(chest, destroyContainedEntities: true);
        
        Assert.False(_world.HasComponent<ContainerComponent>(chest));
        Assert.False(_world.HasComponent<ContainerComponent>(backpack));
        Assert.False(_world.HasComponent<ParentHierarchyComponent>(sword));
    }

    [Fact]
    public void ComplexScenario_MixedContainmentAndAttachment()
    {
        // Arrange - Create mixed scenario
        var playerBackpack = _containerSystem.CreateContainer("PlayerBackpack");
        var rifle = _world.CreateEntity();
        var scope = _world.CreateEntity();
        
        // Act - Place rifle in backpack, attach scope to rifle
        rifle.PlaceIn(playerBackpack, _world, _transformSystem);
        scope.AttachTo(rifle, new Vector2D<float>(0f, 0.1f), _world, _transformSystem);

        // Assert - Verify mixed relationships
        Assert.True(rifle.IsInContainer(_world));
        Assert.False(scope.IsInContainer(_world)); // Attached, not contained
        
        Assert.Equal(playerBackpack, rifle.GetContainer(_world));
        Assert.Null(scope.GetContainer(_world));

        // Verify hierarchy counts
        var backpackHierarchy = GetComponentByQuery<ParentHierarchyComponent>(playerBackpack);
        var rifleHierarchy = GetComponentByQuery<ParentHierarchyComponent>(rifle);
        
        Assert.Equal(1, backpackHierarchy.ChildCount); // Contains rifle
        Assert.Equal(1, rifleHierarchy.ChildCount); // Has scope attached
    }
}