namespace SampleGame;

public static class Program
{
    public static void Main(string[] args)
    {
        // Determine which sample to run
        string sample = args.Length > 0 ? args[0].ToLowerInvariant() : PromptForSample();

        switch (sample)
        {
            case "shootersample":
                ShooterSample.Run(args);
                break;

            case "boidsample":
                BoidSample.Run(args); // assume you add BoidSample.Run
                break;

            // ─── add more samples here ────────────────────
            // case "othersample": OtherSample.Run(args); break;

            default:
                Console.WriteLine($"Unknown sample: '{sample}'");
                ShowUsage();
                break;
        }
    }

    private static string PromptForSample()
    {
        Console.WriteLine("Available samples:");
        Console.WriteLine("  shootersample   - Interactive shooter with shader mode switching (Normal/SoftGlow/Bloom)");
        Console.WriteLine("  boidsample      - Flocking simulation with visual effects demonstration");
        // … list additional samples here …
        Console.Write("Enter sample name: ");
        return Console.ReadLine()!.Trim().ToLowerInvariant();
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
        Console.WriteLine();
        Console.WriteLine("Both samples demonstrate shader mode switching and engine features.");
    }
}
