// ═══════════════════════════════════════════════════════════════════════════════
// DYNAMIC SHADER DISCOVERY AND LOADING SYSTEM
// ═══════════════════════════════════════════════════════════════════════════════
//
// This static class implements a comprehensive shader asset management system with
// automatic discovery, caching, and hot-reload capabilities for development workflows.
//
// KEY GRAPHICS PROGRAMMING CONCEPTS:
// - Shader asset pipeline: File system → Memory cache → GPU compilation
// - Development workflow: Hot-reload support for iterative shader development
// - Resource management: Intelligent caching to avoid redundant file I/O
// - Error resilience: Graceful handling of missing or invalid shader files
//
// EDUCATIONAL FEATURES:
// - File system abstraction for cross-platform shader loading
// - Caching strategies for performance optimization
// - Discovery patterns for extensible asset systems
// - Error handling patterns for robust graphics applications
//
// ARCHITECTURE BENEFITS:
// - Separation of concerns: Loading logic isolated from rendering logic
// - Extensibility: New shader modes automatically discovered
// - Performance: Caching eliminates redundant file operations
// - Development experience: Clear error reporting and validation
//
// ═══════════════════════════════════════════════════════════════════════════════

using System.Collections.Concurrent;
using System.Reflection;

namespace Rac.Rendering.Shader;

/// <summary>
/// Advanced shader asset management system with automatic discovery and intelligent caching.
/// 
/// TECHNICAL CAPABILITIES:
/// - Automatic shader file discovery across multiple search paths
/// - Thread-safe caching for high-performance repeated access
/// - Development-friendly hot-reload support through cache invalidation
/// - Comprehensive validation and error reporting for debugging
/// 
/// GRAPHICS PIPELINE INTEGRATION:
/// - Provides shader source code to OpenGL compiler
/// - Supports multiple shader modes through file-based organization
/// - Enables runtime shader switching and effect variations
/// - Facilitates shader development through clear file organization
/// 
/// PERFORMANCE OPTIMIZATIONS:
/// - ConcurrentDictionary for thread-safe cached access
/// - Lazy directory resolution with persistent caching
/// - Single file read with persistent memory storage
/// - Efficient shader mode availability checking
/// 
/// DEVELOPMENT WORKFLOW:
/// - Hot-reload support through cache clearing
/// - Automatic shader discovery for new effect files
/// - Comprehensive validation reporting for troubleshooting
/// - Cross-platform path resolution for portability
/// </summary>
public static class ShaderLoader
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CACHING AND DIRECTORY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Thread-safe cache for shader source code with lazy loading and directory
    // resolution to optimize performance in multi-threaded rendering scenarios.
    
    private static readonly ConcurrentDictionary<string, string> _cache = new();
    private static string? _shaderDirectory;

    // ───────────────────────────────────────────────────────────────────────────
    // STANDARD SHADER LOADING INTERFACE
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the standard vertex shader used across all rendering modes.
    /// 
    /// VERTEX SHADER PURPOSE:
    /// - Transforms vertex positions from model to screen space
    /// - Handles projection and view matrix transformations
    /// - Passes attributes (color, texture coords) to fragment shader
    /// - Shared across all visual effects for consistency
    /// </summary>
    /// <returns>GLSL vertex shader source code</returns>
    public static string LoadVertexShader()
    {
        return LoadShaderFromFile("vertex.glsl");
    }

    /// <summary>
    /// Loads the fragment shader for a specific visual effect mode.
    /// 
    /// FRAGMENT SHADER MODES:
    /// - Normal: Standard pixel coloring with basic lighting
    /// - SoftGlow: Subtle glow effects with distance-based falloff
    /// - Bloom: HDR bloom with bright pixel extraction and blending
    /// 
    /// EDUCATIONAL NOTE:
    /// Different fragment shaders create dramatically different visual results
    /// from the same vertex data, demonstrating the power of programmable shaders.
    /// </summary>
    /// <param name="mode">Visual effect mode determining which fragment shader to load</param>
    /// <returns>GLSL fragment shader source code for the specified mode</returns>
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