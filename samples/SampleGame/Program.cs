namespace SampleGame;

public static class Program
{
    public static void Main(string[] args)
    {
        // Determine which sample to run
        string sample = args.Length > 0 ? args[0].ToLowerInvariant() : GetDefaultSample();

        switch (sample)
        {
            case "shootersample":
                ShooterSample.Run(args);
                break;

            case "boidsample":
                BoidSample.Run(args); // assume you add BoidSample.Run
                break;

            case "bloomtest":
                BloomTest.Run(args); // HDR bloom demonstration test
                break;

            case "camerademo":
                CameraDemonstration.Run(args); // Camera system demonstration
                break;
                
            case "pipelinedemo":
                RenderingPipelineDemo.Run(args); // 4-phase rendering pipeline demonstration
                break;
                
            case "containersample":
                ContainerSample.Run(args); // Container system demonstration
                break;

            // ─── add more samples here ────────────────────
            // case "othersample": OtherSample.Run(args); break;

            default:
                Console.WriteLine($"Unknown sample: '{sample}'");
                ShowUsage();
                break;
        }
    }

    private static string GetDefaultSample()
    {
        // Check if we're in an interactive console environment
        try
        {
            // Test if console input is available without actually reading
            if (Console.IsInputRedirected || !Environment.UserInteractive)
            {
                // Non-interactive environment, use default
                Console.WriteLine("Non-interactive environment detected, using default sample: boidsample");
                return "boidsample";
            }
            
            // Interactive environment, show prompt
            return PromptForSample();
        }
        catch (Exception ex)
        {
            // If any console detection fails, use default to avoid Windows bell sounds
            Console.WriteLine($"Console detection failed ({ex.Message}), using default sample: boidsample");
            return "boidsample";
        }
    }

    private static string PromptForSample()
    {
        Console.WriteLine("Available samples:");
        Console.WriteLine("  shootersample   - Interactive shooter with shader mode switching (Normal/SoftGlow/Bloom)");
        Console.WriteLine("  boidsample      - Flocking simulation with visual effects demonstration");
        Console.WriteLine("  bloomtest       - HDR bloom effect demonstration (Issue #51)");
        Console.WriteLine("  camerademo      - Interactive camera system demonstration with dual-camera rendering");
        Console.WriteLine("  pipelinedemo    - Educational 4-phase rendering pipeline demonstration");
        Console.WriteLine("  containersample - Container system demonstration with inventory and equipment patterns");
        // … list additional samples here …
        Console.Write("Enter sample name (or press Enter for default 'boidsample'): ");
        
        try
        {
            string? input = Console.ReadLine();
            
            // Handle null input or empty input gracefully
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No input detected, defaulting to 'boidsample'");
                return "boidsample";
            }
            
            return input.Trim().ToLowerInvariant();
        }
        catch (Exception ex)
        {
            // Handle console input errors that might cause Windows bell sounds
            Console.WriteLine($"Console input error: {ex.Message}");
            Console.WriteLine("Defaulting to 'boidsample'");
            return "boidsample";
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- <sampleName>");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- shootersample");
        Console.WriteLine("  dotnet run -- boidsample");
        Console.WriteLine("  dotnet run -- bloomtest");
        Console.WriteLine("  dotnet run -- camerademo");
        Console.WriteLine("  dotnet run -- pipelinedemo");
        Console.WriteLine("  dotnet run -- containersample");
        Console.WriteLine();
        Console.WriteLine("All samples demonstrate shader mode switching and engine features.");
        Console.WriteLine("The bloomtest specifically demonstrates HDR color bloom effects.");
        Console.WriteLine("The pipelinedemo provides educational insight into the 4-phase rendering architecture.");
        Console.WriteLine("The containersample showcases the ECS Container System with inventory and equipment patterns.");
    }
}
