using Rac.ECS.Components;
using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

/// <summary>
/// Tests for ECS debugging and development tools.
/// Validates entity inspection, naming, and tag-based query functionality.
/// </summary>
public class DebuggingToolsTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // INSPECT ENTITY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void InspectEntity_ReturnsEmptyDictionary_WhenEntityHasNoComponents()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var inspection = world.InspectEntity(entity);

        // Assert
        Assert.NotNull(inspection);
        Assert.Empty(inspection);
    }

    [Fact]
    public void InspectEntity_ReturnsAllComponents_WhenEntityHasMultipleComponents()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var nameComponent = new NameComponent("TestEntity");
        var tagComponent = new TagComponent(new[] { "Enemy", "Hostile" });
        var testComponent = new TestComponent(42);

        world.SetComponent(entity, nameComponent);
        world.SetComponent(entity, tagComponent);
        world.SetComponent(entity, testComponent);

        // Act
        var inspection = world.InspectEntity(entity);

        // Assert
        Assert.Equal(3, inspection.Count);
        Assert.Contains("Name", inspection.Keys);
        Assert.Contains("Tag", inspection.Keys);
        Assert.Contains("Test", inspection.Keys);
        
        Assert.Equal(nameComponent, inspection["Name"]);
        Assert.Equal(tagComponent, inspection["Tag"]);
        Assert.Equal(testComponent, inspection["Test"]);
    }

    [Fact]
    public void InspectEntity_ThrowsArgumentException_WhenEntityDoesNotExist()
    {
        // Arrange
        var world = new World();
        var nonExistentEntity = new Entity(999);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => world.InspectEntity(nonExistentEntity));
        Assert.Contains("Entity with ID 999 does not exist", exception.Message);
        Assert.Equal("entity", exception.ParamName);
    }

    [Fact]
    public void InspectEntity_ThrowsArgumentException_WhenEntityIsDestroyed()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new TestComponent(42));
        world.DestroyEntity(entity);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => world.InspectEntity(entity));
        Assert.Contains("does not exist or has been destroyed", exception.Message);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET ENTITY NAME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GetEntityName_ReturnsName_WhenEntityHasNameComponent()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var expectedName = "PlayerCharacter";
        world.SetComponent(entity, new NameComponent(expectedName));

        // Act
        var actualName = world.GetEntityName(entity);

        // Assert
        Assert.Equal(expectedName, actualName);
    }

    [Fact]
    public void GetEntityName_ReturnsEntityId_WhenEntityHasNoNameComponent()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        var name = world.GetEntityName(entity);

        // Assert
        Assert.Equal($"Entity #{entity.Id}", name);
    }

    [Fact]
    public void GetEntityName_ReturnsCorrectName_ForMultipleEntities()
    {
        // Arrange
        var world = new World();
        var namedEntity = world.CreateEntity();
        var unnamedEntity = world.CreateEntity();
        
        world.SetComponent(namedEntity, new NameComponent("NamedEntity"));

        // Act
        var namedEntityName = world.GetEntityName(namedEntity);
        var unnamedEntityName = world.GetEntityName(unnamedEntity);

        // Assert
        Assert.Equal("NamedEntity", namedEntityName);
        Assert.Equal($"Entity #{unnamedEntity.Id}", unnamedEntityName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET ENTITIES WITH TAG TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GetEntitiesWithTag_ReturnsEmpty_WhenNoEntitiesHaveTag()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new TagComponent(new[] { "Friendly" }));

        // Act
        var result = world.GetEntitiesWithTag("Enemy");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetEntitiesWithTag_ReturnsMatchingEntities_WhenEntitiesHaveTag()
    {
        // Arrange
        var world = new World();
        var enemy1 = world.CreateEntity();
        var enemy2 = world.CreateEntity();
        var friendly = world.CreateEntity();

        world.SetComponent(enemy1, new TagComponent(new[] { "Enemy", "Hostile" }));
        world.SetComponent(enemy2, new TagComponent(new[] { "Enemy", "Weak" }));
        world.SetComponent(friendly, new TagComponent(new[] { "Friendly" }));

        // Act
        var enemies = world.GetEntitiesWithTag("Enemy").ToList();

        // Assert
        Assert.Equal(2, enemies.Count);
        Assert.Contains(enemy1, enemies);
        Assert.Contains(enemy2, enemies);
        Assert.DoesNotContain(friendly, enemies);
    }

    [Fact]
    public void GetEntitiesWithTag_ReturnsEmpty_WhenNoEntitiesExist()
    {
        // Arrange
        var world = new World();

        // Act
        var result = world.GetEntitiesWithTag("AnyTag");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetEntitiesWithTag_ThrowsArgumentNullException_WhenTagIsNull()
    {
        // Arrange
        var world = new World();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => world.GetEntitiesWithTag(null!));
        Assert.Equal("tag", exception.ParamName);
        Assert.Contains("Tag cannot be null", exception.Message);
    }

    [Fact]
    public void GetEntitiesWithTag_HandlesEmptyTag()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new TagComponent(new[] { string.Empty, "ValidTag" }));

        // Act
        var result = world.GetEntitiesWithTag(string.Empty).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(entity, result[0]);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NULL WORLD DEBUGGING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void NullWorld_InspectEntity_ReturnsEmptyDictionary()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = nullWorld.CreateEntity();

        // Act
        var inspection = nullWorld.InspectEntity(entity);

        // Assert
        Assert.NotNull(inspection);
        Assert.Empty(inspection);
    }

    [Fact]
    public void NullWorld_GetEntityName_ReturnsNullEntityName()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = nullWorld.CreateEntity();

        // Act
        var name = nullWorld.GetEntityName(entity);

        // Assert
        Assert.Equal($"NullEntity #{entity.Id}", name);
    }

    [Fact]
    public void NullWorld_GetEntitiesWithTag_ReturnsEmpty()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var result = nullWorld.GetEntitiesWithTag("AnyTag");

        // Assert
        Assert.Empty(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DebuggingTools_WorkTogether_InComplexScenario()
    {
        // Arrange - Create a complex scenario with multiple entities
        var world = new World();
        
        var player = world.CreateEntity();
        world.SetComponent(player, new NameComponent("Player"));
        world.SetComponent(player, new TagComponent(new[] { "Player", "Controllable" }));
        
        var enemy1 = world.CreateEntity();
        world.SetComponent(enemy1, new NameComponent("Goblin"));
        world.SetComponent(enemy1, new TagComponent(new[] { "Enemy", "Weak" }));
        
        var enemy2 = world.CreateEntity();
        world.SetComponent(enemy2, new TagComponent(new[] { "Enemy", "Strong" }));

        // Act & Assert - Test all debugging tools together
        
        // 1. Inspect entities
        var playerInspection = world.InspectEntity(player);
        Assert.Equal(2, playerInspection.Count);
        Assert.Contains("Name", playerInspection.Keys);
        Assert.Contains("Tag", playerInspection.Keys);
        
        // 2. Get entity names
        Assert.Equal("Player", world.GetEntityName(player));
        Assert.Equal("Goblin", world.GetEntityName(enemy1));
        Assert.Equal($"Entity #{enemy2.Id}", world.GetEntityName(enemy2));
        
        // 3. Query by tags
        var enemies = world.GetEntitiesWithTag("Enemy").ToList();
        Assert.Equal(2, enemies.Count);
        Assert.Contains(enemy1, enemies);
        Assert.Contains(enemy2, enemies);
        
        var controllableEntities = world.GetEntitiesWithTag("Controllable").ToList();
        Assert.Single(controllableEntities);
        Assert.Equal(player, controllableEntities[0]);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEVELOPMENT MODE VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void SetComponent_ThrowsArgumentException_WhenEntityDoesNotExist()
    {
        // Arrange
        var world = new World();
        var nonExistentEntity = new Entity(999);
        var component = new TestComponent(42);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => world.SetComponent(nonExistentEntity, component));
        Assert.Contains("Entity with ID 999", exception.Message);
        Assert.Contains("does not exist or has been destroyed", exception.Message);
        Assert.Equal("entity", exception.ParamName);
    }

    [Fact]
    public void SetComponent_ThrowsArgumentException_WhenEntityIsDestroyed()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.DestroyEntity(entity);
        var component = new TestComponent(42);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => world.SetComponent(entity, component));
        Assert.Contains("does not exist or has been destroyed", exception.Message);
    }

    [Fact]
    public void SetComponent_ProvidesHelpfulErrorMessage_ForInvalidEntityId()
    {
        // Arrange
        var world = new World();
        var invalidEntity = new Entity(-1);
        var component = new TestComponent(42);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => world.SetComponent(invalidEntity, component));
        Assert.Contains("appears to be invalid", exception.Message);
        Assert.Contains("Ensure you're using entities created by World.CreateEntity()", exception.Message);
    }

    [Fact]
    public void HasComponent_ReturnsFalse_ForNonExistentEntity()
    {
        // Arrange
        var world = new World();
        var nonExistentEntity = new Entity(999);

        // Act
        var hasComponent = world.HasComponent<TestComponent>(nonExistentEntity);

        // Assert
        Assert.False(hasComponent);
    }

    [Fact]
    public void HasComponent_ReturnsFalse_ForDestroyedEntity()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new TestComponent(42));
        world.DestroyEntity(entity);

        // Act
        var hasComponent = world.HasComponent<TestComponent>(entity);

        // Assert
        Assert.False(hasComponent);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPER COMPONENTS
    // ═══════════════════════════════════════════════════════════════════════════

    private record struct TestComponent(int Value) : IComponent;
}