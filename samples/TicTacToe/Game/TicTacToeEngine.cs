// ════════════════════════════════════════════════════════════════════════════════
// TIC-TAC-TOE GAME ENGINE
// ════════════════════════════════════════════════════════════════════════════════
//
// Main game engine coordinating the Entity-Component-System architecture for
// the Tic-Tac-Toe educational sample. This class demonstrates how to integrate
// ECS with traditional game flow and user interface concerns.
//
// ARCHITECTURE DEMONSTRATION:
// - ECS World management for entities and components
// - System coordination for game logic, input, and rendering
// - Clean separation between game state and ECS entity state
// - Event-driven architecture for user input and game events
//
// EDUCATIONAL OBJECTIVES:
// - Show complete ECS implementation in a real game context
// - Demonstrate clean game loop architecture
// - Illustrate separation of concerns in game design
// - Provide foundation for students to extend with new features
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Core;
using TicTacToe.Components;
using TicTacToe.Game;
using TicTacToe.Systems;

namespace TicTacToe.Game;

/// <summary>
/// Main game engine for the Tic-Tac-Toe educational sample.
/// 
/// EDUCATIONAL NOTE:
/// This class demonstrates how to coordinate ECS architecture with traditional
/// game flow. It manages the ECS World, game systems, and overall game lifecycle
/// while maintaining clean separation of concerns.
/// 
/// ARCHITECTURE BENEFITS:
/// - ECS handles entity/component data efficiently
/// - Systems provide modular, testable game logic
/// - Clean game loop with predictable update order
/// - Easy to extend with new features and systems
/// </summary>
public class TicTacToeEngine
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ECS INFRASTRUCTURE
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly World _world;
    private readonly GameState _gameState;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME SYSTEMS (in update order)
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly InputSystem _inputSystem;
    private readonly GameLogicSystem _gameLogicSystem;
    private readonly WinCheckSystem _winCheckSystem;
    private readonly RenderSystem _renderSystem;
    
    /// <summary>
    /// Initializes the game engine with ECS world and systems.
    /// Demonstrates dependency setup and system coordination.
    /// </summary>
    public TicTacToeEngine()
    {
        _world = new World();
        _gameState = new GameState();
        
        // Initialize systems in dependency order
        _inputSystem = new InputSystem(_world, _gameState);
        _gameLogicSystem = new GameLogicSystem(_world, _gameState);
        _winCheckSystem = new WinCheckSystem(_world, _gameState);
        _renderSystem = new RenderSystem(_world, _gameState);
    }
    
    /// <summary>
    /// Initializes the game world with board and player entities.
    /// Demonstrates ECS entity creation and component setup.
    /// </summary>
    public void Initialize()
    {
        Console.WriteLine("Initializing Tic-Tac-Toe game...");
        
        // ─── Initialize Game State ──────────────────────────────────────
        _gameState.StartNewGame(boardSize: 3);
        
        // ─── Create Board Entity ────────────────────────────────────────
        var boardEntity = _world.CreateEntity();
        _world.SetComponent(boardEntity, new BoardComponent(Size: 3));
        
        // ─── Create Cell Entities ───────────────────────────────────────
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var cellEntity = _world.CreateEntity();
                _world.SetComponent(cellEntity, new CellComponent(x, y, CellState.Empty));
            }
        }
        
        // ─── Create Player Entities ─────────────────────────────────────
        var playerXEntity = _world.CreateEntity();
        _world.SetComponent(playerXEntity, new PlayerComponent(Player.X, IsCurrent: true));
        
        var playerOEntity = _world.CreateEntity();
        _world.SetComponent(playerOEntity, new PlayerComponent(Player.O, IsCurrent: false));
        
        Console.WriteLine("Game initialized successfully!");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Main game loop coordinating system updates and user interaction.
    /// Demonstrates clean game loop architecture with ECS systems.
    /// </summary>
    public void Run()
    {
        Console.WriteLine("Starting Tic-Tac-Toe game...");
        Console.WriteLine("Enter moves as coordinates (e.g., 'A1', 'B2', 'C3') or 'quit' to exit.");
        Console.WriteLine();
        
        // Main game loop
        while (_gameState.IsGameActive)
        {
            try
            {
                // ═══════════════════════════════════════════════════════════════
                // SYSTEM UPDATE CYCLE (order matters!)
                // ═══════════════════════════════════════════════════════════════
                
                // 1. Render current game state
                _renderSystem.Update();
                
                // 2. Handle user input
                _inputSystem.Update();
                
                // 3. Process game logic
                _gameLogicSystem.Update();
                
                // 4. Check for win conditions
                _winCheckSystem.Update();
                
                // Small delay for readability
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game error: {ex.Message}");
                Console.WriteLine("Continuing game...");
            }
        }
        
        // ═══════════════════════════════════════════════════════════════════════
        // GAME OVER HANDLING
        // ═══════════════════════════════════════════════════════════════════════
        
        // Final render to show end state
        _renderSystem.Update();
        
        Console.WriteLine();
        Console.WriteLine(_gameState.StatusMessage);
        Console.WriteLine();
        Console.WriteLine("Thanks for playing! Press any key to exit...");
        Console.ReadKey();
    }
}