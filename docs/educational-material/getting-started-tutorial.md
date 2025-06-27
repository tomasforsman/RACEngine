---
title: "Getting Started Tutorial"
description: "Step-by-step tutorial for creating your first RACEngine game project"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["tutorial", "getting-started", "beginner", "first-project"]
---

# Getting Started Tutorial

## Overview

Welcome to RACEngine! This tutorial will guide you through creating your first game project, introducing you to the engine's core concepts while building a simple but complete game. By the end of this tutorial, you'll understand the ECS architecture, rendering pipeline, and how to create interactive gameplay.

## Prerequisites

- Basic C# programming knowledge
- .NET 8.0 SDK installed
- Visual Studio 2022 or VS Code with C# extension
- Git for version control

## What We'll Build

We'll create "Asteroid Dodge," a simple space game where the player controls a ship to avoid asteroids. This project will demonstrate:
- Entity-Component-System (ECS) architecture
- Input handling and player movement
- Collision detection and game state management
- Visual effects and rendering
- Audio integration

## Step 1: Setting Up Your Development Environment

### 1.1 Install Prerequisites

```bash
# Verify .NET SDK installation
dotnet --version
# Should show 8.0.x or later

# Clone RACEngine repository
git clone https://github.com/tomasforsman/RACEngine.git
cd RACEngine

# Build the engine to ensure everything works
dotnet build
```

### 1.2 Test the Installation

```bash
# Run sample games to verify installation
cd samples/SampleGame
dotnet run -- boidsample

# Try different samples
dotnet run -- shootersample
dotnet run -- bloomtest
```

If the samples run successfully, your environment is ready!

## Step 2: Creating Your First Project

### 2.1 Project Structure

Create a new project alongside the existing samples:

```bash
# From the RACEngine root directory
mkdir samples/AsteroidDodge
cd samples/AsteroidDodge
```

### 2.2 Create Project Files

Create `AsteroidDodge.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Rac.Engine\Rac.Engine.csproj" />
    <ProjectReference Include="..\..\src\Rac.ECS\Rac.ECS.csproj" />
    <ProjectReference Include="..\..\src\Rac.Rendering\Rac.Rendering.csproj" />
    <ProjectReference Include="..\..\src\Rac.Audio\Rac.Audio.csproj" />
    <ProjectReference Include="..\..\src\Rac.Core\Rac.Core.csproj" />
  </ItemGroup>

</Project>
```

### 2.3 Program Entry Point

Create `Program.cs`:

```csharp
using Rac.Engine;
using Rac.Core.Math;
using Rac.ECS;
using System;

namespace AsteroidDodge;

/// <summary>
/// Asteroid Dodge - A simple space game demonstrating RACEngine concepts
/// Educational note: This game showcases ECS architecture, input handling, and collision detection
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🚀 ASTEROID DODGE");
        Console.WriteLine("Navigate your ship to avoid asteroids!");
        Console.WriteLine();
        
        var game = new AsteroidDodgeGame();
        game.Run();
    }
}
```

## Step 3: Understanding ECS Basics

### 3.1 What is ECS?

Entity-Component-System is a design pattern that separates:
- **Entities**: Unique identifiers for game objects
- **Components**: Data containers (position, velocity, sprite, etc.)
- **Systems**: Logic that operates on components

> **Developer Tip**: RACEngine provides both direct ECS access (`engine.World.CreateEntity()`) for fine-grained control and convenience methods (`engine.CreateEntity()`) for simpler scenarios. This tutorial uses direct access to demonstrate ECS concepts, but you can use either approach based on your needs.

### 3.2 Direct ECS vs. Convenience Methods

RACEngine provides two approaches for entity management:

#### Direct ECS Access (Fine-grained Control)
```csharp
// Direct world access for full ECS control
var entity = world.CreateEntity();
world.SetComponent(entity, new PositionComponent(Vector2D.Zero));
world.SetComponent(entity, new NameComponent("Player"));
world.SetComponent(entity, new TagComponent("PlayerCharacter"));

// Query entities directly
var playerEntities = world.Query<NameComponent>()
    .Where(result => result.Component1.Name == "Player");
```

