// File: samples/SampleGame/ContainerSample.cs
//
// ═══════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL CONTAINER SYSTEM DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates the RACEngine Container System through practical examples:
//
// 1. CONTAINER SYSTEM CONCEPTS:
//    - PlaceIn: For containment relationships (items in backpack)
//    - AttachTo: For attachment relationships (scope on rifle)
//    - Semantic clarity: Clear distinction between different relationship types
//    - Hierarchical organization: Nested containers and mixed relationships
//
// 2. INVENTORY MANAGEMENT PATTERNS:
//    - Player backpack containing weapon racks and potion bags
//    - Organized storage with nested container hierarchies
//    - Dynamic item placement and removal from containers
//    - Query operations for finding containers and contained items
//
// 3. EQUIPMENT SYSTEM DEMONSTRATION:
//    - Weapons with attachment points for accessories
//    - Simultaneous containment and attachment relationships
//    - Visual representation of equipment configurations
//    - Dynamic attachment/detachment of weapon accessories
//
// 4. SCENE ORGANIZATION SHOWCASE:
//    - Level containers for scene element grouping
//    - Dynamic loading/unloading of scene sections
//    - Persistent vs temporary container management
//    - Hierarchical cleanup and lifecycle management
//
// 5. VISUAL FEEDBACK SYSTEM:
//    - Color-coded entities for different container types
//    - Connection lines showing parent-child relationships
//    - UI overlay displaying container hierarchy information
//    - Real-time updates as containers change
//
// CONTROLS & INTERACTION:
// - WASD: Camera movement (pan world view for exploration)
// - Q/E: Camera zoom in/out (scale world view)
// - Space: Create new inventory item and place in random container
// - 1: Toggle weapon attachments (attach/detach scope and silencer)
// - 2: Move items between containers (demonstrate reorganization)
// - 3: Create new container and populate with items
// - 4: Destroy random container (with contained items)
// - R: Reset scene to initial state
// - Tab: Toggle UI overlay visibility (show/hide container information)
// - V: Cycle through visual modes (normal/highlighted/hierarchy)
//
// EDUCATIONAL OBJECTIVES:
// - Understand container vs attachment semantic differences
// - Learn hierarchical organization patterns for games
// - Experience dynamic container management operations
// - Observe parent-child relationship visualization
// - Practice inventory and equipment system patterns
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Core.Manager;
using Rac.ECS;
using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace SampleGame;

public static class ContainerSample
{
    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL CONSTANTS FOR CONTAINER SYSTEM DEMONSTRATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static readonly Vector4D<float> ContainerColor = new(0.2f, 0.8f, 0.2f, 1f);   // Green for containers
    private static readonly Vector4D<float> ItemColor = new(0.8f, 0.2f, 0.2f, 1f);        // Red for items
    private static readonly Vector4D<float> WeaponColor = new(0.2f, 0.2f, 0.8f, 1f);      // Blue for weapons
    private static readonly Vector4D<float> AttachmentColor = new(0.8f, 0.8f, 0.2f, 1f);  // Yellow for attachments
    private static readonly Vector4D<float> ConnectionColor = new(0.6f, 0.6f, 0.6f, 0.8f); // Gray for connections

    // System references for operations
    private static TransformSystem? _transformSystem;
    private static ContainerSystem? _containerSystem;
    
    // Entity references for demo operations
    private static Entity _playerBackpack;
    private static Entity _weaponRack;
    private static Entity _potionBag;
    private static Entity _rifle;
    private static Entity _scope;
    private static Entity _silencer;
    private static Entity _levelContainer;
    
    private static readonly List<Entity> _items = new();
    private static readonly List<Entity> _containers = new();
    
    private static bool _showUIOverlay = true;
    private static bool _scopeAttached = false;
    private static bool _silencerAttached = false;
    private static int _visualMode = 0; // 0=normal, 1=highlighted, 2=hierarchy

    public static void Run(string[] args)
    {
        Console.WriteLine("=== Container System Demonstration ===");
        Console.WriteLine("This sample demonstrates the RACEngine Container System");
        Console.WriteLine("Use Space to add items, number keys for operations, Tab for UI");

        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        var engine = new EngineFacade(windowManager, inputService, configurationManager);
        
        // Initialize container system demonstration
        InitializeContainerDemo(engine);
        
        // Register input handlers for interactive demonstration
        RegisterInputHandlers(engine);
        
        // Register update and render handlers
        engine.UpdateEvent += OnUpdate;
        engine.RenderEvent += _ => OnRender(engine);
        
        // Run the main demonstration loop
        engine.Run();
    }

