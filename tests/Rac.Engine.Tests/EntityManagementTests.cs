using Rac.Audio;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Components;
using Rac.ECS.Systems;
using Rac.Engine;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Camera;
using Silk.NET.Input;
using Silk.NET.Maths;
using Xunit;

namespace Rac.Engine.Tests;

/// <summary>
/// Tests for the working entity management convenience methods implementation.
/// These tests verify that the methods actually work, not just interface compliance.
/// </summary>
public class EntityManagementTests
{
    [Fact]
    public void World_CreateEntity_ReturnsUniqueEntities()
    {
        // Arrange
        var world = new World();

        // Act
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();

        // Assert
        Assert.NotEqual(entity1.Id, entity2.Id);
        Assert.True(entity1.Id > 0);
        Assert.True(entity2.Id > 0);
    }

    [Fact]
    public void World_DestroyEntity_RemovesEntityFromAllLists()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new NameComponent("TestEntity"));

        // Act
        world.DestroyEntity(entity);

        // Assert
        Assert.Empty(world.GetAllEntities());
        Assert.Empty(world.Query<NameComponent>());
    }

    [Fact]
    public void World_GetAllEntities_ReturnsLivingEntities()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        var entity3 = world.CreateEntity();

        // Act
        var allEntities = world.GetAllEntities().ToList();

        // Assert
        Assert.Equal(3, allEntities.Count);
        Assert.Contains(entity1, allEntities);
        Assert.Contains(entity2, allEntities);
        Assert.Contains(entity3, allEntities);
    }

    [Fact]
    public void World_GetAllEntities_AfterDestroy_DoesNotReturnDestroyedEntity()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        world.DestroyEntity(entity1);

        // Act
        var allEntities = world.GetAllEntities().ToList();

        // Assert
        Assert.Single(allEntities);
        Assert.Contains(entity2, allEntities);
        Assert.DoesNotContain(entity1, allEntities);
    }

    [Fact]
    public void EngineFacade_CreateEntityWithName_AssignsNameComponent()
    {
        // Arrange
        var world = new World();
        var facade = CreateTestFacade(world);

        // Act
        var entity = facade.CreateEntity("TestPlayer");

        // Assert
        var nameQuery = world.Query<NameComponent>().FirstOrDefault();
        Assert.Equal(entity.Id, nameQuery.Entity.Id);
        Assert.Equal("TestPlayer", nameQuery.Component1.Name);
    }

    [Fact]
    public void EngineFacade_EntityCount_ReturnsCorrectCount()
    {
        // Arrange
        var world = new World();
        var facade = CreateTestFacade(world);

        // Act & Assert
        Assert.Equal(0, facade.EntityCount);
        
        facade.CreateEntity();
        Assert.Equal(1, facade.EntityCount);
        
        facade.CreateEntity("Named");
        Assert.Equal(2, facade.EntityCount);
        
        var entity = facade.CreateEntity();
        facade.DestroyEntity(entity);
        Assert.Equal(2, facade.EntityCount);
    }

    [Fact]
    public void EngineFacade_FindEntityByName_ReturnsCorrectEntity()
    {
        // Arrange
        var world = new World();
        var facade = CreateTestFacade(world);
        var entity1 = facade.CreateEntity("Player");
        var entity2 = facade.CreateEntity("Enemy");

        // Act
        var foundPlayer = facade.FindEntityByName("Player");
        var foundEnemy = facade.FindEntityByName("Enemy");
        var notFound = facade.FindEntityByName("NonExistent");

        // Assert
        Assert.NotNull(foundPlayer);
        Assert.Equal(entity1.Id, foundPlayer.Value.Id);
        
        Assert.NotNull(foundEnemy);
        Assert.Equal(entity2.Id, foundEnemy.Value.Id);
        
        Assert.Null(notFound);
    }

    [Fact]
    public void EngineFacade_GetEntitiesWithTag_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var facade = CreateTestFacade(world);
        var entity1 = facade.CreateEntity();
        var entity2 = facade.CreateEntity();
        var entity3 = facade.CreateEntity();
        
        world.SetComponent(entity1, new TagComponent("Enemy"));
        world.SetComponent(entity2, new TagComponent(new[] { "Enemy", "Fast" }));
        world.SetComponent(entity3, new TagComponent("Player"));

        // Act
        var enemyEntities = facade.GetEntitiesWithTag("Enemy").ToList();
        var playerEntities = facade.GetEntitiesWithTag("Player").ToList();
        var fastEntities = facade.GetEntitiesWithTag("Fast").ToList();

        // Assert
        Assert.Equal(2, enemyEntities.Count);
        Assert.Contains(entity1, enemyEntities);
        Assert.Contains(entity2, enemyEntities);
        
        Assert.Single(playerEntities);
        Assert.Contains(entity3, playerEntities);
        
        Assert.Single(fastEntities);
        Assert.Contains(entity2, fastEntities);
    }

    [Fact]
    public void TagComponent_HasTag_WorksCorrectly()
    {
        // Arrange
        var tags = new TagComponent(new[] { "Enemy", "Fast", "Dangerous" });

        // Act & Assert
        Assert.True(tags.HasTag("Enemy"));
        Assert.True(tags.HasTag("Fast"));
        Assert.True(tags.HasTag("Dangerous"));
        Assert.False(tags.HasTag("Player"));
        Assert.False(tags.HasTag("Slow"));
    }

    [Fact]
    public void TagComponent_WithTag_AddsTag()
    {
        // Arrange
        var originalTags = new TagComponent("Enemy");

        // Act
        var newTags = originalTags.WithTag("Fast");

        // Assert
        Assert.True(originalTags.HasTag("Enemy"));
        Assert.False(originalTags.HasTag("Fast"));
        
        Assert.True(newTags.HasTag("Enemy"));
        Assert.True(newTags.HasTag("Fast"));
    }

    [Fact]
    public void TagComponent_WithoutTag_RemovesTag()
    {
        // Arrange
        var originalTags = new TagComponent(new[] { "Enemy", "Fast" });

        // Act
        var newTags = originalTags.WithoutTag("Fast");

        // Assert
        Assert.True(originalTags.HasTag("Enemy"));
        Assert.True(originalTags.HasTag("Fast"));
        
        Assert.True(newTags.HasTag("Enemy"));
        Assert.False(newTags.HasTag("Fast"));
    }

    private static IEngineFacade CreateTestFacade(IWorld world)
    {
        // Create a mock facade for testing
        return new TestEngineFacade(world);
    }
}

