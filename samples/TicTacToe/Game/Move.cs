// ════════════════════════════════════════════════════════════════════════════════
// GAME MOVE REPRESENTATION
// ════════════════════════════════════════════════════════════════════════════════
//
// Represents a player move in the Tic-Tac-Toe game using value semantics.
// This class demonstrates clean data modeling for game actions and provides
// a foundation for features like move history, undo/redo, and game replay.
//
// DESIGN PRINCIPLES:
// - Immutable value type for thread safety and predictable behavior
// - Rich validation to prevent invalid game states
// - Clear semantics for game logic and UI systems
// - Extensible for future features (timestamps, move analysis, etc.)
//
// EDUCATIONAL VALUE:
// Shows how to model game actions as first-class objects, enabling clean
// separation between move representation and move execution logic.
//
// ════════════════════════════════════════════════════════════════════════════════

using TicTacToe.Components;

namespace TicTacToe.Game;

/// <summary>
/// Represents a single move in a Tic-Tac-Toe game.
/// 
/// EDUCATIONAL NOTE:
/// This value type demonstrates how to model game actions as immutable data structures.
/// By representing moves as objects, we enable features like move validation,
/// history tracking, undo/redo functionality, and game replay systems.
/// 
/// DESIGN BENEFITS:
/// - Immutable: Once created, move data cannot change (thread-safe)
/// - Validated: Constructor ensures move is always in valid state
/// - Serializable: Can be saved for game replay or network transmission
/// - Testable: Easy to create test moves for unit testing
/// </summary>
public readonly record struct Move
{
    // ═══════════════════════════════════════════════════════════════════════════
    // MOVE DATA
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>Horizontal position on the board (0-based index).</summary>
    public int X { get; }
    
    /// <summary>Vertical position on the board (0-based index).</summary>
    public int Y { get; }
    
    /// <summary>Player making this move.</summary>
    public Player Player { get; }
    
    /// <summary>Timestamp when this move was created (useful for game analysis).</summary>
    public DateTime Timestamp { get; }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR WITH VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Creates a new move with validation.
    /// Ensures move coordinates are within reasonable bounds.
    /// </summary>
    /// <param name="x">Horizontal position (0-based).</param>
    /// <param name="y">Vertical position (0-based).</param>
    /// <param name="player">Player making the move.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if coordinates are negative or unreasonably large.
    /// </exception>
    public Move(int x, int y, Player player)
    {
        if (x < 0 || x > 50) // Reasonable upper bound for extensibility
            throw new ArgumentOutOfRangeException(nameof(x), "X coordinate must be between 0 and 50");
            
        if (y < 0 || y > 50) // Reasonable upper bound for extensibility  
            throw new ArgumentOutOfRangeException(nameof(y), "Y coordinate must be between 0 and 50");
        
        X = x;
        Y = y;
        Player = player;
        Timestamp = DateTime.UtcNow;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Validates that this move is within the bounds of the specified board size.
    /// Essential for preventing array index out of bounds errors.
    /// </summary>
    /// <param name="boardSize">Size of the square game board.</param>
    /// <returns>True if move is valid for the board size.</returns>
    public bool IsValidForBoard(int boardSize)
    {
        return X >= 0 && X < boardSize && Y >= 0 && Y < boardSize;
    }
    
    /// <summary>
    /// Gets a human-readable representation of this move.
    /// Useful for debugging, logging, and player feedback.
    /// </summary>
    /// <returns>Formatted string describing the move.</returns>
    public override string ToString()
    {
        return $"{Player} -> ({X}, {Y})";
    }
    
    /// <summary>
    /// Gets a chess-style algebraic notation for this move (A1, B2, etc.).
    /// Provides familiar notation for players and move history display.
    /// </summary>
    /// <returns>Algebraic notation string (e.g., "A1", "C3").</returns>
    public string ToAlgebraicNotation()
    {
        if (X > 25) return ToString(); // Fallback for very large boards
        
        char column = (char)('A' + X);
        int row = Y + 1; // Convert to 1-based for display
        return $"{column}{row}";
    }
    
    /// <summary>
    /// Creates a move from algebraic notation (e.g., "A1", "C3").
    /// Enables user-friendly input parsing for text-based interfaces.
    /// </summary>
    /// <param name="notation">Algebraic notation string.</param>
    /// <param name="player">Player making the move.</param>
    /// <returns>Parsed move object.</returns>
    /// <exception cref="ArgumentException">Thrown if notation is invalid.</exception>
    public static Move FromAlgebraicNotation(string notation, Player player)
    {
        if (string.IsNullOrWhiteSpace(notation) || notation.Length < 2)
            throw new ArgumentException("Invalid notation format", nameof(notation));
            
        notation = notation.Trim().ToUpperInvariant();
        
        char columnChar = notation[0];
        if (columnChar < 'A' || columnChar > 'Z')
            throw new ArgumentException("Column must be a letter A-Z", nameof(notation));
            
        if (!int.TryParse(notation[1..], out int row) || row < 1)
            throw new ArgumentException("Row must be a positive number", nameof(notation));
            
        int x = columnChar - 'A';
        int y = row - 1; // Convert to 0-based indexing
        
        return new Move(x, y, player);
    }
}
