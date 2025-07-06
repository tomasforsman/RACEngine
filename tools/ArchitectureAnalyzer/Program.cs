using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer;

/// <summary>
/// Main program for running the Progressive Complexity Metrics Analyzer.
/// Can be executed standalone or integrated into build processes.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("RACEngine Progressive Complexity Metrics Analyzer");
        Console.WriteLine("================================================");

        var projectRoot = args.Length > 0 ? args[0] : FindProjectRoot();
        var srcPath = Path.Combine(projectRoot, "src");

        if (!Directory.Exists(srcPath))
        {
            Console.WriteLine($"Error: Source directory not found at {srcPath}");
            Environment.Exit(1);
        }

        Console.WriteLine($"Analyzing RACEngine projects in: {srcPath}");

        try
        {
            var metrics = await AnalyzeProjectsAsync(srcPath);
            await GenerateReports(metrics, projectRoot);
            
            Console.WriteLine($"\nAnalysis complete! Overall health: {metrics.OverallHealth:F1}%");
            Console.WriteLine($"Analyzed {metrics.Modules.Count} modules");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during analysis: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Analyzes all Rac.* projects and calculates architecture metrics.
    /// </summary>
    private static async Task<ArchitectureMetrics> AnalyzeProjectsAsync(string srcPath)
    {
        var analyzer = new ProgressiveComplexityAnalyzer();
        var calculator = new MetricsCalculator();
        var allPatterns = new List<ServicePattern>();

        // Find all Rac.* project directories
        var projectDirs = Directory.GetDirectories(srcPath, "Rac.*")
            .Where(dir => File.Exists(Path.Combine(dir, Path.GetFileName(dir) + ".csproj")))
            .ToList();

        Console.WriteLine($"Found {projectDirs.Count} RACEngine projects:");
        foreach (var dir in projectDirs)
        {
            Console.WriteLine($"  - {Path.GetFileName(dir)}");
        }

        // Use a single detector across all projects to merge patterns
        var globalDetector = new ServicePatternDetector();

        // Analyze each project
        foreach (var projectDir in projectDirs)
        {
            await AnalyzeProjectAsync(projectDir, analyzer, globalDetector);
        }

        var patterns = globalDetector.GetDetectedPatterns();
        Console.WriteLine($"\nDetected {patterns.Count} service patterns:");
        foreach (var pattern in patterns)
        {
            Console.WriteLine($"  - {pattern.ModuleName}: {pattern.ServiceInterface} → {pattern.Implementation}");
        }

        return calculator.CalculateMetrics(patterns);
    }

    /// <summary>
    /// Analyzes a single project directory for service patterns.
    /// </summary>
    private static async Task AnalyzeProjectAsync(string projectDir, ProgressiveComplexityAnalyzer analyzer, ServicePatternDetector detector)
    {
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj"))
            .ToList();

        foreach (var csFile in csFiles)
        {
            try
            {
                var sourceCode = await File.ReadAllTextAsync(csFile);
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                
                // Create a basic compilation for semantic analysis
                var compilation = CSharpCompilation.Create("TempCompilation")
                    .AddSyntaxTrees(syntaxTree);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                // Use the global detector instead of creating patterns per file
                var context = new AnalysisContextWrapper(semanticModel);
                var root = syntaxTree.GetRoot();

                // Analyze interfaces and classes
                foreach (var node in root.DescendantNodes())
                {
                    if (node is InterfaceDeclarationSyntax interfaceDecl)
                    {
                        detector.AnalyzeInterface(context, interfaceDecl);
                    }
                    else if (node is ClassDeclarationSyntax classDecl)
                    {
                        detector.AnalyzeClass(context, classDecl);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not analyze {csFile}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generates JSON and Markdown reports from architecture metrics.
    /// </summary>
    private static async Task GenerateReports(ArchitectureMetrics metrics, string projectRoot)
    {
        var artifactsDir = Path.Combine(projectRoot, "tools", "artifacts", "architecture-metrics");
        Directory.CreateDirectory(artifactsDir);

        // Generate JSON report
        var jsonPath = Path.Combine(artifactsDir, $"metrics-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.json");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(metrics, jsonOptions);
        await File.WriteAllTextAsync(jsonPath, json);
        Console.WriteLine($"JSON metrics saved to: {jsonPath}");

        // Generate Markdown report
        var docGenerator = new DocumentationGenerator();
        var markdown = docGenerator.GenerateMarkdownReport(metrics);
        var mdPath = Path.Combine(projectRoot, "docs", "architecture", "progressive-complexity-report.md");
        Directory.CreateDirectory(Path.GetDirectoryName(mdPath)!);
        await File.WriteAllTextAsync(mdPath, markdown);
        Console.WriteLine($"Markdown report saved to: {mdPath}");
    }

    /// <summary>
    /// Finds the project root directory by looking for the .sln file.
    /// </summary>
    private static string FindProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir, "RACEngine.sln")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}

/// <summary>
/// Mock implementation for standalone analysis.
/// Since SyntaxNodeAnalysisContext is sealed, we'll use a simpler approach.
/// </summary>
public class AnalysisContextWrapper
{
    public SemanticModel SemanticModel { get; }
    
    public AnalysisContextWrapper(SemanticModel semanticModel)
    {
        SemanticModel = semanticModel;
    }
}