#### Engine Facade Convenience Methods (Simplified API)
```csharp
// Simplified entity creation with automatic naming
var player = engine.CreateEntity("Player");
engine.World.SetComponent(player, new PositionComponent(Vector2D.Zero));

// Convenient entity queries
var allEnemies = engine.GetEntitiesWithTag("Enemy");
var mainCamera = engine.FindEntityByName("MainCamera");
var totalEntities = engine.EntityCount;

// Clean entity destruction
engine.DestroyEntity(player);
```

> **Developer Tip**: Use direct ECS access when you need fine-grained control or are building reusable systems. Use convenience methods for simpler game logic and rapid prototyping. This tutorial demonstrates both approaches - feel free to choose based on your needs.

#### Named Entities and Tags Example
```csharp
// Create named entities for easy lookup
var player = engine.CreateEntity("Player");
var mainCamera = engine.CreateEntity("MainCamera");
var inventoryUI = engine.CreateEntity("InventoryPanel");

// Add tags for categorization
engine.World.SetComponent(player, new TagComponent(new[] { "PlayerCharacter", "Controllable" }));

// Query by tags
var allControllableEntities = engine.GetEntitiesWithTag("Controllable");
var allUIElements = engine.GetEntitiesWithTag("UI");

// Find specific entities by name
var cameraEntity = engine.FindEntityByName("MainCamera");
if (cameraEntity.HasValue)
{
    // Update camera position
    engine.World.SetComponent(cameraEntity.Value, new PositionComponent(newPos));
}
```

### 3.3 Define Game Components

Create `Components.cs`:

```csharp
using Rac.ECS;
using Rac.Core.Math;
using System;

namespace AsteroidDodge;

/// <summary>
/// Position component storing 2D coordinates
/// Educational note: Components are pure data with no behavior
/// </summary>
public readonly record struct PositionComponent(Vector2D<float> Position) : IComponent;

/// <summary>
/// Velocity component for movement calculations
/// Educational note: Separating velocity from position allows flexible movement systems
/// </summary>
public readonly record struct VelocityComponent(Vector2D<float> Velocity) : IComponent;

/// <summary>
/// Sprite component for visual representation
/// Educational note: Separating rendering data from logic improves modularity
/// </summary>
public readonly record struct SpriteComponent(
    Vector2D<float> Size,
    Vector4D<float> Color
) : IComponent;

/// <summary>
/// Player component marking entities controlled by the player
/// Educational note: Tag components identify special entity types
/// </summary>
public readonly record struct PlayerComponent() : IComponent;

/// <summary>
/// Asteroid component for moving obstacles
/// </summary>
public readonly record struct AsteroidComponent(
    float RotationSpeed
) : IComponent;

/// <summary>
/// Collision component defining entity bounds
/// Educational note: Axis-Aligned Bounding Box (AABB) collision detection
/// </summary>
public readonly record struct CollisionComponent(
    Vector2D<float> Size
) : IComponent;

/// <summary>
/// Health component for destructible entities
/// </summary>
public readonly record struct HealthComponent(
    int Current,
    int Maximum
) : IComponent;

/// <summary>
/// Game state component (singleton)
/// Educational note: Single-entity components store global game state
/// </summary>
public readonly record struct GameStateComponent(
    bool IsGameOver,
    int Score,
    float TimeAlive
) : IComponent;
```

## Step 4: Implementing Game Systems

### 4.1 Movement System

Create `Systems/MovementSystem.cs`:

```csharp
using Rac.ECS;
using System;

namespace AsteroidDodge.Systems;

/// <summary>
/// Movement system applies velocity to position
/// Educational note: Classic physics integration using Euler's method
/// Academic reference: "Game Physics Engine Development" (Ian Millington)
/// </summary>
public class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        // Query all entities with both Position and Velocity components
        foreach (var (entity, position, velocity) in world.Query<PositionComponent, VelocityComponent>())
        {
            // Basic physics integration: position = position + velocity * time
            var newPosition = position.Position + velocity.Velocity * deltaTime;
            
            // Update the entity's position
            world.SetComponent(entity, new PositionComponent(newPosition));
        }
    }
}
```

### 4.2 Input System

Create `Systems/InputSystem.cs`:

