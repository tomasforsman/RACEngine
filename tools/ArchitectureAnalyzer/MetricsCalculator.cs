namespace ArchitectureAnalyzer;

/// <summary>
/// Calculates Progressive Complexity metrics for RACEngine service patterns.
/// Computes coverage percentages between Implementation → Service → Facade layers.
/// </summary>
public class MetricsCalculator
{
    /// <summary>
    /// Calculates architecture metrics for all detected service patterns.
    /// </summary>
    /// <param name="patterns">Detected service patterns from code analysis</param>
    /// <returns>Complete architecture metrics with per-module coverage</returns>
    public ArchitectureMetrics CalculateMetrics(IReadOnlyList<ServicePattern> patterns)
    {
        var modules = patterns.Select(CalculateModuleMetrics).ToList();
        var overallHealth = CalculateOverallHealth(modules);

        return new ArchitectureMetrics
        {
            Timestamp = DateTime.UtcNow,
            OverallHealth = overallHealth,
            Modules = modules
        };
    }

    /// <summary>
    /// Calculates metrics for a single module service pattern.
    /// </summary>
    private ModuleMetrics CalculateModuleMetrics(ServicePattern pattern)
    {
        var serviceCoverage = CalculateServiceCoverage(pattern);
        var facadeCoverage = CalculateFacadeCoverage(pattern);
        var health = DetermineHealthStatus(serviceCoverage, facadeCoverage);
        var opportunities = GenerateOpportunities(pattern, serviceCoverage, facadeCoverage);

        return new ModuleMetrics
        {
            Name = pattern.ModuleName,
            ServiceInterface = pattern.ServiceInterface,
            Implementation = pattern.Implementation,
            Facade = DetermineFacadeDescription(pattern),
            ServiceCoverage = serviceCoverage,
            FacadeCoverage = facadeCoverage,
            Health = health,
            Opportunities = opportunities
        };
    }

    /// <summary>
    /// Calculates Implementation → Service coverage percentage.
    /// Measures how well the service interface exposes implementation functionality.
    /// </summary>
    private static double CalculateServiceCoverage(ServicePattern pattern)
    {
        if (pattern.ImplementationMethods.Count == 0)
            return 0.0;

        // Count how many implementation methods are exposed through the service interface
        var exposedMethods = pattern.ImplementationMethods
            .Count(implMethod => pattern.ServiceMethods.Any(serviceMethod => 
                string.Equals(implMethod, serviceMethod, StringComparison.OrdinalIgnoreCase) ||
                IsMethodEquivalent(implMethod, serviceMethod)));

        return (double)exposedMethods / pattern.ImplementationMethods.Count * 100.0;
    }

    /// <summary>
    /// Calculates Service → Facade coverage percentage.
    /// Measures how well facade methods expose basic service functionality for beginners.
    /// </summary>
    private static double CalculateFacadeCoverage(ServicePattern pattern)
    {
        if (pattern.BasicServiceMethods.Count == 0)
            return 0.0;

        // Count how many basic service methods have facade equivalents
        var facadeExposedMethods = pattern.BasicServiceMethods
            .Count(basicMethod => pattern.FacadeMethods.Any(facadeMethod =>
                string.Equals(basicMethod, facadeMethod, StringComparison.OrdinalIgnoreCase) ||
                IsMethodEquivalent(basicMethod, facadeMethod) ||
                IsFacadeMethodRelated(basicMethod, facadeMethod)));

        return (double)facadeExposedMethods / pattern.BasicServiceMethods.Count * 100.0;
    }

