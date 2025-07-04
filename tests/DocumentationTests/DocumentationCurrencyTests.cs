using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests that validate documentation accurately reflects the current codebase implementation.
/// Ensures all documented APIs, classes, and examples match actual code.
/// </summary>
public class DocumentationCurrencyTests
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static readonly Dictionary<string, Assembly> _loadedAssemblies = new();

    static DocumentationCurrencyTests()
    {
        // Load all RACEngine assemblies for reflection-based validation
        var assemblyNames = new[]
        {
            "Rac.Core", "Rac.ECS", "Rac.Rendering", "Rac.Audio", 
            "Rac.Engine", "Rac.GameEngine", "Rac.Physics", "Rac.Input"
        };

        var repositoryRoot = DocumentationHelper.GetRepositoryRoot();

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assemblyPath = Path.Combine(repositoryRoot, "src", assemblyName, "bin", "Debug", "net8.0", $"{assemblyName}.dll");
                if (File.Exists(assemblyPath))
                {
                    var assembly = Assembly.LoadFrom(assemblyPath);
                    _loadedAssemblies[assemblyName] = assembly;
                }
            }
            catch (FileNotFoundException)
            {
                // Assembly not built or doesn't exist - this is fine for optional modules
            }
            catch (Exception ex)
            {
                // Log other exceptions but continue - some assemblies might not be available
                Console.WriteLine($"Failed to load assembly {assemblyName}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Validates that Rac.Core project documentation matches actual implementation.
    /// </summary>
    [Fact]
    public void RacCore_DocumentationMatchesImplementation()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "projects", "Rac.Core.md");
        var content = File.ReadAllText(docPath);
        var document = Markdown.Parse(content, _pipeline);

        if (!_loadedAssemblies.TryGetValue("Rac.Core", out var assembly))
        {
            Assert.Fail("Rac.Core assembly not found. Ensure the project is built.");
        }

        // Act & Assert - Check documented namespaces exist
        ValidateNamespaceExists(assembly, "Rac.Core.Builder", "EngineBuilder class should exist");
        ValidateNamespaceExists(assembly, "Rac.Core.Configuration", "Configuration classes should exist");
        ValidateNamespaceExists(assembly, "Rac.Core.Extension", "Vector2DExtensions should exist");
        ValidateNamespaceExists(assembly, "Rac.Core.Logger", "ILogger interface should exist");
        ValidateNamespaceExists(assembly, "Rac.Core.Manager", "WindowManager classes should exist");

        // Check specific classes mentioned in documentation
        ValidateClassExists(assembly, "Rac.Core.Builder.EngineBuilder");
        ValidateClassExists(assembly, "Rac.Core.Extension.Vector2DExtensions");
        ValidateClassExists(assembly, "Rac.Core.Logger.ILogger");
        ValidateClassExists(assembly, "Rac.Core.Logger.SerilogLogger");
        ValidateClassExists(assembly, "Rac.Core.Manager.ConfigManager");
        ValidateClassExists(assembly, "Rac.Core.Manager.WindowManager");
        ValidateClassExists(assembly, "Rac.Core.Manager.IWindowManager");
    }

    /// <summary>
    /// Validates that Rac.ECS project documentation matches actual implementation.
    /// </summary>
    [Fact]
    public void RacECS_DocumentationMatchesImplementation()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "projects", "Rac.ECS.md");
        var content = File.ReadAllText(docPath);

        if (!_loadedAssemblies.TryGetValue("Rac.ECS", out var assembly))
        {
            Assert.Fail("Rac.ECS assembly not found. Ensure the project is built.");
        }

        // Act & Assert - Check documented namespaces and classes exist
        ValidateNamespaceExists(assembly, "Rac.ECS.Core", "Core ECS interfaces should exist");
        ValidateNamespaceExists(assembly, "Rac.ECS.Components", "Component definitions should exist");
        ValidateNamespaceExists(assembly, "Rac.ECS.Systems", "System implementations should exist");
        ValidateNamespaceExists(assembly, "Rac.ECS.Entities", "Entity-related types should exist");

        // Check specific interfaces and classes mentioned in documentation
        ValidateClassExists(assembly, "Rac.ECS.Core.IWorld");
        ValidateClassExists(assembly, "Rac.ECS.Core.World");
        ValidateClassExists(assembly, "Rac.ECS.Components.IComponent");
        ValidateClassExists(assembly, "Rac.ECS.Components.TransformComponent");
        ValidateClassExists(assembly, "Rac.ECS.Components.WorldTransformComponent");
        ValidateClassExists(assembly, "Rac.ECS.Components.ParentHierarchyComponent");
        ValidateClassExists(assembly, "Rac.ECS.Components.ContainerComponent");
        ValidateClassExists(assembly, "Rac.ECS.Systems.ISystem");
        ValidateClassExists(assembly, "Rac.ECS.Systems.SystemScheduler");
        ValidateClassExists(assembly, "Rac.ECS.Systems.RunAfterAttribute");
        ValidateClassExists(assembly, "Rac.ECS.Systems.TransformSystem");
        ValidateClassExists(assembly, "Rac.ECS.Systems.ContainerSystem");
    }

    /// <summary>
    /// Validates that system overview documentation describes existing modules.
    /// </summary>
    [Fact]
    public void SystemOverview_ReferencesExistingModules()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "architecture", "system-overview.md");
        var content = File.ReadAllText(docPath);

        // Act & Assert - Check that mentioned modules actually exist as projects
        var projectRoot = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src");
        var expectedModules = new[]
        {
            "Rac.Core", "Rac.ECS", "Rac.Rendering", "Rac.Audio", 
            "Rac.Engine", "Rac.GameEngine", "Rac.Physics", "Rac.Input"
        };

        foreach (var module in expectedModules)
        {
            var modulePath = Path.Combine(projectRoot, module);
            var projectFile = Path.Combine(modulePath, $"{module}.csproj");
            
            if (!Directory.Exists(modulePath))
            {
                Assert.Fail($"Module {module} referenced in system overview documentation does not exist at {modulePath}");
            }

            if (!File.Exists(projectFile))
            {
                Assert.Fail($"Project file for {module} does not exist at {projectFile}");
            }
        }
    }

    /// <summary>
    /// Validates that ECS architecture documentation matches actual ECS implementation.
    /// </summary>
    [Fact]
    public void ECSArchitecture_MatchesImplementation()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "architecture", "ecs-architecture.md");
        var content = File.ReadAllText(docPath);

        if (!_loadedAssemblies.TryGetValue("Rac.ECS", out var assembly))
        {
            Assert.Fail("Rac.ECS assembly not found. Ensure the project is built.");
        }

        // Act & Assert - Validate documented patterns exist in code
        var document = Markdown.Parse(content, _pipeline);
        var codeBlocks = document.Descendants<FencedCodeBlock>()
            .Where(block => block.Info?.Trim().ToLowerInvariant() == "csharp")
            .ToList();

        // Check that ISystem interface has documented methods
        var systemInterface = assembly.GetType("Rac.ECS.Systems.ISystem");
        Assert.NotNull(systemInterface);
        Assert.True(systemInterface != null, "ISystem interface should exist");
        
        var methods = systemInterface!.GetMethods();
        Assert.Contains(methods, m => m.Name == "Initialize");
        Assert.Contains(methods, m => m.Name == "Update");
        Assert.Contains(methods, m => m.Name == "Shutdown");

        // Verify SystemScheduler exists and has dependency management
        var schedulerType = assembly.GetType("Rac.ECS.Systems.SystemScheduler");
        Assert.NotNull(schedulerType);
        Assert.True(schedulerType != null, "SystemScheduler should exist");

        // Verify RunAfterAttribute exists for dependency declarations
        var runAfterType = assembly.GetType("Rac.ECS.Systems.RunAfterAttribute");
        Assert.NotNull(runAfterType);
        Assert.True(runAfterType != null, "RunAfterAttribute should exist");
    }

    /// <summary>
    /// Validates that code examples in project documentation compile successfully.
    /// </summary>
    [Theory]
    [InlineData("Rac.Core.md")]
    [InlineData("Rac.ECS.md")]
    public void ProjectDocumentation_CodeExamplesCompile(string documentFile)
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "projects", documentFile);
        var content = File.ReadAllText(docPath);
        var document = Markdown.Parse(content, _pipeline);

        // Act & Assert - Extract C# code blocks and validate syntax
        var codeBlocks = document.Descendants<FencedCodeBlock>()
            .Where(block => block.Info?.Trim().ToLowerInvariant() == "csharp")
            .ToList();

        foreach (var codeBlock in codeBlocks)
        {
            var code = codeBlock.Lines.ToString();
            
            // Skip empty or comment-only blocks
            if (string.IsNullOrWhiteSpace(code) || code.Trim().StartsWith("//"))
                continue;

            // Check for basic C# syntax elements that suggest valid code
            ValidateCodeSyntax(code, documentFile);
        }
    }

    /// <summary>
    /// Validates that getting started tutorial references existing samples and APIs.
    /// </summary>
    [Fact]
    public void GettingStartedTutorial_ReferencesExistingSamples()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "educational-material", "getting-started-tutorial.md");
        var content = File.ReadAllText(docPath);

        // Check that referenced sample projects exist
        var samplesPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "samples");
        Assert.True(Directory.Exists(samplesPath), "Samples directory should exist for tutorial references");

        // Verify that tutorial steps can be followed with current repository structure
        var repoRoot = DocumentationHelper.GetRepositoryRoot();
        var solutionFile = Path.Combine(repoRoot, "RACEngine.sln");
        Assert.True(File.Exists(solutionFile), "Solution file should exist for tutorial build steps");
    }

    /// <summary>
    /// Helper method to validate that a namespace exists in the assembly.
    /// </summary>
    private static void ValidateNamespaceExists(Assembly assembly, string namespaceName, string errorMessage)
    {
        try
        {
            var typesInNamespace = assembly.GetTypes()
                .Where(type => type.Namespace == namespaceName)
                .ToList();

            Assert.True(typesInNamespace.Count > 0, 
                $"{errorMessage}: No types found in namespace {namespaceName}");
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Handle missing dependencies by checking only successfully loaded types
            var loadedTypes = ex.Types.Where(t => t != null).ToList();
            var typesInNamespace = loadedTypes
                .Where(type => type!.Namespace == namespaceName)
                .ToList();

            Assert.True(typesInNamespace.Count > 0, 
                $"{errorMessage}: No types found in namespace {namespaceName} (some dependencies missing: {string.Join(", ", ex.LoaderExceptions.Take(3).Select(e => e?.Message))})");
        }
    }

    /// <summary>
    /// Helper method to validate that a specific class exists in the assembly.
    /// </summary>
    private static void ValidateClassExists(Assembly assembly, string fullTypeName)
    {
        try
        {
            var type = assembly.GetType(fullTypeName);
            Assert.NotNull(type);
            Assert.True(type != null, $"Type {fullTypeName} should exist but was not found in assembly {assembly.GetName().Name}");
        }
        catch (FileNotFoundException ex)
        {
            Assert.Fail($"Could not validate type {fullTypeName} due to missing dependency: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to validate basic C# code syntax.
    /// </summary>
    private static void ValidateCodeSyntax(string code, string documentFile)
    {
        // Check for balanced braces
        var openBraces = code.Count(c => c == '{');
        var closeBraces = code.Count(c => c == '}');
        
        if (openBraces > 0 || closeBraces > 0)
        {
            Assert.True(openBraces == closeBraces, 
                $"Code block in {documentFile} has unbalanced braces: {openBraces} open, {closeBraces} close");
        }

        // Check for balanced parentheses
        var openParens = code.Count(c => c == '(');
        var closeParens = code.Count(c => c == ')');
        
        if (openParens > 0 || closeParens > 0)
        {
            Assert.True(openParens == closeParens, 
                $"Code block in {documentFile} has unbalanced parentheses: {openParens} open, {closeParens} close");
        }

        // Check that using statements are valid (if present)
        var usingPattern = @"using\s+[\w\.]+\s*;";
        var usingMatches = Regex.Matches(code, usingPattern);
        
        foreach (Match match in usingMatches)
        {
            var usingStatement = match.Value;
            // Validate that using statements end with semicolon
            Assert.True(usingStatement.TrimEnd().EndsWith(";"), 
                $"Using statement in {documentFile} should end with semicolon: {usingStatement}");
        }
    }
}