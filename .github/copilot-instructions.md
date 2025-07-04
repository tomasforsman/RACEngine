# Copilot Instructions for RACEngine

RACEngine is an educational C# game engine where **educational clarity takes precedence over performance**. When generating code, your primary goal is to prioritise immediate understandability for learners while simultaneously maintaining a professional-grade architecture.

## Core Philosophy

*   **Educational First**: Code should teach by example. This means you must:
    *   Include educational comments explaining complex algorithms, patterns, and design decisions within the code.
    *   Explain the "why" behind decisions, not just the "what".
    *   Reference academic papers when implementing algorithms (e.g., "Craig Reynolds Boids Algorithm, 1986").
    *   Use clear patterns that demonstrate game engine concepts.
    *   **Prioritise clarity over cleverness**.
*   **Progressive Complexity**: Offer three access layers to engine systems for developers at different skill levels:
    *   **Facade Layer**: For 80% of use cases, providing simple, discoverable operations for common scenarios.
    *   **Service Interface Layer**: For full feature access with dependency injection support.
    *   **Implementation Layer**: For engine contributors and specialised scenarios, offering direct access to core engine development.
*   **Professional-Grade Architecture**: The engine is designed with professional standards, emphasizing clarity, separation of concerns, and practical elegance. This includes:
    *   Writing **production-ready code** from the start, avoiding simplifications or placeholders like "TODO" in main branches.
    *   Utilising modern C# features and patterns.
    *   Designing for **modularity**, ensuring each component is independently testable and swappable.
    *   **Consistency over micro-optimisation** – prefer a slightly slower, uniform approach to inconsistent "clever" code. Optimise where it counts, but **readability comes first**.

## Architectural Patterns

*   **Module Organisation**:
    *   **Namespace Pattern**: Use `Rac.{ModuleName}` for all projects and ensure namespaces match the folder structure.
    *   Source code resides in `src/Rac.{ModuleName}/`.
    *   Components should be grouped in a `Components/` folder and Systems in a `Systems/` folder within the module.
*   **Components**: Define components as `readonly record struct` types implementing `IComponent`. They should contain **pure data only**.
*   **Systems**: Implement `ISystem` with a full lifecycle (`Initialize`/`Update`/`Shutdown`). Systems should generally be **stateless classes**.
*   **Services**: Are **interface-based with null object implementations** for optional subsystems (e.g., `NullRenderer`, `NullAudioService`). Debug warnings should be included in development builds when null objects are used.
*   **Builder Pattern**: Use the Builder pattern for complex configurations (e.g., `PhysicsBuilder`, `WindowBuilder`).
*   **Dependencies**: Avoid cyclic dependencies; dependencies must flow `Core → ECS → Engine → GameEngine`. Never reference up the dependency chain (e.g., `Rac.Engine` cannot depend on `Rac.GameEngine`).
*   **Naming Conventions**: Use **full, descriptive names** for all code elements, avoiding abbreviations (e.g., `Configuration` not `Config`, `Initialize` not `Init`). Names should describe intent and purpose.

## Documentation Standards (MANDATORY)

**Treat incomplete or outdated documentation as failing tests**.

*   **XML Documentation**: **All public APIs** (classes, methods, properties, interfaces) **MUST have comprehensive XML documentation**.
    *   Documentation includes `<summary>`, `<param>`, `<returns>`, and `<example>` tags where applicable.
    *   For exceptions, use `<exception>` tags for those that are part of the method's contract, callers should handle, or indicate misuse of the API.
    *   Follow `csharp_xml_comments_guide.md` strictly for specific rules.
*   **Documentation Currency**: Before any code changes or a pull request is merged, ensure documentation is current.
    *   **Identify impact**: List affected files in `docs/`.
    *   **Update documentation**: Ensure docs match new behavior/API.
    *   **Use present tense** when describing the current state, not change history.
    *   **Include real examples**: Use exact class names, file paths, and working code snippets that compile.
    *   **Cross-referencing**: Link to related sections (e.g., architecture, user guides, other modules).
