using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchitectureAnalyzer;

/// <summary>
/// Standalone analyzer that provides informational insights into RACEngine's Progressive Complexity architecture patterns.
/// Analyzes service patterns and generates reports about facade layer coverage and opportunities.
/// </summary>
public class ProgressiveComplexityAnalyzer
{
    /// <summary>
    /// Analyzes C# syntax trees and returns detected service patterns.
    /// </summary>
    public List<ServicePattern> AnalyzeSyntaxTree(SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        var detector = new ServicePatternDetector();
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

        return detector.GetDetectedPatterns().ToList();
    }

    /// <summary>
    /// Generates architecture insights based on detected patterns.
    /// </summary>
    public List<ArchitectureInsight> GenerateInsights(ArchitectureMetrics metrics)
    {
        var insights = new List<ArchitectureInsight>();

        foreach (var module in metrics.Modules)
        {
            // Report opportunities for systems with good service coverage but low facade coverage
            if (module.ServiceCoverage > 70 && module.FacadeCoverage < 60)
            {
                insights.Add(new ArchitectureInsight
                {
                    Type = "PCM001",
                    Level = "Info",
                    Title = "Architecture opportunity: System could expose more facade methods",
                    Message = $"Architecture opportunity: {module.Name} system could expose more facade methods (current: {module.FacadeCoverage:F0}%)",
                    Module = module.Name
                });
            }

            // Report facade gaps for systems with very low facade coverage
            if (module.FacadeCoverage < 30)
            {
                insights.Add(new ArchitectureInsight
                {
                    Type = "PCM002",
                    Level = "Info", 
                    Title = "Facade gap: System has no beginner-friendly layer",
                    Message = $"Facade gap: {module.Name} system has no beginner-friendly layer ({module.FacadeCoverage:F0}% coverage)",
                    Module = module.Name
                });
            }
        }

        // Report overall health summary
        var healthSummary = GenerateHealthSummary(metrics.Modules);
        insights.Add(new ArchitectureInsight
        {
            Type = "PCM999",
            Level = "Hidden",
            Title = "Progressive Complexity Health Report",
            Message = $"Health Report: {healthSummary}",
            Module = "Overall"
        });

        return insights;
    }

    private static string GenerateHealthSummary(IEnumerable<ModuleMetrics> modules)
    {
        var summaryParts = modules.Select(m =>
        {
            var healthIcon = m.FacadeCoverage >= 70 ? "✅" : m.FacadeCoverage >= 40 ? "⚠️" : "❌";
            return $"{m.Name}={m.ServiceCoverage:F0}%/{m.FacadeCoverage:F0}% {healthIcon}";
        });

        return string.Join(" | ", summaryParts);
    }
}

/// <summary>
/// Represents an architecture insight or recommendation.
/// </summary>
public class ArchitectureInsight
{
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}