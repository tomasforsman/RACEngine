// ════════════════════════════════════════════════════════════════════════════════
// GAME STATE MANAGEMENT
// ════════════════════════════════════════════════════════════════════════════════
//
// Manages high-level game state in the Tic-Tac-Toe implementation.
// This class demonstrates state management patterns commonly used in game development,
// providing a clean interface between the ECS world and game flow control.
//
// DESIGN PATTERNS DEMONSTRATED:
// - State pattern for game phase management
// - Immutable state updates for predictable behavior
// - Clean separation between game rules and state tracking
// - Event-driven state transitions
//
// EDUCATIONAL VALUE:
// Shows how traditional game state management can coexist with ECS architecture,
// handling global game concerns while ECS manages entity-specific data.
//
// ════════════════════════════════════════════════════════════════════════════════

using TicTacToe.Components;

namespace TicTacToe.Game;

/// <summary>
/// Represents the current phase of the game.
/// Enables different behavior and UI based on game progress.
/// </summary>
public enum GamePhase
{
    /// <summary>Game is starting, initializing board and players.</summary>
    Starting,
    
    /// <summary>Game is active, players are making moves.</summary>
    Playing,
    
    /// <summary>Game has ended with a winner.</summary>
    Won,
    
    /// <summary>Game has ended in a draw (board full, no winner).</summary>
    Draw,
    
    /// <summary>Game was terminated early (player quit, error, etc.).</summary>
    Terminated
}

/// <summary>
/// Manages high-level game state for the Tic-Tac-Toe game.
/// 
/// EDUCATIONAL NOTE:
/// This class demonstrates how to manage global game state in an ECS architecture.
/// While the ECS World handles entity/component data, this class tracks game flow,
/// win conditions, and overall game progression.
/// 
/// DESIGN PHILOSOPHY:
/// - Immutable updates: State changes return new instances for predictability
/// - Clear responsibility: Handles only game-level concerns, not entity details
/// - Event-driven: State changes can trigger appropriate game events
/// - Thread-safe: Immutable design prevents concurrency issues
/// </summary>
public class GameState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME STATE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>Current phase of the game (Playing, Won, Draw, etc.).</summary>
    public GamePhase Phase { get; private set; } = GamePhase.Starting;
    
    /// <summary>Player who won the game (null if game is ongoing or drew).</summary>
    public Player? Winner { get; private set; }
    
    /// <summary>Total number of moves made in the current game.</summary>
    public int MoveCount { get; private set; } = 0;
    
    /// <summary>Player whose turn it currently is.</summary>
    public Player CurrentPlayer { get; private set; } = Player.X; // X traditionally goes first
    
    /// <summary>Size of the game board (supports different board dimensions).</summary>
    public int BoardSize { get; private set; } = 3;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME STATE QUERIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>Checks if the game is currently in progress and accepting moves.</summary>
    public bool IsGameActive => Phase == GamePhase.Playing;
    
    /// <summary>Checks if the game has finished (won, draw, or terminated).</summary>
    public bool IsGameFinished => Phase == GamePhase.Won || Phase == GamePhase.Draw || Phase == GamePhase.Terminated;
    
    /// <summary>Gets a descriptive message about the current game state.</summary>
    public string StatusMessage => Phase switch
    {
        GamePhase.Starting => "Game is starting...",
        GamePhase.Playing => $"It's {CurrentPlayer}'s turn (Move #{MoveCount + 1})",
        GamePhase.Won => $"🎉 {Winner} wins the game!",
        GamePhase.Draw => "Game ended in a draw - board is full!",
        GamePhase.Terminated => "Game was terminated",
        _ => "Unknown game state"
    };
    
    // ═══════════════════════════════════════════════════════════════════════════
    // STATE TRANSITION METHODS
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Initializes a new game with the specified board size.
    /// Resets all game state to starting conditions.
    /// </summary>
    /// <param name="boardSize">Size of the square board (default: 3 for standard Tic-Tac-Toe).</param>
    public void StartNewGame(int boardSize = 3)
    {
        if (boardSize < 3 || boardSize > 10)
            throw new ArgumentException("Board size must be between 3 and 10", nameof(boardSize));
            
        BoardSize = boardSize;
        Phase = GamePhase.Playing;
        CurrentPlayer = Player.X; // X always goes first by convention
        Winner = null;
        MoveCount = 0;
    }
    
    /// <summary>
    /// Records a player move and advances to the next turn.
    /// Updates move count and switches active player.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if game is not active.</exception>
    public void RecordMove()
    {
        if (!IsGameActive)
            throw new InvalidOperationException("Cannot record move when game is not active");
            
        MoveCount++;
        CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
    }
    
    /// <summary>
    /// Declares a winner and ends the game.
    /// Transitions game state to Won phase.
    /// </summary>
    /// <param name="winner">The player who won the game.</param>
    public void DeclareWinner(Player winner)
    {
        Winner = winner;
        Phase = GamePhase.Won;
    }
    
    /// <summary>
    /// Declares the game as a draw (board full, no winner).
    /// Transitions game state to Draw phase.
    /// </summary>
    public void DeclareDraw()
    {
        Phase = GamePhase.Draw;
    }
    
    /// <summary>
    /// Terminates the game early (player quit, error, etc.).
    /// Transitions game state to Terminated phase.
    /// </summary>
    public void TerminateGame()
    {
        Phase = GamePhase.Terminated;
    }
}
