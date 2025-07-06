using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace ArchitectureAnalyzer.Tests;

/// <summary>
/// Tests for the ServicePatternDetector to ensure it correctly identifies RACEngine service patterns.
/// </summary>
public class ServicePatternDetectorTests
{
    [Fact]
    public void AnalyzeInterface_DetectsServiceInterface()
    {
        // Arrange
        var detector = new ServicePatternDetector();
        var sourceCode = @"
            namespace Rac.Audio;
            public interface IAudioService
            {
                void PlaySound(string path);
                void SetVolume(float volume);
            }";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("Test").AddSyntaxTrees(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var context = new AnalysisContextWrapper(semanticModel);

        var interfaceDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .First();

        // Act
        detector.AnalyzeInterface(context, interfaceDecl);
        var patterns = detector.GetDetectedPatterns();

        // Assert
        Assert.Single(patterns);
        var pattern = patterns.First();
        Assert.Equal("Audio", pattern.ModuleName);
        Assert.Equal("IAudioService", pattern.ServiceInterface);
        Assert.Contains("PlaySound", pattern.ServiceMethods);
        Assert.Contains("SetVolume", pattern.ServiceMethods);
        Assert.Contains("PlaySound", pattern.BasicServiceMethods);
        Assert.Contains("SetVolume", pattern.BasicServiceMethods);
    }

    [Fact]
    public void AnalyzeClass_DetectsServiceImplementation()
    {
        // Arrange
        var detector = new ServicePatternDetector();
        var sourceCode = @"
            namespace Rac.Audio;
            
            public interface IAudioService
            {
                void PlaySound(string path);
            }
            
            public class OpenALAudioService : IAudioService
            {
                public void PlaySound(string path) { }
                public void PlaySound3D(string path, float x, float y, float z) { }
            }";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("Test").AddSyntaxTrees(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var context = new AnalysisContextWrapper(semanticModel);

        var classDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First();

        // Act
        detector.AnalyzeClass(context, classDecl);
        var patterns = detector.GetDetectedPatterns();

        // Assert
        Assert.Single(patterns);
        var pattern = patterns.First();
        Assert.Equal("Audio", pattern.ModuleName);
        Assert.Equal("OpenALAudioService", pattern.Implementation);
        Assert.Contains("PlaySound", pattern.ImplementationMethods);
        Assert.Contains("PlaySound3D", pattern.ImplementationMethods);
    }

    [Fact]
    public void AnalyzeClass_DetectsNullService()
    {
        // Arrange
        var detector = new ServicePatternDetector();
        var sourceCode = @"
            namespace Rac.Audio;
            
            public interface IAudioService
            {
                void PlaySound(string path);
            }
            
            public class NullAudioService : IAudioService
            {
                public void PlaySound(string path) { }
            }";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("Test").AddSyntaxTrees(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var context = new AnalysisContextWrapper(semanticModel);

        var classDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First();

        // Act
        detector.AnalyzeClass(context, classDecl);
        var patterns = detector.GetDetectedPatterns();

        // Assert
        Assert.Single(patterns);
        var pattern = patterns.First();
        Assert.Equal("Audio", pattern.ModuleName);
        Assert.Equal("NullAudioService", pattern.NullImplementation);
        Assert.Empty(pattern.ImplementationMethods); // Null services don't contribute implementation methods
    }

    [Fact]
    public void MetricsCalculator_CalculatesCorrectCoverage()
    {
        // Arrange
        var calculator = new MetricsCalculator();
        var pattern = new ServicePattern
        {
            ModuleName = "Audio",
            ServiceInterface = "IAudioService",
            Implementation = "OpenALAudioService",
            ServiceMethods = { "PlaySound", "SetVolume" },
            BasicServiceMethods = { "PlaySound", "SetVolume" },
            ImplementationMethods = { "PlaySound", "SetVolume", "PlaySound3D" },
            FacadeMethods = { "PlaySound" }
        };

        // Act
        var metrics = calculator.CalculateMetrics(new[] { pattern });

        // Assert
        var module = metrics.Modules.First();
        Assert.Equal("Audio", module.Name);
        
        // Service coverage: 2 out of 3 implementation methods exposed = 67%
        Assert.Equal(67, Math.Round(module.ServiceCoverage));
        
        // Facade coverage: 1 out of 2 basic service methods exposed = 50%
        Assert.Equal(50, Math.Round(module.FacadeCoverage));
    }
}

/// <summary>
/// Tests for the DocumentationGenerator to ensure it produces correct markdown output.
/// </summary>
public class DocumentationGeneratorTests
{
    [Fact]
    public void GenerateMarkdownReport_ProducesValidReport()
    {
        // Arrange
        var generator = new DocumentationGenerator();
        var metrics = new ArchitectureMetrics
        {
            Timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            OverallHealth = 67.5,
            Modules = new List<ModuleMetrics>
            {
                new ModuleMetrics
                {
                    Name = "Audio",
                    ServiceInterface = "IAudioService",
                    Implementation = "OpenALAudioService",
                    Facade = "EngineFacade.Audio.*",
                    ServiceCoverage = 85.2,
                    FacadeCoverage = 72.1,
                    Health = "excellent",
                    Opportunities = { "Add PlayExplosion() convenience method" }
                }
            }
        };

        // Act
        var markdown = generator.GenerateMarkdownReport(metrics);

        // Assert
        Assert.Contains("# Progressive Complexity Health Report", markdown);
        Assert.Contains("Overall Architecture Health: 68% ✅", markdown);
        Assert.Contains("🎵 Audio System - 72% 🎯 Excellent", markdown);
        Assert.Contains("**Facade Coverage**: 72%", markdown);
        Assert.Contains("**Service Coverage**: 85%", markdown);
        Assert.Contains("Add PlayExplosion() convenience method", markdown);
    }
}