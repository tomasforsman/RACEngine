using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace ArchitectureAnalyzer;

/// <summary>
/// Detects RACEngine service patterns in C# code including interfaces, implementations, and facade methods.
/// Follows the Progressive Complexity principle by identifying the three access layers.
/// </summary>
public class ServicePatternDetector
{
    private readonly List<ServicePattern> _detectedPatterns = new();

    /// <summary>
    /// Analyzes an interface declaration to detect service interfaces.
    /// Service interfaces follow the naming pattern I{ModuleName}Service.
    /// </summary>
    public void AnalyzeInterface(AnalysisContextWrapper context, InterfaceDeclarationSyntax interfaceDecl)
    {
        var interfaceName = interfaceDecl.Identifier.ValueText;
        
        // Check if this is a service interface (I{ModuleName}Service pattern)
        if (IsServiceInterface(interfaceName))
        {
            var moduleName = ExtractModuleName(interfaceName);
            var methods = GetPublicMembers(interfaceDecl);
            
            var pattern = GetOrCreatePattern(moduleName);
            pattern.ServiceInterface = interfaceName;
            pattern.ServiceMethods.AddRange(methods);
            pattern.BasicServiceMethods.AddRange(methods.Where(IsBasicMethod));
        }
    }

    /// <summary>
    /// Analyzes a class declaration to detect service implementations and facade classes.
    /// </summary>
    public void AnalyzeClass(AnalysisContextWrapper context, ClassDeclarationSyntax classDecl)
    {
        var className = classDecl.Identifier.ValueText;
        
        // Check if this is a service implementation
        var implementedInterfaces = GetImplementedInterfaces(classDecl, context.SemanticModel);
        
        foreach (var interfaceName in implementedInterfaces)
        {
            if (IsServiceInterface(interfaceName))
            {
                var moduleName = ExtractModuleName(interfaceName);
                var pattern = GetOrCreatePattern(moduleName);
                
                if (IsNullImplementation(className))
                {
                    pattern.NullImplementation = className;
                }
                else
                {
                    pattern.Implementation = className;
                    var methods = GetPublicMembers(classDecl);
                    pattern.ImplementationMethods.AddRange(methods);
                }
            }
        }

        // Check if this is a facade class (EngineFacade or {Module}Facade)
        if (IsFacadeClass(className))
        {
            var methods = GetPublicMembers(classDecl);
            
            // For EngineFacade, distribute methods to appropriate modules based on context
            if (className == "EngineFacade" || className == "ModularEngineFacade")
            {
                DistributeFacadeMethods(methods, classDecl, context.SemanticModel);
            }
            else
            {
                // Direct module facade
                var moduleName = ExtractModuleNameFromFacade(className);
                var pattern = GetOrCreatePattern(moduleName);
                pattern.FacadeMethods.AddRange(methods);
            }
        }
    }

    /// <summary>Gets all detected service patterns for metrics calculation.</summary>
    public IReadOnlyList<ServicePattern> GetDetectedPatterns() => _detectedPatterns.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════════
    // PATTERN DETECTION HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private static bool IsServiceInterface(string interfaceName)
    {
        return interfaceName.StartsWith("I") && interfaceName.EndsWith("Service");
    }

    private static bool IsNullImplementation(string className)
    {
        return className.StartsWith("Null") && className.EndsWith("Service");
    }

    private static bool IsFacadeClass(string className)
    {
        return className.EndsWith("Facade") || 
               className == "EngineFacade" || 
               className == "ModularEngineFacade";
    }

    private static string ExtractModuleName(string interfaceName)
    {
        // I{Module}Service -> {Module}
        if (interfaceName.StartsWith("I") && interfaceName.EndsWith("Service"))
        {
            return interfaceName.Substring(1, interfaceName.Length - 8); // Remove "I" and "Service"
        }
        return interfaceName;
    }

    private static string ExtractModuleNameFromFacade(string className)
    {
        // {Module}Facade -> {Module}
        if (className.EndsWith("Facade"))
        {
            return className.Substring(0, className.Length - 6); // Remove "Facade"
        }
        return className;
    }

    private static bool IsBasicMethod(string methodName)
    {
        // Basic methods are simple, common operations suitable for facade exposure
        // Exclude advanced methods with many parameters or complex names
        var basicPrefixes = new[] { "Play", "Stop", "Set", "Get", "Create", "Destroy", "Add", "Remove", "Load", "Save" };
        var advancedKeywords = new[] { "3D", "Advanced", "Complex", "Detailed", "Internal" };
        
        // Input events are considered basic facade operations
        var inputEvents = new[] { "KeyEvent", "LeftClickEvent", "MouseScrollEvent", "OnLeftClick", "OnMouseScroll", "PressedKey", "OnKeyEvent" };

        var isBasicPrefix = basicPrefixes.Any(prefix => methodName.StartsWith(prefix));
        var hasAdvancedKeyword = advancedKeywords.Any(keyword => methodName.Contains(keyword));
        var isInputEvent = inputEvents.Contains(methodName);

        return (isBasicPrefix && !hasAdvancedKeyword) || isInputEvent;
    }