    /// <summary>
    /// Determines if two method names are functionally equivalent.
    /// Handles naming variations like Play vs PlaySound.
    /// </summary>
    private static bool IsMethodEquivalent(string method1, string method2)
    {
        // Remove common suffixes/prefixes to find core functionality
        var core1 = ExtractMethodCore(method1);
        var core2 = ExtractMethodCore(method2);
        
        return string.Equals(core1, core2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a facade method is related to a basic service method.
    /// Checks for semantic relationships beyond exact naming.
    /// </summary>
    private static bool IsFacadeMethodRelated(string basicMethod, string facadeMethod)
    {
        // Map common facade patterns to service methods
        var relationships = new Dictionary<string, string[]>
        {
            ["CreateEntity"] = new[] { "Create", "CreateEntity", "Add" },
            ["DestroyEntity"] = new[] { "Destroy", "Remove", "Delete" },
            ["PlaySound"] = new[] { "Play", "PlaySound", "PlayAudio" },
            ["SetVolume"] = new[] { "Set", "SetVolume", "SetMasterVolume" }
        };

        foreach (var (facade, related) in relationships)
        {
            if (string.Equals(facadeMethod, facade, StringComparison.OrdinalIgnoreCase))
            {
                return related.Any(r => basicMethod.Contains(r, StringComparison.OrdinalIgnoreCase));
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts the core functionality from a method name by removing common prefixes/suffixes.
    /// </summary>
    private static string ExtractMethodCore(string methodName)
    {
        var prefixesToRemove = new[] { "Get", "Set", "Is", "Has", "Can" };
        var suffixesToRemove = new[] { "Async", "Service", "Method", "Function" };

        var core = methodName;
        
        foreach (var prefix in prefixesToRemove)
        {
            if (core.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                core = core.Substring(prefix.Length);
                break;
            }
        }

        foreach (var suffix in suffixesToRemove)
        {
            if (core.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                core = core.Substring(0, core.Length - suffix.Length);
                break;
            }
        }

        return core;
    }

    /// <summary>
    /// Calculates overall architecture health across all modules.
    /// </summary>
    private static double CalculateOverallHealth(List<ModuleMetrics> modules)
    {
        if (modules.Count == 0)
            return 0.0;

        // Weight facade coverage more heavily as it's the primary indicator of beginner-friendliness
        var weightedScores = modules.Select(m => (m.ServiceCoverage * 0.3) + (m.FacadeCoverage * 0.7));
        return weightedScores.Average();
    }

    /// <summary>
    /// Determines health status based on coverage metrics.
    /// </summary>
    private static string DetermineHealthStatus(double serviceCoverage, double facadeCoverage)
    {
        // Priority on facade coverage for beginner experience
        return facadeCoverage switch
        {
            >= 70 => "excellent",
            >= 50 => "good", 
            >= 30 => "needs-improvement",
            _ => "critical"
        };
    }

    /// <summary>
    /// Generates specific improvement opportunities for a module.
    /// </summary>
    private static List<string> GenerateOpportunities(ServicePattern pattern, double serviceCoverage, double facadeCoverage)
    {
        var opportunities = new List<string>();

        if (facadeCoverage < 50)
        {
            opportunities.Add($"Add facade methods for common {pattern.ModuleName.ToLower()} operations");
            
            // Suggest specific missing facade methods
            var missingFacadeMethods = pattern.BasicServiceMethods
                .Where(basic => !pattern.FacadeMethods.Any(facade => 
                    IsMethodEquivalent(basic, facade) || IsFacadeMethodRelated(basic, facade)))
                .Take(3); // Limit to top 3 suggestions

            foreach (var missing in missingFacadeMethods)
            {
                opportunities.Add($"Consider adding {missing}() convenience method");
            }
        }

        if (serviceCoverage < 70 && pattern.ImplementationMethods.Count > pattern.ServiceMethods.Count)
        {
            opportunities.Add($"Expose more {pattern.ModuleName.ToLower()} implementation methods through service interface");
        }

        if (string.IsNullOrEmpty(pattern.ServiceInterface))
        {
            opportunities.Add($"Create I{pattern.ModuleName}Service interface for dependency injection support");
        }

        return opportunities;
    }

    /// <summary>
    /// Determines facade description based on detected facade patterns.
    /// </summary>
    private static string DetermineFacadeDescription(ServicePattern pattern)
    {
        if (pattern.FacadeMethods.Count > 0)
        {
            return $"EngineFacade.{pattern.ModuleName}.*";
        }

        return "No facade layer detected";
    }
}

/// <summary>
/// Complete architecture metrics for all analyzed modules.
/// </summary>
public class ArchitectureMetrics
{
    public DateTime Timestamp { get; set; }
    public double OverallHealth { get; set; }
    public List<ModuleMetrics> Modules { get; set; } = new();
}

/// <summary>
/// Architecture metrics for a specific module.
/// </summary>
public class ModuleMetrics
{
    public string Name { get; set; } = string.Empty;
    public string ServiceInterface { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
    public string Facade { get; set; } = string.Empty;
    public double ServiceCoverage { get; set; }
    public double FacadeCoverage { get; set; }
    public string Health { get; set; } = string.Empty;
    public List<string> Opportunities { get; set; } = new();
}