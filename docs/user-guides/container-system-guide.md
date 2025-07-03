# Container System User Guide

The RACEngine Container System provides an intuitive API for managing spatial relationships between entities, enabling composition-based game development with clear semantic distinctions between different relationship types.

## Quick Start

### Basic Setup

```csharp
using Rac.ECS;
using Rac.ECS.Core;
using Rac.ECS.Systems;

// Create world and systems
var world = new World();
var transformSystem = new TransformSystem();
var containerSystem = new ContainerSystem();

// Initialize systems
transformSystem.Initialize(world);
containerSystem.Initialize(world);
```

### Creating Your First Container

```csharp
// Create a simple container
var backpack = containerSystem.CreateContainer("PlayerBackpack");

// Create items to place in the container
var sword = world.CreateEntity();
var potion = world.CreateEntity();

// Place items in the container
sword.PlaceIn(backpack, world, transformSystem);
potion.PlaceIn(backpack, world, transformSystem, new Vector2D<float>(1f, 0f));
```

## Core Concepts

### Containment vs Attachment

The container system distinguishes between two fundamental relationship types:

#### Containment (PlaceIn)
Used for logical containers where items are "inside" something:
- Items in backpack
- Objects in room
- Files in folder
- Characters in vehicle

```csharp
// Containment requires target to have ContainerComponent
sword.PlaceIn(backpack, world, transformSystem);
character.PlaceIn(vehicle, world, transformSystem);
```

#### Attachment (AttachTo)
Used for physical connections where something is "attached to" something else:
- Scope on rifle
- Wheel on car
- Component on motherboard
- Decoration on wall

```csharp
// Attachment works with any entity (no container requirement)
scope.AttachTo(rifle, new Vector2D<float>(0f, 0.1f), world, transformSystem);
wheel.AttachTo(car, wheelMountPoint, world, transformSystem);
```

### Container Properties

Containers support three key properties:

- **ContainerName**: Human-readable identifier for the container's purpose
- **IsLoaded**: Whether the container is currently active in the world
- **IsPersistent**: Whether the container survives scene changes

```csharp
// Create container with all properties
var levelContainer = containerSystem.CreateContainer(
    name: "Forest_Level",
    position: new Vector2D<float>(100f, 200f),
    isLoaded: true,
    isPersistent: true
);
```

## Common Usage Patterns

### Inventory Systems

```csharp
// Create player inventory
var playerInventory = containerSystem.CreateContainer("PlayerInventory");

// Create item containers
var weaponRack = containerSystem.CreateContainer("WeaponRack");
var potionBag = containerSystem.CreateContainer("PotionBag");

// Organize inventory with nested containers
weaponRack.PlaceIn(playerInventory, world, transformSystem);
potionBag.PlaceIn(playerInventory, world, transformSystem);

// Place items in appropriate containers
sword.PlaceIn(weaponRack, world, transformSystem);
bow.PlaceIn(weaponRack, world, transformSystem);
healthPotion.PlaceIn(potionBag, world, transformSystem);
```

### Equipment Systems

```csharp
// Create weapon with attachment points
var rifle = world.CreateEntity();

// Attach accessories to specific points
var scope = world.CreateEntity();
var silencer = world.CreateEntity();
var flashlight = world.CreateEntity();

scope.AttachTo(rifle, new Vector2D<float>(0f, 0.1f), world, transformSystem);
silencer.AttachTo(rifle, new Vector2D<float>(0.5f, 0f), world, transformSystem);
flashlight.AttachTo(rifle, new Vector2D<float>(0.2f, -0.1f), world, transformSystem);

// Weapon can be placed in inventory while maintaining attachments
rifle.PlaceIn(playerBackpack, world, transformSystem);
// Scope, silencer, and flashlight remain attached to rifle
```

### Scene Organization

```csharp
// Create scene containers for organization
var mainLevel = containerSystem.CreateContainer("MainLevel", isPersistent: true);
var dynamicObjects = containerSystem.CreateContainer("DynamicObjects");
var staticEnvironment = containerSystem.CreateContainer("StaticEnvironment");

// Organize scene elements
dynamicObjects.PlaceIn(mainLevel, world, transformSystem);
staticEnvironment.PlaceIn(mainLevel, world, transformSystem);

// Place objects in appropriate containers
enemies.PlaceIn(dynamicObjects, world, transformSystem);
pickups.PlaceIn(dynamicObjects, world, transformSystem);
trees.PlaceIn(staticEnvironment, world, transformSystem);
rocks.PlaceIn(staticEnvironment, world, transformSystem);
```

### Level Streaming

```csharp
// Create containers for different areas
var currentArea = containerSystem.CreateContainer("CurrentArea", isLoaded: true);
var distantArea = containerSystem.CreateContainer("DistantArea", isLoaded: false);

// Dynamic loading based on player position
if (playerNearDistantArea)
{
    containerSystem.SetContainerLoaded(distantArea, true);
}

if (playerFarFromCurrentArea)
{
    containerSystem.SetContainerLoaded(currentArea, false);
}
```

## Advanced Operations

### Querying Container Relationships

