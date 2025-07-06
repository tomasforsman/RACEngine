using System.Text;

namespace ArchitectureAnalyzer;

/// <summary>
/// Generates human-readable documentation from architecture metrics.
/// Creates Markdown reports suitable for documentation and GitHub display.
/// </summary>
public class DocumentationGenerator
{
    /// <summary>
    /// Generates a comprehensive Markdown report from architecture metrics.
    /// </summary>
    public string GenerateMarkdownReport(ArchitectureMetrics metrics)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("# Progressive Complexity Health Report");
        sb.AppendLine();
        sb.AppendLine($"*Last updated: {metrics.Timestamp:MMMM d, yyyy 'at' HH:mm} UTC*");
        sb.AppendLine();

        // Overall health summary
        var healthIcon = GetHealthIcon(metrics.OverallHealth);
        sb.AppendLine($"## Overall Architecture Health: {metrics.OverallHealth:F0}% {healthIcon}");
        sb.AppendLine();

        // Health explanation
        AppendHealthExplanation(sb, metrics.OverallHealth);
        sb.AppendLine();

        // Module breakdown
        sb.AppendLine("## Module Breakdown");
        sb.AppendLine();

        var modulesByHealth = metrics.Modules
            .OrderByDescending(m => m.FacadeCoverage)
            .ThenByDescending(m => m.ServiceCoverage);

        foreach (var module in modulesByHealth)
        {
            AppendModuleSection(sb, module);
        }

        // Summary recommendations
        AppendGlobalRecommendations(sb, metrics);