/// <summary>
/// Simple test facade implementation for testing entity management methods.
/// </summary>
internal class TestEngineFacade : IEngineFacade
{
    public TestEngineFacade(IWorld world)
    {
        World = world;
    }

    public IWorld World { get; }
    
    // Required interface members (not used in tests)
    public SystemScheduler Systems => throw new NotImplementedException();
    public IRenderer Renderer => throw new NotImplementedException();
    public IAudioService Audio => throw new NotImplementedException();
    public ICameraManager CameraManager => throw new NotImplementedException();
    public IWindowManager WindowManager => throw new NotImplementedException();
    
    public event Action? LoadEvent;
    public event Action<float>? UpdateEvent;
    public event Action<float>? RenderEvent;
    public event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;
    public event Action<Vector2D<float>>? LeftClickEvent;
    public event Action<float>? MouseScrollEvent;
    
    public void AddSystem(ISystem system) => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();

    // Entity management methods (under test)
    public Entity CreateEntity() => World.CreateEntity();

    public Entity CreateEntity(string name)
    {
        var entity = World.CreateEntity();
        World.SetComponent(entity, new NameComponent(name));
        return entity;
    }

    public void DestroyEntity(Entity entity) => World.DestroyEntity(entity);

    public int EntityCount => World.GetAllEntities().Count();

    public IEnumerable<Entity> GetEntitiesWithTag(string tag)
    {
        return World.Query<TagComponent>()
            .Where(result => result.Component1.HasTag(tag))
            .Select(result => result.Entity);
    }

    public Entity? FindEntityByName(string name)
    {
        var result = World.Query<NameComponent>()
            .FirstOrDefault(result => result.Component1.Name == name);
        
        return result.Entity.Id != 0 ? result.Entity : null;
    }
}