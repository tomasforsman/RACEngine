# Engine Access Patterns Guide

## Overview

This guide explains how to effectively access RACEngine functionality as a game developer. Understanding these patterns will help you choose the right level of complexity for your needs and discover engine features naturally.

## Progressive Access Philosophy

### Start Simple, Grow Complex
RACEngine provides multiple ways to access the same functionality, designed for different experience levels and use cases:
- **Start with the facade** for common operations
- **Move to service interfaces** when you need more control
- **Access implementations directly** only for specialized scenarios

### Choose Your Complexity Level
Don't jump to advanced patterns unless you need them. The facade provides most functionality you'll use in typical game development.

## Access Level Guide

### Facade Level (Recommended Starting Point)

**Use facade methods for common operations:**
```csharp
// Entity management
var player = engine.CreateEntity("Player");
engine.DestroyEntity(enemy);

// Audio
engine.Audio.PlaySound("explosion.wav");
engine.Audio.SetMasterVolume(0.8f);

// Rendering
engine.Renderer.SetColor(1, 0, 0);
engine.Renderer.Draw();

// Systems
engine.AddSystem(new MovementSystem());
```

**When to use facade:**
- Learning the engine
- Prototyping and simple games
- Common operations (90% of game development)
- When you want simple, discoverable APIs

### Service Interface Level (When You Need More Control)

**Use service properties for advanced features:**
```csharp
// Advanced audio positioning
engine.Audio.PlaySound3D("footsteps.wav", x: 10f, y: 0f, z: 5f);
engine.Audio.SetListener(playerX, playerY, playerZ, forwardX, forwardY, forwardZ);

// Complex entity operations
var entities = engine.World.Query<PositionComponent, VelocityComponent>();
engine.World.SetComponent(entity, new CustomComponent());

// Advanced rendering
engine.Renderer.SetShaderMode(ShaderMode.Bloom);
engine.Renderer.SetUniform("glowIntensity", 2.5f);
```

**When to use service interfaces:**
- Need features not available on facade
- Complex configuration requirements
- Professional/production development
- Testing scenarios (mocking services)

### Implementation Level (Specialized Scenarios Only)

**Direct implementation access for engine-specific features:**
```csharp
// OpenGL-specific rendering operations
engine.AdvancedRenderer.SetPrimitiveType(PrimitiveType.LineStrip);

// Physics engine internals
var bepuPhysics = (BepuPhysicsService)engine.Physics;
```

**When to use implementations:**
- Absolutely need implementation-specific features
- Contributing to engine development
- Performance-critical scenarios with profiling data

## System-by-System Patterns

### Entity-Component-System (ECS)

**Common Operations (Facade):**
```csharp
// Simple entity creation
var player = engine.CreateEntity("Player");
var enemy = engine.CreateEntity();

// Quick queries
var playerEntities = engine.GetEntitiesWithTag("Player");
var boss = engine.FindEntityByName("Boss");
```

**Advanced Operations (World Service):**
```csharp
// Complex queries
var movingEntities = engine.World.Query<PositionComponent, VelocityComponent>();

// Direct component management
engine.World.SetComponent(entity, new HealthComponent(100, 100));
var position = engine.World.GetComponent<PositionComponent>(entity);
```

### Audio System

**Common Operations (Facade):**
```csharp
// Basic audio - most games only need this
engine.Audio.PlaySound("jump.wav");
engine.Audio.PlayMusic("background.ogg", loop: true);
engine.Audio.SetMasterVolume(0.7f);
```

**Advanced Operations (Service):**
```csharp
// 3D spatial audio
engine.Audio.PlaySound3D("engine.wav", x: carX, y: carY, z: carZ);
engine.Audio.SetListener(playerX, playerY, playerZ, lookX, lookY, lookZ);

// Category-specific volume control
engine.Audio.SetMusicVolume(0.6f);
engine.Audio.SetSfxVolume(0.9f);
```

### Rendering System

**Common Operations (Facade):**
```csharp
// Basic rendering - covers most 2D games
engine.Renderer.SetColor(1, 0, 0);
engine.Renderer.UpdateVertices(triangleVertices);
engine.Renderer.Draw();
```

**Advanced Operations (Service):**
```csharp
// Shader effects and post-processing
engine.Renderer.SetShaderMode(ShaderMode.Bloom);
engine.Renderer.SetUniform("bloomIntensity", 1.5f);

// Typed vertex data
engine.Renderer.UpdateVertices(new FullVertex[] { /* ... */ });
```

### Physics System

**Common Operations (Presets):**
```csharp
// Most games can use presets
var physics = PhysicsPresets.TopDown2D();     // For roguelikes, shooters
var physics = PhysicsPresets.Platformer2D();  // For side-scrollers
```

**Advanced Operations (Builder):**
```csharp
// Custom physics configuration
var physics = PhysicsBuilder.Create()
    .WithGravity(GravityType.Realistic, strength: 9.81f)
    .WithCollision(CollisionType.Advanced)
    .WithFluid(FluidType.Water)
    .Build();
```

