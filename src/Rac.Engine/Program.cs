using Rac.Core.Manager;
using Rac.Input.Service;

namespace Rac.Engine;

public class Program
{
    /// <summary>
    ///   Main entry point for the Engine.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
	{
		static void Main(string[] args)
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
}