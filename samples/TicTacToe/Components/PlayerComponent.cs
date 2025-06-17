// ════════════════════════════════════════════════════════════════════════════════
// PLAYER COMPONENT - TURN MANAGEMENT AND PLAYER STATE
// ════════════════════════════════════════════════════════════════════════════════
//
// Manages player entities and turn-based game flow using ECS principles.
// Each player is represented as an entity with this component containing
// their identity and current turn status.
//
// TURN-BASED GAME DESIGN:
// This component enables clean turn management by allowing systems to query
// for the current player and alternate turns through component updates.
// The design supports easy extension to multiplayer scenarios or AI players.
//
// ECS BENEFITS DEMONSTRATED:
// - Player state is decoupled from game logic
// - Systems can efficiently query for active player
// - Turn switching becomes a simple component update
// - Easy to extend with additional player properties
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Components;

namespace TicTacToe.Components;

/// <summary>
/// Identifies the two players in a Tic-Tac-Toe game.
/// 
/// CONVENTION:
/// - X: Traditional first player, typically goes first
/// - O: Second player, moves after X
/// 
/// This enum provides type safety and clear semantics for player identification
/// throughout the game systems.
/// </summary>
public enum Player
{
    /// <summary>Player 1, uses X marker. Traditionally moves first.</summary>
    X,
    
    /// <summary>Player 2, uses O marker. Moves second in alternating turns.</summary>
    O,
}

/// <summary>
/// Component representing a player entity in the turn-based game system.
/// 
/// EDUCATIONAL NOTE:
/// This demonstrates how ECS handles game state management for turn-based games.
/// The IsCurrent flag enables systems to quickly identify whose turn it is,
/// while PlayerId maintains player identity throughout the game.
/// 
/// DESIGN BENEFITS:
/// - Efficient turn management through simple queries
/// - Clean separation of player identity and game state
/// - Extensible for future features (scores, statistics, AI difficulty)
/// - Type-safe player identification prevents common bugs
/// </summary>
/// <param name="PlayerId">Identifies which player this entity represents.</param>
/// <param name="IsCurrent">Indicates if it's currently this player's turn.</param>
public readonly record struct PlayerComponent(Player PlayerId, bool IsCurrent) : IComponent
{
    /// <summary>
    /// Gets the cell state that this player creates when making moves.
    /// Provides consistent mapping between player identity and board state.
    /// </summary>
    public CellState CellState => PlayerId switch
    {
        Player.X => Components.CellState.X,
        Player.O => Components.CellState.O,
        _ => Components.CellState.Empty
    };
    
    /// <summary>
    /// Gets a human-readable name for this player.
    /// Used in UI displays and game messages.
    /// </summary>
    public string DisplayName => PlayerId switch
    {
        Player.X => "Player X",
        Player.O => "Player O",
        _ => "Unknown Player"
    };
    
    /// <summary>
    /// Gets the character symbol used to represent this player on the board.
    /// Essential for text-based rendering systems.
    /// </summary>
    public char Symbol => PlayerId switch
    {
        Player.X => 'X',
        Player.O => 'O',
        _ => '?'
    };
}
