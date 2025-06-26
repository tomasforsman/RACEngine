# Hierarchical Transform System Usage Examples

The hierarchical transform system allows entities to have parent-child relationships with automatic world-space transform computation.

## Basic Usage Example

```csharp
using Rac.ECS.Core;
using Rac.ECS.Components;
using Rac.ECS.Systems;
using Silk.NET.Maths;

// Create world and transform system
var world = new World();
var transformSystem = new TransformSystem(world);

// Create entities
var character = world.CreateEntity();
var weapon = world.CreateEntity();
var scope = world.CreateEntity();

// Set up local transforms
character.SetLocalTransform(world, new Vector2D<float>(100f, 200f));
weapon.SetLocalTransform(world, new Vector2D<float>(10f, 0f)); // Right side of character
scope.SetLocalTransform(world, new Vector2D<float>(5f, 2f)); // Top of weapon

// Set up hierarchy: character -> weapon -> scope
character.AddChild(world, weapon, transformSystem);
weapon.AddChild(world, scope, transformSystem);

// Update transforms (typically called once per frame)
transformSystem.Update(0f);

// Query world positions
var characterWorldPos = character.GetWorldPosition(world); // (100, 200)
var weaponWorldPos = weapon.GetWorldPosition(world);       // (110, 200)
var scopeWorldPos = scope.GetWorldPosition(world);         // (115, 202)
```

## Core Components

### TransformComponent
Stores local position, rotation, and scale relative to parent:

```csharp
var transform = new TransformComponent(
    new Vector2D<float>(10f, 20f), // Local position
    MathF.PI / 4f,                 // Local rotation (45 degrees)
    new Vector2D<float>(2f, 2f)    // Local scale (2x size)
);
world.SetComponent(entity, transform);
```

### ParentHierarchyComponent
Manages parent-child relationships:

```csharp
// Make entity2 a child of entity1
transformSystem.SetParent(entity1, entity2);

// Remove parent relationship
transformSystem.RemoveParent(entity2);
```

### WorldTransformComponent
Contains computed world-space transform (automatically created by TransformSystem):

```csharp
var worldTransform = entity.GetWorldTransform(world);
if (worldTransform.HasValue)
{
    var worldPos = worldTransform.Value.WorldPosition;
    var worldRot = worldTransform.Value.WorldRotation;
    var worldScale = worldTransform.Value.WorldScale;
    var worldMatrix = worldTransform.Value.WorldMatrix;
}
```

## Advanced Usage

### Complex Hierarchy with Rotation and Scale

```csharp
// Create a rotating turret with a scaling barrel
var tank = world.CreateEntity();
var turret = world.CreateEntity();
var barrel = world.CreateEntity();

// Tank at origin
tank.SetLocalTransform(world, Vector2D<float>.Zero);

// Turret rotates around tank center
turret.SetLocalTransform(world, 
    Vector2D<float>.Zero,  // No offset from tank
    MathF.PI / 4f,         // 45 degree rotation
    Vector2D<float>.One    // Normal size
);

// Barrel extends from turret with scaling
barrel.SetLocalTransform(world,
    new Vector2D<float>(20f, 0f), // Extend forward from turret
    0f,                           // No additional rotation
    new Vector2D<float>(1f, 0.5f) // Half height
);

// Set up hierarchy
tank.AddChild(world, turret, transformSystem);
turret.AddChild(world, barrel, transformSystem);

// Update and query
transformSystem.Update(0f);

var barrelWorldPos = barrel.GetWorldPosition(world);
// Result: Barrel position includes tank position + rotated turret offset + rotated barrel offset
```

### Hierarchy Queries

```csharp
// Check entity status
bool isRoot = entity.IsRoot(world);
bool isLeaf = entity.IsLeaf(world);
int childCount = entity.GetChildCount(world);

// Get relationships
var parent = entity.GetParent(world);
var children = entity.GetChildren(world);

// Traverse hierarchy
foreach (var child in entity.GetChildren(world))
{
    ProcessEntity(child);
    
    // Recursively process grandchildren
    foreach (var grandchild in child.GetChildren(world))
    {
        ProcessEntity(grandchild);
    }
}
```

## Integration with Existing Systems

The transform system works alongside existing components. You can use both PositionComponent (for simple entities) and TransformComponent (for hierarchical entities) in the same world:

```csharp
// Simple entity with PositionComponent (existing code)
var simpleEntity = world.CreateEntity();
world.SetComponent(simpleEntity, new PositionComponent(50f, 60f));

// Hierarchical entity with TransformComponent (new code)
var hierarchicalEntity = world.CreateEntity();
hierarchicalEntity.SetLocalTransform(world, new Vector2D<float>(50f, 60f));
```

## Performance Considerations

- TransformSystem processes entities depth-first to minimize matrix computations
- World transforms are cached and only recomputed when needed
- The system scales O(n) with the number of entities with TransformComponent
- Consider batching hierarchy changes when possible

## Thread Safety

The transform system is not thread-safe. Ensure all hierarchy modifications and updates happen on the same thread, typically the main game thread.