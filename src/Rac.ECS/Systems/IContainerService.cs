using Rac.ECS.Components;
using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.ECS.Systems;

/// <summary>
/// Interface for container lifecycle management and basic container operations.
/// 
/// PROGRESSIVE COMPLEXITY DESIGN:
/// This interface follows RACEngine's progressive complexity principle, providing
/// both simple operations for common use cases and advanced features for complex scenarios.
/// 
/// EDUCATIONAL NOTES:
/// Service interfaces in game engines serve multiple architectural purposes:
/// - Dependency Injection: Enable modular system composition and testing
/// - Multiple Implementations: Support different backends (production vs debug vs null)
/// - Abstraction: Hide implementation complexity behind well-defined contracts
/// - Mockability: Enable comprehensive unit testing of dependent systems
/// 
/// CONTAINER LIFECYCLE MANAGEMENT:
/// - Container Creation: Establishes new containers with proper components
/// - Container Destruction: Safely removes containers and handles contained entities
/// - Container State: Manages loaded/unloaded state for streaming scenarios
/// - Container Persistence: Handles containers that persist across scenes
/// 
/// USAGE PATTERNS:
/// - Inventory Systems: Create/destroy backpacks, chests, storage containers
/// - Scene Management: Create/destroy level sections, area containers
/// - Prefab Systems: Create/destroy vehicle assemblies, building components
/// - Modding Infrastructure: Create/destroy mod containers, asset bundles
/// 
/// IMPLEMENTATION REQUIREMENTS:
/// All implementations must provide a corresponding null object implementation
/// following the NullContainerService pattern for graceful degradation scenarios.
/// </summary>
public interface IContainerService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SIMPLE OPERATIONS (80% of use cases)
    // Educational note: Most common container operations get the simplest API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Simple factory method for most common container creation scenario.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    /// <example>
    /// <code>
    /// var backpack = containerService.CreateContainer("PlayerBackpack");
    /// var chest = containerService.CreateContainer("TreasureChest");
    /// </code>
    /// </example>
    Entity CreateContainer(string containerName);

    /// <summary>
    /// Destroys a container and all contained entities.
    /// Simple cleanup method for most common destruction scenario.
    /// </summary>
    /// <param name="container">The container entity to destroy</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    /// <example>
    /// <code>
    /// containerService.DestroyContainer(backpack);
    /// </code>
    /// </example>
    void DestroyContainer(Entity container);

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERMEDIATE OPERATIONS (15% of use cases)
    // Educational note: Progressive complexity - same operations with more control
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with position and basic configuration.
    /// Intermediate factory method with common customization options.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <param name="position">World position for the container</param>
    /// <param name="isLoaded">Whether the container starts in loaded state (optional, defaults to true)</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    /// <example>
    /// <code>
    /// var chest = containerService.CreateContainer("TreasureChest", 
    ///     new Vector2D&lt;float&gt;(100f, 200f), isLoaded: true);
    /// </code>
    /// </example>
    Entity CreateContainer(string containerName, Vector2D<float> position, bool isLoaded = true);

    /// <summary>
    /// Destroys a container with configurable handling of contained entities.
    /// Intermediate cleanup method with control over entity preservation.
    /// </summary>
    /// <param name="container">The container entity to destroy</param>
    /// <param name="destroyContainedEntities">Whether to destroy all contained entities (default: true)</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    /// <example>
    /// <code>
    /// // Destroy container and all contents
    /// containerService.DestroyContainer(backpack, destroyContainedEntities: true);
    /// 
    /// // Destroy container but move contents to world
    /// containerService.DestroyContainer(backpack, destroyContainedEntities: false);
    /// </code>
    /// </example>
    void DestroyContainer(Entity container, bool destroyContainedEntities);

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED OPERATIONS (5% of use cases)
    // Educational note: Full control for specialized scenarios
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with full configuration options.
    /// Advanced factory method for complex container setup scenarios.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <param name="position">World position for the container</param>
    /// <param name="isLoaded">Whether the container starts in loaded state</param>
    /// <param name="isPersistent">Whether the container persists across scenes</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    /// <example>
    /// <code>
    /// var persistentChest = containerService.CreateContainer("TreasureChest", 
    ///     new Vector2D&lt;float&gt;(100f, 200f), isLoaded: true, isPersistent: true);
    /// </code>
    /// </example>
    Entity CreateContainer(string containerName, Vector2D<float> position, bool isLoaded, bool isPersistent);

    /// <summary>
    /// Sets the loaded state of a container.
    /// Advanced state management for streaming scenarios.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="isLoaded">New loaded state</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    void SetContainerLoaded(Entity container, bool isLoaded);

    /// <summary>
    /// Sets the persistence state of a container.
    /// Advanced state management for scene lifecycle scenarios.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="isPersistent">New persistence state</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    void SetContainerPersistent(Entity container, bool isPersistent);

    /// <summary>
    /// Renames a container.
    /// Advanced management operation for dynamic container scenarios.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="newName">New name for the container</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container or name is invalid</exception>
    void RenameContainer(Entity container, string newName);
}