```csharp
using Rac.ECS;
using Rac.Core.Math;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AsteroidDodge.Systems;

/// <summary>
/// Input system handles player controls
/// Educational note: Separating input from movement logic allows flexible control schemes
/// </summary>
public class InputSystem : ISystem
{
    private readonly KeyboardState _keyboard;
    private readonly float _playerSpeed = 300.0f; // pixels per second
    
    public InputSystem(KeyboardState keyboard)
    {
        _keyboard = keyboard;
    }
    
    public void Update(World world, float deltaTime)
    {
        // Find the player entity
        foreach (var (entity, player, velocity) in world.Query<PlayerComponent, VelocityComponent>())
        {
            var newVelocity = Vector2D<float>.Zero;
            
            // Calculate movement based on input
            // Educational note: Using normalized diagonal movement for consistent speed
            if (_keyboard.IsKeyDown(Keys.W) || _keyboard.IsKeyDown(Keys.Up))
                newVelocity.Y += 1.0f;
            if (_keyboard.IsKeyDown(Keys.S) || _keyboard.IsKeyDown(Keys.Down))
                newVelocity.Y -= 1.0f;
            if (_keyboard.IsKeyDown(Keys.A) || _keyboard.IsKeyDown(Keys.Left))
                newVelocity.X -= 1.0f;
            if (_keyboard.IsKeyDown(Keys.D) || _keyboard.IsKeyDown(Keys.Right))
                newVelocity.X += 1.0f;
            
            // Normalize diagonal movement to maintain consistent speed
            if (newVelocity.LengthSquared > 0)
            {
                newVelocity = newVelocity.Normalized() * _playerSpeed;
            }
            
            world.SetComponent(entity, new VelocityComponent(newVelocity));
        }
    }
}
```

### 4.3 Collision System

Create `Systems/CollisionSystem.cs`:

```csharp
using Rac.ECS;
using Rac.Core.Math;
using System.Collections.Generic;

namespace AsteroidDodge.Systems;

/// <summary>
/// Collision detection system using AABB (Axis-Aligned Bounding Box)
/// Educational note: AABB collision is fast and suitable for simple games
/// Academic reference: "Real-Time Collision Detection" (Christer Ericson)
/// </summary>
public class CollisionSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        var collisionEntities = new List<(Entity, PositionComponent, CollisionComponent)>();
        
        // Collect all entities with collision components
        foreach (var (entity, position, collision) in world.Query<PositionComponent, CollisionComponent>())
        {
            collisionEntities.Add((entity, position, collision));
        }
        
        // Check all pairs for collisions
        for (int i = 0; i < collisionEntities.Count; i++)
        {
            for (int j = i + 1; j < collisionEntities.Count; j++)
            {
                var (entity1, pos1, col1) = collisionEntities[i];
                var (entity2, pos2, col2) = collisionEntities[j];
                
                if (CheckAABBCollision(pos1, col1, pos2, col2))
                {
                    HandleCollision(world, entity1, entity2);
                }
            }
        }
    }
    
    /// <summary>
    /// AABB collision detection algorithm
    /// Educational note: Two boxes collide if they overlap on both X and Y axes
    /// </summary>
    private bool CheckAABBCollision(
        PositionComponent pos1, CollisionComponent col1,
        PositionComponent pos2, CollisionComponent col2)
    {
        var min1 = pos1.Position - col1.Size * 0.5f;
        var max1 = pos1.Position + col1.Size * 0.5f;
        var min2 = pos2.Position - col2.Size * 0.5f;
        var max2 = pos2.Position + col2.Size * 0.5f;
        
        return max1.X >= min2.X && min1.X <= max2.X &&
               max1.Y >= min2.Y && min1.Y <= max2.Y;
    }
    
    /// <summary>
    /// Handle collision between two entities
    /// Educational note: Different collision responses based on entity types
    /// </summary>
    private void HandleCollision(World world, Entity entity1, Entity entity2)
    {
        var isPlayer1 = world.GetComponent<PlayerComponent>(entity1).HasValue;
        var isPlayer2 = world.GetComponent<PlayerComponent>(entity2).HasValue;
        var isAsteroid1 = world.GetComponent<AsteroidComponent>(entity1).HasValue;
        var isAsteroid2 = world.GetComponent<AsteroidComponent>(entity2).HasValue;
        
        // Player-Asteroid collision
        if ((isPlayer1 && isAsteroid2) || (isPlayer2 && isAsteroid1))
        {
            var playerEntity = isPlayer1 ? entity1 : entity2;
            DamagePlayer(world, playerEntity);
        }
    }
    
    private void DamagePlayer(World world, Entity playerEntity)
    {
        var health = world.GetComponent<HealthComponent>(playerEntity);
        if (health.HasValue)
        {
            var newHealth = new HealthComponent(health.Value.Current - 1, health.Value.Maximum);
            world.SetComponent(playerEntity, newHealth);
            
            // Game over if health reaches zero
            if (newHealth.Current <= 0)
            {
                TriggerGameOver(world);
            }
        }
    }
    
    private void TriggerGameOver(World world)
    {
        // Find game state entity and mark game over
        foreach (var (entity, gameState) in world.Query<GameStateComponent>())
        {
            var newGameState = gameState with { IsGameOver = true };
            world.SetComponent(entity, newGameState);
            break;
        }
    }
}
```

