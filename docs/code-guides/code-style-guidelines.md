# RACEngine Code Style Guide

This document establishes the coding standards and conventions for the RACEngine project. These guidelines ensure consistency, readability, and maintainability across the codebase while supporting the project's educational and professional goals.

## Table of Contents

- [General Principles](#general-principles)
- [File Organization](#file-organization)
- [Naming Conventions](#naming-conventions)
- [Code Structure](#code-structure)
- [Documentation Standards](#documentation-standards)
- [Language Features](#language-features)
- [Formatting Rules](#formatting-rules)
- [Error Handling](#error-handling)
- [Performance Considerations](#performance-considerations)

## General Principles

### Educational Excellence
- **Clarity over cleverness**: Code should be immediately understandable to learners and contributors
- **Self-documenting**: Code structure and naming should tell a story
- **Educational comments**: Explain algorithms, patterns, and design decisions
- **No magic numbers**: Use named constants with descriptive names

### Professional Standards
- **No simplifications**: Write production-ready code from the start
- **No placeholders**: Avoid "TODO" implementations in main branches
- **Modern practices**: Utilize current C# features and patterns
- **Modular design**: Each component should be independently testable and swappable

### Descriptive Naming
- **No abbreviations**: Use full words (`Configuration` not `Config`, `Initialize` not `Init`)
- **Describe intent**: Names should explain purpose and behavior
- **Avoid technical jargon**: Prefer domain language over implementation details

## File Organization

### Project Structure
```
src/
├── Rac.{ModuleName}/
│   ├── {FeatureArea}/
│   │   ├── {ConcreteClass}.cs
│   │   └── I{Interface}.cs
│   └── Rac.{ModuleName}.csproj
tests/
├── Rac.{ModuleName}.Tests/
samples/
└── {SampleName}/
```

### File Naming
- **One primary type per file**: File name must match the primary public type
- **PascalCase**: All file names use PascalCase (`WindowManager.cs`)
- **Interface prefix**: Interfaces start with `I` (`IRenderer.cs`)
- **Descriptive names**: Avoid abbreviations (`ConfigurationManager.cs` not `ConfigMgr.cs`)

### Namespace Organization
```csharp
// ✅ Good: File-scoped namespace
namespace Rac.Core.Manager;

public class WindowManager
{
    // Implementation
}
```

```csharp
// ❌ Avoid: Block-scoped namespace (legacy style)
namespace Rac.Core.Manager
{
    public class WindowManager
    {
        // Implementation
    }
}
```

## Naming Conventions

### Classes and Interfaces
```csharp
// ✅ Good: Descriptive, unabbreviated names
public class ConfigurationManager { }
public interface IInputService { }
public class OpenGLRenderer { }

// ❌ Avoid: Abbreviations and unclear names
public class ConfigMgr { }
public interface IInpSvc { }
public class GLRenderer { }
```

### Methods and Properties
```csharp
// ✅ Good: Verb-noun pattern for methods
public Entity CreateEntity() { }
public void SetComponent<T>(Entity entity, T component) { }
public void UpdateVertices(float[] vertices) { }

// ✅ Good: Noun pattern for properties
public Vector2D<int> Size { get; }
public float AspectRatio { get; }
public World World { get; }
```

### Fields and Variables
```csharp
public class ExampleClass
{
    // ✅ Private fields: underscore prefix + camelCase
    private readonly GL _gl;
    private float _aspectRatio;
    private bool _disposed;
    
    // ✅ Constants: PascalCase
    private const float BulletSpeed = 0.75f;
    private const float FireInterval = 0.2f;
    
    public void ExampleMethod(WindowManager windowManager, IInputService inputService)
    {
        // ✅ Parameters and locals: camelCase
        var currentShaderMode = ShaderMode.Normal;
        int vertexCount = vertices.Length;
    }
}
```

### Enumerations
```csharp
// ✅ Good: Descriptive enum and values
public enum ShaderMode
{
    Normal,
    SoftGlow,
    Bloom
}

public enum EngineProfile
{
    FullGame,
    Headless,
    Custom
}
```

## Code Structure

### Class Organization
Classes should be organized in the following order:

```csharp
namespace Rac.Example;

/// <summary>
/// Example class demonstrating proper organization.
/// </summary>
public class ExampleClass : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS AND STATIC READONLY FIELDS
    // ═══════════════════════════════════════════════════════════════════════════
    
    private const float DefaultValue = 1.0f;
    private static readonly Vector4D<float> DefaultColor = new(1f, 1f, 1f, 1f);
    
    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS AND PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly GL _gl;
    private bool _disposed;
    
    public Vector2D<int> Size { get; private set; }
    public bool IsInitialized { get; private set; }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════════════════════════════════════
    
    public event Action<Vector2D<int>>? OnResize;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════
    
    public ExampleClass(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC INTERFACE
    // ═══════════════════════════════════════════════════════════════════════════
    
    public void Initialize() { }
    public void Update(float deltaTime) { }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    private void InternalMethod() { }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // IDISPOSABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            // Dispose managed resources
        }
        
        // Free unmanaged resources
        _disposed = true;
    }
}
```

### Region Usage
Use regions with descriptive ASCII art headers for major sections:

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// SECTION NAME AND PURPOSE
// ═══════════════════════════════════════════════════════════════════════════
//
// Brief description of what this section contains and why it exists.
// Educational notes about patterns, algorithms, or design decisions.
//
// SUBSECTION DETAILS:
// - Specific responsibilities
// - Key design considerations
// - Performance implications
```

## Documentation Standards

### XML Documentation
All public APIs must have comprehensive XML documentation:

```csharp
/// <summary>
/// Manages the lifecycle and configuration of the application window.
/// Provides abstraction over platform-specific windowing systems.
/// </summary>
/// <remarks>
/// This class implements the facade pattern to simplify window management
/// and provides event-driven resize handling for responsive layouts.
/// </remarks>
public class WindowManager : IWindowManager
{
    /// <summary>
    /// Creates a new window with the specified configuration options.
    /// </summary>
    /// <param name="options">Window configuration including size, title, and display settings.</param>
    /// <returns>A configured window instance ready for rendering operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when window creation fails.</exception>
    public IWindow CreateWindow(WindowOptions options)
    {
        // Implementation
    }
}
```

### Educational Comments
Include educational comments for complex algorithms and design patterns:

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// BOIDS ALGORITHM IMPLEMENTATION (Craig Reynolds, 1986)
// ═══════════════════════════════════════════════════════════════════════════
//
// The boids algorithm creates emergent flocking behavior through three simple rules:
// 1. Separation: Avoid crowding neighbors
// 2. Alignment: Steer towards average heading of neighbors  
// 3. Cohesion: Steer towards average position of neighbors
//
// This demonstrates how complex group behaviors emerge from simple individual rules,
// a fundamental concept in artificial life and swarm intelligence.

public void UpdateBoidBehavior(float deltaTime)
{
    // Calculate separation force to avoid collisions
    var separationForce = CalculateSeparation();
    
    // Calculate alignment force to match neighbor velocities
    var alignmentForce = CalculateAlignment();
    
    // Calculate cohesion force to move toward group center
    var cohesionForce = CalculateCohesion();
}
```

### Code Examples in Comments
Provide usage examples for complex APIs:

```csharp
/// <summary>
/// Updates vertex data with automatic layout detection and type safety.
/// </summary>
/// <typeparam name="T">Vertex structure type (BasicVertex, TexturedVertex, FullVertex)</typeparam>
/// <param name="vertices">Array of structured vertex data</param>
/// <example>
/// <code>
/// // Type-safe basic vertices
/// var vertices = new BasicVertex[]
/// {
///     new(new Vector2D&lt;float&gt;(-0.5f, -0.5f)),
///     new(new Vector2D&lt;float&gt;( 0.5f, -0.5f)),
///     new(new Vector2D&lt;float&gt;( 0.0f,  0.5f))
/// };
/// renderer.UpdateVertices(vertices);
/// </code>
/// </example>
public void UpdateVertices<T>(T[] vertices) where T : unmanaged
```

## Language Features

### Modern C# Usage
Embrace modern C# features while maintaining readability:

```csharp
// ✅ Good: File-scoped namespaces
namespace Rac.Core.Manager;

// ✅ Good: Record types for data structures
public readonly record struct Entity(int Id, bool IsAlive = true);

// ✅ Good: Pattern matching
public string GetShaderFilename(ShaderMode mode) => mode switch
{
    ShaderMode.Normal => "normal.frag",
    ShaderMode.SoftGlow => "softglow.frag", 
    ShaderMode.Bloom => "bloom.frag",
    _ => "normal.frag"
};

// ✅ Good: Expression-bodied members for simple properties
public float AspectRatio => Size.Y / (float)Size.X;

// ✅ Good: Null-conditional operators
public void InvokeEvent() => OnResize?.Invoke(newSize);
```

### Type Usage
```csharp
// ✅ Good: Explicit types for clarity
float deltaTime = 0.016f;
ShaderMode currentMode = ShaderMode.Normal;

// ✅ Good: var when type is obvious
var windowManager = new WindowManager();
var vertices = new BasicVertex[3];

// ❌ Avoid: var for primitive types
var x = 5; // Use: int x = 5;
var name = "test"; // Use: string name = "test";
```

### Nullable Reference Types
Enable and properly use nullable reference types:

```csharp
// ✅ Good: Proper nullable annotations
public string? Title { get; set; }
public IWindow Window { get; private set; } = null!; // Initialized in method

public void Initialize(IWindow? window)
{
    Window = window ?? throw new ArgumentNullException(nameof(window));
}
```

## Formatting Rules

### Indentation and Spacing
- Use 4 spaces for indentation (no tabs)
- One statement per line
- One declaration per line
- Add blank lines between logical sections

```csharp
// ✅ Good: Proper spacing and indentation
public void ExampleMethod(string parameter)
{
    if (parameter == null)
        throw new ArgumentNullException(nameof(parameter));
    
    var result = ProcessParameter(parameter);
    
    if (result.IsSuccess)
    {
        OnSuccess?.Invoke(result.Value);
    }
    else
    {
        LogError(result.Error);
    }
}
```

### Braces and Control Flow
```csharp
// ✅ Good: Allman brace style
public void ExampleMethod()
{
    if (condition)
    {
        DoSomething();
    }
    else
    {
        DoSomethingElse();
    }
}

// ✅ Good: Single-line statements (optional braces)
if (condition)
    DoSomething();

// ✅ Good: Switch expressions for simple cases
var result = input switch
{
    1 => "One",
    2 => "Two", 
    _ => "Other"
};
```

## Error Handling

### Parameter Validation
```csharp
public void ProcessEntity(Entity entity, IComponent component)
{
    // ✅ Good: Guard clauses with descriptive messages
    if (entity.Id <= 0)
        throw new ArgumentException("Entity must have a valid ID", nameof(entity));
        
    if (component == null)
        throw new ArgumentNullException(nameof(component));
        
    // Implementation continues...
}
```

### Exception Handling
```csharp
// ✅ Good: Specific exception handling with context
try
{
    var shaderProgram = new ShaderProgram(_gl, vertexSource, fragmentSource);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("compilation failed"))
{
    Console.WriteLine($"⚠️ Shader compilation failed: {ex.Message}");
    // Fallback to default shader
}
catch (Exception ex)
{
    throw new InvalidOperationException($"Failed to initialize shader system: {ex.Message}", ex);
}
```

### Resource Management
```csharp
// ✅ Good: Proper disposal pattern implementation
public class ResourceManager : IDisposable
{
    private bool _disposed;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
            
        if (disposing)
        {
            // Dispose managed resources
            _managedResource?.Dispose();
        }
        
        // Free unmanaged resources
        DeleteNativeHandle();
        _disposed = true;
    }
}
```

## Performance Considerations

### Memory Allocation
```csharp
// ✅ Good: Minimize allocations in hot paths
private readonly List<Entity> _reusableEntityList = new();

public void ProcessEntities()
{
    _reusableEntityList.Clear(); // Reuse existing list
    
    foreach (var entity in GetActiveEntities())
    {
        _reusableEntityList.Add(entity);
    }
}

// ✅ Good: Use spans for performance-critical operations
public void ProcessVertices(ReadOnlySpan<float> vertices)
{
    // Process without allocating new arrays
}
```

### Caching and Optimization
```csharp
// ✅ Good: Cache expensive operations
private readonly ConcurrentDictionary<string, ShaderProgram> _shaderCache = new();

public ShaderProgram GetShader(string shaderName)
{
    return _shaderCache.GetOrAdd(shaderName, LoadShaderFromFile);
}
```

## Code Review Checklist

### Before Submitting
- [ ] All public APIs have XML documentation
- [ ] Educational comments explain complex algorithms
- [ ] No abbreviations in public APIs
- [ ] Proper error handling with meaningful messages
- [ ] Resource disposal follows standard patterns
- [ ] Unit tests cover critical paths
- [ ] Performance implications considered
- [ ] Follows established naming conventions

### Architecture Review
- [ ] Component is modular and testable
- [ ] Dependencies are properly abstracted
- [ ] Interface segregation is maintained
- [ ] Single responsibility principle followed
- [ ] Open/closed principle considered for extensions

## Tools and Automation

### EditorConfig
The project includes a comprehensive `.editorconfig` file that enforces:
- 4-space indentation
- CRLF line endings
- File-scoped namespaces
- Consistent code formatting

### Static Analysis
Consider integrating:
- **StyleCop.Analyzers**: Enforce coding style rules
- **Microsoft.CodeAnalysis.NetAnalyzers**: Detect common issues
- **SonarAnalyzer.CSharp**: Advanced code quality analysis

### IDE Configuration
Recommended IDE settings:
- Enable nullable reference types warnings
- Show all compiler warnings
- Enable code cleanup on save
- Configure automatic formatting rules

---

## Conclusion

This style guide prioritizes **clarity**, **education**, and **professional quality**. When in doubt, choose the approach that makes the code more understandable to future contributors and learners. Remember that code is read far more often than it is written, so optimize for readability and maintainability.

For questions about specific scenarios not covered in this guide, refer to the [.NET Runtime Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) and [Roslyn Contributing Guidelines](https://github.com/dotnet/roslyn/blob/main/CONTRIBUTING.md#coding-style) as supplementary resources.