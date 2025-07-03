using Rac.ECS.Components;

namespace Rac.ECS.Components;

/// <summary>
/// Marks an entity as a container that can hold other entities within it.
/// 
/// CONTAINER DESIGN PRINCIPLES:
/// - Logical Grouping: Containers serve as foundational entities for composition
/// - Semantic Clarity: Distinguishes between containment (items in backpack) vs attachment (scope on rifle)
/// - Tool Foundation: Containers are identifiable for editors and development tools
/// - Modding Support: Enables file system containers for mod discovery and management
/// - Scene Management: Supports scene containers with layered persistence
/// 
/// EDUCATIONAL NOTES:
/// Containers are fundamental to composition-based game development:
/// - Composition Root: Containers identify the root of object compositions
/// - Hierarchical Organization: Enable nested structures like inventory→bags→items
/// - Lifecycle Management: Container destruction handles all contained entities
/// - Asset Pipeline: Foundation for prefab systems and modular content
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - ContainerComponent is a lightweight marker - no significant overhead
/// - Uses existing ParentHierarchyComponent for actual parent-child relationships
/// - Container validation occurs only during PlaceIn operations
/// - Enables hierarchical culling optimizations for rendering systems
/// 
/// USAGE PATTERNS:
/// - Inventory Systems: Backpacks, chests, storage containers
/// - Scene Composition: Level containers, area sections, spawn groups
/// - Prefab Systems: Vehicle assemblies, building components, character loadouts
/// - Modding Infrastructure: Mod containers, asset bundles, plugin systems
/// </summary>
/// <param name="ContainerName">Human-readable name identifying the container's purpose</param>
/// <param name="IsLoaded">Whether the container is currently active and loaded in the world</param>
/// <param name="IsPersistent">Whether the container should persist across scene changes</param>
public readonly record struct ContainerComponent(
    string ContainerName,
    bool IsLoaded = true,
    bool IsPersistent = false
) : IComponent
{
    /// <summary>
    /// Creates a basic container with just a name, using default loaded and non-persistent settings.
    /// </summary>
    /// <param name="name">Human-readable name for the container</param>
    public ContainerComponent(string name) : this(name, true, false) { }

    /// <summary>
    /// Creates a container component with default values (empty name, loaded, non-persistent).
    /// </summary>
    public ContainerComponent() : this(string.Empty, true, false) { }

    /// <summary>
    /// Creates a new container component with the loaded state changed.
    /// </summary>
    /// <param name="loaded">New loaded state</param>
    /// <returns>New ContainerComponent with updated loaded state</returns>
    public ContainerComponent WithLoaded(bool loaded) =>
        this with { IsLoaded = loaded };

    /// <summary>
    /// Creates a new container component with the persistence changed.
    /// </summary>
    /// <param name="persistent">New persistence state</param>
    /// <returns>New ContainerComponent with updated persistence</returns>
    public ContainerComponent WithPersistent(bool persistent) =>
        this with { IsPersistent = persistent };

    /// <summary>
    /// Creates a new container component with a different name.
    /// </summary>
    /// <param name="name">New container name</param>
    /// <returns>New ContainerComponent with updated name</returns>
    public ContainerComponent WithName(string name) =>
        this with { ContainerName = name ?? string.Empty };
}