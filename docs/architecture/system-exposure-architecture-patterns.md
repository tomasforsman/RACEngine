# System Exposure Architecture Patterns

## Overview

This document establishes the architectural thinking and patterns for exposing engine systems to game developers. These patterns ensure consistency, discoverability, and appropriate abstraction levels across all RACEngine systems.

## Core Exposure Philosophy

### Progressive Complexity Principle
Engine systems should support developers at different skill levels through layered access:
- **Facade Layer**: Simple, discoverable operations for common use cases
- **Service Interface Layer**: Full feature access with dependency injection support
- **Implementation Layer**: Direct access for engine contributors and specialized scenarios

### Consistency Over Optimization
When choosing between multiple valid approaches, prioritize consistency with existing patterns. A slightly less optimal but consistent API is preferable to fragmented exposure patterns.

## Service Interface Pattern

### When to Create Service Interfaces
Create `IServiceName` interfaces when a system:
- Provides functionality that may need mocking in tests
- Could benefit from multiple implementations (production vs debug vs null)
- Needs dependency injection support
- Contains complex state that should be abstracted

### Interface Design Principles
```csharp
// ✅ Good: Progressive complexity in single interface
public interface IAudioService
{
    // Simple methods for common operations
    void PlaySound(string soundId);
    void SetMasterVolume(float volume);
    
    // Advanced methods for complex scenarios
    void PlaySound3D(string soundId, float x, float y, float z, float volume = 1.0f);
    void SetListener(float x, float y, float z, float forwardX, float forwardY, float forwardZ);
}

// ❌ Avoid: Splitting simple/advanced into separate interfaces unless truly necessary
public interface ISimpleAudioService { }
public interface IAdvancedAudioService { }
```

### Exception Cases
Not every system needs a service interface:
- **Pure data containers** (World, Entity) may remain concrete
- **Utility classes** with no state may not need abstraction
- **Internal systems** not exposed to game developers

## Null Object Pattern

### Mandatory Implementation
Every service interface **must** have a corresponding null object implementation:

```csharp
public class NullAudioService : IAudioService
{
    public void PlaySound(string soundId) { }
    public void SetMasterVolume(float volume) { }
    
    // Include debug warnings in development builds
    public void PlaySound3D(string soundId, float x, float y, float z, float volume = 1.0f)
    {
#if DEBUG
        Console.WriteLine($"[NullAudioService] Attempted to play 3D sound: {soundId}");
#endif
    }
}
```

### Null Object Guidelines
- **Safe no-ops**: Never throw exceptions, always provide safe behavior
- **Development feedback**: Include debug warnings to help developers understand when null objects are active
- **Consistent naming**: Always prefix with `Null` (NullAudioService, NullRenderer, NullPhysicsService)
- **Complete implementation**: Implement all interface methods, not just common ones

## Facade Integration Strategy

### What Gets Direct Facade Exposure
Expose operations directly on the engine facade when they are:
- **High frequency**: Used in most games (entity creation, basic rendering, common audio)
- **Entry-level friendly**: Simple operations new developers need quickly
- **Single concept**: Operations that don't require understanding multiple systems

```csharp
// ✅ Good: Common operations directly accessible
engine.CreateEntity("Player");
engine.PlaySound("explosion.wav");
engine.SetColor(1, 0, 0);

// ✅ Good: Full service access for advanced scenarios  
engine.Audio.SetListener(x, y, z, fx, fy, fz);
engine.Renderer.SetShaderMode(ShaderMode.Bloom);
```

### Property vs Method Decision
- **Properties**: For accessing service interfaces (`engine.Audio`, `engine.World`)
- **Methods**: For common operations that delegate to services (`engine.CreateEntity()`, `engine.PlaySound()`)

### Avoiding Duplication
When a facade method exists, prefer it over service access in documentation and examples:
```csharp
// ✅ Prefer: Facade method
engine.AddSystem(new MovementSystem());

// ⚠️ Avoid in examples: Direct service access (though still valid)
engine.Systems.Add(new MovementSystem());
```

## Builder Pattern Integration

### When to Provide Builders
Create fluent builders for systems with:
- **Complex configuration**: Many optional parameters or settings
- **Validation requirements**: Configuration that needs validation before use
- **Multiple creation patterns**: Different ways to set up the same system

### Builder Design Principles
```csharp
// ✅ Good: Validation and immutability
public class PhysicsBuilder
{
    public PhysicsBuilder WithGravity(GravityType type) { /* validation */ }
    public PhysicsBuilder WithCollision(CollisionType type) { /* validation */ }
    public IPhysicsService Build() { /* create and validate final config */ }
}

// ✅ Good: Preset factory methods
public static class PhysicsPresets
{
    public static IPhysicsService TopDown2D() => PhysicsBuilder.Create()...;
    public static IPhysicsService Platformer2D() => PhysicsBuilder.Create()...;
}
```

## Access Level Hierarchy

### Clear Boundaries
Each access level should have distinct purposes:

**Facade Level** (`engine.CreateEntity()`):
- Common operations used in most games
- Single-concept operations
- Beginner-friendly with sensible defaults

**Service Level** (`engine.Audio.SetListener()`):
- Complete feature set of each system
- Advanced configuration and control
- Professional development scenarios

**Implementation Level** (`engine.AdvancedRenderer`):
- Direct access to implementation-specific features
- Engine contributors and specialized use cases
- Breaking changes more acceptable

### Transition Guidance
Provide clear guidance when developers need to move between levels:
```csharp
/// <summary>
/// Sets basic entity position. For advanced transform operations including 
/// rotation and scale, use engine.World.SetComponent() with TransformComponent.
/// </summary>
public void SetEntityPosition(Entity entity, float x, float y) { }
```

## Consistency Validation

### New System Checklist
When adding a new engine system, ensure:
- [ ] Service interface follows `ISystemName` naming
- [ ] Null object implementation provided
- [ ] Common operations exposed on facade
- [ ] Builder pattern for complex configuration (if applicable)
- [ ] Documentation explains access level progression
- [ ] Examples use recommended patterns consistently

### Architectural Review Questions
- Does this fit the progressive complexity model?
- Are we creating the minimum necessary interfaces?
- Do the patterns match similar existing systems?
- Will developers naturally discover the functionality they need?

## Flexibility and Evolution

### When to Deviate
These patterns are guidelines, not rigid rules. Consider deviation when:
- **Domain requirements**: Some systems have unique needs (physics modularity)
- **Performance constraints**: Direct access may be necessary for critical paths
- **Third-party integration**: External libraries may impose different patterns

### Documenting Deviations
When deviating from standard patterns, document:
- **Why the standard pattern doesn't fit**
- **What alternative pattern is used**
- **How developers should think about accessing this system**

### Pattern Evolution
These patterns should evolve based on:
- Developer feedback and usage patterns
- Performance discoveries
- New engine capabilities
- Community best practices

## See Also

- [Engine Access Patterns Guide](../code-guides/engine-access-patterns.md) - For game developers using these patterns
- [Code Style Guidelines](../code-guides/code-style-guidelines.md) - Implementation standards
- [System Overview](system-overview.md) - Overall architecture context