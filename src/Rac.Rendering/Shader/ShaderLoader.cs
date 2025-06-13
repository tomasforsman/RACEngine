using System.Collections.Concurrent;
using System.Reflection;

namespace Rac.Rendering.Shader;

public static class ShaderLoader
{
    private static readonly ConcurrentDictionary<string, string> _cache = new();
    private static string? _shaderDirectory;

    public static string LoadVertexShader()
    {
        return LoadShaderFromFile("vertex.glsl");
    }

    public static string LoadFragmentShader(ShaderMode mode)
    {
        var filename = GetFragmentShaderFilename(mode);
        return LoadShaderFromFile(filename);
    }

    public static string LoadShaderFromFile(string filename)
    {
        if (_cache.TryGetValue(filename, out var cached))
            return cached;

        var directory = GetShaderDirectory();
        var fullPath = Path.Combine(directory, filename);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Shader file not found: {fullPath}");

        var content = File.ReadAllText(fullPath);
        _cache[filename] = content;
        return content;
    }

    public static void ClearCache()
    {
        _cache.Clear();
        _shaderDirectory = null;
    }

    public static ShaderMode[] DiscoverAvailableShaderModes()
    {
        var availableModes = new List<ShaderMode>();
        var shaderDirectory = GetShaderDirectory();

        try
        {
            var shaderFiles = Directory.GetFiles(shaderDirectory, "*.frag");

            foreach (var filePath in shaderFiles)
            {
                var filename = Path.GetFileNameWithoutExtension(filePath);

                if (Enum.TryParse<ShaderMode>(filename, ignoreCase: true, out var mode))
                {
                    availableModes.Add(mode);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to discover shader modes: {ex.Message}");
        }

        return availableModes.ToArray();
    }

    public static bool IsShaderModeAvailable(ShaderMode mode)
    {
        try
        {
            var filename = GetFragmentShaderFilename(mode);
            var fullPath = Path.Combine(GetShaderDirectory(), filename);
            return File.Exists(fullPath);
        }
        catch
        {
            return false;
        }
    }

    public static Dictionary<ShaderMode, ShaderAvailability> GetShaderAvailabilityReport()
    {
        var report = new Dictionary<ShaderMode, ShaderAvailability>();
        var shaderDirectory = GetShaderDirectory();

        foreach (ShaderMode mode in Enum.GetValues<ShaderMode>())
        {
            var filename = GetFragmentShaderFilename(mode);
            var fullPath = Path.Combine(shaderDirectory, filename);

            var availability = new ShaderAvailability
            {
                Mode = mode,
                Filename = filename,
                FullPath = fullPath,
                Exists = File.Exists(fullPath),
                IsAvailable = File.Exists(fullPath)
            };

            report[mode] = availability;
        }

        return report;
    }

    public static ShaderDirectoryStatus ValidateShaderDirectory()
    {
        var status = new ShaderDirectoryStatus();

        try
        {
            status.DirectoryPath = GetShaderDirectory();
            status.Exists = Directory.Exists(status.DirectoryPath);

            if (status.Exists)
            {
                var files = Directory.GetFiles(status.DirectoryPath, "*.frag");
                status.FragmentShaderCount = files.Length;

                status.HasVertexShader = File.Exists(Path.Combine(status.DirectoryPath, "vertex.glsl"));

                status.Status = status.HasVertexShader && status.FragmentShaderCount > 0
                    ? "Ready"
                    : "Missing required shaders";
            }
            else
            {
                status.Status = "Shader directory not found";
            }
        }
        catch (Exception ex)
        {
            status.Status = $"Error accessing shader directory: {ex.Message}";
        }

        return status;
    }

    private static string GetFragmentShaderFilename(ShaderMode mode)
    {
        return mode switch
        {
            ShaderMode.Normal => "normal.frag",
            ShaderMode.SoftGlow => "softglow.frag",
            ShaderMode.Bloom => "bloom.frag",
            _ => "normal.frag"
        };
    }

    private static string GetShaderDirectory()
    {
        if (_shaderDirectory != null)
            return _shaderDirectory;

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        var searchPaths = new[]
        {
            Path.Combine(assemblyDirectory!, "Shader", "Files"),
            Path.Combine(Directory.GetCurrentDirectory(), "Shader", "Files"),
            SearchUpForShaderDirectory(assemblyDirectory!),
            Path.Combine(assemblyDirectory!, "Assets", "Shaders"),
            Path.Combine(assemblyDirectory!, "Shaders")
        };

        foreach (var path in searchPaths.Where(p => p != null))
        {
            if (Directory.Exists(path))
            {
                _shaderDirectory = path;
                return path;
            }
        }

        var defaultPath = Path.Combine(assemblyDirectory!, "Shader", "Files");
        _shaderDirectory = defaultPath;
        return defaultPath;
    }

    private static string? SearchUpForShaderDirectory(string startDirectory)
    {
        var currentDir = startDirectory;
        for (int i = 0; i < 5; i++)
        {
            var testPath = Path.Combine(currentDir, "Shader", "Files");
            if (Directory.Exists(testPath))
                return testPath;

            var parentDir = Path.GetDirectoryName(currentDir);
            if (parentDir == null) break;
            currentDir = parentDir;
        }
        return null;
    }
}

public class ShaderAvailability
{
    public ShaderMode Mode { get; set; }
    public string Filename { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool Exists { get; set; }
    public bool IsAvailable { get; set; }
}

public class ShaderDirectoryStatus
{
    public string DirectoryPath { get; set; } = "";
    public bool Exists { get; set; }
    public bool HasVertexShader { get; set; }
    public int FragmentShaderCount { get; set; }
    public string Status { get; set; } = "";
}