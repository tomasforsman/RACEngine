using Markdig;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests that validate content quality requirements based on document types.
/// </summary>
public class ContentQualityTests
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
    
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

    /// <summary>
    /// Validates that API documentation contains appropriate code examples.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateApiDocumentation(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var frontmatter = ExtractAndParseFrontmatter(content);
        var relativePath = GetRelativePath(filePath);

        // Only validate API documentation
        if (frontmatter == null || !IsApiDocumentation(frontmatter))
        {
            return;
        }

        var document = Markdown.Parse(content, _pipeline);

        // Act & Assert - API docs should have code examples
        var codeBlocks = document.Descendants<FencedCodeBlock>().ToList();
        
        if (codeBlocks.Count == 0)
        {
            Assert.Fail($"API documentation {relativePath} should contain code examples demonstrating usage.");
        }

        // Check for C# code examples (primary language for this engine)
        var csharpBlocks = codeBlocks
            .Where(block => block.Info?.Trim().ToLowerInvariant() == "csharp")
            .ToList();

        if (csharpBlocks.Count == 0)
        {
            Assert.Fail($"API documentation {relativePath} should contain C# code examples since this is a C# game engine.");
        }

        // API docs should have method/property signatures or usage examples
        var hasMethodSignatures = csharpBlocks.Any(block => 
            block.Lines.ToString().Contains("public ") || 
            block.Lines.ToString().Contains("void ") ||
            block.Lines.ToString().Contains("class ") ||
            block.Lines.ToString().Contains("interface "));

        var hasUsageExamples = csharpBlocks.Any(block =>
            block.Lines.ToString().Contains("var ") ||
            block.Lines.ToString().Contains("new ") ||
            block.Lines.ToString().Contains("();"));

        if (!hasMethodSignatures && !hasUsageExamples)
        {
            Assert.Fail($"API documentation {relativePath} should contain either method signatures or usage examples in its C# code blocks.");
        }
    }

    /// <summary>
    /// Validates that tutorial documentation has proper step-by-step structure.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateTutorialStructure(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var frontmatter = ExtractAndParseFrontmatter(content);
        var relativePath = GetRelativePath(filePath);

        // Only validate tutorial documentation
        if (frontmatter == null || !IsTutorialDocumentation(frontmatter))
        {
            return;
        }

        var document = Markdown.Parse(content, _pipeline);

        // Act & Assert - Tutorials should have step-by-step structure
        var headings = document.Descendants<HeadingBlock>().ToList();
        
        if (headings.Count < 3)
        {
            Assert.Fail($"Tutorial {relativePath} should have multiple sections to guide users step-by-step.");
        }

        // Look for step indicators in headings or ordered lists
        var hasStepStructure = headings.Any(h => 
            GetHeadingText(h).ToLowerInvariant().Contains("step") ||
            GetHeadingText(h).ToLowerInvariant().Contains("getting started") ||
            GetHeadingText(h).ToLowerInvariant().Contains("prerequisites") ||
            GetHeadingText(h).ToLowerInvariant().Contains("setup"));

        var orderedLists = document.Descendants<ListBlock>()
            .Where(list => list.IsOrdered)
            .ToList();

        if (!hasStepStructure && orderedLists.Count == 0)
        {
            Assert.Fail($"Tutorial {relativePath} should have clear step-by-step structure using numbered steps, " +
                      "sequential headings, or ordered lists.");
        }

        // Tutorials should have code examples
        var codeBlocks = document.Descendants<FencedCodeBlock>().ToList();
        if (codeBlocks.Count == 0)
        {
            Assert.Fail($"Tutorial {relativePath} should contain practical code examples to help users follow along.");
        }
    }

    /// <summary>
    /// Validates that sample documentation demonstrates actual functionality.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateSampleDocumentation(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var frontmatter = ExtractAndParseFrontmatter(content);
        var relativePath = GetRelativePath(filePath);

        // Only validate sample documentation
        if (frontmatter == null || !IsSampleDocumentation(frontmatter))
        {
            return;
        }

        var document = Markdown.Parse(content, _pipeline);

        // Act & Assert - Samples should demonstrate functionality
        var codeBlocks = document.Descendants<FencedCodeBlock>().ToList();
        
        if (codeBlocks.Count == 0)
        {
            Assert.Fail($"Sample documentation {relativePath} should contain code examples showing the demonstrated functionality.");
        }

        // Check for meaningful code content (not just empty blocks)
        var substantialCodeBlocks = codeBlocks
            .Where(block => block.Lines.ToString().Trim().Length > 20)
            .ToList();

        if (substantialCodeBlocks.Count == 0)
        {
            Assert.Fail($"Sample documentation {relativePath} should contain substantial code examples, not just trivial snippets.");
        }

        // Samples should explain what they demonstrate
        var textContent = GetTextContent(document).ToLowerInvariant();
        var hasFeatureExplanation = textContent.Contains("demonstrates") ||
                                  textContent.Contains("shows") ||
                                  textContent.Contains("example") ||
                                  textContent.Contains("illustrates");

        if (!hasFeatureExplanation)
        {
            Assert.Fail($"Sample documentation {relativePath} should clearly explain what functionality it demonstrates.");
        }
    }

    /// <summary>
    /// Provides test data for the content quality validation tests.
    /// </summary>
    public static IEnumerable<object[]> DiscoverMarkdownFiles()
    {
        return DocumentationHelper.DiscoverMarkdownFiles();
    }

    /// <summary>
    /// Extracts and parses YAML frontmatter from markdown content.
    /// </summary>
    private Dictionary<string, object>? ExtractAndParseFrontmatter(string content)
    {
        var lines = content.Split('\n');
        
        // Check if file starts with frontmatter delimiter
        if (lines.Length < 2 || !lines[0].Trim().Equals("---"))
        {
            return null;
        }

        // Find the closing delimiter
        var endIndex = -1;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim().Equals("---"))
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex == -1)
        {
            return null;
        }

        // Extract and parse frontmatter
        var frontmatterContent = string.Join("\n", lines.Skip(1).Take(endIndex - 1));
        try
        {
            return _yamlDeserializer.Deserialize<Dictionary<string, object>>(frontmatterContent);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if the document is API documentation based on frontmatter.
    /// </summary>
    private static bool IsApiDocumentation(Dictionary<string, object> frontmatter)
    {
        return frontmatter.TryGetValue("type", out var type) && 
               type?.ToString()?.ToLowerInvariant() == "api";
    }

    /// <summary>
    /// Determines if the document is tutorial documentation based on frontmatter.
    /// </summary>
    private static bool IsTutorialDocumentation(Dictionary<string, object> frontmatter)
    {
        return frontmatter.TryGetValue("type", out var type) && 
               type?.ToString()?.ToLowerInvariant() == "tutorial";
    }

    /// <summary>
    /// Determines if the document is sample documentation based on frontmatter.
    /// </summary>
    private static bool IsSampleDocumentation(Dictionary<string, object> frontmatter)
    {
        return frontmatter.TryGetValue("type", out var type) && 
               type?.ToString()?.ToLowerInvariant() == "sample";
    }

    /// <summary>
    /// Extracts the text content from a heading block.
    /// </summary>
    private static string GetHeadingText(HeadingBlock heading)
    {
        if (heading.Inline == null) return "";
        
        var text = heading.Inline.ToString();
        return text ?? "";
    }

    /// <summary>
    /// Extracts all text content from the document.
    /// </summary>
    private static string GetTextContent(MarkdownDocument document)
    {
        // This is a simplified approach - in a more sophisticated implementation,
        // we might walk the AST more carefully
        return document.ToString() ?? "";
    }

    /// <summary>
    /// Gets the relative path from the repository root for display purposes.
    /// </summary>
    private static string GetRelativePath(string fullPath)
    {
        var repositoryRoot = DocumentationHelper.GetRepositoryRoot();
        return Path.GetRelativePath(repositoryRoot, fullPath);
    }
}