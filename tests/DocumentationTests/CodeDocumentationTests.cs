using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests to validate code documentation standards compliance across the RACEngine codebase.
/// Ensures XML documentation completeness and educational content quality.
/// </summary>
public class CodeDocumentationTests
{
    /// <summary>
    /// Validates that the enhanced Physics module documentation meets educational standards.
    /// </summary>
    [Fact]
    public void PhysicsModule_HasComprehensiveDocumentation()
    {
        // Verify IPhysicsService interface documentation
        var physicsServiceContent = File.ReadAllText(
            Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src/Rac.Physics/IPhysicsService.cs")
        );
        
        // Check for educational content markers
        Assert.Contains("Educational Notes:", physicsServiceContent);
        Assert.Contains("Performance Characteristics:", physicsServiceContent);
        Assert.Contains("O(n", physicsServiceContent); // Big O notation present
        Assert.Contains("Newton's Second Law", physicsServiceContent);
        Assert.Contains("quaternion", physicsServiceContent.ToLowerInvariant());
        
        // Check for ASCII art section headers
        Assert.Contains("═══════════════════════════════════════", physicsServiceContent);
        
        // Verify comprehensive XML documentation tags
        Assert.Contains("<summary>", physicsServiceContent);
        Assert.Contains("/// <param name=", physicsServiceContent);
        Assert.Contains("<returns>", physicsServiceContent);
        Assert.Contains("<remarks>", physicsServiceContent);
        
        // Verify NullPhysicsService documentation
        var nullPhysicsContent = File.ReadAllText(
            Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src/Rac.Physics/NullPhysicsService.cs")
        );
        
        Assert.Contains("Educational Note:", nullPhysicsContent);
        Assert.Contains("Null Object pattern", nullPhysicsContent);
        Assert.Contains("impulse", nullPhysicsContent);
    }
    
    /// <summary>
    /// Validates that the enhanced Boids algorithm implementation contains proper academic references.
    /// </summary>
    [Fact]
    public void BoidSystem_HasAcademicReferencesAndEducationalContent()
    {
        var boidSystemContent = File.ReadAllText(
            Path.Combine(DocumentationHelper.GetRepositoryRoot(), "samples/SampleGame/Systems/BoidSystem.cs")
        );
        
        // Check for academic reference
        Assert.Contains("Craig Reynolds", boidSystemContent);
        Assert.Contains("1986", boidSystemContent);
        Assert.Contains("BOIDS ALGORITHM IMPLEMENTATION", boidSystemContent);
        
        // Check for educational algorithm explanation
        Assert.Contains("Separation: Avoid crowding neighbors", boidSystemContent);
        Assert.Contains("Alignment: Steer towards average heading", boidSystemContent);
        Assert.Contains("Cohesion: Steer towards average position", boidSystemContent);
        
        // Check for performance analysis
        Assert.Contains("O(n²)", boidSystemContent);
        Assert.Contains("PERFORMANCE CHARACTERISTICS:", boidSystemContent);
        Assert.Contains("PERFORMANCE BOTTLENECKS:", boidSystemContent);
        
        // Check for mathematical formulas
        Assert.Contains("F_sep", boidSystemContent);
        Assert.Contains("F_align", boidSystemContent);
        Assert.Contains("F_cohesion", boidSystemContent);
        
        // Check for implementation phases documentation
        Assert.Contains("PHASE 1:", boidSystemContent);
        Assert.Contains("PHASE 2:", boidSystemContent);
        Assert.Contains("PHASE 3:", boidSystemContent);
        Assert.Contains("PHASE 4:", boidSystemContent);
        
        // Check for ASCII art section headers
        Assert.Contains("═══════════════════════════════════════", boidSystemContent);
    }
    
