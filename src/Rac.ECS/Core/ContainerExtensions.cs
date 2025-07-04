using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;

namespace Rac.ECS.Core;

/// <summary>
/// Extension methods for Entity to provide intuitive container and attachment operations.
/// 
/// DESIGN PRINCIPLES:
/// - Semantic Clarity: PlaceIn vs AttachTo conveys intent clearly
/// - Intuitive API: Methods read naturally (item.PlaceIn(container))
/// - Type Safety: PlaceIn validates container, AttachTo works with any entity
/// - Integration: Uses existing ParentHierarchyComponent and TransformSystem
/// 
/// EDUCATIONAL NOTES:
/// Extension methods in C# enable adding functionality to existing types without modification.
/// This pattern is particularly valuable in ECS architectures where:
/// - Core entity types remain simple data containers
/// - Domain-specific operations can be grouped logically
/// - API discoverability is improved through IntelliSense
/// - Code organization follows single-responsibility principle
/// 
/// CONTAINER SEMANTICS:
/// - PlaceIn: For containment relationships (items in backpack, objects in room)
/// - AttachTo: For attachment relationships (scope on rifle, wheel on car)
/// - LoadToWorld: Makes entity part of world space (opposite of containment)
/// - RemoveFrom: Removes from current container or attachment point
/// 
/// USAGE PATTERNS:
/// - Inventory Systems: sword.PlaceIn(backpack)
/// - Scene Composition: furniture.PlaceIn(room) 
/// - Equipment Systems: scope.AttachTo(rifle, attachPoint)
/// - Level Management: entity.LoadToWorld(spawnPosition)
/// </summary>
public static class ContainerExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER PLACEMENT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Places an item inside a container at the specified local position.
    /// The target entity must have a ContainerComponent to be valid for placement.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <param name="localPosition">Local position within the container (optional, defaults to origin)</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    /// <example>
    /// <code>
    /// var sword = world.CreateEntity();
    /// var backpack = world.CreateEntity();
    /// world.SetComponent(backpack, new ContainerComponent("PlayerBackpack"));
    /// 
    /// // Place item in container at origin
    /// sword.PlaceIn(backpack, world, transformSystem);
    /// 
    /// // Place item at specific position in container
    /// sword.PlaceIn(backpack, new Vector2D&lt;float&gt;(0.1f, 0.2f), world, transformSystem);
    /// </code>
    /// </example>
    public static void PlaceIn(this Entity item, Entity container, IWorld world, TransformSystem transformSystem, Vector2D<float> localPosition = default)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        // Validate that target is actually a container
        if (!world.HasComponent<ContainerComponent>(container))
        {
            throw new ArgumentException($"Entity {world.GetEntityName(container)} is not a container. Use AttachTo for non-container relationships.", nameof(container));
        }

        // Set the transform to the specified local position if provided
        if (localPosition != default(Vector2D<float>))
        {
            var currentTransform = world.TryGetComponent<TransformComponent>(item, out var existingTransform) 
                ? existingTransform 
                : new TransformComponent();
            
            var newTransform = currentTransform.WithPosition(localPosition);
            world.SetComponent(item, newTransform);
        }

        // Establish parent-child relationship using existing hierarchy system
        transformSystem.SetParent(container, item);
    }

    /// <summary>
    /// Attaches an entity to another entity at a specific local position.
    /// Works with any target entity - no container validation required.
    /// </summary>
    /// <param name="item">The entity to attach</param>
    /// <param name="target">The target entity to attach to (any entity)</param>
    /// <param name="localPosition">Local position relative to target (required for attachment)</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <example>
    /// <code>
    /// var scope = world.CreateEntity();
    /// var rifle = world.CreateEntity();
    /// 
    /// // Attach scope to rifle at specific mount point
    /// scope.AttachTo(rifle, new Vector2D&lt;float&gt;(0f, 0.1f), world, transformSystem);
    /// </code>
    /// </example>
    public static void AttachTo(this Entity item, Entity target, Vector2D<float> localPosition, IWorld world, TransformSystem transformSystem)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        // Set the transform to the specified attachment position
        var currentTransform = world.TryGetComponent<TransformComponent>(item, out var existingTransform) 
            ? existingTransform 
            : new TransformComponent();
        
        var newTransform = currentTransform.WithPosition(localPosition);
        world.SetComponent(item, newTransform);

        // Establish parent-child relationship using existing hierarchy system
        transformSystem.SetParent(target, item);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WORLD PLACEMENT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads an entity into world space at the specified position, making it a root entity.
    /// Removes the entity from any current container or attachment.
    /// </summary>
    /// <param name="item">The entity to load into world space</param>
    /// <param name="worldPosition">World position to place the entity (optional, defaults to origin)</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <example>
    /// <code>
    /// var player = world.CreateEntity();
    /// var spawnPoint = new Vector2D&lt;float&gt;(100f, 200f);
    /// 
    /// // Load player into world at spawn point
    /// player.LoadToWorld(spawnPoint, world, transformSystem);
    /// 
    /// // Load at origin
    /// player.LoadToWorld(world, transformSystem);
    /// </code>
    /// </example>
    public static void LoadToWorld(this Entity item, IWorld world, TransformSystem transformSystem, Vector2D<float> worldPosition = default)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        // Remove from any current parent (container or attachment)
        transformSystem.RemoveParent(item);

        // Set world position
        var currentTransform = world.TryGetComponent<TransformComponent>(item, out var existingTransform) 
            ? existingTransform 
            : new TransformComponent();
        
        var newTransform = currentTransform.WithPosition(worldPosition);
        world.SetComponent(item, newTransform);
    }

    /// <summary>
    /// Removes an entity from its current container or attachment point, making it a root entity at origin.
    /// </summary>
    /// <param name="item">The entity to remove from its current parent</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <example>
    /// <code>
    /// var attachedScope = world.CreateEntity();
    /// // ... scope is attached to rifle ...
    /// 
    /// // Remove scope from rifle
    /// attachedScope.RemoveFrom(world, transformSystem);
    /// // Scope is now a root entity at origin
    /// </code>
    /// </example>
    public static void RemoveFrom(this Entity item, IWorld world, TransformSystem transformSystem)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        // Remove from current parent and make root entity
        transformSystem.RemoveParent(item);

        // Reset position to origin for consistency
        var currentTransform = world.TryGetComponent<TransformComponent>(item, out var existingTransform) 
            ? existingTransform 
            : new TransformComponent();
        
        var newTransform = currentTransform.WithPosition(Vector2D<float>.Zero);
        world.SetComponent(item, newTransform);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER QUERY OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if an entity is currently inside a container (has a parent with ContainerComponent).
    /// </summary>
    /// <param name="item">The entity to check</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>True if the entity is inside a container, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static bool IsInContainer(this Entity item, IWorld world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        // Check if entity has a parent
        if (!world.TryGetComponent<ParentHierarchyComponent>(item, out var hierarchy) || hierarchy.IsRoot)
            return false;

        // Check if parent has ContainerComponent
        return world.HasComponent<ContainerComponent>(hierarchy.ParentEntity);
    }

    /// <summary>
    /// Gets the container that holds this entity, if any.
    /// </summary>
    /// <param name="item">The entity to check</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The container entity if found, or null if not in a container</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static Entity? GetContainer(this Entity item, IWorld world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        // Check if entity has a parent
        if (!world.TryGetComponent<ParentHierarchyComponent>(item, out var hierarchy) || hierarchy.IsRoot)
            return null;

        // Check if parent has ContainerComponent
        if (world.HasComponent<ContainerComponent>(hierarchy.ParentEntity))
            return hierarchy.ParentEntity;

        return null;
    }
}