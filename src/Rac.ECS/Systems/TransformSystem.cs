using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;

namespace Rac.ECS.Systems;

/// <summary>
/// Computes world-space transformations for entities with hierarchical parent-child relationships.
/// 
/// HIERARCHICAL TRANSFORM ALGORITHM:
/// The system implements a depth-first traversal of the scene graph to compute world transforms:
/// 1. Start with root entities (no parent in ParentHierarchyComponent)
/// 2. Compute their world transform from local transform (local = world for roots)
/// 3. Recursively process all children, combining parent world + child local transforms
/// 4. Cache results in WorldTransformComponent for use by rendering/physics systems
/// 
/// MATHEMATICAL FOUNDATION:
/// For each entity in hierarchy:
/// - Root: WorldMatrix = LocalMatrix
/// - Child: WorldMatrix = ParentWorldMatrix * LocalMatrix
/// 
/// Transform composition follows standard graphics pipeline order:
/// - Scale applied first (changes object size)
/// - Rotation applied second (around scaled origin)
/// - Translation applied last (moves to final position)
/// 
/// PERFORMANCE OPTIMIZATIONS:
/// - Processes only entities with TransformComponent (sparse data)
/// - Depth-first traversal minimizes matrix computations
/// - Caches world transforms to avoid recomputation
/// - Batch processes siblings for improved cache locality
/// 
/// EDUCATIONAL NOTES:
/// This system demonstrates several key computer graphics concepts:
/// - Scene graph traversal algorithms
/// - Matrix composition and transform inheritance
/// - Hierarchical coordinate systems
/// - Data-oriented system design in ECS architecture
/// 
/// SYSTEM DEPENDENCIES:
/// - Requires: TransformComponent (local transform data)
/// - Optional: ParentHierarchyComponent (determines hierarchy, defaults to root)
/// - Produces: WorldTransformComponent (computed world transform)
/// - Used by: Rendering systems, physics systems, input systems
/// </summary>
public class TransformSystem : ISystem
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM STATE AND CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    private IWorld _world = null!;
    private readonly HashSet<int> _processedEntities = new();
    
    /// <summary>
    /// Initializes the system with access to the ECS world.
    /// Called once when the system is registered with the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for entity and component operations.</param>
    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM UPDATE AND MAIN PROCESSING LOOP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates world transforms for all entities in the hierarchy.
    /// Implements depth-first traversal starting from root entities.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update (unused by transform calculations)</param>
    public void Update(float deltaTime)
    {
        _processedEntities.Clear();

        // Find and process all root entities first
        var entitiesWithTransforms = _world.Query<TransformComponent>().ToList();
        
        foreach (var (entity, transform) in entitiesWithTransforms)
        {
            if (_processedEntities.Contains(entity.Id))
                continue;

            // Check if this entity is a root (no parent or parent not alive)
            var hierarchyResult = _world.Query<ParentHierarchyComponent>()
                .FirstOrDefault(h => h.Entity.Id == entity.Id);
            
            bool isRoot = hierarchyResult.Entity.Id == 0 || 
                         hierarchyResult.Component1.IsRoot;

            if (isRoot)
            {
                // Process root entity and its entire subtree
                ProcessEntityHierarchy(entity, transform, null);
            }
        }

        // Handle orphaned entities (have TransformComponent but no hierarchy)
        foreach (var (entity, transform) in entitiesWithTransforms)
        {
            if (!_processedEntities.Contains(entity.Id))
            {
                // Treat as root entity
                ProcessEntityHierarchy(entity, transform, null);
            }
        }
    }

    /// <summary>
    /// Cleans up system resources before the system is removed.
    /// Called once when the system is unregistered from the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for final cleanup operations.</param>
    public void Shutdown(IWorld world)
    {
        // Clear processed entities cache
        _processedEntities.Clear();
        
        // No additional cleanup needed for TransformSystem
        // World transform components are managed by the ECS world itself
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HIERARCHY TRAVERSAL AND TRANSFORM COMPUTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recursively processes an entity and all its children, computing world transforms.
    /// </summary>
    /// <param name="entity">The entity to process</param>
    /// <param name="localTransform">The local transform component of the entity</param>
    /// <param name="parentWorldTransform">The world transform of the parent (null for root entities)</param>
    private void ProcessEntityHierarchy(Entity entity, TransformComponent localTransform, WorldTransformComponent? parentWorldTransform)
    {
        if (_processedEntities.Contains(entity.Id))
            return;

        _processedEntities.Add(entity.Id);

        // ───────────────────────────────────────────────────────────────────────
        // COMPUTE WORLD TRANSFORM
        // ───────────────────────────────────────────────────────────────────────
        
        WorldTransformComponent worldTransform;
        
        if (parentWorldTransform.HasValue)
        {
            // Child entity: combine parent world transform with local transform
            var parent = parentWorldTransform.Value;
            
            // Compute world position: parent position + rotated local position
            var rotatedLocalPos = RotateVector(localTransform.LocalPosition, parent.WorldRotation);
            var scaledRotatedLocalPos = new Vector2D<float>(
                rotatedLocalPos.X * parent.WorldScale.X,
                rotatedLocalPos.Y * parent.WorldScale.Y
            );
            var worldPosition = parent.WorldPosition + scaledRotatedLocalPos;
            
            // Compute world rotation: parent rotation + local rotation
            var worldRotation = parent.WorldRotation + localTransform.LocalRotation;
            
            // Compute world scale: parent scale * local scale
            var worldScale = new Vector2D<float>(
                parent.WorldScale.X * localTransform.LocalScale.X,
                parent.WorldScale.Y * localTransform.LocalScale.Y
            );
            
            worldTransform = new WorldTransformComponent(worldPosition, worldRotation, worldScale);
        }
        else
        {
            // Root entity: local transform becomes world transform
            worldTransform = new WorldTransformComponent(
                localTransform.LocalPosition,
                localTransform.LocalRotation,
                localTransform.LocalScale
            );
        }
        
        _world.SetComponent(entity, worldTransform);

        // ───────────────────────────────────────────────────────────────────────
        // PROCESS CHILDREN RECURSIVELY
        // ───────────────────────────────────────────────────────────────────────
        
        // Find hierarchy component for this entity
        var hierarchyQuery = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);

        if (hierarchyQuery.Entity.Id != 0)
        {
            var hierarchy = hierarchyQuery.Component1;
            
            // Process each child entity
            foreach (var childId in hierarchy.ChildEntityIds)
            {
                var childEntity = new Entity(childId);
                
                // Get child's local transform
                var childTransformQuery = _world.Query<TransformComponent>()
                    .FirstOrDefault(t => t.Entity.Id == childId);
                
                if (childTransformQuery.Entity.Id != 0)
                {
                    ProcessEntityHierarchy(childEntity, childTransformQuery.Component1, worldTransform);
                }
            }
        }
    }

    /// <summary>
    /// Rotates a 2D vector by the specified angle.
    /// </summary>
    /// <param name="vector">The vector to rotate</param>
    /// <param name="angle">The rotation angle in radians</param>
    /// <returns>The rotated vector</returns>
    private static Vector2D<float> RotateVector(Vector2D<float> vector, float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        return new Vector2D<float>(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos
        );
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HIERARCHY MANAGEMENT UTILITIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Establishes a parent-child relationship between two entities.
    /// Updates both parent and child hierarchy components.
    /// </summary>
    /// <param name="parent">The parent entity</param>
    /// <param name="child">The child entity</param>
    /// <exception cref="ArgumentException">Thrown when trying to create circular references</exception>
    public void SetParent(Entity parent, Entity child)
    {
        if (parent.Id == child.Id)
            throw new ArgumentException("Entity cannot be its own parent");

        // Prevent circular references by checking if parent is descendant of child
        if (IsDescendant(child, parent))
            throw new ArgumentException("Cannot create circular parent-child relationship");

        // Remove child from its current parent (if any)
        RemoveFromCurrentParent(child);

        // Update child's hierarchy component
        var childHierarchy = GetOrCreateHierarchy(child).WithParent(parent);
        _world.SetComponent(child, childHierarchy);

        // Update parent's hierarchy component
        var parentHierarchy = GetOrCreateHierarchy(parent).WithChild(child.Id);
        _world.SetComponent(parent, parentHierarchy);
    }

    /// <summary>
    /// Removes the parent-child relationship, making the child a root entity.
    /// </summary>
    /// <param name="child">The child entity to detach</param>
    public void RemoveParent(Entity child)
    {
        RemoveFromCurrentParent(child);
        
        // Make child a root entity
        var childHierarchy = GetOrCreateHierarchy(child).AsRoot();
        _world.SetComponent(child, childHierarchy);
    }

    /// <summary>
    /// Gets an entity's hierarchy component or creates a default one.
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <returns>The hierarchy component</returns>
    private ParentHierarchyComponent GetOrCreateHierarchy(Entity entity)
    {
        var query = _world.Query<ParentHierarchyComponent>()
            .FirstOrDefault(h => h.Entity.Id == entity.Id);
        
        return query.Entity.Id != 0 ? query.Component1 : new ParentHierarchyComponent();
    }

    /// <summary>
    /// Removes an entity from its current parent's child list.
    /// </summary>
    /// <param name="child">The child entity to remove</param>
    private void RemoveFromCurrentParent(Entity child)
    {
        var childHierarchy = GetOrCreateHierarchy(child);
        if (!childHierarchy.IsRoot)
        {
            var parent = childHierarchy.ParentEntity;
            var parentHierarchy = GetOrCreateHierarchy(parent).WithoutChild(child.Id);
            _world.SetComponent(parent, parentHierarchy);
        }
    }

    /// <summary>
    /// Checks if an entity is a descendant of another entity.
    /// </summary>
    /// <param name="ancestor">The potential ancestor</param>
    /// <param name="descendant">The potential descendant</param>
    /// <returns>True if descendant is in ancestor's subtree</returns>
    private bool IsDescendant(Entity ancestor, Entity descendant)
    {
        var current = descendant;
        while (true)
        {
            var hierarchy = GetOrCreateHierarchy(current);
            if (hierarchy.IsRoot)
                return false;
            
            if (hierarchy.ParentEntity.Id == ancestor.Id)
                return true;
            
            current = hierarchy.ParentEntity;
        }
    }
}