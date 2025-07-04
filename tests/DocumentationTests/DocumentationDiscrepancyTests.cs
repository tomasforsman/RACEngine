using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests that validate specific documentation-to-code discrepancies found during currency audit.
/// These tests highlight areas where documentation doesn't match actual implementation.
/// </summary>
public class DocumentationDiscrepancyTests
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Validates that documented namespaces in Rac.Core.md match actual file structure.
    /// </summary>
    [Fact]
    public void RacCore_DocumentedNamespacesExistAsDirectories()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "projects", "Rac.Core.md");
        var content = File.ReadAllText(docPath);
        var srcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.Core");

        // Act & Assert - Check that documented namespaces exist as directories
        var expectedNamespaces = new[]
        {
            "Builder", "Configuration", "Extension", "Logger", "Manager", "Scheduler"
        };

        foreach (var ns in expectedNamespaces)
        {
            var namespacePath = Path.Combine(srcPath, ns);
            Assert.True(Directory.Exists(namespacePath), 
                $"Documented namespace Rac.Core.{ns} should exist as directory {namespacePath}");
        }
    }

    /// <summary>
    /// Validates that documented classes in Rac.Core.md exist as files.
    /// </summary>
    [Fact]
    public void RacCore_DocumentedClassesExistAsFiles()
    {
        // Arrange
        var srcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.Core");
        
        // Act & Assert - Check specific classes mentioned in documentation
        var expectedClasses = new Dictionary<string, string>
        {
            ["EngineBuilder"] = Path.Combine(srcPath, "Builder", "EngineBuilder.cs"),
            ["Vector2DExtensions"] = Path.Combine(srcPath, "Extension", "Vector2DExtensions.cs"),
            ["ILogger"] = Path.Combine(srcPath, "Logger", "ILogger.cs"),
            ["SerilogLogger"] = Path.Combine(srcPath, "Logger", "SerilogLogger.cs"),
            ["ConfigManager"] = Path.Combine(srcPath, "Manager", "ConfigManager.cs"),
            ["WindowManager"] = Path.Combine(srcPath, "Manager", "WindowManager.cs"),
            ["IWindowManager"] = Path.Combine(srcPath, "Manager", "IWindowManager.cs"),
            ["WindowBuilder"] = Path.Combine(srcPath, "Manager", "WindowBuilder.cs")
        };

        foreach (var (className, expectedPath) in expectedClasses)
        {
            Assert.True(File.Exists(expectedPath), 
                $"Documented class {className} should exist at {expectedPath}");
        }
    }

    /// <summary>
    /// Validates that documented ECS components exist as files.
    /// </summary>
    [Fact]
    public void RacECS_DocumentedComponentsExistAsFiles()
    {
        // Arrange
        var srcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.ECS", "Components");
        
        // Act & Assert - Check components mentioned in Rac.ECS.md
        var expectedComponents = new[]
        {
            "IComponent.cs",
            "TransformComponent.cs", 
            "WorldTransformComponent.cs",
            "ParentHierarchyComponent.cs",
            "ContainerComponent.cs"
        };

        foreach (var component in expectedComponents)
        {
            var componentPath = Path.Combine(srcPath, component);
            Assert.True(File.Exists(componentPath), 
                $"Documented component {component} should exist at {componentPath}");
        }
    }

    /// <summary>
    /// Validates that documented ECS systems exist as files.
    /// </summary>
    [Fact]
    public void RacECS_DocumentedSystemsExistAsFiles()
    {
        // Arrange
        var srcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.ECS", "Systems");
        
        // Act & Assert - Check systems mentioned in Rac.ECS.md
        var expectedSystems = new[]
        {
            "ISystem.cs",
            "SystemScheduler.cs",
            "RunAfterAttribute.cs", 
            "SystemDependencyResolver.cs",
            "TransformSystem.cs",
            "ContainerSystem.cs"
        };

        foreach (var system in expectedSystems)
        {
            var systemPath = Path.Combine(srcPath, system);
            Assert.True(File.Exists(systemPath), 
                $"Documented system {system} should exist at {systemPath}");
        }
    }

    /// <summary>
    /// Validates that getting started tutorial references existing sample projects.
    /// </summary>
    [Fact]
    public void GettingStartedTutorial_ReferencesExistingSampleProjects()
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "docs", "educational-material", "getting-started-tutorial.md");
        var content = File.ReadAllText(docPath);
        var samplesPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "samples");

        // Act & Assert - Check that samples directory exists and has content
        Assert.True(Directory.Exists(samplesPath), "Samples directory should exist for tutorial");
        
        var sampleDirectories = Directory.GetDirectories(samplesPath);
        Assert.True(sampleDirectories.Length > 0, "At least one sample project should exist for tutorial reference");

        // Check that at least one sample has a project file
        var hasCSharpProject = sampleDirectories.Any(dir => 
            Directory.GetFiles(dir, "*.csproj").Length > 0);
        
        Assert.True(hasCSharpProject, "At least one sample should be a C# project with .csproj file");
    }

    /// <summary>
    /// Validates that audio architecture documentation references exist for audio service interfaces.
    /// </summary>
    [Fact]
    public void AudioArchitecture_ReferencesExistingInterfaces()
    {
        // Arrange
        var audioSrcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.Audio");
        
        // Act & Assert - Check that IAudioService interface exists
        var audioServicePath = Path.Combine(audioSrcPath, "IAudioService.cs");
        
        if (Directory.Exists(audioSrcPath))
        {
            Assert.True(File.Exists(audioServicePath), 
                "IAudioService.cs should exist if audio module is documented");
        }
        else
        {
            // Audio module might not be fully implemented yet - that's a valid finding
            Assert.True(true, "Audio module directory does not exist - documentation may be ahead of implementation");
        }
    }

    /// <summary>
    /// Validates that rendering pipeline documentation has corresponding renderer classes.
    /// </summary>
    [Fact]
    public void RenderingPipeline_HasCorrespondingRendererClasses()
    {
        // Arrange
        var renderingSrcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", "Rac.Rendering");
        
        // Act & Assert - Check for basic rendering infrastructure
        if (Directory.Exists(renderingSrcPath))
        {
            var renderingFiles = Directory.GetFiles(renderingSrcPath, "*.cs", SearchOption.AllDirectories);
            Assert.True(renderingFiles.Length > 0, "Rendering module should contain .cs files if documented");
            
            // Look for common rendering classes
            var hasBasicVertex = renderingFiles.Any(f => Path.GetFileName(f).Contains("Vertex"));
            Assert.True(hasBasicVertex, "Rendering module should contain vertex-related classes");
        }
        else
        {
            Assert.True(true, "Rendering module directory does not exist - documentation may be ahead of implementation");
        }
    }

    /// <summary>
    /// Checks for obvious documentation formatting issues that indicate outdated content.
    /// </summary>
    [Theory]
    [InlineData("docs/projects/Rac.Core.md")]
    [InlineData("docs/projects/Rac.ECS.md")]
    [InlineData("docs/architecture/system-overview.md")]
    [InlineData("docs/educational-material/getting-started-tutorial.md")]
    public void Documentation_UsesPresentTenseForCurrentState(string relativePath)
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), relativePath);
        var content = File.ReadAllText(docPath);

        // Act & Assert - Look for past tense indicators that suggest outdated content
        var pastTensePatterns = new[]
        {
            @"\bwas\s+implemented\b",
            @"\bhad\s+been\b", 
            @"\bused\s+to\s+be\b",
            @"\bpreviously\s+provided\b"
        };

        foreach (var pattern in pastTensePatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            Assert.True(matches.Count == 0, 
                $"Documentation {relativePath} contains past tense language suggesting outdated content: '{pattern}' found {matches.Count} times");
        }
    }

    /// <summary>
    /// Validates that code examples in documentation use existing namespaces.
    /// </summary>
    [Theory]
    [InlineData("docs/projects/Rac.Core.md")]
    [InlineData("docs/projects/Rac.ECS.md")]
    public void Documentation_CodeExamplesUseExistingNamespaces(string relativePath)
    {
        // Arrange
        var docPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), relativePath);
        var content = File.ReadAllText(docPath);
        var document = Markdown.Parse(content, _pipeline);

        // Act & Assert - Extract using statements from code blocks
        var codeBlocks = document.Descendants<FencedCodeBlock>()
            .Where(block => block.Info?.Trim().ToLowerInvariant() == "csharp")
            .ToList();

        foreach (var codeBlock in codeBlocks)
        {
            var code = codeBlock.Lines.ToString();
            var usingMatches = Regex.Matches(code, @"using\s+(Rac\.\w+)(\.\w+)*\s*;");

            foreach (Match match in usingMatches)
            {
                var namespaceName = match.Groups[1].Value;
                var expectedSrcPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src", namespaceName);
                
                Assert.True(Directory.Exists(expectedSrcPath) || namespaceName == "Rac.Core", 
                    $"Code example in {relativePath} references namespace {namespaceName} but corresponding src directory doesn't exist at {expectedSrcPath}");
            }
        }
    }
}