## Step 5: Creating the Game Class

### 5.1 Main Game Class

Create `AsteroidDodgeGame.cs`:

```csharp
using Rac.Engine;
using Rac.ECS;
using Rac.Core.Math;
using Rac.Rendering;
using AsteroidDodge.Systems;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AsteroidDodge;

/// <summary>
/// Main game class demonstrating RACEngine usage
/// Educational note: Game loop pattern with ECS system scheduling
/// </summary>
public class AsteroidDodgeGame : GameWindow
{
    private World _world;
    private List<ISystem> _systems;
    private Entity _playerEntity;
    private Entity _gameStateEntity;
    
    private Random _random = new();
    private float _asteroidSpawnTimer = 0.0f;
    private const float AsteroidSpawnInterval = 2.0f;
    
    public AsteroidDodgeGame() : base(
        GameWindowSettings.Default,
        new NativeWindowSettings()
        {
            Size = new OpenTK.Mathematics.Vector2i(800, 600),
            Title = "Asteroid Dodge - RACEngine Tutorial",
            WindowBorder = WindowBorder.Fixed
        })
    {
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        Console.WriteLine("🎮 Initializing Asteroid Dodge...");
        
        InitializeECS();
        CreatePlayer();
        CreateGameState();
        
        Console.WriteLine("✅ Game initialized successfully!");
        Console.WriteLine("🎮 Controls: WASD or Arrow Keys to move");
    }
    
    /// <summary>
    /// Initialize ECS world and systems
    /// Educational note: System order matters - input before movement before collision
    /// </summary>
    private void InitializeECS()
    {
        _world = new World();
        _systems = new List<ISystem>
        {
            new InputSystem(KeyboardState),
            new MovementSystem(),
            new AsteroidSpawnerSystem(_random),
            new CollisionSystem(),
            new BoundarySystem(Size.X, Size.Y),
            new GameStateSystem(),
            new RenderSystem() // Renders all sprites
        };
    }
    
    /// <summary>
    /// Create the player entity with all necessary components
    /// Educational note: Using engine convenience methods for simplified entity management
    /// </summary>
    private void CreatePlayer()
    {
        // Use convenience method to create named entity
        _playerEntity = engine.CreateEntity("Player");
        
        // Position at center of screen
        engine.World.SetComponent(_playerEntity, new PositionComponent(
            new Vector2D<float>(Size.X / 2.0f, Size.Y / 2.0f)));
        
        // Initially stationary
        engine.World.SetComponent(_playerEntity, new VelocityComponent(Vector2D<float>.Zero));
        
        // Blue square sprite
        engine.World.SetComponent(_playerEntity, new SpriteComponent(
            new Vector2D<float>(30, 30),
            new Vector4D<float>(0.0f, 0.5f, 1.0f, 1.0f))); // Blue
        
        // Add tags for easy querying
        engine.World.SetComponent(_playerEntity, new TagComponent(new[] { "Player", "Controllable" }));
        
        // Collision detection
        engine.World.SetComponent(_playerEntity, new CollisionComponent(
            new Vector2D<float>(30, 30)));
        
        // Health
        engine.World.SetComponent(_playerEntity, new HealthComponent(3, 3));
        
        Console.WriteLine($"✅ Created player entity (ID: {_playerEntity.Id}) with name 'Player'");
    }
    
    /// <summary>
    /// Create game state entity
    /// Educational note: Singleton pattern using ECS - one entity holds global state
    /// </summary>
    private void CreateGameState()
    {
        _gameStateEntity = _world.CreateEntity();
        _world.SetComponent(_gameStateEntity, new GameStateComponent(
            IsGameOver: false,
            Score: 0,
            TimeAlive: 0.0f));
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        float deltaTime = (float)args.Time;
        
        // Update all systems in order
        foreach (var system in _systems)
        {
            system.Update(_world, deltaTime);
        }
        
        // Check for game over
        var gameState = _world.GetComponent<GameStateComponent>(_gameStateEntity);
        if (gameState.HasValue && gameState.Value.IsGameOver)
        {
            Console.WriteLine($"💀 GAME OVER! You survived {gameState.Value.TimeAlive:F1} seconds");
            Console.WriteLine($"📊 Final Score: {gameState.Value.Score}");
            Close();
        }
        
        // Spawn asteroids periodically
        _asteroidSpawnTimer += deltaTime;
        if (_asteroidSpawnTimer >= AsteroidSpawnInterval)
        {
            SpawnAsteroid();
            _asteroidSpawnTimer = 0.0f;
        }
    }
    
    /// <summary>
    /// Spawn an asteroid from a random edge of the screen
    /// Educational note: Procedural content generation for endless gameplay
    /// </summary>
    private void SpawnAsteroid()
    {
        var asteroid = _world.CreateEntity();
        
        // Random spawn position on screen edge
        Vector2D<float> spawnPos;
        Vector2D<float> velocity;
        
        int edge = _random.Next(4); // 0=top, 1=right, 2=bottom, 3=left
        float speed = _random.NextSingle() * 100 + 50; // 50-150 pixels/second
        
        switch (edge)
        {
            case 0: // Top
                spawnPos = new Vector2D<float>(_random.NextSingle() * Size.X, Size.Y + 50);
                velocity = new Vector2D<float>(0, -speed);
                break;
            case 1: // Right
                spawnPos = new Vector2D<float>(Size.X + 50, _random.NextSingle() * Size.Y);
                velocity = new Vector2D<float>(-speed, 0);
                break;
            case 2: // Bottom
                spawnPos = new Vector2D<float>(_random.NextSingle() * Size.X, -50);
                velocity = new Vector2D<float>(0, speed);
                break;
            default: // Left
                spawnPos = new Vector2D<float>(-50, _random.NextSingle() * Size.Y);
                velocity = new Vector2D<float>(speed, 0);
                break;
        }
        
        var size = _random.NextSingle() * 30 + 20; // 20-50 pixels
        
        _world.SetComponent(asteroid, new PositionComponent(spawnPos));
        _world.SetComponent(asteroid, new VelocityComponent(velocity));
        _world.SetComponent(asteroid, new SpriteComponent(
            new Vector2D<float>(size, size),
            new Vector4D<float>(0.8f, 0.4f, 0.0f, 1.0f))); // Orange
        _world.SetComponent(asteroid, new AsteroidComponent(_random.NextSingle() * 360 - 180));
        _world.SetComponent(asteroid, new CollisionComponent(new Vector2D<float>(size, size)));
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        // Clear screen
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        // The RenderSystem handles actual rendering of sprites
        // This is just the OpenGL context management
        
        SwapBuffers();
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}
```

