using Rac.ECS.Components;
using Silk.NET.Maths;

namespace PupperQuest.Components;

/// <summary>
/// Represents an entity's position on the game grid.
/// Grid coordinates are discrete integer positions for turn-based movement.
/// </summary>
/// <param name="X">The X coordinate on the grid</param>
/// <param name="Y">The Y coordinate on the grid</param>
/// <remarks>
/// Educational Note: Grid-based positioning simplifies collision detection and turn-based logic.
/// Each tile represents a discrete space that can contain at most one entity.
/// This is common in roguelike games for strategic movement and clear spatial relationships.
/// </remarks>
public readonly record struct GridPositionComponent(int X, int Y) : IComponent
{
    /// <summary>
    /// Convert grid position to world coordinates for rendering.
    /// Flips Y coordinate to match OpenGL convention (Y increases upward).
    /// </summary>
    /// <param name="tileSize">Size of each grid tile in world units</param>
    /// <returns>World position as Vector2D for rendering systems</returns>
    public Vector2D<float> ToWorldPosition(float tileSize)
    {
        // Flip Y coordinate: Grid Y=0 (top) becomes World Y=high (top in world space)
        // This ensures that moving "up" in the game (decreasing grid Y) appears as moving up on screen
        return new Vector2D<float>(X * tileSize, -Y * tileSize);
    }
}

/// <summary>
/// Stores movement direction and timing for grid-based movement animation.
/// </summary>
/// <param name="Direction">Direction vector for next movement</param>
/// <param name="MoveTimer">Timer for smooth animation between grid positions</param>
public readonly record struct MovementComponent(Vector2D<int> Direction, float MoveTimer) : IComponent;

/// <summary>
/// Marks an entity as the player-controlled puppy with game stats.
/// </summary>
/// <param name="Health">Current health points</param>
/// <param name="Energy">Current energy/stamina for actions</param>
/// <param name="SmellRadius">Range for detecting items and enemies</param>
public readonly record struct PuppyComponent(int Health, int Energy, int SmellRadius) : IComponent;

/// <summary>
/// Represents a tile in the game world with type and passability.
/// </summary>
/// <param name="Type">The type of tile (Floor, Wall, Door, etc.)</param>
/// <param name="IsPassable">Whether entities can move through this tile</param>
public readonly record struct TileComponent(TileType Type, bool IsPassable) : IComponent;

/// <summary>
/// Visual representation data for entities and tiles.
/// </summary>
/// <param name="Size">Size of the sprite in world coordinates</param>
/// <param name="Color">RGBA color for rendering</param>
public readonly record struct SpriteComponent(Vector2D<float> Size, Vector4D<float> Color) : IComponent;

/// <summary>
/// AI behavior component for enemy entities.
/// </summary>
/// <param name="Behavior">The type of AI behavior to execute</param>
/// <param name="Target">Entity ID of the current target (0 if no target)</param>
/// <param name="PatrolRoute">List of positions for patrol behavior</param>
public readonly record struct AIComponent(AIBehavior Behavior, uint Target, Vector2D<int>[] PatrolRoute) : IComponent;

/// <summary>
/// Marks an entity as an enemy with combat properties.
/// </summary>
/// <param name="Type">The type of enemy</param>
/// <param name="AttackDamage">Damage dealt to player on contact</param>
/// <param name="DetectionRange">Range for detecting the player</param>
public readonly record struct EnemyComponent(EnemyType Type, int AttackDamage, int DetectionRange) : IComponent;

/// <summary>
/// Game level information and progression state.
/// </summary>
/// <param name="CurrentLevel">Current level number</param>
/// <param name="HasExit">Whether this level has an exit to next level</param>
/// <param name="IsComplete">Whether level objectives are completed</param>
public readonly record struct LevelComponent(int CurrentLevel, bool HasExit, bool IsComplete) : IComponent;

/// <summary>
/// Collectible items with gameplay effects.
/// </summary>
/// <param name="Type">Type of item</param>
/// <param name="Value">Numeric value for the item effect</param>
public readonly record struct ItemComponent(ItemType Type, int Value) : IComponent;