        return sb.ToString();
    }

    /// <summary>
    /// Appends a detailed section for a specific module.
    /// </summary>
    private static void AppendModuleSection(StringBuilder sb, ModuleMetrics module)
    {
        var healthIcon = GetModuleHealthIcon(module.Health);
        var facadeIcon = GetCoverageIcon(module.FacadeCoverage);
        var serviceIcon = GetCoverageIcon(module.ServiceCoverage);

        sb.AppendLine($"### {GetModuleEmoji(module.Name)} {module.Name} System - {module.FacadeCoverage:F0}% {healthIcon} {module.Health.ToTitleCase()}");
        sb.AppendLine();

        // Coverage metrics
        sb.AppendLine("**Coverage Metrics:**");
        sb.AppendLine($"- **Facade Coverage**: {module.FacadeCoverage:F0}% {facadeIcon} ({GetCoverageDescription(module.FacadeCoverage)})");
        sb.AppendLine($"- **Service Coverage**: {module.ServiceCoverage:F0}% {serviceIcon} ({GetCoverageDescription(module.ServiceCoverage)})");
        sb.AppendLine();

        // Architecture details
        sb.AppendLine("**Architecture Details:**");
        sb.AppendLine($"- **Service Interface**: `{module.ServiceInterface}`");
        sb.AppendLine($"- **Implementation**: `{module.Implementation}`");
        sb.AppendLine($"- **Facade Access**: {module.Facade}");
        sb.AppendLine();

        // Opportunities
        if (module.Opportunities.Count > 0)
        {
            sb.AppendLine("**Improvement Opportunities:**");
            foreach (var opportunity in module.Opportunities)
            {
                sb.AppendLine($"- {opportunity}");
            }
            sb.AppendLine();
        }

        // Priority assessment
        AppendPriorityAssessment(sb, module);
        sb.AppendLine();
    }

    /// <summary>
    /// Appends a priority assessment for the module.
    /// </summary>
    private static void AppendPriorityAssessment(StringBuilder sb, ModuleMetrics module)
    {
        var priority = DeterminePriority(module);
        var priorityIcon = priority switch
        {
            "High" => "🔥",
            "Medium" => "⚠️",
            "Low" => "✅",
            _ => "ℹ️"
        };

        sb.AppendLine($"**Priority**: {priorityIcon} {priority} - {GetPriorityReason(module, priority)}");
    }

    /// <summary>
    /// Appends global recommendations based on overall metrics.
    /// </summary>
    private static void AppendGlobalRecommendations(StringBuilder sb, ArchitectureMetrics metrics)
    {
        sb.AppendLine("## Architecture Recommendations");
        sb.AppendLine();

        var criticalModules = metrics.Modules.Where(m => m.FacadeCoverage < 30).ToList();
        var goodModules = metrics.Modules.Where(m => m.FacadeCoverage >= 70).ToList();

        if (criticalModules.Count > 0)
        {
            sb.AppendLine("### 🚨 Critical Actions Needed");
            sb.AppendLine();
            foreach (var module in criticalModules)
            {
                sb.AppendLine($"- **{module.Name}**: Create facade layer for beginner accessibility");
            }
            sb.AppendLine();
        }

        if (goodModules.Count > 0)
        {
            sb.AppendLine("### ✅ Well-Architected Systems");
            sb.AppendLine();
            sb.AppendLine("These systems demonstrate good Progressive Complexity patterns:");
            foreach (var module in goodModules)
            {
                sb.AppendLine($"- **{module.Name}**: Excellent facade coverage ({module.FacadeCoverage:F0}%)");
            }
            sb.AppendLine();
        }

        // Educational impact
        sb.AppendLine("### 📚 Educational Impact");
        sb.AppendLine();
        var learnerFriendlyPercent = (double)goodModules.Count / metrics.Modules.Count * 100;
        sb.AppendLine($"Currently **{learnerFriendlyPercent:F0}%** of RACEngine systems are beginner-friendly.");
        sb.AppendLine();
        
        if (learnerFriendlyPercent < 70)
        {
            sb.AppendLine("**Recommendation**: Focus on facade layer development to improve learning curve.");
            sb.AppendLine("Beginner developers should be able to accomplish common tasks through simple facade methods.");
        }
        else
        {
            sb.AppendLine("**Status**: Good progress on educational accessibility! Continue maintaining facade coverage.");
        }
    }

    /// <summary>
    /// Appends explanation of current health status.
    /// </summary>
    private static void AppendHealthExplanation(StringBuilder sb, double overallHealth)
    {
        string explanation = overallHealth switch
        {
            >= 80 => "**Excellent**: RACEngine demonstrates strong Progressive Complexity patterns with comprehensive facade coverage.",
            >= 60 => "**Good**: Most systems follow Progressive Complexity principles with room for facade improvements.",
            >= 40 => "**Needs Improvement**: Several systems lack beginner-friendly facade layers.",
            _ => "**Critical**: Significant facade coverage gaps impact educational accessibility."
        };

        sb.AppendLine(explanation);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS FOR FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    private static string GetHealthIcon(double health) => health switch
    {
        >= 80 => "🎯",
        >= 60 => "✅", 
        >= 40 => "⚠️",
        _ => "❌"
    };

    private static string GetModuleHealthIcon(string health) => health switch
    {
        "excellent" => "🎯",
        "good" => "✅",
        "needs-improvement" => "⚠️",
        "critical" => "❌",
        _ => "ℹ️"
    };

    private static string GetCoverageIcon(double coverage) => coverage switch
    {
        >= 70 => "🟢",
        >= 50 => "🟡",
        >= 30 => "🟠", 
        _ => "🔴"
    };

    private static string GetModuleEmoji(string moduleName) => moduleName switch
    {
        "Audio" => "🎵",
        "Rendering" => "🎨",
        "ECS" or "Container" => "🎮",
        "Input" => "⌨️",
        "Physics" => "⚡",
        "Core" => "🔧",
        "Engine" => "🚂",
        _ => "📦"
    };

    private static string GetCoverageDescription(double coverage) => coverage switch
    {
        >= 70 => "excellent coverage",
        >= 50 => "good coverage",
        >= 30 => "needs improvement",
        _ => "critical gaps"
    };

    private static string DeterminePriority(ModuleMetrics module)
    {
        if (module.FacadeCoverage < 30) return "High";
        if (module.FacadeCoverage < 50) return "Medium";
        return "Low";
    }

    private static string GetPriorityReason(ModuleMetrics module, string priority) => priority switch
    {
        "High" => "Critical facade coverage gap impacts beginner accessibility",
        "Medium" => "Moderate improvements needed for better discoverability",
        "Low" => "System shows good Progressive Complexity patterns",
        _ => "Monitoring recommended"
    };
}

/// <summary>
/// String extension methods for formatting.
/// </summary>
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var words = input.Split('-', '_', ' ');
        return string.Join(" ", words.Select(word => 
            char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
}