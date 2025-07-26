namespace PupperQuest.Components;

/// <summary>
/// Types of tiles in the game world.
/// </summary>
/// <remarks>
/// Educational Note: Tile-based level design is fundamental to grid-based games.
/// Each tile type has different properties affecting movement, rendering, and gameplay.
/// </remarks>
public enum TileType
{
    /// <summary>Empty walkable floor space</summary>
    Floor,
    
    /// <summary>Solid impassable wall</summary>
    Wall,
    
    /// <summary>Door that can be opened/closed</summary>
    Door,
    
    /// <summary>Stairs leading to next level</summary>
    Stairs,
    
    /// <summary>Starting position for the player</summary>
    Start,
    
    /// <summary>Exit/goal position</summary>
    Exit
}

/// <summary>
/// AI behavior patterns for enemy entities.
/// </summary>
/// <remarks>
/// Educational Note: Simple state-based AI is effective for roguelike games.
/// Each behavior represents a different challenge type for the player.
/// Academic Reference: "Artificial Intelligence for Games" (Millington & Funge, 2009)
/// </remarks>
public enum AIBehavior
{
    /// <summary>Moves toward player when detected</summary>
    Hostile,
    
    /// <summary>Runs away from player when nearby</summary>
    Flee,
    
    /// <summary>Follows predefined patrol route</summary>
    Patrol,
    
    /// <summary>Stands still until player gets close</summary>
    Guard,
    
    /// <summary>Wanders randomly around the level</summary>
    Wander
}

/// <summary>
/// Types of enemy entities with different behaviors and appearances.
/// </summary>
/// <remarks>
/// Educational Note: Enemy variety creates different tactical challenges.
/// Each type demonstrates different AI patterns and player interaction strategies.
/// </remarks>
public enum EnemyType
{
    /// <summary>Small fast enemies that chase the player</summary>
    Rat,
    
    /// <summary>Medium enemies that flee from the player</summary>
    Cat,
    
    /// <summary>Large enemies that patrol and chase when close</summary>
    Mailman,
    
    /// <summary>Stationary guards that block passages</summary>
    FenceGuard
}

/// <summary>
/// Types of collectible items with different effects.
/// </summary>
public enum ItemType
{
    /// <summary>Restores health points</summary>
    Treat,
    
    /// <summary>Restores energy/stamina</summary>
    Water,
    
    /// <summary>Increases smell detection radius</summary>
    Bone,
    
    /// <summary>Key to unlock doors</summary>
    Key,
    
    /// <summary>Special item needed to complete level</summary>
    Toy
}