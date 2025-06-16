# Copilot Instructions for RACEngine

These instructions guide GitHub Copilot to generate code following RACEngine-specific patterns. See `code-style-guide.md` for comprehensive guidelines.

## RACEngine-Specific Patterns (HIGH PRIORITY)

### Educational Code Requirements
- Include educational comments explaining graphics algorithms and game engine concepts
- Add ASCII art section headers: `// ═══════════════════════════════════════`
- Explain coordinate systems, mathematical concepts, and design patterns in comments
- Reference academic papers or standards when implementing algorithms (e.g., "Craig Reynolds Boids Algorithm, 1986")

### Engine Architecture Patterns
- All engine modules follow `Rac.{ModuleName}` namespace pattern
- Components are readonly record structs implementing IComponent
- Systems are stateless classes operating only on World data
- Services provide both simple and advanced API methods
- Include null object implementations (NullRenderer, NullAudioService) for optional subsystems

## Naming Conventions

### Apply These Naming Rules
- Private fields: underscore prefix + camelCase (`_gl`, `_aspectRatio`, `_disposed`)
- Public properties: PascalCase (`Size`, `AspectRatio`, `IsInitialized`)
- Methods: Verb-noun pattern (`CreateEntity`, `UpdateVertices`, `SetComponent`)
- Constants: PascalCase (`BulletSpeed`, `FireInterval`, `DefaultColor`)
- Interfaces: I prefix + PascalCase (`IRenderer`, `IInputService`, `IWindowManager`)
- Enums: PascalCase for type and values (`ShaderMode.SoftGlow`, `EngineProfile.FullGame`)

### Avoid These Naming Patterns
- Never use abbreviations in public APIs (`Mgr`, `Svc`, `Cfg`, `Init`, `Calc`)
- No single-letter variables except for common loop counters (`i`, `j`)
- No Hungarian notation or type prefixes
- No generic names like `data`, `info`, `item`, `obj`

## Code Structure Patterns

### Class Organization Template
```csharp
// Constants and static fields first
// Instance fields and properties 
// Events
// Constructors
// Public interface methods
// Private implementation methods
// IDisposable implementation last
```

### Use ASCII Art Section Headers
- Start major sections with `// ═══════════════════════════════════════`
- Include educational comments explaining the purpose and design
- Add subsection details with `// ───────────────────────────────────────`

### File Organization Rules
- One primary public type per file
- File name must match the primary type exactly
- Use file-scoped namespaces (`namespace Rac.Core.Manager;`)
- Place interfaces in separate files with `I` prefix

### Code Style Consistency
- Always comply with the `code-style-guide.md` in the repository root
- At the end of an issue, review the code style guide to ensure compliance


## Modern C# Feature Usage

### Required Language Features
- Use file-scoped namespaces for all new files
- Use record types for data structures (`public readonly record struct Entity(int Id, bool IsAlive = true);`)
- Use pattern matching in switch expressions for simple mappings
- Use expression-bodied members for simple properties (`public float AspectRatio => Size.Y / (float)Size.X;`)
- Enable and properly handle nullable reference types

### Type Declaration Guidelines
- Use explicit types for primitives and unclear contexts (`float deltaTime = 0.016f;`)
- Use `var` only when the type is obvious from the right side (`var manager = new WindowManager();`)
- Always specify generic type constraints explicitly
- Use `readonly` for fields that don't change after construction

## Documentation Requirements

### XML Documentation Template
```csharp
/// <summary>
/// Brief description of what this does and why it exists.
/// </summary>
/// <param name="paramName">Clear description of parameter purpose and constraints.</param>
/// <returns>Description of return value and its meaning.</returns>
/// <exception cref="SpecificException">When and why this exception is thrown.</exception>
/// <example>
/// <code>
/// // Practical usage example
/// var result = method.Call(parameter);
/// </code>
/// </example>
```