    private static void InitializeContainerDemo(EngineFacade engine)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CONTAINER SYSTEM INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════
        
        var world = engine.World;
        var transformSystem = new TransformSystem();
        var containerSystem = new ContainerSystem();
        
        // Add systems to engine
        engine.AddSystem(transformSystem);
        engine.AddSystem(containerSystem);
        
        // Store system references for later use
        _transformSystem = transformSystem;
        _containerSystem = containerSystem;
        
        // Clear any existing entities
        _items.Clear();
        _containers.Clear();
        
        // ─── Create Player Inventory Hierarchy ────────────────────────────────────
        // Demonstrates nested container organization
        
        _playerBackpack = containerSystem.CreateContainer(
            "PlayerBackpack", 
            new Vector2D<float>(0f, 0.5f),
            isLoaded: true,
            isPersistent: true
        );
        _containers.Add(_playerBackpack);
        
        _weaponRack = containerSystem.CreateContainer("WeaponRack");
        _weaponRack.PlaceIn(_playerBackpack, world, transformSystem, new Vector2D<float>(-0.3f, 0f));
        _containers.Add(_weaponRack);
        
        _potionBag = containerSystem.CreateContainer("PotionBag");
        _potionBag.PlaceIn(_playerBackpack, world, transformSystem, new Vector2D<float>(0.3f, 0f));
        _containers.Add(_potionBag);
        
        // ─── Create Level Organization Container ───────────────────────────────────
        // Demonstrates scene organization patterns
        
        _levelContainer = containerSystem.CreateContainer(
            "MainLevel", 
            new Vector2D<float>(0f, -0.7f),
            isLoaded: true,
            isPersistent: true
        );
        _containers.Add(_levelContainer);
        
        // ─── Create Weapon with Attachments ────────────────────────────────────────
        // Demonstrates mixed containment and attachment relationships
        
        _rifle = CreateItem(world, "Rifle", WeaponColor);
        _rifle.PlaceIn(_weaponRack, world, transformSystem, new Vector2D<float>(0f, 0f));
        _items.Add(_rifle);
        
        _scope = CreateItem(world, "Scope", AttachmentColor);
        _silencer = CreateItem(world, "Silencer", AttachmentColor);
        
        // Initially detached - can be attached with key '1'
        _scope.LoadToWorld(world, transformSystem, new Vector2D<float>(-0.8f, 0f));
        _silencer.LoadToWorld(world, transformSystem, new Vector2D<float>(-0.8f, -0.3f));
        _items.Add(_scope);
        _items.Add(_silencer);
        
        // ─── Create Initial Inventory Items ────────────────────────────────────────
        
        for (int i = 0; i < 3; i++)
        {
            var potion = CreateItem(world, $"Potion{i + 1}", ItemColor);
            potion.PlaceIn(_potionBag, world, transformSystem, new Vector2D<float>(i * 0.1f - 0.1f, 0f));
            _items.Add(potion);
        }
        
        // ─── Create Level Items ────────────────────────────────────────────────────
        
        for (int i = 0; i < 5; i++)
        {
            var levelItem = CreateItem(world, $"LevelItem{i + 1}", ItemColor);
            levelItem.PlaceIn(_levelContainer, world, transformSystem, 
                new Vector2D<float>((i - 2) * 0.2f, 0f));
            _items.Add(levelItem);
        }
        
