using Json.Schema;
using System.Text.Json;
using YamlDotNet.Serialization;
using Xunit;

namespace DocumentationTests;

/// <summary>
/// Tests that validate YAML frontmatter in markdown files against the JSON schema.
/// </summary>
public class FrontmatterSchemaTests
{
    private static readonly JsonSchema _schema = LoadSchema();
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

    /// <summary>
    /// Loads the JSON schema for frontmatter validation.
    /// </summary>
    private static JsonSchema LoadSchema()
    {
        var schemaPath = DocumentationHelper.GetSchemaPath();
        var schemaJson = File.ReadAllText(schemaPath);
        return JsonSchema.FromText(schemaJson);
    }

    /// <summary>
    /// Validates that each markdown file's frontmatter conforms to the JSON schema.
    /// </summary>
    /// <param name="filePath">Path to the markdown file to validate</param>
    [Theory]
    [MemberData(nameof(DiscoverMarkdownFiles))]
    public void ValidateSchema(string filePath)
    {
        // Arrange
        var content = File.ReadAllText(filePath);
        var frontmatter = ExtractFrontmatter(content);
        
        // Skip files without frontmatter - they may be valid (like simple README files)
        if (frontmatter == null)
        {
            return;
        }

        // Act & Assert
        try
        {
            var jsonDocument = ConvertYamlToJson(frontmatter);
            var validationResult = _schema.Evaluate(jsonDocument);
            
            if (!validationResult.IsValid)
            {
                var errorMessages = FormatValidationErrors(validationResult);
                var relativePath = GetRelativePath(filePath);
                Assert.Fail($"Schema validation failed for {relativePath}:\n{errorMessages}");
            }
        }
        catch (Exception ex)
        {
            var relativePath = GetRelativePath(filePath);
            Assert.Fail($"Failed to parse frontmatter in {relativePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Provides test data for the schema validation test.
    /// </summary>
    public static IEnumerable<object[]> DiscoverMarkdownFiles()
    {
        return DocumentationHelper.DiscoverMarkdownFiles();
    }

    /// <summary>
    /// Extracts YAML frontmatter from markdown content.
    /// </summary>
    private static string? ExtractFrontmatter(string content)
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
            return null; // No closing delimiter found
        }

        // Extract frontmatter content (excluding delimiters)
        var frontmatterLines = lines.Skip(1).Take(endIndex - 1);
        return string.Join("\n", frontmatterLines);
    }

    /// <summary>
    /// Converts YAML content to a JsonDocument for schema validation.
    /// </summary>
    private JsonDocument ConvertYamlToJson(string yamlContent)
    {
        var yamlObject = _yamlDeserializer.Deserialize(yamlContent);
        var jsonString = JsonSerializer.Serialize(yamlObject);
        return JsonDocument.Parse(jsonString);
    }

    /// <summary>
    /// Formats validation errors into a readable string.
    /// </summary>
    private static string FormatValidationErrors(EvaluationResults results)
    {
        var errors = new List<string>();
        CollectErrors(results, errors);
        return string.Join("\n", errors);
    }

    /// <summary>
    /// Recursively collects validation errors from the results.
    /// </summary>
    private static void CollectErrors(EvaluationResults results, List<string> errors)
    {
        if (!results.IsValid)
        {
            if (results.HasErrors)
            {
                foreach (var error in results.Errors!)
                {
                    errors.Add($"  - {error.Key}: {error.Value}");
                }
            }
            
            if (results.HasDetails)
            {
                foreach (var detail in results.Details!)
                {
                    CollectErrors(detail, errors);
                }
            }
        }
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