# Copilot Instructions for RACEngine

RACEngine is an educational C# game engine with unique architectural patterns. Follow `docs/code-guides/code-style-guidelines.md` for comprehensive style rules.

## RACEngine-Specific Patterns

### Educational Code Requirements
- Include educational comments explaining graphics algorithms and game engine concepts
- Reference academic papers when implementing algorithms (e.g., "Craig Reynolds Boids Algorithm, 1986")
- Add ASCII art section headers: `// ═══════════════════════════════════════`

### Core Architecture
- **Modules**: Follow `Rac.{ModuleName}` namespace pattern
- **Components**: Must be `readonly record struct` implementing `IComponent`
- **Systems**: Stateless classes operating only on `World` data
- **Services**: Provide both simple and advanced API methods with null object implementations

### 4-Phase Rendering Pipeline
- Configuration → Preprocessing → Processing → Post-processing
- Each phase has distinct responsibilities and cannot be mixed
- Educational comments should explain phase separation benefits

### ECS Patterns
```csharp
// Components are pure data
public readonly record struct PositionComponent(Vector2D<float> Position) : IComponent;

// Systems are stateless logic
public class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime) { /* ... */ }
}
```

### Null Object Pattern
- Required for optional subsystems: `NullRenderer`, `NullAudioService`, `NullPhysicsService`
- Include debug warnings in development builds
- Document when null objects are used vs exceptions

### UV Coordinate Guidelines
- **Critical**: Calculate UV from original local positions before transformations
- Procedural effects: Center at (0,0) for distance calculations
- Traditional texturing: Use [0,1] range for sampling
- Document coordinate system choice in comments

## Key Documentation References

**Always consult these files for relevant context:**
- `docs/code-guides/code-style-guidelines.md` - Comprehensive coding standards and patterns
- `docs/code-guides/csharp_xml_comments_guide.md` - XML documentation requirements and examples
- `docs/architecture/ecs-architecture.md` - ECS implementation patterns and design decisions
- `docs/architecture/rendering-pipeline.md` - 4-phase rendering system details
- `docs/architecture/system-overview.md` - Overall engine architecture and module relationships
- `docs/projects/Rac.{ModuleName}.md` - Specific implementation details for each engine module
- `docs/educational-material/getting-started-tutorial.md` - Practical usage patterns and examples

**When to reference specific docs:**
- Code style questions → `code-style-guidelines.md`
- XML documentation → `csharp_xml_comments_guide.md`
- ECS components/systems → `ecs-architecture.md`
- Rendering code → `rendering-pipeline.md`
- Module-specific work → relevant `docs/projects/` file
- Architecture decisions → `system-overview.md`

## Project Context
- Educational focus over performance where they conflict
- Comprehensive XML documentation required for public APIs
- Each major feature gets its own project (Rac.Audio, Rac.Physics, etc.)
- Check relevant project documentation before making architectural assumptions

## Documentation currency (MANDATORY)

Before a pull request is merged, authors **must**:

1. **Identify impact** – list which files in `docs/` describe the code you touched.
2. **Update or create docs** – ensure those files now match the _new_ behavior, API, or architecture.
3. **Use present tense** – docs should describe the current state, not the change history.
4. **Reference real symbols** – include exact class names, file paths, and code snippets that compile.
5. **Link PR → Docs** – in the PR description, add a checklist of the updated doc files.

The PR reviewer should treat incomplete documentation the same as failing tests.