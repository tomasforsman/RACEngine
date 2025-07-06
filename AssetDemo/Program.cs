using Rac.Assets;
using Rac.Assets.FileSystem;

namespace AssetDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("RACEngine Asset System Demo");
        Console.WriteLine("===========================");

        try
        {
            // Test Layer 1: Static facade (beginner usage)
            Console.WriteLine("\n1. Testing Static Facade (Layer 1 - Beginner):");
            
            try
            {
                var vertexShader = Assets.LoadShaderSource("basic.vert");
                Console.WriteLine($"✓ Loaded vertex shader ({vertexShader.Length} characters)");
                Console.WriteLine($"  First line: {vertexShader.Split('\n')[0]}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"✗ Vertex shader not found: {ex.Message}");
            }

            try
            {
                var fragmentShader = Assets.LoadShaderSource("basic.frag");
                Console.WriteLine($"✓ Loaded fragment shader ({fragmentShader.Length} characters)");
                Console.WriteLine($"  First line: {fragmentShader.Split('\n')[0]}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"✗ Fragment shader not found: {ex.Message}");
            }

            // Test Layer 2: Service interface (advanced usage)
            Console.WriteLine("\n2. Testing Service Interface (Layer 2 - Advanced):");
            
            var assetService = AssetServiceBuilder.Create()
                .WithBasePath("test_assets")
                .Build();

            Console.WriteLine($"✓ Created asset service with base path: test_assets");
            Console.WriteLine($"✓ Cache status: {assetService.CachedAssetCount} assets, {assetService.CacheMemoryUsage} bytes");

            // Test caching behavior
            Console.WriteLine("\n3. Testing Caching Behavior:");
            
            var shader1 = assetService.LoadAsset<string>("basic.vert");
            Console.WriteLine($"✓ First load: {assetService.CachedAssetCount} assets cached");
            
            var shader2 = assetService.LoadAsset<string>("basic.vert");
            Console.WriteLine($"✓ Second load (cached): {assetService.CachedAssetCount} assets cached");
            Console.WriteLine($"✓ Same instance: {ReferenceEquals(shader1, shader2)}");

            // Test async loading
            Console.WriteLine("\n4. Testing Async Loading:");
            
            var asyncShader = await assetService.LoadAssetAsync<string>("basic.frag");
            Console.WriteLine($"✓ Async loaded fragment shader ({asyncShader.Length} characters)");

            // Test error handling
            Console.WriteLine("\n5. Testing Error Handling:");
            
            if (assetService.TryLoadAsset<string>("nonexistent.txt", out var missing))
            {
                Console.WriteLine("✗ Should not have loaded nonexistent file");
            }
            else
            {
                Console.WriteLine("✓ Correctly failed to load nonexistent file");
            }

            // Display migration guidance
            Console.WriteLine("\n6. Migration Guidance:");
            Console.WriteLine(Assets.MigrationGuidance);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\nDemo completed. Press any key to exit...");
        Console.ReadKey();
    }
}