*   **Changelogs**: Add an entry to `changelog.md` for new features, breaking changes, bug fixes affecting users, or performance improvements, including an impact description and educational value. Skip for internal refactoring, test-only changes, or documentation fixes.
*   **File Naming**: The primary public type should match the file name (e.g., `WindowManager.cs`). Interfaces start with `I` (e.g., `IRenderer.cs`).
*   **AI-Specific Documentation**: `docs/copilot` contains compact documentation specifically suitable for AI agents, focusing on code snippets and usage examples to provide context, guide location of relevant code, speed up discovery, and avoid reinvention of existing solutions.

## Contribution & Review Readiness

*   **Pull Request Adherence**:
    *   Include **unit, integration, and manual testing** details.
    *   Clearly describe the **educational impact** of the change.
    *   Clearly list any **breaking changes** with migration steps.
    *   A **self-review** must be completed.
    *   PRs should be **small, focused, and well-tested**.
    *   Link the PR to updated documentation files in the description.
*   **Branch Naming**: Use `feature/...`, `fix/...`, or `docs/...` for branch names.
*   **Project Alignment**: Ensure code aligns with the long-term product roadmap and vision, supports anticipated future requirements, and fosters extensibility to accelerate future development. Architectural decisions should not "paint the product into a corner".
*   **Legal Compliance**: Only original work or properly licensed code should be contributed. Attribution is required for academic references.

## Key Documentation References

Always consult these files for context, detailed patterns, and examples:

*   `docs/code-guides/code-style-guidelines.md` - Comprehensive coding standards.
*   `docs/code-guides/csharp_xml_comments_guide.md` - XML documentation requirements and examples.
*   `docs/architecture/ecs-architecture.md` - ECS patterns and design decisions.
*   `docs/architecture/rendering-pipeline.md` - 4-phase rendering details.
*   `docs/architecture/system-overview.md` - Overall architecture and module relationships.
*   `docs/projects/Rac.{ModuleName}.md` - Module-specific details.
*   `docs/code-guides/engine-access-patterns.md` - Engine API design and access patterns.
*   `docs/educational-material/getting-started-tutorial.md` - Practical usage patterns and examples for new users.

## General AI Agent Guidance

*   **Prioritise Documentation**: Always consult and adhere to the provided documentation before generating code or suggesting changes.
*   **Modular Design**: Always propose solutions that maintain or enhance the existing modular design and separation of concerns.
*   **Descriptive Naming**: Use full, descriptive names for all code elements. Avoid abbreviations.
*   **Deep Discussions**: Be prepared to engage in deep discussions about architectural choices, design patterns, and performance trade-offs, always prioritizing the educational goals of the project.
*   **Structured but Flexible**: Follow established patterns but be ready to adapt when justified by performance, clarity, or new requirements, documenting any deviations.
*   **Focus on Learning and Discovery**: Frame explanations and suggestions with an educational perspective.
*   **Small Wins**: When refactoring or adding features, aim for small, incremental changes that contribute to overall project goals.
*   **LLM-Friendly**: Strive to produce modular, educational, testable, and LLM-friendly code that respects the engine's architecture, style, and long-term maintainability.
*   **Predictable, Safe, and Helpful**: Ensure AI-generated code is predictable, safe, and helpful, aiming to teach, scale, and compose.

## File Organization Template

```
src/Rac.{ModuleName}/
├── {FeatureArea}/
│ ├── I{Interface}.cs // Interface first
│ ├── {Implementation}.cs // Concrete implementation
│ ├── Null{Service}.cs // Null object pattern
│ └── {Feature}Builder.cs // Builder if complex
├── Components/ // All components grouped
│ └── {Feature}Component.cs
└── Systems/ // All systems grouped
    └── {Feature}System.cs
```

## Common Pitfalls to Avoid

*   **Magic Numbers**: Avoid magic numbers; define all constants with descriptive names.
*   **Abbreviations**: Do not use abbreviations in public APIs or descriptive naming.
*   **Mutable Components**: ECS components should be `readonly record struct` (pure data only) to prevent mutable components.
*   **Breaking Encapsulation**: Never break encapsulation for a shortcut; use and extend patterns instead.
*   **Bypassing Public API**: Do not write new "manual" APIs when builder or façade patterns exist, or bypass the public API unless extending the core engine.
