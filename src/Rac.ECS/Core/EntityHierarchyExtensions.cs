using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;

namespace Rac.ECS.Core;

/// <summary>
/// Extension methods for Entity to provide convenient hierarchy management operations.
/// 
/// DESIGN PRINCIPLES:
/// - Extension methods preserve the immutable nature of Entity record struct
/// - All operations require a World instance for component access
/// - Methods encapsulate complex hierarchy logic for better usability
/// - Consistent with ECS patterns where logic resides in systems, not entities
/// 
/// EDUCATIONAL NOTES:
/// Extension methods in C# allow adding functionality to existing types without
/// modification. This pattern is particularly useful for ECS architectures where
/// core entity types should remain simple data containers.
/// 
/// USAGE PATTERNS:
/// - Game object creation: entity.SetParent(world, parentEntity)
/// - Hierarchy queries: entity.GetChildren(world)
/// - Transform management: entity.SetLocalTransform(world, position, rotation)
/// - Scene graph operations: entity.GetWorldPosition(world)
/// </summary>
public static class EntityHierarchyExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // HIERARCHY RELATIONSHIP MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the parent of this entity in the hierarchy.
    /// Automatically manages parent-child relationships in both directions.
    /// </summary>
    /// <param name="entity">The child entity</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="parent">The parent entity to attach to</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <exception cref="ArgumentException">Thrown when trying to create circular references</exception>
    /// <example>
    /// <code>
    /// var weapon = world.CreateEntity();
    /// var character = world.CreateEntity();
    /// weapon.SetParent(world, character, transformSystem);
    /// </code>
    /// </example>
    public static void SetParent(this Entity entity, World world, Entity parent, TransformSystem transformSystem)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        transformSystem.SetParent(parent, entity);
    }

    /// <summary>
    /// Removes this entity from its parent, making it a root entity.
    /// </summary>
    /// <param name="entity">The entity to detach from its parent</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <example>
    /// <code>
    /// attachedWeapon.RemoveParent(world, transformSystem);
    /// // Weapon is now independent of character
    /// </code>
    /// </example>
    public static void RemoveParent(this Entity entity, World world, TransformSystem transformSystem)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        transformSystem.RemoveParent(entity);
    }

    /// <summary>
    /// Adds a child entity to this parent entity.
    /// This is equivalent to calling SetParent on the child entity.
    /// </summary>
    /// <param name="entity">The parent entity</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="child">The child entity to attach</param>
    /// <param name="transformSystem">Transform system for hierarchy management</param>
    /// <exception cref="ArgumentNullException">Thrown when world or transformSystem is null</exception>
    /// <exception cref="ArgumentException">Thrown when trying to create circular references</exception>
    /// <example>
    /// <code>
    /// var character = world.CreateEntity();
    /// var weapon = world.CreateEntity();
    /// character.AddChild(world, weapon, transformSystem);
    /// </code>
    /// </example>
    public static void AddChild(this Entity entity, World world, Entity child, TransformSystem transformSystem)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (transformSystem == null) throw new ArgumentNullException(nameof(transformSystem));

        transformSystem.SetParent(entity, child);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HIERARCHY QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the parent entity of this entity, if any.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The parent entity, or null if this is a root entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    /// <example>
    /// <code>
    /// var parent = weapon.GetParent(world);
    /// if (parent.HasValue)
    /// {
    ///     Console.WriteLine($"Weapon is attached to entity {parent.Value.Id}");
    /// }
    /// </code>
    /// </example>
    public static Entity? GetParent(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var hierarchyQuery = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        if (hierarchyQuery.Entity.Id != 0 && !hierarchyQuery.Component1.IsRoot)
        {
            return hierarchyQuery.Component1.ParentEntity;
        }

        return null;
    }

    /// <summary>
    /// Gets all direct children of this entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>Collection of child entities</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    /// <example>
    /// <code>
    /// var children = character.GetChildren(world);
    /// foreach (var child in children)
    /// {
    ///     Console.WriteLine($"Child: {child.Id}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<Entity> GetChildren(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var hierarchyQuery = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        if (hierarchyQuery.Entity.Id != 0)
        {
            foreach (var childId in hierarchyQuery.Component1.ChildEntityIds)
            {
                yield return new Entity(childId);
            }
        }
    }

    /// <summary>
    /// Checks if this entity is a root entity (has no parent).
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>True if the entity is a root, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static bool IsRoot(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var hierarchyQuery = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        return hierarchyQuery.Entity.Id == 0 || hierarchyQuery.Component1.IsRoot;
    }

    /// <summary>
    /// Checks if this entity is a leaf entity (has no children).
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>True if the entity is a leaf, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static bool IsLeaf(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var hierarchyQuery = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        return hierarchyQuery.Entity.Id == 0 || hierarchyQuery.Component1.IsLeaf;
    }

    /// <summary>
    /// Gets the number of direct children this entity has.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>Number of direct children</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static int GetChildCount(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var hierarchyQuery = world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        return hierarchyQuery.Entity.Id != 0 ? hierarchyQuery.Component1.ChildCount : 0;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRANSFORM CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the local transform of this entity relative to its parent.
    /// </summary>
    /// <param name="entity">The entity to modify</param>
    /// <param name="world">The ECS world containing components</param>
    /// <param name="localPosition">Local position relative to parent</param>
    /// <param name="localRotation">Local rotation in radians</param>
    /// <param name="localScale">Local scale relative to parent</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    /// <example>
    /// <code>
    /// weapon.SetLocalTransform(world, 
    ///     new Vector2D&lt;float&gt;(1f, 0f), // Right side of character
    ///     0f,                            // No rotation
    ///     Vector2D&lt;float&gt;.One);         // Normal size
    /// </code>
    /// </example>
    public static void SetLocalTransform(this Entity entity, World world, 
        Silk.NET.Maths.Vector2D<float> localPosition, 
        float localRotation = 0f, 
        Silk.NET.Maths.Vector2D<float>? localScale = null)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var scale = localScale ?? Silk.NET.Maths.Vector2D<float>.One;
        var transform = new TransformComponent(localPosition, localRotation, scale);
        world.SetComponent(entity, transform);
    }

    /// <summary>
    /// Gets the local transform of this entity.
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The local transform component, or null if not present</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static TransformComponent? GetLocalTransform(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var transformQuery = world.Query<TransformComponent>()
            .FirstOrDefault(t => t.Entity.Id == entity.Id);

        return transformQuery.Entity.Id != 0 ? transformQuery.Component1 : null;
    }

    /// <summary>
    /// Gets the world transform of this entity (computed by TransformSystem).
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The world transform component, or null if not computed yet</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    /// <example>
    /// <code>
    /// var worldTransform = weapon.GetWorldTransform(world);
    /// if (worldTransform.HasValue)
    /// {
    ///     var worldPosition = worldTransform.Value.WorldPosition;
    ///     Console.WriteLine($"Weapon world position: {worldPosition}");
    /// }
    /// </code>
    /// </example>
    public static WorldTransformComponent? GetWorldTransform(this Entity entity, World world)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));

        var worldTransformQuery = world.Query<WorldTransformComponent>()
            .FirstOrDefault(wt => wt.Entity.Id == entity.Id);

        return worldTransformQuery.Entity.Id != 0 ? worldTransformQuery.Component1 : null;
    }

    /// <summary>
    /// Gets the world position of this entity (convenience method).
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The world position, or Vector2D.Zero if no world transform</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static Silk.NET.Maths.Vector2D<float> GetWorldPosition(this Entity entity, World world)
    {
        var worldTransform = entity.GetWorldTransform(world);
        return worldTransform?.WorldPosition ?? Silk.NET.Maths.Vector2D<float>.Zero;
    }

    /// <summary>
    /// Gets the world rotation of this entity (convenience method).
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The world rotation in radians, or 0 if no world transform</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static float GetWorldRotation(this Entity entity, World world)
    {
        var worldTransform = entity.GetWorldTransform(world);
        return worldTransform?.WorldRotation ?? 0f;
    }

    /// <summary>
    /// Gets the world scale of this entity (convenience method).
    /// </summary>
    /// <param name="entity">The entity to query</param>
    /// <param name="world">The ECS world containing components</param>
    /// <returns>The world scale, or Vector2D.One if no world transform</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    public static Silk.NET.Maths.Vector2D<float> GetWorldScale(this Entity entity, World world)
    {
        var worldTransform = entity.GetWorldTransform(world);
        return worldTransform?.WorldScale ?? Silk.NET.Maths.Vector2D<float>.One;
    }
}