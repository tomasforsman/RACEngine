using Rac.ECS.Components;
using Rac.ECS.Core;

namespace Rac.ECS.Components;

/// <summary>
/// Defines parent-child relationships between entities in the scene hierarchy.
/// 
/// HIERARCHICAL DESIGN PRINCIPLES:
/// - Scene Graph Structure: Entities form a tree with parent-child relationships
/// - Transform Inheritance: Child entities inherit and combine parent transformations
/// - Lifecycle Management: Parent deletion should handle child entity cleanup
/// - Query Optimization: Enables efficient traversal for transform calculations
/// 
/// EDUCATIONAL NOTES:
/// Scene graphs are fundamental data structures in game engines and 3D graphics:
/// - Root nodes have no parent (ParentEntity.IsAlive = false)
/// - Leaf nodes have empty ChildEntityIds collections
/// - Transform propagation flows from root to leaves
/// - Common operations: add child, remove child, find siblings, traverse ancestors
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Child collections use HashSet for O(1) add/remove operations
/// - Parent reference enables upward traversal without tree search
/// - Transform systems can cache hierarchy depth for optimization
/// - Batch operations on siblings improve cache locality
/// 
/// USAGE PATTERNS:
/// - Game objects with attachments (weapon → character)
/// - UI element hierarchies (button → panel → window)
/// - Bone structures for skeletal animation
/// - Vehicle part assemblies (wheel → axle → chassis)
/// </summary>
/// <param name="ParentEntity">Reference to parent entity (invalid Entity if this is a root)</param>
/// <param name="ChildEntityIds">Set of child entity IDs for efficient management</param>
public readonly record struct ParentHierarchyComponent(
    Entity ParentEntity,
    HashSet<int> ChildEntityIds
) : IComponent
{
    /// <summary>
    /// Creates a root hierarchy component (no parent, no children).
    /// </summary>
    public ParentHierarchyComponent() : this(new Entity(0, false), new HashSet<int>()) { }

    /// <summary>
    /// Creates a child hierarchy component with specified parent.
    /// </summary>
    /// <param name="parent">The parent entity in the hierarchy</param>
    public ParentHierarchyComponent(Entity parent) : this(parent, new HashSet<int>()) { }

    /// <summary>
    /// Checks if this entity is a root node (has no parent).
    /// </summary>
    public bool IsRoot => !ParentEntity.IsAlive;

    /// <summary>
    /// Checks if this entity is a leaf node (has no children).
    /// </summary>
    public bool IsLeaf => ChildEntityIds.Count == 0;

    /// <summary>
    /// Gets the number of direct children.
    /// </summary>
    public int ChildCount => ChildEntityIds.Count;

    /// <summary>
    /// Checks if the specified entity is a direct child.
    /// </summary>
    /// <param name="entityId">Entity ID to check</param>
    /// <returns>True if the entity is a direct child</returns>
    public bool HasChild(int entityId) => ChildEntityIds.Contains(entityId);

    /// <summary>
    /// Creates a new hierarchy component with an additional child.
    /// </summary>
    /// <param name="childEntityId">ID of the child entity to add</param>
    /// <returns>New ParentHierarchyComponent with the child added</returns>
    public ParentHierarchyComponent WithChild(int childEntityId)
    {
        var newChildren = new HashSet<int>(ChildEntityIds) { childEntityId };
        return this with { ChildEntityIds = newChildren };
    }

    /// <summary>
    /// Creates a new hierarchy component with a child removed.
    /// </summary>
    /// <param name="childEntityId">ID of the child entity to remove</param>
    /// <returns>New ParentHierarchyComponent with the child removed</returns>
    public ParentHierarchyComponent WithoutChild(int childEntityId)
    {
        var newChildren = new HashSet<int>(ChildEntityIds);
        newChildren.Remove(childEntityId);
        return this with { ChildEntityIds = newChildren };
    }

    /// <summary>
    /// Creates a new hierarchy component with a different parent.
    /// </summary>
    /// <param name="newParent">The new parent entity</param>
    /// <returns>New ParentHierarchyComponent with updated parent</returns>
    public ParentHierarchyComponent WithParent(Entity newParent) =>
        this with { ParentEntity = newParent };

    /// <summary>
    /// Creates a root hierarchy component (removes parent relationship).
    /// </summary>
    /// <returns>New ParentHierarchyComponent as a root node</returns>
    public ParentHierarchyComponent AsRoot() =>
        this with { ParentEntity = new Entity(0, false) };

    /// <summary>
    /// Creates a new hierarchy component with multiple children added.
    /// </summary>
    /// <param name="childEntityIds">Collection of child entity IDs to add</param>
    /// <returns>New ParentHierarchyComponent with all children added</returns>
    public ParentHierarchyComponent WithChildren(IEnumerable<int> childEntityIds)
    {
        var newChildren = new HashSet<int>(ChildEntityIds);
        foreach (var childId in childEntityIds)
        {
            newChildren.Add(childId);
        }
        return this with { ChildEntityIds = newChildren };
    }

    /// <summary>
    /// Creates a new hierarchy component with all children removed.
    /// </summary>
    /// <returns>New ParentHierarchyComponent with no children (leaf node)</returns>
    public ParentHierarchyComponent WithoutAllChildren() =>
        this with { ChildEntityIds = new HashSet<int>() };
}