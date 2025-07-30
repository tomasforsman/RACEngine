using Rac.Core.Manager;
using Rac.Input.Service;

namespace Rac.Engine;

/// <summary>
/// Entry point class for the RACEngine application.
/// Demonstrates proper engine initialization with required subsystems.
/// </summary>
/// <remarks>
/// This class showcases the proper initialization sequence for RACEngine:
/// 1. Construct required subsystems (window, input, configuration)
/// 2. Inject dependencies into the engine
/// 3. Start the main game loop
/// 
/// Educational Note: This pattern demonstrates dependency injection at the application level,
/// showing how game engines manage complex subsystem dependencies.
/// </remarks>
public class Program
{
    /// <summary>
    /// The main entry point for the RACEngine demonstration application.
    /// Initializes all required engine subsystems and starts the game loop.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application. Currently unused.</param>
    /// <remarks>
    /// This method demonstrates the standard RACEngine initialization pattern:
    /// - WindowManager: Handles platform-specific window creation and management
    /// - SilkInputService: Provides cross-platform input handling via Silk.NET
    /// - ConfigManager: Manages engine configuration and settings
    /// 
    /// For production games, consider using a game-specific entry point that inherits
    /// this initialization pattern while adding game-specific setup logic.
    /// </remarks>
    public static void Main(string[] args)
    {
        // Construct required subsystems
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configManager = new ConfigManager();

        // Inject into engine
        var engine = new GameEngine.Engine(windowManager, inputService, configManager);
        engine.Run();
    }
}