### Educational Comment Standards
- Explain complex algorithms with background theory and references
- Document design pattern usage and architectural decisions
- Include performance implications and optimization notes
- Reference academic papers or standards when applicable
- Explain coordinate systems, mathematical concepts, and graphics theory

## Error Handling Patterns

### Parameter Validation Rules
- Validate all public method parameters with guard clauses
- Use specific exception types with descriptive messages
- Include parameter name in ArgumentException and ArgumentNullException
- Place validation at the start of methods before any logic

### Exception Handling Standards
- Catch specific exceptions rather than generic Exception
- Provide context in exception messages including attempted operation
- Use exception filters (`when`) for conditional handling
- Wrap and rethrow with additional context when appropriate
- Never swallow exceptions silently

### Resource Management Requirements
- Implement IDisposable for all classes managing unmanaged resources
- Use the standard disposal pattern with protected virtual Dispose(bool disposing)
- Include finalizer only when managing unmanaged resources directly
- Set disposed flag and check it in public methods
- Call GC.SuppressFinalize(this) in Dispose()

## Performance and Memory Considerations

### Allocation Optimization
- Reuse collections and arrays in hot paths instead of creating new ones
- Use object pooling for frequently created/destroyed objects
- Prefer ReadOnlySpan<T> and Span<T> for performance-critical operations
- Cache expensive calculations and lookups using ConcurrentDictionary
- Minimize boxing of value types

### Threading and Concurrency
- Use thread-safe collections for shared data (ConcurrentDictionary, etc.)
- Implement proper locking for complex shared state modifications
- Prefer immutable data structures for thread-safe sharing
- Use cancellation tokens for long-running operations
- Document thread-safety guarantees in XML comments

## Testing Integration

### Test-Friendly Design
- Design all classes to accept dependencies through constructor injection
- Provide interface abstractions for all external dependencies
- Use factory patterns for complex object creation
- Avoid static dependencies and global state
- Make internal state observable through properties when needed for testing

### Null Object Pattern Implementation
- Provide null object implementations for optional services (NullRenderer, NullAudioService)
- Include debug warnings in null implementations when appropriate
- Document when null objects are used vs exceptions for missing dependencies

## Graphics and Game Engine Specific

### Rendering Pipeline Patterns
- Always implement IDisposable for OpenGL resources (shaders, buffers, textures)
- Use strongly-typed vertex structures with layout methods
- Provide multiple overloads for vertex data upload (float[], typed arrays, explicit layout)
- Include educational comments explaining graphics algorithms and coordinate systems
- Cache shader programs and uniform locations for performance

### ECS Architecture Requirements
- Components must be readonly record structs implementing IComponent
- Systems must be stateless and operate only on World data
- Entities are immutable value types with only Id and IsAlive
- World manages all component storage and querying
- Use generic query methods with tuple returns for multiple components

### Engine Module Organization
- Each major feature area gets its own project (Rac.Audio, Rac.Physics, etc.)
- Provide both simple and advanced API methods in service interfaces
- Include null object implementations for optional subsystems
- Use builder patterns for complex configuration objects
- Support both immediate mode and deferred execution patterns

## Code Generation Guidelines

### When Generating New Files
- Include appropriate file header with namespace and imports
- Add comprehensive XML documentation for all public members
- Implement proper error handling and parameter validation
- Include relevant educational comments for complex logic
- Follow the established class organization template

### When Extending Existing Code
- Match the existing code style and organization patterns
- Maintain consistency with existing naming conventions
- Add appropriate documentation following established patterns
- Ensure thread-safety guarantees match existing code
- Preserve educational value and clarity of existing comments

### Integration Points
- Always use dependency injection for external dependencies
- Provide interface abstractions for new services
- Follow established event patterns for notifications
- Use consistent error handling strategies with existing code
- Maintain compatibility with existing builder and factory patterns

## Further instructions

### Files to comply with in root of repo
- For evaluating code: CopilotAgent_Code_Evaluation_Process.md
- For code style guidance: code-style-guidelines.md