```csharp
// Check if entity is in any container
bool isContained = entity.IsInContainer(world);

// Get the container entity
Entity? container = entity.GetContainer(world);

// Check if entity is in specific container
bool isInBackpack = entity.GetContainer(world) == backpack;
```

### Container Lifecycle Management

```csharp
// Create container with full configuration
var container = containerSystem.CreateContainer(
    name: "TempContainer",
    position: Vector2D<float>.Zero,
    isLoaded: true,
    isPersistent: false
);

// Modify container properties
containerSystem.SetContainerLoaded(container, false);
containerSystem.RenameContainer(container, "NewName");

// Cleanup with options
containerSystem.DestroyContainer(container, destroyContainedEntities: true);
```

### World Management

```csharp
// Load entity directly to world (removes from any container)
entity.LoadToWorld(world, transformSystem, spawnPosition);

// Remove entity from current container/attachment
entity.RemoveFrom(world, transformSystem);
```

## Best Practices

### Naming Conventions

Use descriptive container names that indicate purpose:

```csharp
// Good: Clear purpose
var playerBackpack = containerSystem.CreateContainer("PlayerBackpack");
var weaponStorage = containerSystem.CreateContainer("WeaponStorage");
var enemySpawner = containerSystem.CreateContainer("EnemySpawner");

// Avoid: Generic names
var container1 = containerSystem.CreateContainer("Container1");
var temp = containerSystem.CreateContainer("Temp");
```

### Container Hierarchy Design

Design container hierarchies that match your game's logical structure:

```csharp
// Logical hierarchy: Player → Equipment → Individual Items
player
├── backpack (container)
│   ├── weapons (container)
│   │   ├── sword
│   │   └── bow
│   └── consumables (container)
│       ├── potion
│       └── food
└── equipped (container)
    └── rifle
        ├── scope (attached)
        └── silencer (attached)
```

### Performance Considerations

- **Container Validation**: Only occurs during PlaceIn operations
- **Query Efficiency**: Use existing entity queries rather than iterating containers
- **Batch Operations**: Group multiple container operations when possible

```csharp
// Efficient: Batch creation
var containers = new[]
{
    containerSystem.CreateContainer("Container1"),
    containerSystem.CreateContainer("Container2"),
    containerSystem.CreateContainer("Container3")
};

// Efficient: Use ECS queries for finding containers
var allContainers = world.Query<ContainerComponent>();
```

### Error Handling

The container system provides clear validation and error handling:

```csharp
try
{
    // This will throw if target doesn't have ContainerComponent
    item.PlaceIn(nonContainer, world, transformSystem);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Container operation failed: {ex.Message}");
}

// Safe alternative: Check if entity is a container first
if (world.HasComponent<ContainerComponent>(target))
{
    item.PlaceIn(target, world, transformSystem);
}
```

## Integration with Other Systems

### Transform System Integration

Container operations work seamlessly with the transform system:

```csharp
// Positions are maintained in local space relative to container
item.PlaceIn(container, world, transformSystem, localPosition);

// World transforms are computed automatically
var worldPos = item.GetComponent<WorldTransformComponent>(world).Position;
```

### Rendering System Integration

Use containers for rendering optimizations:

```csharp
// Render only loaded containers
var loadedContainers = world.Query<ContainerComponent>()
    .Where(entity => entity.GetComponent<ContainerComponent>(world).IsLoaded);

foreach (var container in loadedContainers)
{
    // Render all entities in this container
    RenderContainer(container, world);
}
```

### Save System Integration

Leverage container persistence for save systems:

```csharp
// Save only persistent containers
var persistentContainers = world.Query<ContainerComponent>()
    .Where(entity => entity.GetComponent<ContainerComponent>(world).IsPersistent);

var saveData = new SaveData
{
    PersistentContainers = persistentContainers.ToList()
};
```

## Troubleshooting

### Common Issues

**PlaceIn fails with InvalidOperationException**
- Ensure target entity has ContainerComponent
- Use AttachTo for attachment relationships instead

**Entities not appearing in expected positions**
- Check local vs world position usage
- Verify transform system is running and up to date

**Container relationships not preserved after scene changes**
- Set IsPersistent = true for containers that should survive scene changes
- Ensure proper save/load handling of container relationships

### Debugging Container Hierarchies

```csharp
// Debug: Print container hierarchy
void PrintContainerHierarchy(Entity entity, World world, int depth = 0)
{
    var indent = new string(' ', depth * 2);
    var name = world.HasComponent<ContainerComponent>(entity)
        ? world.GetComponent<ContainerComponent>(entity).ContainerName
        : $"Entity_{entity.Id}";
    
    Console.WriteLine($"{indent}{name}");
    
    if (world.HasComponent<ParentHierarchyComponent>(entity))
    {
        var hierarchy = world.GetComponent<ParentHierarchyComponent>(entity);
        foreach (var child in hierarchy.Children)
        {
            PrintContainerHierarchy(child, world, depth + 1);
        }
    }
}
```

## Examples

See the sample game implementations for complete working examples of the container system in action, including inventory management, equipment systems, and scene organization patterns.