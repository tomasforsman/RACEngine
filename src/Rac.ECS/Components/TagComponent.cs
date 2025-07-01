using Rac.ECS.Components;

namespace Rac.ECS.Components;

/// <summary>
/// Stores a collection of string tags for an entity, enabling tag-based entity queries and categorization.
/// 
/// USAGE PATTERNS:
/// - Categorizing entities (e.g., "Enemy", "Collectible", "UI")
/// - System filtering (e.g., only process entities with "Renderable" tag)
/// - Gameplay mechanics (e.g., "Flammable", "Solid", "Interactive")
/// - Debug and development features (e.g., "DebugVisible", "Selected")
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Tag queries iterate through all entities with TagComponent
/// - HashSet provides O(1) tag membership testing
/// - Multiple tags per entity are supported and efficient
/// - Consider tag indexing for very large entity counts
/// 
/// EDUCATIONAL NOTES:
/// Tags provide a flexible alternative to rigid entity hierarchies or types.
/// Multiple systems can use different tag combinations for filtering,
/// enabling dynamic entity behavior without complex inheritance structures.
/// This follows the composition-over-inheritance principle of ECS architecture.
/// </summary>
/// <param name="Tags">Set of string tags associated with this entity</param>
public readonly record struct TagComponent(HashSet<string> Tags) : IComponent
{
    /// <summary>
    /// Creates a TagComponent with an empty tag set.
    /// </summary>
    public TagComponent() : this(new HashSet<string>()) { }

    /// <summary>
    /// Creates a TagComponent with a single tag.
    /// </summary>
    /// <param name="tag">Single tag to assign to the entity</param>
    public TagComponent(string tag) : this(new HashSet<string> { tag ?? string.Empty }) { }

    /// <summary>
    /// Creates a TagComponent with multiple tags.
    /// </summary>
    /// <param name="tags">Collection of tags to assign to the entity</param>
    public TagComponent(IEnumerable<string> tags) : this(new HashSet<string>(tags ?? Enumerable.Empty<string>())) { }

    /// <summary>
    /// Checks if this entity has the specified tag.
    /// </summary>
    /// <param name="tag">Tag to check for</param>
    /// <returns>True if the entity has the tag, false otherwise</returns>
    public bool HasTag(string tag) => Tags.Contains(tag ?? string.Empty);

    /// <summary>
    /// Checks if this entity has any of the specified tags.
    /// </summary>
    /// <param name="tags">Tags to check for</param>
    /// <returns>True if the entity has at least one of the tags, false otherwise</returns>
    public bool HasAnyTag(IEnumerable<string> tags) => tags?.Any(HasTag) == true;

    /// <summary>
    /// Checks if this entity has all of the specified tags.
    /// </summary>
    /// <param name="tags">Tags to check for</param>
    /// <returns>True if the entity has all of the tags, false otherwise</returns>
    public bool HasAllTags(IEnumerable<string> tags) => tags?.All(HasTag) == true;

    /// <summary>
    /// Creates a new TagComponent with an additional tag.
    /// </summary>
    /// <param name="tag">Tag to add</param>
    /// <returns>New TagComponent with the added tag</returns>
    public TagComponent WithTag(string tag)
    {
        var newTags = new HashSet<string>(Tags) { tag ?? string.Empty };
        return new TagComponent(newTags);
    }

    /// <summary>
    /// Creates a new TagComponent with a tag removed.
    /// </summary>
    /// <param name="tag">Tag to remove</param>
    /// <returns>New TagComponent with the tag removed</returns>
    public TagComponent WithoutTag(string tag)
    {
        var newTags = new HashSet<string>(Tags);
        newTags.Remove(tag ?? string.Empty);
        return new TagComponent(newTags);
    }
}