    private static List<string> GetPublicMembers(TypeDeclarationSyntax typeDecl)
    {
        var members = new List<string>();
        
        // Get public methods
        var methods = typeDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword) ||
                       (typeDecl is InterfaceDeclarationSyntax)) // Interface methods are implicitly public
            .Select(m => m.Identifier.ValueText);
        
        members.AddRange(methods);
        
        // Get public events (important for facade pattern detection)
        var events = typeDecl.Members
            .OfType<EventDeclarationSyntax>()
            .Where(e => e.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword) ||
                       (typeDecl is InterfaceDeclarationSyntax)) // Interface events are implicitly public
            .Select(e => e.Identifier.ValueText);
        
        // Also check for event field declarations (like "public event Action<> EventName;")
        var eventFields = typeDecl.Members
            .OfType<EventFieldDeclarationSyntax>()
            .Where(e => e.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword) ||
                       (typeDecl is InterfaceDeclarationSyntax))
            .SelectMany(e => e.Declaration.Variables.Select(v => v.Identifier.ValueText));
        
        members.AddRange(events);
        members.AddRange(eventFields);
        
        // Get public properties (for comprehensive facade coverage)
        var properties = typeDecl.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword) ||
                       (typeDecl is InterfaceDeclarationSyntax)) // Interface properties are implicitly public
            .Select(p => p.Identifier.ValueText);
        
        members.AddRange(properties);
        
        return members;
    }

    private static List<string> GetImplementedInterfaces(ClassDeclarationSyntax classDecl, SemanticModel? semanticModel)
    {
        var interfaces = new List<string>();
        
        if (classDecl.BaseList != null)
        {
            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                
                // Simple heuristic: if it starts with I and looks like an interface, assume it is
                if (typeName.StartsWith("I") && char.IsUpper(typeName[1]))
                {
                    interfaces.Add(typeName);
                }
                
                // Try semantic model if available
                if (semanticModel != null)
                {
                    try
                    {
                        var symbolInfo = semanticModel.GetSymbolInfo(baseType.Type);
                        if (symbolInfo.Symbol is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Interface)
                        {
                            interfaces.Add(namedType.Name);
                        }
                    }
                    catch
                    {
                        // Ignore semantic model errors, fall back to simple analysis
                    }
                }
            }
        }

        return interfaces.Distinct().ToList();
    }

    private void DistributeFacadeMethods(List<string> methods, ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        // Analyze method implementations to determine which module they belong to
        // This is a simplified approach - in reality, we'd need more sophisticated analysis
        
        foreach (var methodName in methods)
        {
            var moduleName = InferModuleFromMethodName(methodName);
            if (!string.IsNullOrEmpty(moduleName))
            {
                var pattern = GetOrCreatePattern(moduleName);
                pattern.FacadeMethods.Add(methodName);
            }
        }
    }

    private static string InferModuleFromMethodName(string methodName)
    {
        // Simple heuristic to map facade method names to modules
        if (methodName.Contains("Entity") || methodName.Contains("Component"))
            return "ECS";
        if (methodName.Contains("Sound") || methodName.Contains("Audio") || methodName.Contains("Music"))
            return "Audio";
        if (methodName.Contains("Render") || methodName.Contains("Draw") || methodName.Contains("Color"))
            return "Rendering";
        if (methodName.Contains("Input") || methodName.Contains("Key") || methodName.Contains("Mouse") || 
            methodName == "KeyEvent" || methodName == "LeftClickEvent" || methodName == "MouseScrollEvent")
            return "Input";
        if (methodName.Contains("Physics") || methodName.Contains("Collision"))
            return "Physics";
        if (methodName.Contains("Container"))
            return "Container";

        return string.Empty; // Unknown module
    }

    private ServicePattern GetOrCreatePattern(string moduleName)
    {
        var existing = _detectedPatterns.FirstOrDefault(p => p.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return existing;

        var newPattern = new ServicePattern { ModuleName = moduleName };
        _detectedPatterns.Add(newPattern);
        return newPattern;
    }
}

/// <summary>
/// Represents a detected service pattern for a specific module.
/// Contains information about the service interface, implementation, facade methods, and metrics.
/// </summary>
public class ServicePattern
{
    public string ModuleName { get; set; } = string.Empty;
    public string ServiceInterface { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
    public string NullImplementation { get; set; } = string.Empty;
    
    public List<string> ServiceMethods { get; } = new();
    public List<string> BasicServiceMethods { get; } = new();
    public List<string> ImplementationMethods { get; } = new();
    public List<string> FacadeMethods { get; } = new();
}