## Step 6: Additional Systems

### 6.1 Asteroid Spawner System

Create `Systems/AsteroidSpawnerSystem.cs`:

```csharp
using Rac.ECS;
using System;

namespace AsteroidDodge.Systems;

/// <summary>
/// System responsible for spawning asteroids
/// Educational note: Separating spawning logic into its own system improves modularity
/// </summary>
public class AsteroidSpawnerSystem : ISystem
{
    private readonly Random _random;
    
    public AsteroidSpawnerSystem(Random random)
    {
        _random = random;
    }
    
    public void Update(World world, float deltaTime)
    {
        // This system could handle more complex spawning logic
        // For now, spawning is handled in the main game class
        // In a larger game, this would manage spawn rates, patterns, etc.
    }
}
```

### 6.2 Boundary System

Create `Systems/BoundarySystem.cs`:

```csharp
using Rac.ECS;
using System.Collections.Generic;

namespace AsteroidDodge.Systems;

/// <summary>
/// System that removes entities when they leave the screen bounds
/// Educational note: Automatic cleanup prevents memory leaks from endless spawning
/// </summary>
public class BoundarySystem : ISystem
{
    private readonly float _screenWidth;
    private readonly float _screenHeight;
    private readonly float _boundary = 100.0f; // Extra space outside screen
    
    public BoundarySystem(float screenWidth, float screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }
    
    public void Update(World world, float deltaTime)
    {
        var entitiesToRemove = new List<Entity>();
        
        foreach (var (entity, position) in world.Query<PositionComponent>())
        {
            // Skip player entity (should be kept in bounds differently)
            if (world.GetComponent<PlayerComponent>(entity).HasValue)
                continue;
                
            // Check if entity is outside screen bounds
            if (position.Position.X < -_boundary ||
                position.Position.X > _screenWidth + _boundary ||
                position.Position.Y < -_boundary ||
                position.Position.Y > _screenHeight + _boundary)
            {
                entitiesToRemove.Add(entity);
            }
        }
        
        // Remove out-of-bounds entities
        foreach (var entity in entitiesToRemove)
        {
            world.DestroyEntity(entity);
        }
    }
}
```

