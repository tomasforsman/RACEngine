using System;

namespace SampleGame
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			// Determine which sample to run
			var sample = args.Length > 0
				? args[0].ToLowerInvariant()
				: PromptForSample();

			switch (sample)
			{
				case "shootersample":
					ShooterSample.Run(args);
					break;

				case "boidsample":
					BoidSample.Run(args);  // assume you add BoidSample.Run
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
			Console.WriteLine("  shootersample   - Stand‐in‐middle shooter demo");
			Console.WriteLine("  boidsample       - Fish/Boids swarm demo");
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
			Console.WriteLine("  dotnet run -- shootersample1");
			Console.WriteLine("  dotnet run -- boidsample");
		}
	}
}