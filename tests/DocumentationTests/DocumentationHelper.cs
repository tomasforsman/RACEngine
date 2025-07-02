using System.Reflection;

namespace DocumentationTests;

/// <summary>
/// Helper class for discovering and validating markdown documentation files.
/// </summary>
public static class DocumentationHelper
{
    /// <summary>
    /// Gets the root directory of the repository by walking up from the test assembly location.
    /// </summary>
    public static string GetRepositoryRoot()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)!);
        
        // Walk up the directory structure to find the repository root
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RACEngine.sln")))
        {
            directory = directory.Parent;
        }
        
        if (directory == null)
        {
            throw new InvalidOperationException("Could not find repository root (RACEngine.sln not found)");
        }
        
        return directory.FullName;
    }

    /// <summary>
    /// Discovers all markdown files in the specified directories relative to the repository root.
    /// </summary>
    public static IEnumerable<object[]> DiscoverMarkdownFiles()
    {
        var repositoryRoot = GetRepositoryRoot();
        var searchDirectories = new[] { "docs", "samples", "tests", "src" };
        
        var markdownFiles = new List<string>();
        
        foreach (var searchDir in searchDirectories)
        {
            var fullSearchPath = Path.Combine(repositoryRoot, searchDir);
            if (Directory.Exists(fullSearchPath))
            {
                markdownFiles.AddRange(
                    Directory.GetFiles(fullSearchPath, "*.md", SearchOption.AllDirectories)
                        .Where(file => !ShouldExcludeFile(file))
                );
            }
        }
        
        return markdownFiles.Select(file => new object[] { file }).ToArray();
    }

    /// <summary>
    /// Determines if a file should be excluded from validation (e.g., generated files, obj directories).
    /// </summary>
    private static bool ShouldExcludeFile(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');
        
        // Exclude common build artifacts and temporary directories
        var excludePatterns = new[]
        {
            "/bin/",
            "/obj/",
            "/.git/",
            "/node_modules/",
            "/.vs/",
            "/packages/"
        };
        
        return excludePatterns.Any(pattern => normalizedPath.Contains(pattern));
    }

    /// <summary>
    /// Gets the path to the documentation frontmatter schema file.
    /// </summary>
    public static string GetSchemaPath()
    {
        var repositoryRoot = GetRepositoryRoot();
        return Path.Combine(repositoryRoot, "schemas", "documentation-frontmatter.json");
    }
}