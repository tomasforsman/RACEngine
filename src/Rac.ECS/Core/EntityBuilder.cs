using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Core;

// ═══════════════════════════════════════════════════════════════════════════
// ENTITY BUILDER API - FLUENT ENTITY CREATION
// Educational note: Builder pattern for creating entities with multiple components
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Fluent builder for creating entities with multiple components in a readable, chainable manner.
/// 
/// EDUCATIONAL NOTES:
/// This implements the Builder pattern (Gang of Four) specifically for ECS entity creation.
/// The fluent interface allows for readable entity composition without the verbosity of
/// multiple individual component assignments.
/// 
/// DESIGN DECISIONS:
/// - Mutable builder for performance (avoids object creation on each method call)
/// - Immediate component assignment to world for simplicity
/// - Component-specific methods for common use cases
/// - Generic WithComponent method for extensibility
/// 
/// USAGE PATTERNS:
/// - Simple entities: world.CreateEntity().WithName("Player").Build()
/// - Complex entities: world.CreateEntity().WithTransform(pos, rot).WithTag("Enemy").Build()
/// - Named entities: world.CreateEntity("Player").WithTransform(startPos).Build()
/// </summary>
/// <remarks>
/// The builder maintains a reference to the created entity and the world,
/// allowing components to be added incrementally. The entity is created immediately
/// when the builder is constructed, and components are added as builder methods are called.
/// </remarks>
public class EntityBuilder
{
    private readonly IWorld _world;
    private readonly Entity _entity;

    /// <summary>
    /// Initializes a new EntityBuilder with an already-created entity.
    /// </summary>
    /// <param name="world">The world that owns the entity</param>
    /// <param name="entity">The entity being built</param>
    internal EntityBuilder(IWorld world, Entity entity)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _entity = entity;
    }

    /// <summary>
    /// Adds a name component to the entity.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithName(string name)
    {
        _world.SetComponent(_entity, new NameComponent(name ?? string.Empty));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with a single tag.
    /// </summary>
    /// <param name="tag">Tag to assign to the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTag(string tag)
    {
        _world.SetComponent(_entity, new TagComponent(tag));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags.
    /// </summary>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTags(params string[] tags)
    {
        _world.SetComponent(_entity, new TagComponent(tags));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags.
    /// </summary>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTags(IEnumerable<string> tags)
    {
        _world.SetComponent(_entity, new TagComponent(tags));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with position only.
    /// </summary>
    /// <param name="position">Local position of the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithPosition(Vector2D<float> position)
    {
        _world.SetComponent(_entity, new TransformComponent(position));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with position only.
    /// </summary>
    /// <param name="x">X coordinate of the position</param>
    /// <param name="y">Y coordinate of the position</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithPosition(float x, float y)
    {
        return WithPosition(new Vector2D<float>(x, y));
    }

    /// <summary>
    /// Adds a transform component to the entity with position and rotation.
    /// </summary>
    /// <param name="position">Local position of the entity</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTransform(Vector2D<float> position, float rotation)
    {
        _world.SetComponent(_entity, new TransformComponent(position, rotation));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with position, rotation, and scale.
    /// </summary>
    /// <param name="position">Local position of the entity</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <param name="scale">Local scale of the entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTransform(Vector2D<float> position, float rotation, Vector2D<float> scale)
    {
        _world.SetComponent(_entity, new TransformComponent(position, rotation, scale));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with full parameters.
    /// </summary>
    /// <param name="x">X coordinate of the position</param>
    /// <param name="y">Y coordinate of the position</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <param name="scaleX">X scale factor</param>
    /// <param name="scaleY">Y scale factor</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithTransform(float x, float y, float rotation = 0f, float scaleX = 1f, float scaleY = 1f)
    {
        var position = new Vector2D<float>(x, y);
        var scale = new Vector2D<float>(scaleX, scaleY);
        return WithTransform(position, rotation, scale);
    }

    /// <summary>
    /// Adds a generic component to the entity.
    /// </summary>
    /// <typeparam name="T">Type of component to add</typeparam>
    /// <param name="component">Component instance to add</param>
    /// <returns>This builder for method chaining</returns>
    public EntityBuilder WithComponent<T>(T component) where T : IComponent
    {
        _world.SetComponent(_entity, component);
        return this;
    }

    /// <summary>
    /// Completes the entity building process and returns the built entity.
    /// </summary>
    /// <returns>The entity that was built with all specified components</returns>
    public Entity Build()
    {
        return _entity;
    }

    /// <summary>
    /// Implicit conversion to Entity for convenience.
    /// Allows the builder to be used directly where an Entity is expected.
    /// </summary>
    /// <param name="builder">The builder to convert</param>
    /// <returns>The entity being built</returns>
    public static implicit operator Entity(EntityBuilder builder)
    {
        return builder._entity;
    }
}