// ════════════════════════════════════════════════════════════════════════════════
// CELL COMPONENT - INDIVIDUAL BOARD POSITION DATA
// ════════════════════════════════════════════════════════════════════════════════
//
// Represents individual cells within the Tic-Tac-Toe board using ECS principles.
// Each cell is an entity with this component containing its position and state.
//
// DESIGN RATIONALE:
// By making each cell an entity with a component, we achieve several benefits:
// - Uniform data access patterns for all game elements
// - Easy querying and filtering of cells by state or position
// - Consistent with ECS philosophy of composition over inheritance
// - Enables future extensibility (animations, special effects per cell)
//
// COORDINATE SYSTEM:
// Uses zero-based indexing where (0,0) represents the top-left corner,
// matching common programming conventions and making array indexing natural.
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Components;

namespace TicTacToe.Components;

/// <summary>
/// Represents the state of a game board cell.
/// 
/// GAME DESIGN:
/// - Empty: Cell is available for player moves
/// - X: Cell occupied by the first player (traditionally goes first)
/// - O: Cell occupied by the second player
/// 
/// This enum enables clear state management and makes game logic more readable.
/// </summary>
public enum CellState
{
    /// <summary>Cell is unoccupied and available for moves.</summary>
    Empty,
    
    /// <summary>Cell is occupied by Player 1 (X marker).</summary>
    X,
    
    /// <summary>Cell is occupied by Player 2 (O marker).</summary>
    O,
}

/// <summary>
/// Component representing an individual cell in the Tic-Tac-Toe board.
/// 
/// EDUCATIONAL NOTE:
/// This demonstrates how ECS components store game state as pure data.
/// Each cell entity in the game world will have this component, allowing
/// systems to query and manipulate cell states efficiently.
/// 
/// COORDINATE SYSTEM:
/// Uses zero-based indexing where (0,0) is the top-left corner:
/// ```
/// (0,0) (1,0) (2,0)
/// (0,1) (1,1) (2,1) 
/// (0,2) (1,2) (2,2)
/// ```
/// </summary>
/// <param name="X">Horizontal position on the board (0-based index).</param>
/// <param name="Y">Vertical position on the board (0-based index).</param>
/// <param name="State">Current occupancy state of the cell.</param>
public readonly record struct CellComponent(int X, int Y, CellState State) : IComponent
{
    /// <summary>
    /// Checks if this cell is available for a player move.
    /// Essential for move validation in game logic systems.
    /// </summary>
    public bool IsEmpty => State == CellState.Empty;
    
    /// <summary>
    /// Checks if this cell is occupied by any player.
    /// Useful for board analysis and win condition checking.
    /// </summary>
    public bool IsOccupied => State != CellState.Empty;
    
    /// <summary>
    /// Gets a display character representing the cell state.
    /// Used by rendering systems to convert game state to visual representation.
    /// </summary>
    public char DisplayCharacter => State switch
    {
        CellState.X => 'X',
        CellState.O => 'O',
        CellState.Empty => ' ',
        _ => '?'
    };
}