## Configuration Strategies

### Builder Patterns

**Use builders for complex system setup:**
```csharp
// Engine configuration
var config = EngineBuilder
    .Create(EngineProfile.FullGame)
    .WithAudio(true)
    .AddSingleton<IGameService, GameService>()
    .Build();

// Window configuration
var window = WindowBuilder
    .Configure(windowManager)
    .WithTitle("My Game")
    .WithSize(1920, 1080)
    .WithVSync(true)
    .Create();
```

**When builders are overkill:**
```csharp
// Simple cases - direct construction is fine
var world = new World();
var audioService = new OpenALAudioService();
```

## Understanding Fallback Behavior

### Null Object Pattern
When systems aren't available, RACEngine uses safe fallbacks instead of crashes:

```csharp
// Audio not available? No problem - calls become safe no-ops
engine.Audio.PlaySound("explosion.wav");  // Won't crash, just silent

// Graphics not available? NullRenderer handles it gracefully
engine.Renderer.Draw();  // Safe for headless scenarios
```

**In development builds**, you'll see warnings:
```
[NullAudioService] Attempted to play sound: explosion.wav
```

### Testing Benefits
Null objects make testing easier:
```csharp
// Test environments automatically get null implementations
// No need to mock every engine system for unit tests
```

## When to Move Between Levels

### Facade → Service Interface

**Move to service level when you need:**
- Features not available on facade
- More precise control over system behavior
- Multiple configuration options
- Testing with mocks

**Example progression:**
```csharp
// Start: Simple audio
engine.Audio.PlaySound("beep.wav");

// Grow: Need 3D positioning
engine.Audio.PlaySound3D("beep.wav", x, y, z);

// Advanced: Need listener tracking
engine.Audio.SetListener(camera.Position, camera.Forward);
```

### Service Interface → Implementation

**Move to implementation level only when:**
- Profiling shows you need implementation-specific optimizations
- Service interface genuinely lacks required functionality
- Contributing to engine development

**Warning signs you might be going too deep:**
- Code becomes engine-specific (can't easily switch implementations)
- Logic becomes tightly coupled to OpenGL/OpenAL/etc.
- Other developers can't easily understand your code

## Best Practices

### Discovery Pattern
**Explore APIs progressively:**
1. Start typing `engine.` and see what IntelliSense offers
2. If facade doesn't have what you need, try `engine.Audio.` or `engine.World.`
3. Only resort to implementation access if service interface lacks functionality

### Consistency in Projects
**Pick a pattern and stick to it within each project:**
```csharp
// ✅ Good: Consistent facade usage
engine.CreateEntity("Player");
engine.CreateEntity("Enemy");

// ❌ Inconsistent: Mixing patterns without reason
engine.CreateEntity("Player");
engine.World.CreateEntity();  // Why switch?
```

### Documentation Reading
**When stuck, check documentation in this order:**
1. Code completion (IntelliSense) on facade
2. This access patterns guide
3. System-specific documentation
4. Architecture docs (for contributing to engine)

## Common Pitfalls

### Over-Engineering Early
```csharp
// ❌ Don't jump to complex patterns immediately
var advancedPhysics = PhysicsBuilder.Create()...  // For simple movement?

// ✅ Start simple, complexify when needed
var physics = PhysicsPresets.TopDown2D();
```

### Ignoring Facade Convenience
```csharp
// ❌ Verbose when facade method exists
engine.World.CreateEntity();

// ✅ Use provided convenience methods
engine.CreateEntity();
```

### Premature Implementation Access
```csharp
// ❌ Unless you've profiled and confirmed the need
var openGLRenderer = (OpenGLRenderer)engine.Renderer;

// ✅ Service interface covers most needs
engine.Renderer.SetShaderMode(ShaderMode.Bloom);
```

## Getting Help

### When You Can't Find What You Need
1. **Check facade first**: `engine.` + IntelliSense
2. **Check service properties**: `engine.Audio.`, `engine.World.`, etc.
3. **Check builder patterns**: `PhysicsBuilder`, `WindowBuilder`, etc.
4. **Ask**: Is this a missing convenience method? (might be worth a feature request)

### Performance Questions
- **Start with recommended patterns** (facade/service interface)
- **Profile before optimizing** to implementation level
- **Measure twice, optimize once**

### Architecture Questions
- Most game developers won't need implementation-level access
- If you're frequently fighting the engine patterns, consider if you're over-engineering
- The engine is designed to make common things easy and complex things possible

## See Also

- [System Exposure Architecture Patterns](../architecture/system-exposure-patterns.md) - How these patterns are designed
- [Getting Started Tutorial](../educational-material/getting-started-tutorial.md) - Hands-on examples
- [Code Style Guidelines](code-style-guidelines.md) - When contributing to engine
- Individual system documentation for specific APIs