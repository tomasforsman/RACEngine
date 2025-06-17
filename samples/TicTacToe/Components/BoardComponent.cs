// ════════════════════════════════════════════════════════════════════════════════
// BOARD COMPONENT - GAME STATE DATA
// ════════════════════════════════════════════════════════════════════════════════
//
// Component representing the game board in the Entity-Component-System architecture.
// This demonstrates the ECS principle of components being pure data containers
// with no behavior or logic.
//
// ECS DESIGN PRINCIPLE:
// Components should contain only data, never behavior. All game logic is handled
// by Systems that operate on this component data, providing clean separation of
// concerns and enabling highly optimized data-oriented design.
//
// EXTENSIBILITY:
// The Size parameter allows for different board configurations (3x3, 5x5, etc.)
// demonstrating how ECS components can be designed for flexibility and reuse.
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Components;

namespace TicTacToe.Components;

/// <summary>
/// Represents the game board configuration in the ECS architecture.
/// This component is attached to the board entity and contains board metadata.
/// 
/// EDUCATIONAL NOTE:
/// This demonstrates the ECS principle where components are pure data structures.
/// The Size property enables different board configurations, showing how ECS
/// components can be designed for extensibility and reuse across game variants.
/// </summary>
/// <param name="Size">
/// The dimensions of the square game board (default: 3 for standard Tic-Tac-Toe).
/// Supports different board sizes for game variations.
/// </param>
public readonly record struct BoardComponent(int Size = 3) : IComponent
{
    /// <summary>
    /// Total number of cells in the board (Size × Size).
    /// Useful for game logic calculations and bounds checking.
    /// </summary>
    public int TotalCells => Size * Size;
    
    /// <summary>
    /// Validates that the board size is within reasonable bounds.
    /// Prevents invalid configurations that could cause game logic errors.
    /// </summary>
    public bool IsValidSize => Size >= 3 && Size <= 10;
}
