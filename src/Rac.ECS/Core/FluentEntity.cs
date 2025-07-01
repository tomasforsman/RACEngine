using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Core;

// ═══════════════════════════════════════════════════════════════════════════
// FLUENT ENTITY - HYBRID ENTITY/BUILDER PATTERN
// Educational note: Combines Entity behavior with Builder pattern for fluent API
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// A wrapper around Entity that provides fluent component addition while maintaining Entity compatibility.
/// 
/// EDUCATIONAL NOTES:
/// This implements a hybrid pattern that combines:
/// - **Proxy Pattern**: Acts as a proxy for the underlying Entity
/// - **Builder Pattern**: Provides fluent component addition methods
/// - **Adapter Pattern**: Adapts Entity to provide additional fluent interface
/// 
/// DESIGN GOALS:
/// - Backward compatibility: Can be used wherever Entity is expected
/// - Fluent syntax: Enables world.CreateEntity().WithPosition() chaining
/// - Performance: Minimal overhead over direct Entity usage
/// - Discoverability: IDE shows available With* methods after CreateEntity()
/// 
/// This allows the syntax: world.CreateEntity().WithPosition(x, y).WithName("Player")
/// while still being compatible with existing code that expects Entity.
/// </summary>
/// <remarks>
/// The FluentEntity maintains references to both the underlying Entity and the World
/// to enable component operations. It can be implicitly converted to Entity for
/// backward compatibility with existing systems and methods.
/// </remarks>
public readonly struct FluentEntity
{
    private readonly IWorld _world;
    private readonly Entity _entity;

    /// <summary>
    /// Initializes a new FluentEntity wrapper.
    /// </summary>
    /// <param name="world">The world that owns the entity</param>
    /// <param name="entity">The entity being wrapped</param>
    internal FluentEntity(IWorld world, Entity entity)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _entity = entity;
    }

    /// <summary>
    /// Gets the underlying Entity ID for compatibility.
    /// </summary>
    public int Id => _entity.Id;

    /// <summary>
    /// Gets whether the underlying Entity is alive.
    /// </summary>
    public bool IsAlive => _entity.IsAlive;

    /// <summary>
    /// Adds a name component to the entity.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithName(string name)
    {
        _world.SetComponent(_entity, new NameComponent(name ?? string.Empty));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with a single tag.
    /// </summary>
    /// <param name="tag">Tag to assign to the entity</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTag(string tag)
    {
        _world.SetComponent(_entity, new TagComponent(tag));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags.
    /// </summary>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTags(params string[] tags)
    {
        _world.SetComponent(_entity, new TagComponent(tags));
        return this;
    }

    /// <summary>
    /// Adds a tag component to the entity with multiple tags.
    /// </summary>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTags(IEnumerable<string> tags)
    {
        _world.SetComponent(_entity, new TagComponent(tags));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with position only.
    /// </summary>
    /// <param name="position">Local position of the entity</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithPosition(Vector2D<float> position)
    {
        _world.SetComponent(_entity, new TransformComponent(position));
        return this;
    }

    /// <summary>
    /// Adds a transform component to the entity with position only.
    /// </summary>
    /// <param name="x">X coordinate of the position</param>
    /// <param name="y">Y coordinate of the position</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithPosition(float x, float y)
    {
        return WithPosition(new Vector2D<float>(x, y));
    }

    /// <summary>
    /// Adds a transform component to the entity with position and rotation.
    /// </summary>
    /// <param name="position">Local position of the entity</param>
    /// <param name="rotation">Local rotation in radians</param>
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTransform(Vector2D<float> position, float rotation)
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
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTransform(Vector2D<float> position, float rotation, Vector2D<float> scale)
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
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithTransform(float x, float y, float rotation = 0f, float scaleX = 1f, float scaleY = 1f)
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
    /// <returns>This FluentEntity for method chaining</returns>
    public FluentEntity WithComponent<T>(T component) where T : IComponent
    {
        _world.SetComponent(_entity, component);
        return this;
    }

    /// <summary>
    /// Implicit conversion to Entity for backward compatibility.
    /// Allows FluentEntity to be used wherever Entity is expected.
    /// </summary>
    /// <param name="fluentEntity">The FluentEntity to convert</param>
    /// <returns>The underlying Entity</returns>
    public static implicit operator Entity(FluentEntity fluentEntity)
    {
        return fluentEntity._entity;
    }

    /// <summary>
    /// Equality comparison based on underlying Entity.
    /// </summary>
    /// <param name="other">The other FluentEntity to compare with</param>
    /// <returns>True if both wrap the same Entity</returns>
    public bool Equals(FluentEntity other)
    {
        return _entity.Equals(other._entity);
    }

    /// <summary>
    /// Equality comparison with Entity.
    /// </summary>
    /// <param name="other">The Entity to compare with</param>
    /// <returns>True if the underlying Entity matches</returns>
    public bool Equals(Entity other)
    {
        return _entity.Equals(other);
    }

    /// <summary>
    /// Object equality override.
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>True if equal</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            FluentEntity fluentEntity => Equals(fluentEntity),
            Entity entity => Equals(entity),
            _ => false
        };
    }

    /// <summary>
    /// Hash code based on underlying Entity.
    /// </summary>
    /// <returns>Hash code of the underlying Entity</returns>
    public override int GetHashCode()
    {
        return _entity.GetHashCode();
    }

    /// <summary>
    /// String representation based on underlying Entity.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        return $"FluentEntity({_entity})";
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(FluentEntity left, FluentEntity right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(FluentEntity left, FluentEntity right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Equality operator with Entity.
    /// </summary>
    public static bool operator ==(FluentEntity left, Entity right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator with Entity.
    /// </summary>
    public static bool operator !=(FluentEntity left, Entity right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Equality operator with Entity (reversed).
    /// </summary>
    public static bool operator ==(Entity left, FluentEntity right)
    {
        return right.Equals(left);
    }

    /// <summary>
    /// Inequality operator with Entity (reversed).
    /// </summary>
    public static bool operator !=(Entity left, FluentEntity right)
    {
        return !right.Equals(left);
    }
}