### 6.3 Game State System

Create `Systems/GameStateSystem.cs`:

```csharp
using Rac.ECS;

namespace AsteroidDodge.Systems;

/// <summary>
/// System that manages game state updates
/// Educational note: Centralized state management for game rules
/// </summary>
public class GameStateSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        foreach (var (entity, gameState) in world.Query<GameStateComponent>())
        {
            if (!gameState.IsGameOver)
            {
                // Update time alive (score based on survival time)
                var newTimeAlive = gameState.TimeAlive + deltaTime;
                var newScore = (int)(newTimeAlive * 10); // 10 points per second
                
                var newGameState = gameState with 
                { 
                    TimeAlive = newTimeAlive,
                    Score = newScore
                };
                
                world.SetComponent(entity, newGameState);
            }
            break; // Only one game state entity
        }
    }
}
```

## Step 7: Running Your Game

### 7.1 Build and Run

```bash
# From samples/AsteroidDodge directory
dotnet build
dotnet run
```

### 7.2 Testing Your Game

1. **Movement**: Use WASD or arrow keys to move your blue square
2. **Obstacles**: Orange asteroids will spawn from screen edges
3. **Collision**: Touching asteroids damages you (you have 3 health)
4. **Score**: Points increase based on survival time
5. **Game Over**: Game ends when health reaches zero

## Step 8: Understanding What You Built

### 8.1 ECS Architecture Benefits

**Data-Oriented Design**: Components store data, systems process logic
```csharp
// Traditional OOP approach (avoid this)
public class GameObject
{
    public Vector2 Position;
    public Vector2 Velocity;
    
    public void Update() // Mixed data and behavior - harder to test and maintain
    {
        Position += Velocity * Time.deltaTime;
    }
}

// ECS approach (preferred)
public readonly record struct PositionComponent(Vector2 Position) : IComponent; // Pure data
public readonly record struct VelocityComponent(Vector2 Velocity) : IComponent; // Pure data

public class MovementSystem : ISystem // Pure logic - easy to test
{
    public void Update(World world, float deltaTime) { /* ... */ }
}
```

**Composition over Inheritance**: Build entities by combining components
```csharp
// Player = Position + Velocity + Sprite + Player + Collision + Health
// Asteroid = Position + Velocity + Sprite + Asteroid + Collision
// Bullet = Position + Velocity + Sprite + Bullet + Collision
```

### 8.2 System Dependencies and Order

