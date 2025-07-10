namespace SampleGame;

public static class Program
{
    private static readonly Dictionary<string, (string description, Action<string[]> action)> Samples = new()
    {
        ["shootersample"] = ("Interactive shooter with shader mode switching (Normal/SoftGlow/Bloom)", ShooterSample.Run),
        ["boidsample"] = ("Flocking simulation with visual effects demonstration", BoidSample.Run),
        ["bloomtest"] = ("HDR bloom effect demonstration (Issue #51)", BloomTest.Run),
        ["camerademo"] = ("Interactive camera system demonstration with dual-camera rendering", CameraDemonstration.Run),
        ["pipelinedemo"] = ("Educational 4-phase rendering pipeline demonstration", RenderingPipelineDemo.Run),
        ["containersample"] = ("Container system demonstration with inventory and equipment patterns", ContainerSample.Run),
        ["assetdemo"] = ("Assets system demonstration", AssetDemo.Run)
    };

    public static void Main(string[] args)
    {
        string sample = args.Length > 0 ? args[0].ToLowerInvariant() : GetDefaultSample();

        if (Samples.TryGetValue(sample, out var sampleInfo))
        {
            sampleInfo.action(args);
        }
        else
        {
            Console.WriteLine($"Unknown sample: '{sample}'");
            ShowUsage();
        }
    }

    private static string GetDefaultSample()
    {
        try
        {
            if (Console.IsInputRedirected || !Environment.UserInteractive)
            {
                Console.WriteLine("Non-interactive environment detected, using default sample: boidsample");
                return "boidsample";
            }

            return PromptForSample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Console detection failed ({ex.Message}), using default sample: boidsample");
            return "boidsample";
        }
    }

    private static string PromptForSample()
    {
        Console.WriteLine("Available samples:");
        foreach (var (name, (description, _)) in Samples.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"  {name.PadRight(15)} - {description}");
        }

        Console.Write("Enter sample name (or press Enter for default 'boidsample'): ");

        try
        {
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No input detected, defaulting to 'boidsample'");
                return "boidsample";
            }

            return input.Trim().ToLowerInvariant();
        }
        catch (Exception ex)
        {
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
        foreach (var sampleName in Samples.Keys.OrderBy(x => x))
        {
            Console.WriteLine($"  dotnet run -- {sampleName}");
        }
        Console.WriteLine();
        Console.WriteLine("All samples demonstrate shader mode switching and engine features.");
    }
}
