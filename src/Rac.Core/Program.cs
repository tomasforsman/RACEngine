using Rac.Core.Manager;
using Rac.Input.Service;

namespace Rac.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            // Construct required subsystems
            var windowManager = new WindowManager();
            var inputService  = new SilkInputService();
            var configManager = new ConfigManager();

            // Inject into engine
            var engine = new GameEngine(windowManager, inputService, configManager);
            engine.Run();
        }

    }
}