        Console.WriteLine("Container demonstration initialized");
        Console.WriteLine($"Created {_containers.Count} containers and {_items.Count} items");
    }

    private static Entity CreateItem(IWorld world, string name, Vector4D<float> color)
    {
        // Create entity with visual representation
        var entity = world.CreateEntity()
            .WithTransform(world, Vector2D<float>.Zero, 0f, new Vector2D<float>(0.05f, 0.05f))
            .WithComponent(world, new ItemComponent(name, color));
        
        return entity;
    }

    private static void RegisterInputHandlers(EngineFacade engine)
    {
        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                OnKeyPressed(key, engine);
            }
        };
    }

    private static void OnKeyPressed(Key key, EngineFacade engine)
    {
        var world = engine.World;
        var transformSystem = _transformSystem!;
        var containerSystem = _containerSystem!;
        
        switch (key)
        {
            // ─── Camera Controls ────────────────────────────────────────────────────
            case Key.W: // Move up
                var camera = engine.CameraManager.GameCamera;
                camera.Position += new Vector2D<float>(0f, 0.1f);
                break;
                
            case Key.S: // Move down
                camera = engine.CameraManager.GameCamera;
                camera.Position += new Vector2D<float>(0f, -0.1f);
                break;
                
            case Key.A: // Move left
                camera = engine.CameraManager.GameCamera;
                camera.Position += new Vector2D<float>(-0.1f, 0f);
                break;
                
            case Key.D: // Move right
                camera = engine.CameraManager.GameCamera;
                camera.Position += new Vector2D<float>(0.1f, 0f);
                break;
                
            case Key.Q: // Zoom out
                camera = engine.CameraManager.GameCamera;
                camera.Zoom = Math.Max(0.1f, camera.Zoom - 0.1f);
                break;
                
            case Key.E: // Zoom in
                camera = engine.CameraManager.GameCamera;
                camera.Zoom = Math.Min(5f, camera.Zoom + 0.1f);
                break;
                
            case Key.R: // Reset camera and scene
                camera = engine.CameraManager.GameCamera;
                camera.Position = Vector2D<float>.Zero;
                camera.Zoom = 1f;
                camera.Rotation = 0f;
                InitializeContainerDemo(engine);
                Console.WriteLine("Scene reset to initial state");
                break;
                
            // ─── Container System Operations ────────────────────────────────────────
            case Key.Space: // Create new item and place in random container
                CreateRandomItem(world, transformSystem);
                break;
                
            case Key.Number1: // Toggle weapon attachments
                ToggleWeaponAttachments(world, transformSystem);
                break;
                
            case Key.Number2: // Move items between containers
                MoveItemsBetweenContainers(world, transformSystem);
                break;
                
            case Key.Number3: // Create new container with items
                CreateNewContainerWithItems(world, transformSystem, containerSystem);
                break;
                
            case Key.Number4: // Destroy random container
                DestroyRandomContainer(world, transformSystem, containerSystem);
                break;
                
            // ─── UI Controls ────────────────────────────────────────────────────────
            case Key.Tab: // Toggle UI overlay
                _showUIOverlay = !_showUIOverlay;
                Console.WriteLine($"UI overlay: {(_showUIOverlay ? "ON" : "OFF")}");
                break;
                
            case Key.V: // Cycle visual modes
                _visualMode = (_visualMode + 1) % 3;
                Console.WriteLine($"Visual mode: {(_visualMode == 0 ? "Normal" : _visualMode == 1 ? "Highlighted" : "Hierarchy")}");
                break;
        }
    }

    private static void CreateRandomItem(IWorld world, TransformSystem transformSystem)
    {
        var itemNames = new[] { "Sword", "Potion", "Gem", "Scroll", "Key" };
        var itemName = itemNames[Random.Shared.Next(itemNames.Length)];
        
        var item = CreateItem(world, itemName, ItemColor);
        
        // Place in random container
        if (_containers.Count > 0)
        {
            var container = _containers[Random.Shared.Next(_containers.Count)];
            var offset = new Vector2D<float>(
                (Random.Shared.NextSingle() - 0.5f) * 0.4f,
                (Random.Shared.NextSingle() - 0.5f) * 0.4f
            );
            
            item.PlaceIn(container, world, transformSystem, offset);
            _items.Add(item);
            
            if (world.TryGetComponent<ContainerComponent>(container, out var containerComp))
            {
                var containerName = containerComp.ContainerName;
                Console.WriteLine($"Created {itemName} and placed in {containerName}");
            }
        }
    }

    private static void ToggleWeaponAttachments(IWorld world, TransformSystem transformSystem)
    {
        if (!_scopeAttached)
        {
            // Attach scope to rifle
            _scope.AttachTo(_rifle, new Vector2D<float>(0f, 0.1f), world, transformSystem);
            _scopeAttached = true;
            Console.WriteLine("Scope attached to rifle");
        }
        else if (!_silencerAttached)
        {
            // Attach silencer to rifle  
            _silencer.AttachTo(_rifle, new Vector2D<float>(0.15f, 0f), world, transformSystem);
            _silencerAttached = true;
            Console.WriteLine("Silencer attached to rifle");
        }
        else
        {
            // Detach both
            _scope.RemoveFrom(world, transformSystem);
            _silencer.RemoveFrom(world, transformSystem);
            _scopeAttached = false;
            _silencerAttached = false;
            Console.WriteLine("Attachments removed from rifle");
        }
    }

    private static void MoveItemsBetweenContainers(IWorld world, TransformSystem transformSystem)
    {
        // Find items in containers and move them around
        var containedItems = _items.Where(item => item.IsInContainer(world)).ToList();
        
        if (containedItems.Count > 0 && _containers.Count > 1)
        {
            var item = containedItems[Random.Shared.Next(containedItems.Count)];
            var currentContainer = item.GetContainer(world);
            var targetContainer = _containers.Where(c => c != currentContainer).FirstOrDefault();
            
            if (targetContainer != default(Entity))
            {
                var offset = new Vector2D<float>(
                    (Random.Shared.NextSingle() - 0.5f) * 0.3f,
                    (Random.Shared.NextSingle() - 0.5f) * 0.3f
                );
                
                item.PlaceIn(targetContainer, world, transformSystem, offset);
                
                if (currentContainer.HasValue &&
                    world.TryGetComponent<ContainerComponent>(currentContainer.Value, out var currentComp) &&
                    world.TryGetComponent<ContainerComponent>(targetContainer, out var targetComp) &&
                    world.TryGetComponent<ItemComponent>(item, out var itemComp))
                {
                    var currentName = currentComp.ContainerName;
                    var targetName = targetComp.ContainerName;
                    var itemName = itemComp.Name;
                    
                    Console.WriteLine($"Moved {itemName} from {currentName} to {targetName}");
                }
            }
        }
    }

    private static void CreateNewContainerWithItems(IWorld world, TransformSystem transformSystem, ContainerSystem containerSystem)
    {
        // Create new container at random position
        var position = new Vector2D<float>(
            (Random.Shared.NextSingle() - 0.5f) * 1.5f,
            (Random.Shared.NextSingle() - 0.5f) * 1.0f
        );
        
        var newContainer = containerSystem.CreateContainer($"Container{_containers.Count + 1}", position);
        _containers.Add(newContainer);
        
        // Add a few items to it
        for (int i = 0; i < 3; i++)
        {
            var item = CreateItem(world, $"Item{_items.Count + 1}", ItemColor);
            var offset = new Vector2D<float>(i * 0.1f - 0.1f, 0f);
            item.PlaceIn(newContainer, world, transformSystem, offset);
            _items.Add(item);
        }
        
        if (world.TryGetComponent<ContainerComponent>(newContainer, out var containerComp))
        {
            var containerName = containerComp.ContainerName;
            Console.WriteLine($"Created new container: {containerName} with 3 items");
        }
    }

    private static void DestroyRandomContainer(IWorld world, TransformSystem transformSystem, ContainerSystem containerSystem)
    {
        if (_containers.Count > 2) // Keep at least 2 containers
        {
            var container = _containers[Random.Shared.Next(_containers.Count)];
            
            if (world.TryGetComponent<ContainerComponent>(container, out var containerComp))
            {
                var containerName = containerComp.ContainerName;
                
                // Remove contained items from our tracking lists
                var containedItems = _items.Where(item => item.GetContainer(world) == container).ToList();
                foreach (var item in containedItems)
                {
                    _items.Remove(item);
                }
                
                containerSystem.DestroyContainer(container, destroyContainedEntities: true);
                _containers.Remove(container);
                
                Console.WriteLine($"Destroyed container: {containerName} and {containedItems.Count} contained items");
            }
        }
        else
        {
            Console.WriteLine("Cannot destroy container - minimum 2 containers required");
        }
    }

    private static void OnUpdate(float deltaTime)
    {
        // Updates are handled automatically by the engine
    }

    private static void OnRender(EngineFacade engine)
    {
        var world = engine.World;
        
        // Clear the screen
        engine.Renderer.Clear();
        
        // Set game camera for world rendering
        engine.Renderer.SetActiveCamera(engine.CameraManager.GameCamera);
        
        // Draw background grid for reference
        DrawBackgroundGrid(engine);
        
        // Draw container hierarchy connections
        if (_visualMode >= 1)
        {
            DrawHierarchyConnections(engine, world);
        }
        
        // Draw all entities
        DrawContainers(engine, world);
        DrawItems(engine, world);
        
        // Draw UI overlay
        if (_showUIOverlay)
        {
            DrawUIOverlay(engine, world);
        }
        
        // Finalize frame
        engine.Renderer.FinalizeFrame();
    }

    private static void DrawBackgroundGrid(EngineFacade engine)
    {
        const float gridSize = 2f;
        const float gridSpacing = 0.2f;
        var gridVertices = new List<float>();

        // Vertical lines
        for (float x = -gridSize; x <= gridSize; x += gridSpacing)
        {
            gridVertices.AddRange(new[] { x, -gridSize, x, gridSize });
        }
        
        // Horizontal lines
        for (float y = -gridSize; y <= gridSize; y += gridSpacing)
        {
            gridVertices.AddRange(new[] { -gridSize, y, gridSize, y });
        }

        engine.Renderer.SetShaderMode(ShaderMode.Normal);
        engine.Renderer.SetPrimitiveType(PrimitiveType.Lines);
        engine.Renderer.SetColor(new Vector4D<float>(0.3f, 0.3f, 0.3f, 0.5f));
        engine.Renderer.UpdateVertices(gridVertices.ToArray());
        engine.Renderer.Draw();
    }

    private static void DrawHierarchyConnections(EngineFacade engine, IWorld world)
    {
        var connectionVertices = new List<float>();
        
        // Draw connections from parents to children
        foreach (var entity in _containers.Concat(_items))
        {
            if (world.TryGetComponent<ParentHierarchyComponent>(entity, out var hierarchy) && hierarchy.ParentEntity.IsAlive)
            {
                if (world.TryGetComponent<WorldTransformComponent>(hierarchy.ParentEntity, out var parentTransform) &&
                    world.TryGetComponent<WorldTransformComponent>(entity, out var childTransform))
                {
                    connectionVertices.AddRange(new[]
                    {
                        parentTransform.WorldPosition.X, parentTransform.WorldPosition.Y,
                        childTransform.WorldPosition.X, childTransform.WorldPosition.Y
                    });
                }
            }
        }
        
        if (connectionVertices.Count > 0)
        {
            engine.Renderer.SetShaderMode(ShaderMode.Normal);
            engine.Renderer.SetPrimitiveType(PrimitiveType.Lines);
            engine.Renderer.SetColor(ConnectionColor);
            engine.Renderer.UpdateVertices(connectionVertices.ToArray());
            engine.Renderer.Draw();
        }
    }

    private static void DrawContainers(EngineFacade engine, IWorld world)
    {
        var vertices = new List<float>();
        
        foreach (var container in _containers)
        {
            if (world.TryGetComponent<WorldTransformComponent>(container, out var transform))
            {
                AddSquareVertices(vertices, transform.WorldPosition, 0.1f);
            }
        }
        
        if (vertices.Count > 0)
        {
            engine.Renderer.SetShaderMode(_visualMode == 1 ? ShaderMode.SoftGlow : ShaderMode.Normal);
            engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
            engine.Renderer.SetColor(ContainerColor);
            engine.Renderer.UpdateVertices(vertices.ToArray());
            engine.Renderer.Draw();
        }
    }

    private static void DrawItems(EngineFacade engine, IWorld world)
    {
        var vertices = new List<float>();
        
        foreach (var item in _items)
        {
            if (world.TryGetComponent<WorldTransformComponent>(item, out var transform))
            {
                AddSquareVertices(vertices, transform.WorldPosition, 0.05f);
            }
        }
        
        if (vertices.Count > 0)
        {
            engine.Renderer.SetShaderMode(_visualMode == 1 ? ShaderMode.SoftGlow : ShaderMode.Normal);
            engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
            engine.Renderer.SetColor(ItemColor);
            engine.Renderer.UpdateVertices(vertices.ToArray());
            engine.Renderer.Draw();
        }
    }

    private static void AddSquareVertices(List<float> vertices, Vector2D<float> position, float size)
    {
        float halfSize = size / 2f;
        
        // Triangle 1
        vertices.AddRange(new[]
        {
            position.X - halfSize, position.Y - halfSize,
            position.X + halfSize, position.Y - halfSize,
            position.X - halfSize, position.Y + halfSize
        });
        
        // Triangle 2
        vertices.AddRange(new[]
        {
            position.X + halfSize, position.Y - halfSize,
            position.X + halfSize, position.Y + halfSize,
            position.X - halfSize, position.Y + halfSize
        });
    }

    private static void DrawUIOverlay(EngineFacade engine, IWorld world)
    {
        engine.Renderer.SetActiveCamera(engine.CameraManager.UICamera);
        
        // UI background
        engine.Renderer.SetShaderMode(ShaderMode.Normal);
        engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
        engine.Renderer.SetColor(new Vector4D<float>(0f, 0f, 0f, 0.7f));
        
        var uiVertices = new[]
        {
            -1f, 0.5f, 1f, 0.5f, -1f, 1f,
            1f, 0.5f, 1f, 1f, -1f, 1f
        };
        
        engine.Renderer.UpdateVertices(uiVertices);
        engine.Renderer.Draw();
        
        engine.Renderer.SetActiveCamera(engine.CameraManager.GameCamera);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// HELPER COMPONENTS FOR CONTAINER DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Component for tracking items in the container demonstration.
/// </summary>
public readonly record struct ItemComponent(
    string Name,
    Vector4D<float> Color
) : IComponent;