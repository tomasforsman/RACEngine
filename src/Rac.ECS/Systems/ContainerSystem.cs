using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;

namespace Rac.ECS.Systems;

/// <summary>
/// System for managing container lifecycle and basic container operations.
/// Implements IContainerService interface following RACEngine's progressive complexity principle.
/// 
/// PROGRESSIVE COMPLEXITY IMPLEMENTATION:
/// This class serves as the concrete implementation layer in RACEngine's three-tier architecture:
/// - Facade Layer: Simple operations through EngineFacade convenience methods
/// - Service Layer: Full feature set through IContainerService interface (this class)
/// - Implementation Layer: Direct access to ContainerSystem for specialized scenarios
/// 
/// CONTAINER LIFECYCLE MANAGEMENT:
/// - Container Creation: Establishes new containers with proper components
/// - Container Destruction: Safely removes containers and handles contained entities
/// - Container State: Manages loaded/unloaded state for streaming scenarios
/// - Container Persistence: Handles containers that persist across scenes
/// 
/// EDUCATIONAL NOTES:
/// This system demonstrates several important patterns in game engine architecture:
/// - Service Interface Pattern: Implements IContainerService for dependency injection and testing
/// - Factory Pattern: CreateContainer methods act as factories for container entities
/// - Lifecycle Management: Proper initialization and cleanup of complex entities
/// - Composition over Inheritance: Containers are entities with components, not special types
/// - System Coordination: Works with TransformSystem for spatial relationships
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Container operations are infrequent (creation/destruction)
/// - Update loop is lightweight - only handles state transitions
/// - Uses existing ECS queries for efficient entity management
/// - Batch operations when possible for multiple container changes
/// 
/// USAGE PATTERNS:
/// - Inventory Systems: Create/destroy backpacks, chests, storage containers
/// - Scene Management: Create/destroy level sections, area containers
/// - Prefab Systems: Create/destroy vehicle assemblies, building components
/// - Modding Infrastructure: Create/destroy mod containers, asset bundles
/// 
/// SYSTEM DEPENDENCIES:
/// - Requires: IWorld for entity and component management
/// - Uses: TransformSystem for spatial hierarchy management
/// - Produces: Entities with ContainerComponent and supporting components
/// - Integrates: With existing ParentHierarchyComponent for entity relationships
/// </summary>
public class ContainerSystem : ISystem, IContainerService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM STATE AND DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private IWorld? _world;
    private TransformSystem? _transformSystem;

    /// <summary>
    /// Initializes the container system with the ECS world.
    /// </summary>
    /// <param name="world">The ECS world containing entities and components</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        
        // TransformSystem will be set when available - container system can work without it
        // but spatial operations will be limited
    }

    /// <summary>
    /// Sets the transform system for spatial operations.
    /// This is called by the system scheduler when TransformSystem is available.
    /// </summary>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    public void SetTransformSystem(TransformSystem transformSystem)
    {
        _transformSystem = transformSystem;
    }

    /// <summary>
    /// Updates container lifecycle and state management.
    /// Handles loading/unloading of containers and persistence management.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public void Update(float deltaTime)
    {
        // In a basic implementation, container lifecycle is handled by explicit calls
        // This Update method can be extended for:
        // - Streaming: Load/unload containers based on distance or LOD
        // - Persistence: Save/restore container state across scenes
        // - Optimization: Batch container operations for performance
        
        // For now, this is a no-op as containers are managed through explicit API calls
        // Future enhancements could include:
        // - Automatic container cleanup based on empty state
        // - Container state validation and repair
        // - Performance metrics collection
    }

    /// <summary>
    /// Shuts down the container system and cleans up resources.
    /// </summary>
    /// <param name="world">The ECS world (unused in basic implementation)</param>
    public void Shutdown(IWorld world)
    {
        // Clean up any system-specific resources
        // In basic implementation, no cleanup needed
        _transformSystem = null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION - SIMPLE OPERATIONS
    // Educational note: Simple overloads delegate to advanced methods with sensible defaults
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Simple factory method implementation delegating to advanced method with default parameters.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    public Entity CreateContainer(string containerName)
    {
        return CreateContainer(containerName, Vector2D<float>.Zero, isLoaded: true, isPersistent: false);
    }

    /// <summary>
    /// Creates a new container entity with position and basic configuration.
    /// Intermediate factory method implementation delegating to advanced method.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <param name="position">World position for the container</param>
    /// <param name="isLoaded">Whether the container starts in loaded state (optional, defaults to true)</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    public Entity CreateContainer(string containerName, Vector2D<float> position, bool isLoaded = true)
    {
        return CreateContainer(containerName, position, isLoaded, isPersistent: false);
    }

    /// <summary>
    /// Destroys a container and all contained entities.
    /// Simple cleanup method implementation delegating to advanced method with default behavior.
    /// </summary>
    /// <param name="container">The container entity to destroy</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    public void DestroyContainer(Entity container)
    {
        DestroyContainer(container, destroyContainedEntities: true);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER CREATION AND MANAGEMENT - ADVANCED IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with full configuration options.
    /// This is the primary implementation method that all other overloads delegate to.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <param name="position">World position for the container (optional, defaults to origin)</param>
    /// <param name="isLoaded">Whether the container starts in loaded state (optional, defaults to true)</param>
    /// <param name="isPersistent">Whether the container persists across scenes (optional, defaults to false)</param>
    /// <returns>The newly created container entity</returns>
    /// <exception cref="ArgumentException">Thrown when containerName is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when system is not initialized</exception>
    /// <example>
    /// <code>
    /// var backpack = containerSystem.CreateContainer("PlayerBackpack", new Vector2D&lt;float&gt;(100f, 200f));
    /// var persistentChest = containerSystem.CreateContainer("TreasureChest", Vector2D&lt;float&gt;.Zero, true, true);
    /// </code>
    /// </example>
    public Entity CreateContainer(string containerName, Vector2D<float> position, bool isLoaded, bool isPersistent)
    {
        if (_world == null)
            throw new InvalidOperationException("ContainerSystem must be initialized before creating containers");

        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));

        // Create the container entity
        var container = _world.CreateEntity();

        // Add ContainerComponent to mark as container
        var containerComponent = new ContainerComponent(containerName, isLoaded, isPersistent);
        _world.SetComponent(container, containerComponent);

        // Add NameComponent for debugging and identification
        var nameComponent = new NameComponent(containerName);
        _world.SetComponent(container, nameComponent);

        // Add TransformComponent for spatial operations
        var transformComponent = new TransformComponent(position);
        _world.SetComponent(container, transformComponent);

        // Add ParentHierarchyComponent to enable parent-child relationships
        var hierarchyComponent = new ParentHierarchyComponent();
        _world.SetComponent(container, hierarchyComponent);

        return container;
    }

    /// <summary>
    /// Destroys a container with configurable handling of contained entities.
    /// This is the primary implementation method that the simple overload delegates to.
    /// </summary>
    /// <param name="container">The container entity to destroy</param>
    /// <param name="destroyContainedEntities">Whether to destroy all contained entities</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    /// <exception cref="InvalidOperationException">Thrown when system is not initialized</exception>
    /// <example>
    /// <code>
    /// // Destroy container and all contents
    /// containerSystem.DestroyContainer(backpack, destroyContainedEntities: true);
    /// 
    /// // Destroy container but move contents to world
    /// containerSystem.DestroyContainer(backpack, destroyContainedEntities: false);
    /// </code>
    /// </example>
    public void DestroyContainer(Entity container, bool destroyContainedEntities)
    {
        if (_world == null)
            throw new InvalidOperationException("ContainerSystem must be initialized before destroying containers");

        // Validate that entity is actually a container
        if (!_world.HasComponent<ContainerComponent>(container))
        {
            throw new ArgumentException($"Entity {_world.GetEntityName(container)} is not a container", nameof(container));
        }

        // Handle contained entities
        if (_world.TryGetComponent<ParentHierarchyComponent>(container, out var hierarchy))
        {
            // Create a copy of child IDs to avoid modification during iteration
            var childIds = hierarchy.ChildEntityIds.ToList();
            
            foreach (var childId in childIds)
            {
                var childEntity = new Entity(childId);
                
                if (destroyContainedEntities)
                {
                    // Recursively destroy contained entities
                    DestroyEntityRecursive(childEntity);
                }
                else
                {
                    // Move contained entities to world space
                    if (_transformSystem != null && _world != null)
                    {
                        childEntity.LoadToWorld(_world, _transformSystem);
                    }
                    else
                    {
                        // Fallback: just remove parent relationship
                        if (_world.TryGetComponent<ParentHierarchyComponent>(childEntity, out var childHierarchy))
                        {
                            _world.SetComponent(childEntity, childHierarchy.AsRoot());
                        }
                    }
                }
            }
        }

        // Remove the container entity itself
        // Note: In a full implementation, this would use World.DestroyEntity()
        // For now, we remove the key components to effectively "destroy" it
        _world.RemoveComponent<ContainerComponent>(container);
        _world.RemoveComponent<ParentHierarchyComponent>(container);
        _world.RemoveComponent<TransformComponent>(container);
        _world.RemoveComponent<NameComponent>(container);
    }

    /// <summary>
    /// Recursively destroys an entity and all its children.
    /// </summary>
    /// <param name="entity">The entity to destroy</param>
    private void DestroyEntityRecursive(Entity entity)
    {
        if (_world == null) return; // Guard against null world
        
        // Handle children first
        if (_world.TryGetComponent<ParentHierarchyComponent>(entity, out var hierarchy))
        {
            var childIds = hierarchy.ChildEntityIds.ToList();
            foreach (var childId in childIds)
            {
                DestroyEntityRecursive(new Entity(childId));
            }
        }

        // Remove key components to effectively destroy the entity
        // In a full implementation, this would use World.DestroyEntity()
        _world.RemoveComponent<ContainerComponent>(entity);
        _world.RemoveComponent<ParentHierarchyComponent>(entity);
        _world.RemoveComponent<TransformComponent>(entity);
        _world.RemoveComponent<NameComponent>(entity);
        // Note: Could remove all components or mark entity as destroyed
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER STATE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the loaded state of a container.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="isLoaded">New loaded state</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    /// <exception cref="InvalidOperationException">Thrown when system is not initialized</exception>
    public void SetContainerLoaded(Entity container, bool isLoaded)
    {
        if (_world == null)
            throw new InvalidOperationException("ContainerSystem must be initialized");

        if (!_world.TryGetComponent<ContainerComponent>(container, out var containerComponent))
        {
            throw new ArgumentException($"Entity {_world.GetEntityName(container)} is not a container", nameof(container));
        }

        var updatedComponent = containerComponent.WithLoaded(isLoaded);
        _world.SetComponent(container, updatedComponent);
    }

    /// <summary>
    /// Sets the persistence state of a container.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="isPersistent">New persistence state</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container</exception>
    /// <exception cref="InvalidOperationException">Thrown when system is not initialized</exception>
    public void SetContainerPersistent(Entity container, bool isPersistent)
    {
        if (_world == null)
            throw new InvalidOperationException("ContainerSystem must be initialized");

        if (!_world.TryGetComponent<ContainerComponent>(container, out var containerComponent))
        {
            throw new ArgumentException($"Entity {_world.GetEntityName(container)} is not a container", nameof(container));
        }

        var updatedComponent = containerComponent.WithPersistent(isPersistent);
        _world.SetComponent(container, updatedComponent);
    }

    /// <summary>
    /// Renames a container.
    /// </summary>
    /// <param name="container">The container entity</param>
    /// <param name="newName">New name for the container</param>
    /// <exception cref="ArgumentException">Thrown when entity is not a container or name is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when system is not initialized</exception>
    public void RenameContainer(Entity container, string newName)
    {
        if (_world == null)
            throw new InvalidOperationException("ContainerSystem must be initialized");

        if (string.IsNullOrEmpty(newName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(newName));

        if (!_world.TryGetComponent<ContainerComponent>(container, out var containerComponent))
        {
            throw new ArgumentException($"Entity {_world.GetEntityName(container)} is not a container", nameof(container));
        }

        // Update container component name
        var updatedContainerComponent = containerComponent.WithName(newName);
        _world.SetComponent(container, updatedContainerComponent);

        // Update name component if it exists
        if (_world.TryGetComponent<NameComponent>(container, out var nameComponent))
        {
            var updatedNameComponent = new NameComponent(newName);
            _world.SetComponent(container, updatedNameComponent);
        }
    }
}