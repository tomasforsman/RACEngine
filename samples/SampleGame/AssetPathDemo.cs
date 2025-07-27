using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;

namespace SampleGame;

/// <summary>
/// Demonstrates the asset type-specific path configuration feature.
/// This example shows how to organize assets in different folders for different types.
/// </summary>
public class AssetPathDemo
{
    public static void Run(string[] args)
    {
        Console.WriteLine("RACEngine Asset Path Configuration Demo");
        Console.WriteLine("=======================================");
        Console.WriteLine("This demo shows how to configure different base paths for different asset types.");
        Console.WriteLine();

        try
        {
            // Create engine with default configuration (same as other samples)
            var windowManager = new WindowManager();
            var inputService = new NullInputService();
            var configurationManager = new ConfigManager();
            var engine = new EngineFacade(windowManager, inputService, configurationManager);

            // Demonstrate default asset loading
            Console.WriteLine("1. Default asset loading (Assets folder):");
            Console.WriteLine($"   Default asset path: {Path.Combine(AppContext.BaseDirectory, "Assets")}");

            // Test if we can load from default path
            try
            {
                var texture = engine.LoadTexture("SampleTexture.png");
                Console.WriteLine($"   ✓ Texture loaded successfully: {texture.Width}x{texture.Height}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Texture loading failed: {ex.Message}");
            }

            Console.WriteLine();

            // Demonstrate type-specific path configuration
            Console.WriteLine("2. Type-specific path configuration:");

            // Configure audio to load from "Audio" folder
            Console.WriteLine("   Setting audio assets to load from 'Audio' folder...");
            engine.SetAudioBasePath("Audio");

            // Configure textures to load from "Textures" folder
            Console.WriteLine("   Setting texture assets to load from 'Textures' folder...");
            engine.SetTextureBasePath("Textures");

            // Configure text files to load from "Data" folder
            Console.WriteLine("   Setting text assets to load from 'Data' folder...");
            engine.SetTextBasePath("Data");

            Console.WriteLine();

            // Demonstrate fallback behavior
            Console.WriteLine("3. Fallback behavior:");
            Console.WriteLine("   When type-specific paths are set, the engine will:");
            Console.WriteLine("   - Load audio files from Audio/ folder");
            Console.WriteLine("   - Load texture files from Textures/ folder");
            Console.WriteLine("   - Load text files from Data/ folder");
            Console.WriteLine("   - Load any other asset types from the default Assets/ folder");

            Console.WriteLine();

            // Show the flexibility
            Console.WriteLine("4. Usage examples:");
            Console.WriteLine("   engine.LoadAudio(\"jump.wav\")        // Loads from Audio/jump.wav");
            Console.WriteLine("   engine.LoadTexture(\"player.png\")    // Loads from Textures/player.png");
            Console.WriteLine("   engine.LoadShaderSource(\"basic.vert\") // Loads from Data/basic.vert");

            Console.WriteLine();

            // Show configuration options
            Console.WriteLine("5. Configuration options:");
            Console.WriteLine("   - SetAssetBasePath(\"GameContent\")    // Changes default for all types");
            Console.WriteLine("   - SetAudioBasePath(\"Audio\")         // Audio-specific folder");
            Console.WriteLine("   - SetTextureBasePath(\"Textures\")    // Texture-specific folder");
            Console.WriteLine("   - SetTextBasePath(\"Data\")           // Text-specific folder");

            Console.WriteLine();

            // Show benefits
            Console.WriteLine("6. Benefits:");
            Console.WriteLine("   ✓ Organize assets by type for better project structure");
            Console.WriteLine("   ✓ Backward compatible - existing code continues to work");
            Console.WriteLine("   ✓ Extensible - new asset types automatically use default path");
            Console.WriteLine("   ✓ Flexible - can use absolute or relative paths");
            Console.WriteLine("   ✓ Runtime configurable - can change paths during execution");

            Console.WriteLine();
            Console.WriteLine("✓ Asset path configuration demo completed successfully!");
            Console.WriteLine("Note: This demo shows the API without requiring actual asset files.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Demo failed: {ex.Message}");
            throw;
        }
    }
}