    /// <summary>
    /// Validates that Vector2D extensions have proper mathematical documentation.
    /// </summary>
    [Fact]
    public void VectorExtensions_HasMathematicalDocumentation()
    {
        var vectorExtensionsContent = File.ReadAllText(
            Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src/Rac.Core/Extension/Vector2DExtensions.cs")
        );
        
        // Check for mathematical background
        Assert.Contains("Mathematical Background:", vectorExtensionsContent);
        Assert.Contains("Vector normalization", vectorExtensionsContent);
        Assert.Contains("normalized = v / |v|", vectorExtensionsContent);
        
        // Check for comprehensive XML documentation
        Assert.Contains("<summary>", vectorExtensionsContent);
        Assert.Contains("<returns>", vectorExtensionsContent);
        Assert.Contains("<example>", vectorExtensionsContent);
        Assert.Contains("<code>", vectorExtensionsContent);
        
        // Check for educational use cases
        Assert.Contains("game development", vectorExtensionsContent);
        Assert.Contains("physics simulations", vectorExtensionsContent);
        Assert.Contains("AI steering behaviors", vectorExtensionsContent);
    }
    
    /// <summary>
    /// Validates that educational comments follow the established style guidelines.
    /// </summary>
    [Theory]
    [InlineData("src/Rac.Physics/IPhysicsService.cs")]
    [InlineData("src/Rac.Physics/NullPhysicsService.cs")]
    [InlineData("samples/SampleGame/Systems/BoidSystem.cs")]
    public void DocumentedFiles_FollowEducationalCommentStandards(string filePath)
    {
        var fullPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), filePath);
        var content = File.ReadAllText(fullPath);
        
        // Check for educational comment patterns
        var educationalPatterns = new[]
        {
            @"Educational Note:",
            @"Educational Notes:",
            @"PERFORMANCE CHARACTERISTICS:",
            @"Academic Reference:",
            @"Mathematical",
            @"Algorithm",
            @"O\([^)]+\)" // Big O notation pattern
        };
        
        var hasEducationalContent = educationalPatterns.Any(pattern => 
            Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase));
        
        Assert.True(hasEducationalContent, 
            $"File {filePath} should contain educational comments or documentation");
    }
    
    /// <summary>
    /// Validates that ASCII art section headers are properly formatted.
    /// </summary>
    [Theory]
    [InlineData("src/Rac.Physics/NullPhysicsService.cs")]
    [InlineData("samples/SampleGame/Systems/BoidSystem.cs")]
    public void DocumentedFiles_UseProperASCIIArtHeaders(string filePath)
    {
        var fullPath = Path.Combine(DocumentationHelper.GetRepositoryRoot(), filePath);
        var content = File.ReadAllText(fullPath);
        
        // Check for ASCII art section headers pattern
        var asciiHeaderPattern = @"// ═══════════════════════════════════════";
        
        Assert.True(Regex.IsMatch(content, asciiHeaderPattern), 
            $"File {filePath} should use ASCII art section headers per style guidelines");
    }
    
    /// <summary>
    /// Validates that XML documentation includes required tags for public APIs.
    /// </summary>
    [Fact]
    public void PublicAPIs_HaveRequiredXMLDocumentationTags()
    {
        var physicsServiceContent = File.ReadAllText(
            Path.Combine(DocumentationHelper.GetRepositoryRoot(), "src/Rac.Physics/IPhysicsService.cs")
        );
        
        // Count XML documentation tag occurrences
        var summaryCount = Regex.Matches(physicsServiceContent, @"<summary>").Count;
        var paramCount = Regex.Matches(physicsServiceContent, @"<param").Count;
        var returnsCount = Regex.Matches(physicsServiceContent, @"<returns>").Count;
        var remarksCount = Regex.Matches(physicsServiceContent, @"<remarks>").Count;
        
        // Verify substantial documentation coverage
        Assert.True(summaryCount >= 10, "Should have summary tags for all public methods");
        Assert.True(paramCount >= 20, "Should have param documentation for method parameters");
        Assert.True(returnsCount >= 5, "Should have returns documentation for non-void methods");
        Assert.True(remarksCount >= 5, "Should have remarks for complex APIs");
    }
}