Systems run in a specific order to ensure correct behavior:
1. **InputSystem**: Process user input first
2. **MovementSystem**: Apply movement based on velocities
3. **CollisionSystem**: Detect and handle collisions
4. **BoundarySystem**: Clean up out-of-bounds entities
5. **GameStateSystem**: Update game state
6. **RenderSystem**: Draw everything to screen

### 8.3 Performance Benefits

- **Cache-friendly**: Components stored contiguously in memory
- **Batch processing**: Systems operate on all entities of a type at once
- **Parallel potential**: Different systems can run in parallel (advanced topic)

## Step 9: Next Steps and Improvements

### 9.1 Visual Enhancements

Add more sophisticated rendering:
```csharp
// Rotation component for spinning asteroids
public readonly record struct RotationComponent(float Angle, float Speed) : IComponent;

// Animation component for sprite frames
public readonly record struct AnimationComponent(string AnimationName, float Time) : IComponent;

// Particle effects for explosions
public readonly record struct ParticleEmitterComponent(string EffectName) : IComponent;
```

### 9.2 Audio Integration

Add sound effects and music:
```csharp
// Audio component for sound effects
public readonly record struct AudioSourceComponent(string SoundId, bool Loop = false) : IComponent;

// Play sound when collision occurs
private void HandleCollision(World world, Entity entity1, Entity entity2)
{
    // Add explosion sound
    world.SetComponent(entity1, new AudioSourceComponent("explosion"));
    // ... existing collision logic
}
```

### 9.3 Advanced Gameplay Features

**Power-ups and Abilities**:
```csharp
public readonly record struct PowerUpComponent(PowerUpType Type, float Duration) : IComponent;
public readonly record struct ShieldComponent(float RemainingTime) : IComponent;
public readonly record struct WeaponComponent(float FireRate, float LastFireTime) : IComponent;
```

**AI and Pathfinding**:
```csharp
public readonly record struct AIComponent(AIBehavior Behavior, Entity Target) : IComponent;
public readonly record struct PathfindingComponent(Vector2[] Waypoints, int CurrentWaypoint) : IComponent;
```

### 9.4 Engine Convenience Methods

The engine facade provides helpful convenience methods that simplify common entity management tasks:

#### Entity Creation and Naming
```csharp
// Create unnamed entities (traditional approach)
var bullet = engine.CreateEntity();

// Create named entities for easy lookup
var player = engine.CreateEntity("Player");
var boss = engine.CreateEntity("BossEnemy");
var healthBar = engine.CreateEntity("HealthUI");

// Find entities by name later
var playerEntity = engine.FindEntityByName("Player");
if (playerEntity.HasValue)
{
    // Update player position
    engine.World.SetComponent(playerEntity.Value, new PositionComponent(newPosition));
}
```

#### Tag-Based Entity Management
```csharp
// Add tags to entities for categorization
engine.World.SetComponent(enemy1, new TagComponent("Enemy"));
engine.World.SetComponent(enemy2, new TagComponent(new[] { "Enemy", "Fast" }));
engine.World.SetComponent(collectible, new TagComponent("Powerup"));

// Query entities by tags
var allEnemies = engine.GetEntitiesWithTag("Enemy");
var fastEnemies = engine.GetEntitiesWithTag("Fast");
var powerups = engine.GetEntitiesWithTag("Powerup");

// Perform operations on tagged entities
foreach (var enemy in allEnemies)
{
    // Apply damage to all enemies
    var health = engine.World.GetComponent<HealthComponent>(enemy);
    if (health.HasValue)
    {
        var newHealth = health.Value with { Current = health.Value.Current - damage };
        engine.World.SetComponent(enemy, newHealth);
    }
}
```

#### Entity Counting and Management
```csharp
// Get total entity count for debugging/UI
Console.WriteLine($"Total entities: {engine.EntityCount}");

// Clean entity destruction (removes from all systems and components)
engine.DestroyEntity(expiredBullet);

// Bulk operations
var expiredEntities = engine.GetEntitiesWithTag("Temporary")
    .Where(entity => ShouldExpire(entity));
    
foreach (var entity in expiredEntities)
{
    engine.DestroyEntity(entity);
}

Console.WriteLine($"Cleaned up {expiredEntities.Count()} expired entities");
Console.WriteLine($"Remaining entities: {engine.EntityCount}");
```

