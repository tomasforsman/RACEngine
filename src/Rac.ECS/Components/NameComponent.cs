using Rac.ECS.Components;

namespace Rac.ECS.Components;

/// <summary>
/// Stores a human-readable name for an entity, enabling name-based entity queries.
/// 
/// USAGE PATTERNS:
/// - Entity identification in development tools and debugging
/// - Finding specific entities by name (e.g., "Player", "MainCamera")
/// - Logging and error reporting with meaningful entity names
/// - Save/load systems that reference entities by stable names
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Name queries use linear search through entities with NameComponent
/// - For high-performance scenarios, consider caching name-to-entity mappings
/// - Names are not enforced to be unique - multiple entities can share names
/// 
/// EDUCATIONAL NOTES:
/// This follows the ECS pattern of using components for metadata rather than
/// inheriting from base entity classes. This allows flexible composition where
/// entities can optionally have names without affecting other systems.
/// </summary>
/// <param name="Name">Human-readable name for the entity</param>
public readonly record struct NameComponent(string Name) : IComponent;