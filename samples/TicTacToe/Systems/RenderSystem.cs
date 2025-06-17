// ════════════════════════════════════════════════════════════════════════════════
// RENDER SYSTEM - CONSOLE-BASED GAME DISPLAY
// ════════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Core;
using TicTacToe.Components;
using TicTacToe.Game;

namespace TicTacToe.Systems;

/// <summary>
/// ECS System responsible for rendering the game state to the console.
/// </summary>
public class RenderSystem
{
    private readonly World _world;
    private readonly GameState _gameState;

    public RenderSystem(World world, GameState gameState)
    {
        _world = world;
        _gameState = gameState;
    }

    public void Update()
    {
        Console.Clear();
        Console.WriteLine("Tic-Tac-Toe");
        Console.WriteLine("═══════════");
        
        // Basic board display - this is a simplified implementation
        Console.WriteLine("  A   B   C");
        Console.WriteLine("1   |   |   ");
        Console.WriteLine("  ─────────");
        Console.WriteLine("2   |   |   ");
        Console.WriteLine("  ─────────");
        Console.WriteLine("3   |   |   ");
        Console.WriteLine();
        Console.WriteLine(_gameState.StatusMessage);
        Console.WriteLine();
    }
}
