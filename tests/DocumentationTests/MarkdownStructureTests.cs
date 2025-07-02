using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests that validate markdown structure and formatting requirements.
/// </summary>
public class MarkdownStructureTests
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Validates that each markdown file has proper heading hierarchy and structure.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateHeadingHierarchy(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var document = Markdown.Parse(content, _pipeline);
        var relativePath = GetRelativePath(filePath);

        // Act
        var headings = document.Descendants<HeadingBlock>().ToList();
        
        // Assert - Basic structure requirements
        if (headings.Count == 0)
        {
            // Files without headings are acceptable (e.g., simple README files)
            return;
        }

        // Check for single H1
        var h1Headings = headings.Where(h => h.Level == 1).ToList();
        if (h1Headings.Count > 1)
        {
            Assert.Fail($"Multiple H1 headings found in {relativePath}. Should have exactly one H1.");
        }

        // Check heading progression (no skipping levels)
        for (int i = 1; i < headings.Count; i++)
        {
            var currentLevel = headings[i].Level;
            var previousLevel = headings[i - 1].Level;
            
            // Allow same level or one level deeper, or any level shallower
            if (currentLevel > previousLevel + 1)
            {
                var currentHeading = GetHeadingText(headings[i]);
                var previousHeading = GetHeadingText(headings[i - 1]);
                Assert.Fail($"Invalid heading hierarchy in {relativePath}: " +
                          $"H{currentLevel} '{currentHeading}' follows H{previousLevel} '{previousHeading}'. " +
                          $"Cannot skip heading levels.");
            }
        }
    }

    /// <summary>
    /// Validates that code blocks specify their language for proper syntax highlighting.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateCodeBlockLanguages(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var document = Markdown.Parse(content, _pipeline);
        var relativePath = GetRelativePath(filePath);

        // Act
        var codeBlocks = document.Descendants<FencedCodeBlock>().ToList();
        
        if (codeBlocks.Count == 0)
        {
            return; // No code blocks to validate
        }

        // Assert
        var unspecifiedBlocks = codeBlocks
            .Where(block => string.IsNullOrWhiteSpace(block.Info))
            .ToList();

        if (unspecifiedBlocks.Any())
        {
            var blockCount = unspecifiedBlocks.Count;
            var totalCount = codeBlocks.Count;
            Assert.Fail($"Found {blockCount} out of {totalCount} code blocks without language specification in {relativePath}. " +
                      "All code blocks should specify their language (e.g., ```csharp, ```javascript, ```bash).");
        }
    }

    /// <summary>
    /// Validates minimum content requirements for documentation files.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateMinimumContent(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var document = Markdown.Parse(content, _pipeline);
        var relativePath = GetRelativePath(filePath);

        // Skip frontmatter for content length calculation
        var contentWithoutFrontmatter = RemoveFrontmatter(content);
        
        // Act & Assert - Basic content requirements
        if (string.IsNullOrWhiteSpace(contentWithoutFrontmatter))
        {
            Assert.Fail($"File {relativePath} appears to be empty or contains only frontmatter.");
        }

        // Check for minimum meaningful content (more than just a title)
        var textContent = GetTextContent(document);
        if (textContent.Length < 50) // Minimum threshold for meaningful content
        {
            Assert.Fail($"File {relativePath} has insufficient content. " +
                      "Documentation should provide meaningful information beyond just a title.");
        }

        // Check for at least some structure (headings, paragraphs, lists, etc.)
        var structuralElements = document.Descendants()
            .Where(block => block is HeadingBlock || 
                           block is ParagraphBlock || 
                           block is ListBlock ||
                           block is FencedCodeBlock)
            .Count();

        if (structuralElements < 2)
        {
            Assert.Fail($"File {relativePath} lacks proper structure. " +
                      "Documentation should have multiple structural elements (headings, paragraphs, lists, code blocks).");
        }
    }

    /// <summary>
    /// Provides test data for the structure validation tests.
    /// </summary>
    public static IEnumerable<object[]> DiscoverMarkdownFiles()
    {
        return DocumentationHelper.DiscoverMarkdownFiles();
    }

    /// <summary>
    /// Extracts the text content from a heading block.
    /// </summary>
    private static string GetHeadingText(HeadingBlock heading)
    {
        var inline = heading.Inline;
        if (inline == null) return "";
        
        return string.Join("", inline.Descendants<LiteralInline>().Select(l => l.Content.ToString()));
    }

    /// <summary>
    /// Extracts all text content from the document for content length validation.
    /// </summary>
    private static string GetTextContent(MarkdownDocument document)
    {
        var textParts = new List<string>();
        
        foreach (var literal in document.Descendants<LiteralInline>())
        {
            textParts.Add(literal.Content.ToString());
        }
        
        return string.Join(" ", textParts);
    }

    /// <summary>
    /// Removes YAML frontmatter from content for content analysis.
    /// </summary>
    private static string RemoveFrontmatter(string content)
    {
        var lines = content.Split('\n');
        
        // Check if file starts with frontmatter delimiter
        if (lines.Length < 2 || !lines[0].Trim().Equals("---"))
        {
            return content;
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
            return content; // No closing delimiter found, treat as regular content
        }

        // Return content after frontmatter
        return string.Join("\n", lines.Skip(endIndex + 1));
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