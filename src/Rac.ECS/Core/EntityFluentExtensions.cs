using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Core;

// ═══════════════════════════════════════════════════════════════════════════
// ENTITY FLUENT EXTENSIONS - CLEAN FLUENT API
// Educational note: Extension methods provide fluent interface without wrapper complexity
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Extension methods that provide a fluent API for entity component assignment.
/// 
/// EDUCATIONAL NOTES:
/// This implements the Fluent Interface pattern using C# extension methods,
/// providing a clean, readable way to compose entities without wrapper classes
/// or complex inheritance hierarchies.
/// 
/// DESIGN BENEFITS:
/// - **Simplicity**: No wrapper classes, just extends Entity directly
/// - **Performance**: Zero overhead compared to direct component assignment
/// - **Discoverability**: IDE shows available With* methods after entity creation
/// - **Composability**: Methods can be chained in any order
/// - **Extensibility**: New component types can add their own extension methods
/// 
/// USAGE PATTERNS:
/// Basic: world.CreateEntity().WithName("Player").WithPosition(100, 200)
/// Complex: world.CreateEntity().WithName("Enemy").WithTags("AI", "Hostile").WithTransform(pos, rot, scale)
/// 
/// Educational reference: "Fluent Interfaces" by Martin Fowler
/// https://martinfowler.com/bliki/FluentInterface.html
/// </summary>
/// <remarks>
/// Extension methods are a C# language feature that allows adding methods to existing types
/// without modifying the original type. This enables us to add fluent component assignment
/// to Entity without changing the core Entity struct, maintaining clean separation of concerns.
/// </remarks>
public static class EntityFluentExtensions
{
    /// <summary>
    /// Adds a name component to the entity for human-readable identification.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var player = world.CreateEntity().WithName(world, "Player");
    /// </code>
    /// </example>
    public static Entity WithName(this Entity entity, IWorld world, string name)
    {
        world.SetComponent(entity, new NameComponent(name ?? string.Empty));
        return entity;
    }

    /// <summary>
    /// Adds a tag component to the entity with a single tag.
    /// Tags are useful for categorizing entities and enabling efficient queries.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="tag">Tag to assign to the entity</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var enemy = world.CreateEntity().WithTag(world, "Enemy");
    /// </code>
    /// </example>
    public static Entity WithTag(this Entity entity, IWorld world, string tag)
    {
        world.SetComponent(entity, new TagComponent(tag));
        return entity;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags.
    /// Useful for entities that belong to multiple categories.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var boss = world.CreateEntity().WithTags(world, "Enemy", "Boss", "Elite");
    /// </code>
    /// </example>
    public static Entity WithTags(this Entity entity, IWorld world, params string[] tags)
    {
        world.SetComponent(entity, new TagComponent(tags));
        return entity;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags from a collection.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>The same entity for method chaining</returns>
    public static Entity WithTags(this Entity entity, IWorld world, IEnumerable<string> tags)
    {
        world.SetComponent(entity, new TagComponent(tags));
        return entity;
    }

    /// <summary>
    /// Adds a transform component to the entity with position only.
    /// Transform components define spatial relationships in the game world.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="position">Local position of the entity</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var item = world.CreateEntity().WithPosition(world, new Vector2D&lt;float&gt;(100, 200));
    /// </code>
    /// </example>
    public static Entity WithPosition(this Entity entity, IWorld world, Vector2D<float> position)
    {
        world.SetComponent(entity, new TransformComponent(position));
        return entity;
    }

    /// <summary>
    /// Adds a transform component to the entity with position coordinates.
    /// Convenience overload for common position assignment.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="x">X coordinate of the position</param>
    /// <param name="y">Y coordinate of the position</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var player = world.CreateEntity().WithPosition(world, 100, 200);
    /// </code>
    /// </example>
    public static Entity WithPosition(this Entity entity, IWorld world, float x, float y)
    {
        return entity.WithPosition(world, new Vector2D<float>(x, y));
    }

    /// <summary>
    /// Adds a transform component to the entity with position and rotation.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="position">Local position of the entity</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var rotatedObject = world.CreateEntity().WithTransform(world, position, MathF.PI / 4);
    /// </code>
    /// </example>
    public static Entity WithTransform(this Entity entity, IWorld world, Vector2D<float> position, float rotation)
    {
        world.SetComponent(entity, new TransformComponent(position, rotation));
        return entity;
    }

    /// <summary>
    /// Adds a transform component to the entity with position, rotation, and scale.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="position">Local position of the entity</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <param name="scale">Local scale of the entity</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var scaledObject = world.CreateEntity()
    ///     .WithTransform(world, position, 0f, new Vector2D&lt;float&gt;(2f, 2f));
    /// </code>
    /// </example>
    public static Entity WithTransform(this Entity entity, IWorld world, Vector2D<float> position, float rotation, Vector2D<float> scale)
    {
        world.SetComponent(entity, new TransformComponent(position, rotation, scale));
        return entity;
    }

    /// <summary>
    /// Adds a transform component to the entity with full parameters.
    /// Convenience overload for easy transform setup with individual values.
    /// </summary>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="x">X coordinate of the position</param>
    /// <param name="y">Y coordinate of the position</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <param name="scaleX">X scale factor</param>
    /// <param name="scaleY">Y scale factor</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var entity = world.CreateEntity()
    ///     .WithTransform(world, 100, 200, MathF.PI / 4, 1.5f, 1.5f);
    /// </code>
    /// </example>
    public static Entity WithTransform(this Entity entity, IWorld world, float x, float y, float rotation = 0f, float scaleX = 1f, float scaleY = 1f)
    {
        var position = new Vector2D<float>(x, y);
        var scale = new Vector2D<float>(scaleX, scaleY);
        return entity.WithTransform(world, position, rotation, scale);
    }

    /// <summary>
    /// Adds a generic component to the entity.
    /// This is the most flexible method that works with any component type.
    /// </summary>
    /// <typeparam name="T">Type of component to add, must implement IComponent</typeparam>
    /// <param name="entity">The entity to add the component to</param>
    /// <param name="world">The world that manages the entity</param>
    /// <param name="component">Component instance to add</param>
    /// <returns>The same entity for method chaining</returns>
    /// <example>
    /// <code>
    /// var entity = world.CreateEntity()
    ///     .WithComponent(world, new VelocityComponent(10f, 5f))
    ///     .WithComponent(world, new HealthComponent(100));
    /// </code>
    /// </example>
    public static Entity WithComponent<T>(this Entity entity, IWorld world, T component) where T : IComponent
    {
        world.SetComponent(entity, component);
        return entity;
    }
}