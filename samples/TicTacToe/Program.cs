// ════════════════════════════════════════════════════════════════════════════════
// TICTACTOE SAMPLE - EDUCATIONAL ECS GAME IMPLEMENTATION
// ════════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates a complete game implementation using the Entity-Component-System
// architecture pattern, showcasing how to build a fully functional game with clean separation
// of concerns and data-oriented design principles.
//
// EDUCATIONAL OBJECTIVES:
//
// 1. ENTITY-COMPONENT-SYSTEM (ECS) PATTERN:
//    - Entities: Game board, cells, players as unique identifiers
//    - Components: Pure data structures (BoardComponent, CellComponent, PlayerComponent)
//    - Systems: Logic processors (InputSystem, GameLogicSystem, RenderSystem, WinCheckSystem)
//    - Benefits: Modularity, performance, maintainability, data locality
//
// 2. GAME ARCHITECTURE PATTERNS:
//    - State management with immutable game state
//    - Event-driven input handling
//    - Separation of game logic from presentation
//    - Clean game loop implementation
//
// 3. USER INTERFACE DESIGN:
//    - Console-based UI with clear visual feedback
//    - Input validation and error handling
//    - Responsive user experience design
//
// 4. GAME DESIGN PRINCIPLES:
//    - Turn-based game flow
//    - Win condition detection
//    - Player feedback and game state communication
//    - Extensible design for different board sizes
//
// GAME RULES:
// - Classic 3x3 Tic-Tac-Toe (also known as Noughts and Crosses)
// - Two players alternate placing X and O marks
// - First player to get three marks in a row (horizontal, vertical, or diagonal) wins
// - Game ends in a draw if all cells are filled without a winner
//
// ARCHITECTURE DEMONSTRATION:
// This implementation shows how complex game logic can be broken down into simple,
// testable components and systems, making the codebase easy to understand, maintain,
// and extend with new features.
//
// ════════════════════════════════════════════════════════════════════════════════

using TicTacToe.Game;

namespace TicTacToe;

/// <summary>
/// Main entry point for the TicTacToe educational sample.
/// Demonstrates Entity-Component-System architecture in a complete game implementation.
/// </summary>
internal class Program
{
    /// <summary>
    /// Application entry point. Initializes the ECS world and starts the game loop.
    /// </summary>
    /// <param name="args">Command line arguments (currently unused).</param>
    private static void Main(string[] args)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // EDUCATIONAL GAME STARTUP
        // ═══════════════════════════════════════════════════════════════════════════
        
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    TIC-TAC-TOE SAMPLE                       ║");
        Console.WriteLine("║              Educational ECS Implementation                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("This sample demonstrates:");
        Console.WriteLine("• Entity-Component-System architecture");
        Console.WriteLine("• Clean separation of game logic and presentation");
        Console.WriteLine("• Data-oriented design principles");
        Console.WriteLine("• Event-driven input handling");
        Console.WriteLine();
        
        // ═══════════════════════════════════════════════════════════════════════════
        // GAME INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════
        
        try
        {
            var gameEngine = new TicTacToeEngine();
            gameEngine.Initialize();
            gameEngine.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Game encountered an error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
