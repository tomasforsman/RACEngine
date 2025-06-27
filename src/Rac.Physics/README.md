# Rac.Physics Usage Examples

This document demonstrates how to use the modular physics system implemented in Week 9-10 foundation.

## Quick Start - TopDown2D Physics (Roguelike Shooter)

```csharp
using Rac.Physics.Builder;
using Rac.Physics.Components;
using Rac.ECS.Core;
using Silk.NET.Maths;

// Create physics service for top-down roguelike shooter
var physics = PhysicsPresets.TopDown2D();

// Simple API usage
var playerId = physics.AddDynamicSphere(0, 0, 0, 0.5f, 1.0f);  // Player
var wallId = physics.AddStaticBox(-5, 0, 0, 1, 3, 1);          // Wall
var enemyId = physics.AddDynamicSphere(3, 0, 0, 0.5f, 1.0f);   // Enemy

// Game loop
float deltaTime = 1.0f / 60.0f; // 60 FPS
physics.Update(deltaTime);

// Apply forces (player movement)
physics.ApplyForce(playerId, 10.0f, 0, 0); // Move right

// Raycast for bullet collision
bool hit = physics.Raycast(0, 0, 0, 5, 0, 0, out int hitId);
if (hit)
{
    Console.WriteLine($"Bullet hit object {hitId}");
}
```

## Custom Physics Configuration

```csharp
using Rac.Physics.Builder;
using Rac.Physics.Core;

// Build custom physics with specific modules
var customPhysics = PhysicsBuilder.Create()
    .WithGravity(GravityType.Constant, new Vector3D<float>(0, -9.81f, 0))  // Earth gravity
    .WithCollision(CollisionType.AABB)                                     // AABB collision
    .WithFluid(FluidType.None)                                             // No fluid effects
    .Build();
```

## ECS Integration (Future - Week 11-12)

```csharp
// This will be available in future weeks
using Rac.Physics.Components;
using Rac.ECS.Core;

var world = new World();
var entity = world.CreateEntity();

// Add physics components
world.SetComponent(entity, RigidBodyComponent.Dynamic(mass: 2.0f));
world.SetComponent(entity, ColliderComponent.Box2D(1.0f, 1.0f));
world.SetComponent(entity, PhysicsMaterialComponent.Metal);
```

## Available Presets

- `PhysicsPresets.TopDown2D()` - Perfect for roguelike shooters, no gravity
- `PhysicsPresets.Platformer2D()` - 2D platformers with gravity
- `PhysicsPresets.Headless()` - Testing and server scenarios
- `PhysicsPresets.Debug()` - Development with reduced gravity

## Performance Characteristics

- **TopDown2D**: O(n²) collision detection, suitable for 100-200 objects
- **Platformer2D**: Same performance as TopDown2D with gravity overhead
- **Simple AABB**: Basic but educational implementation
- **Future optimizations**: Spatial hash grids, Bepu integration (Week 11-12+)

## Educational Notes

The Week 9-10 foundation implements:
- ✅ Modular architecture with composable physics modules
- ✅ No gravity, constant gravity modules
- ✅ Simple AABB collision detection with impulse response
- ✅ Euler integration for motion
- ✅ Builder pattern for physics service configuration
- ✅ ECS components for future integration
- ✅ Educational documentation throughout

Future weeks will add:
- 🔄 Realistic gravity (N-body)
- 🔄 Spatial optimization (hash grids, octrees)
- 🔄 Advanced fluid dynamics
- 🔄 External physics engine integration (Bepu)
- 🔄 Complete ECS physics system