#### Practical Usage Patterns
```csharp
/// <summary>
/// Example system that demonstrates practical convenience method usage
/// </summary>
public class GameManagerSystem : ISystem
{
    private readonly IEngineFacade _engine;
    
    public GameManagerSystem(IEngineFacade engine)
    {
        _engine = engine;
    }
    
    public void Update(World world, float deltaTime)
    {
        // Find player for camera following
        var player = _engine.FindEntityByName("Player");
        if (player.HasValue)
        {
            UpdateCameraTarget(player.Value);
        }
        
        // Spawn enemies based on current count
        var currentEnemies = _engine.GetEntitiesWithTag("Enemy").Count();
        if (currentEnemies < 5)
        {
            SpawnEnemy();
        }
        
        // Check if boss exists
        var boss = _engine.FindEntityByName("Boss");
        if (boss == null && ShouldSpawnBoss())
        {
            SpawnBoss();
        }
        
        // Display entity count for debugging
        if (Time.frameCount % 60 == 0) // Every second at 60 FPS
        {
            Console.WriteLine($"Entities: {_engine.EntityCount}");
        }
    }
    
    private void SpawnEnemy()
    {
        var enemy = _engine.CreateEntity($"Enemy_{Random.Next(1000, 9999)}");
        _engine.World.SetComponent(enemy, new TagComponent("Enemy"));
        // ... add other components
    }
    
    private void SpawnBoss()
    {
        var boss = _engine.CreateEntity("Boss");
        _engine.World.SetComponent(boss, new TagComponent(new[] { "Enemy", "Boss" }));
        // ... add boss-specific components
    }
}
```

### 9.5 Performance Optimization

**Spatial Partitioning for Collision Detection**:
```csharp
/// <summary>
/// Spatial hash grid for O(n) collision detection instead of O(n²)
/// Educational note: Divides space into grid cells for efficient querying
/// </summary>
public class SpatialHashGrid
{
    private readonly Dictionary<int, List<Entity>> _grid = new();
    private readonly float _cellSize;
    
    public IEnumerable<Entity> Query(Vector2 position, float radius)
    {
        // Return only entities in nearby grid cells
    }
}
```

## Troubleshooting

### Common Issues

**Build Errors**:
- Ensure .NET 8.0 SDK is installed
- Check that all project references are correct
- Run `dotnet restore` to install dependencies

**Runtime Errors**:
- Check console output for detailed error messages
- Verify OpenGL drivers are up to date
- Ensure all components are properly initialized

**Performance Issues**:
- Use Debug vs Release build configuration
- Profile with Visual Studio or dotMemory
- Check for memory leaks from undestroyed entities

### Getting Help

- Review [Architecture Documentation](../architecture/) for design concepts
- Check [Code Style Guidelines](../code-guides/code-style-guidelines.md) for best practices
- Explore existing [Sample Games](../../samples/) for more examples
- Open issues on GitHub for specific problems

## Conclusion

Congratulations! You've built your first RACEngine game and learned:

- **ECS Architecture**: Entity-Component-System design pattern
- **Game Loop**: Update systems in proper order each frame
- **Input Handling**: Converting user input to game actions
- **Collision Detection**: AABB collision detection algorithm
- **Game State Management**: Tracking score, health, and game over conditions
- **Procedural Generation**: Spawning enemies dynamically

This foundation will serve you well as you explore more advanced features like 3D graphics, physics simulation, audio systems, and AI behaviors.

### What's Next?

1. **Enhance your game** with the improvements suggested above
2. **Study the sample games** to see more advanced techniques
3. **Read the architecture docs** to understand engine internals
4. **Contribute to RACEngine** by fixing bugs or adding features
5. **Build larger projects** using the knowledge you've gained

Welcome to the RACEngine community! Happy game development! 🎮

## See Also

- [Architecture Documentation](../architecture/) - Deep dive into engine design
- [User Guides](../user-guides/) - Detailed feature documentation
- [Sample Games](../../samples/) - More complete examples
- [Educational Material](../educational-material/) - Game engine concepts and theory

## Changelog

- 2025-06-26: Initial comprehensive getting started tutorial with complete Asteroid Dodge game example