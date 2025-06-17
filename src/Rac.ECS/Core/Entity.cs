namespace Rac.ECS.Core;

/// <summary>
/// Represents a unique entity in the Entity-Component-System architecture.
/// Entities are lightweight identifiers that serve as containers for components.
/// This follows the data-oriented design principle where entities are just IDs.
/// </summary>
/// <param name="Id">The unique identifier for this entity.</param>
/// <param name="IsAlive">Indicates whether the entity is active and should be processed by systems.</param>
public readonly record struct Entity(int Id, bool IsAlive = true);
