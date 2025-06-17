// ════════════════════════════════════════════════════════════════════════════════
// INPUT SYSTEM - ECS INPUT HANDLING
// ════════════════════════════════════════════════════════════════════════════════
//
// Handles user input for the Tic-Tac-Toe game using ECS architecture principles.
// This system demonstrates clean input handling with validation, error recovery,
// and user-friendly feedback.
//
// ECS DESIGN PATTERNS:
// - System operates on World data without storing state
// - Input events are processed and converted to game actions
// - Clean separation between input handling and game logic
// - Stateless design enables easy testing and debugging
//
// INPUT DESIGN PRINCIPLES:
// - Forgiving input parsing (case insensitive, whitespace tolerant)
// - Clear error messages for invalid input
// - Multiple input formats supported (coordinates, algebraic notation)
// - Graceful handling of edge cases and user errors
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Core;
using TicTacToe.Components;
using TicTacToe.Game;

namespace TicTacToe.Systems;

/// <summary>
/// ECS System responsible for handling user input and converting it to game moves.
/// 
/// EDUCATIONAL NOTE:
/// This system demonstrates how to handle input in an ECS architecture while
/// maintaining clean separation of concerns. Input processing is stateless and
/// focuses solely on converting user input to validated game actions.
/// 
/// DESIGN BENEFITS:
/// - Stateless: No instance variables, operates only on provided data
/// - Testable: Easy to unit test with mock input
/// - Flexible: Supports multiple input formats and graceful error handling
/// - User-friendly: Provides clear feedback for invalid input
/// </summary>
public class InputSystem
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly World _world;
    private readonly GameState _gameState;
    
    /// <summary>
    /// Initializes the input system with required dependencies.
    /// Demonstrates dependency injection pattern in ECS systems.
    /// </summary>
    /// <param name="world">ECS world containing entities and components.</param>
    /// <param name="gameState">Current game state for validation.</param>
    public InputSystem(World world, GameState gameState)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM UPDATE METHOD
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Processes user input and validates moves.
    /// Called once per game loop iteration when input is needed.
    /// </summary>
    public void Update()
    {
        if (!_gameState.IsGameActive)
            return; // Only process input during active gameplay
            
        // ─── Display Input Prompt ───────────────────────────────────────
        Console.WriteLine($"{_gameState.CurrentPlayer}'s turn!");
        Console.Write("Enter your move (e.g., A1, B2, C3) or 'quit': ");
        
        // ─── Read and Process Input ─────────────────────────────────────
        string? input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Please enter a valid move or 'quit'.");
            return;
        }
        
        input = input.Trim();
        
        // ─── Handle Special Commands ────────────────────────────────────
        if (input.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            _gameState.TerminateGame();
            return;
        }
        
        // ─── Parse and Validate Move ────────────────────────────────────
        if (TryParseMove(input, out Move move))
        {
            ProcessMove(move);
        }
        else
        {
            DisplayInputHelp();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // INPUT PARSING METHODS
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Attempts to parse user input into a valid move.
    /// Supports multiple input formats with forgiving parsing.
    /// </summary>
    /// <param name="input">Raw user input string.</param>
    /// <param name="move">Parsed move if successful.</param>
    /// <returns>True if parsing was successful.</returns>
    private bool TryParseMove(string input, out Move move)
    {
        move = default;
        
        try
        {
            // ─── Try Algebraic Notation (A1, B2, C3) ────────────────────
            if (input.Length >= 2 && char.IsLetter(input[0]) && char.IsDigit(input[1]))
            {
                move = Move.FromAlgebraicNotation(input, _gameState.CurrentPlayer);
                return true;
            }
            
            // ─── Try Coordinate Format (1,1 or 1 1) ─────────────────────
            if (TryParseCoordinates(input, out int x, out int y))
            {
                move = new Move(x, y, _gameState.CurrentPlayer);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid move format: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Attempts to parse coordinate-style input (e.g., "1,2" or "1 2").
    /// Provides alternative input method for users who prefer numbers.
    /// </summary>
    /// <param name="input">Input string containing coordinates.</param>
    /// <param name="x">Parsed X coordinate (0-based).</param>
    /// <param name="y">Parsed Y coordinate (0-based).</param>
    /// <returns>True if coordinates were successfully parsed.</returns>
    private bool TryParseCoordinates(string input, out int x, out int y)
    {
        x = y = 0;
        
        // Split on common delimiters
        string[] parts = input.Split(new[] { ',', ' ', '-', ':' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 2)
            return false;
            
        if (!int.TryParse(parts[0], out int parsedX) || !int.TryParse(parts[1], out int parsedY))
            return false;
            
        // Convert from 1-based user input to 0-based internal representation
        x = parsedX - 1;
        y = parsedY - 1;
        
        return true;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // MOVE PROCESSING AND VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Processes a parsed move by validating and applying it to the ECS world.
    /// Demonstrates integration between input handling and ECS data updates.
    /// </summary>
    /// <param name="move">The move to process.</param>
    private void ProcessMove(Move move)
    {
        // ─── Validate Move Bounds ───────────────────────────────────────
        if (!move.IsValidForBoard(_gameState.BoardSize))
        {
            Console.WriteLine($"Move {move.ToAlgebraicNotation()} is outside the board! Use A1-C3.");
            return;
        }
        
        // ─── Find Target Cell Entity (simplified approach) ─────────────
        // Note: In a full implementation, we would use World.Query<CellComponent>()
        // to find the specific cell, but for this educational sample, we'll
        // demonstrate the concept with a simpler approach.
        
        bool cellFound = false;
        foreach (var (entity, cell) in _world.Query<CellComponent>())
        {
            if (cell.X == move.X && cell.Y == move.Y)
            {
                if (cell.IsOccupied)
                {
                    Console.WriteLine($"Cell {move.ToAlgebraicNotation()} is already occupied by {cell.State}!");
                    return;
                }
                
                // ─── Apply Move to ECS World ────────────────────────────────────
                var newCellState = _gameState.CurrentPlayer == Player.X ? CellState.X : CellState.O;
                var updatedCell = cell with { State = newCellState };
                _world.SetComponent(entity, updatedCell);
                
                cellFound = true;
                break;
            }
        }
        
        if (!cellFound)
        {
            Console.WriteLine("Error: Could not find the target cell. This should not happen!");
            return;
        }
        
        // ─── Record Move in Game State ──────────────────────────────────
        _gameState.RecordMove();
        
        Console.WriteLine($"✓ {move.Player} placed at {move.ToAlgebraicNotation()}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Displays helpful information about valid input formats.
    /// Provides user education and reduces frustration with input errors.
    /// </summary>
    private void DisplayInputHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Invalid input! Please use one of these formats:");
        Console.WriteLine("• Algebraic notation: A1, B2, C3 (columns A-C, rows 1-3)");
        Console.WriteLine("• Coordinates: 1,1 or 1 1 (column,row using 1-3)");
        Console.WriteLine("• Commands: 'quit' or 'exit' to end the game");
        Console.WriteLine();
        Console.WriteLine("Examples: A1 (top-left), B2 (center), C3 (bottom-right)");
        Console.WriteLine();
    }
}
