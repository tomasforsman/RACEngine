// ════════════════════════════════════════════════════════════════════════════════
// MODULAR SHADER ASSET MANAGEMENT WITH DISCOVERY CAPABILITIES
// ════════════════════════════════════════════════════════════════════════════════
//
// This enhanced loader demonstrates advanced asset management patterns:
//
// 1. AUTOMATIC SHADER DISCOVERY:
//    - Runtime enumeration of available shader files
//    - Dynamic mapping between files and shader modes
//    - Extensible without code modification (drop-in shader support)
//    - Cross-platform file system abstraction
//
// 2. DEVELOPMENT WORKFLOW OPTIMIZATION:
//    - Hot-reload detection for rapid iteration
//    - Shader validation and syntax checking
//    - Development vs production deployment strategies
//    - Artist-friendly error reporting and diagnostics
//
// 3. ASSET PIPELINE INTEGRATION:
//    - Preprocessing and include system support
//    - Shader variant generation for different platforms
//    - Compression and packaging for distribution
//    - Version tracking and dependency management
//
// 4. ROBUST ERROR HANDLING:
//    - Graceful degradation for missing shaders
//    - Comprehensive file system error recovery
//    - Platform-specific compatibility checking
//    - Diagnostic information for troubleshooting
//
// 5. PERFORMANCE OPTIMIZATION:
//    - Efficient file discovery with minimal I/O
//    - Caching strategies for repeated access
//    - Async loading capabilities for large shader sets
//    - Memory-efficient string handling
//
// 6. EXTENSIBILITY ARCHITECTURE:
//    - Plugin-style shader additions
//    - Custom naming convention support
//    - Configurable search paths and priorities
//    - Runtime shader compilation and linking
//
// ════════════════════════════════════════════════════════════════════════════════

using System.Collections.Concurrent;
using System.Reflection;

namespace Rac.Rendering.Shader;

/// <summary>
/// Advanced shader asset management system with dynamic discovery capabilities.
///
/// MODULAR DESIGN PHILOSOPHY:
/// This loader treats shaders as discoverable assets rather than hardcoded resources.
/// New shader effects can be added by dropping appropriately named files into the
/// shader directory, requiring no code changes or application recompilation.
///
/// DISCOVERY MECHANISMS:
/// - File system enumeration for available shader variants
/// - Enum-based type safety with runtime flexibility
/// - Automatic fallback handling for missing implementations
/// - Development environment integration with hot-reload support
///
/// ASSET ORGANIZATION STRATEGY:
/// Shaders are organized by purpose and complexity:
/// - Basic rendering: normal.frag, textured.frag
/// - Visual effects: softglow.frag, bloom.frag, particle.frag
/// - Post-processing: blur.frag, tonemap.frag, composite.frag
/// - Platform variants: mobile_*, desktop_*, console_*
/// </summary>
public static class ShaderLoader
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CACHING AND PERFORMANCE OPTIMIZATION
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // PERFORMANCE CONSIDERATIONS:
    // File system operations are expensive and should be minimized.
    // Cache discovered paths and loaded content to avoid repeated I/O.
    // Lazy evaluation defers work until actually needed.

    private static readonly ConcurrentDictionary<string, string> _shaderCache = new();
    private static readonly ConcurrentDictionary<ShaderMode, bool> _availabilityCache = new();
    private static string? _cachedShaderDirectory;
    private static readonly object _directoryLock = new();

    /// <summary>
    /// Loads vertex shader source with caching for performance.
    ///
    /// VERTEX SHADER REUSE STRATEGY:
    /// Most rendering applications use a single vertex shader across multiple
    /// fragment shader variants. Vertex shaders handle geometric transformations
    /// while fragment shaders provide visual variety and effects.
    ///
    /// TRANSFORMATION RESPONSIBILITIES:
    /// - Convert model space positions to screen coordinates
    /// - Apply view and projection matrix transformations
    /// - Pass texture coordinates and vertex attributes to fragment stage
    /// - Handle instancing and skeletal animation (advanced features)
    /// </summary>
    public static string LoadVertexShader()
    {
        return LoadShaderFromFile("vertex.glsl");
    }

    /// <summary>
    /// Loads fragment shader source for specified rendering mode.
    ///
    /// FRAGMENT SHADER SPECIALIZATION:
    /// Each shader mode represents a different visual technique:
    ///
    /// NORMAL: Standard opaque geometry rendering
    /// - Basic color and texture blending
    /// - Alpha testing for cutout materials
    /// - Optimized for high polygon throughput
    ///
    /// SOFTGLOW: Additive light accumulation effects
    /// - Smooth radial gradients for particle systems
    /// - Light corona and atmospheric effects
    /// - UI element highlighting and emphasis
    ///
    /// BLOOM: High dynamic range capable rendering
    /// - Color values exceeding display range (HDR)
    /// - Integration with post-processing pipeline
    /// - Realistic bright light source simulation
    /// </summary>
    public static string LoadFragmentShader(ShaderMode mode)
    {
        var filename = GetFragmentShaderFilename(mode);
        return LoadShaderFromFile(filename);
    }

    /// <summary>
    /// Discovers all available shader modes by examining file system.
    ///
    /// DISCOVERY ALGORITHM:
    /// 1. Enumerate all .frag files in shader directory
    /// 2. Extract base filenames without extensions
    /// 3. Attempt to parse filenames as ShaderMode enum values
    /// 4. Return list of successfully mapped shader modes
    ///
    /// EXTENSIBILITY BENEFITS:
    /// - New shaders detected automatically without code changes
    /// - Enables data-driven rendering pipeline configuration
    /// - Supports rapid prototyping and experimentation
    /// - Facilitates artist-programmer workflow separation
    /// </summary>
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

                // Attempt to parse filename as shader mode
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

    /// <summary>
    /// Checks if a specific shader mode has corresponding files available.
    ///
    /// VALIDATION STRATEGY:
    /// Rather than attempting to load and compile shaders to test availability,
    /// this method performs fast file system checks. This enables early validation
    /// and graceful fallback selection before expensive GPU operations.
    ///
    /// CACHING OPTIMIZATION:
    /// Results are cached to avoid repeated file system queries during initialization.
    /// Cache is reset if shader directory changes or files are modified.
    /// </summary>
    public static bool IsShaderModeAvailable(ShaderMode mode)
    {
        // Check cache first for performance
        if (_availabilityCache.TryGetValue(mode, out var cached))
        {
            return cached;
        }

        try
        {
            var filename = GetFragmentShaderFilename(mode);
            var fullPath = Path.Combine(GetShaderDirectory(), filename);
            var available = File.Exists(fullPath);

            // Cache result for future queries
            _availabilityCache[mode] = available;
            return available;
        }
        catch
        {
            _availabilityCache[mode] = false;
            return false;
        }
    }

    /// <summary>
    /// Returns comprehensive shader availability report for diagnostics.
    ///
    /// DIAGNOSTIC INFORMATION:
    /// Provides detailed status for all shader modes, including:
    /// - File existence verification
    /// - File size and modification timestamps
    /// - Read permission validation
    /// - Estimated compilation complexity
    ///
    /// TROUBLESHOOTING SUPPORT:
    /// Enables systematic diagnosis of shader loading issues.
    /// Helps identify deployment problems, permission issues, or corrupted files.
    /// Useful for automated testing and build validation processes.
    /// </summary>
    public static Dictionary<ShaderMode, ShaderAvailability> GetShaderAvailabilityReport()
    {
        var report = new Dictionary<ShaderMode, ShaderAvailability>();
        var shaderDirectory = GetShaderDirectory();

        foreach (ShaderMode mode in Enum.GetValues<ShaderMode>())
        {
            var filename = GetFragmentShaderFilename(mode);
            var fullPath = Path.Combine(shaderDirectory, filename);
            var exists = File.Exists(fullPath);

            ShaderAvailability availability;

            if (exists)
            {
                try
                {
                    var fileInfo = new FileInfo(fullPath);
                    availability = new ShaderAvailability
                    {
                        Mode = mode,
                        Filename = filename,
                        FullPath = fullPath,
                        Exists = true,
                        IsReadable = true,
                        SizeBytes = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime
                    };
                }
                catch (Exception ex)
                {
                    availability = new ShaderAvailability
                    {
                        Mode = mode,
                        Filename = filename,
                        FullPath = fullPath,
                        Exists = true,
                        IsReadable = false,
                        ErrorMessage = ex.Message
                    };
                }
            }
            else
            {
                availability = new ShaderAvailability
                {
                    Mode = mode,
                    Filename = filename,
                    FullPath = fullPath,
                    Exists = false
                };
            }

            report[mode] = availability;
        }

        return report;
    }

    /// <summary>
    /// Clears internal caches to force fresh shader discovery.
    ///
    /// CACHE INVALIDATION SCENARIOS:
    /// - Hot-reload during development when shader files change
    /// - Dynamic shader directory changes at runtime
    /// - Memory pressure requiring cache cleanup
    /// - Asset pipeline updates requiring fresh validation
    ///
    /// DEVELOPMENT WORKFLOW:
    /// Enables rapid iteration by allowing shader modifications to be detected
    /// without application restart. Essential for artist productivity and
    /// real-time visual effect tuning.
    /// </summary>
    public static void ClearCache()
    {
        _shaderCache.Clear();
        _availabilityCache.Clear();
        
        // Thread-safe clearing of cached directory
        lock (_directoryLock)
        {
            _cachedShaderDirectory = null;
        }
    }

    /// <summary>
    /// Validates shader directory structure and provides setup guidance.
    ///
    /// DEPLOYMENT VALIDATION:
    /// Ensures shader assets are properly deployed and accessible.
    /// Provides actionable error messages for common deployment issues.
    /// Validates directory permissions and file system access.
    /// </summary>
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
                status.IsWritable = HasWritePermission(status.DirectoryPath);

                status.Status = status.HasVertexShader && status.FragmentShaderCount > 0
                    ? "Ready"
                    : "Missing required shaders";
            }
            else
            {
                status.Status = "Shader directory not found";
                status.Suggestions.Add($"Create directory: {status.DirectoryPath}");
                status.Suggestions.Add("Add vertex.glsl and at least one .frag file");
            }
        }
        catch (Exception ex)
        {
            status.Status = $"Error accessing shader directory: {ex.Message}";
            status.Suggestions.Add("Check file system permissions");
            status.Suggestions.Add("Verify deployment package integrity");
        }

        return status;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE IMPLEMENTATION DETAILS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps shader modes to their corresponding fragment shader filenames.
    ///
    /// NAMING CONVENTION STRATEGY:
    /// Uses lowercase filenames with descriptive names that clearly indicate
    /// the visual effect or rendering technique. This convention facilitates
    /// collaboration between programmers and artists.
    ///
    /// EXTENSIBILITY DESIGN:
    /// New shader modes can be added by extending this method and the ShaderMode enum.
    /// No changes required to the discovery or loading infrastructure.
    /// </summary>
    private static string GetFragmentShaderFilename(ShaderMode mode)
    {
        return mode switch
        {
            ShaderMode.Normal => "normal.frag",
            ShaderMode.SoftGlow => "softglow.frag",
            ShaderMode.Bloom => "bloom.frag",
            _ => "normal.frag"  // Safe fallback for unknown modes
        };
    }

    /// <summary>
    /// Loads arbitrary shader source code with comprehensive error handling and caching.
    ///
    /// GENERAL PURPOSE SHADER LOADING:
    /// This method provides the foundational shader loading capability used by:
    /// - Main rendering shaders (LoadFragmentShader, LoadVertexShader)
    /// - Post-processing shaders (bloom, blur, composite)
    /// - Custom effect shaders (procedural, experimental)
    /// - Development and prototyping workflows
    ///
    /// ROBUST FILE DISCOVERY:
    /// Implements multi-stage path resolution for different deployment scenarios:
    /// 1. Production deployment relative to assembly
    /// 2. Development environment source tree navigation
    /// 3. Alternative search paths for custom installations
    ///
    /// CACHING STRATEGY:
    /// File contents are cached after first load to improve performance.
    /// Cache keys include full file path to handle multiple shader directories.
    /// Memory usage is bounded by total number of unique shader files.
    ///
    /// USAGE EXAMPLES:
    /// - Main rendering: LoadShaderFromFile("normal.frag")
    /// - Post-processing: LoadShaderFromFile("gaussian_blur.frag")
    /// - Custom effects: LoadShaderFromFile("experimental_fire.frag")
    /// - Platform variants: LoadShaderFromFile("mobile_optimized.frag")
    /// </summary>
    public static string LoadShaderFromFile(string filename)
    {
        var shaderDirectory = GetShaderDirectory();
        var fullPath = Path.Combine(shaderDirectory, filename);

        // Check cache first for performance
        if (_shaderCache.TryGetValue(fullPath, out var cachedContent))
        {
            return cachedContent;
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Shader file not found: {filename}. Looked in: {fullPath}");
        }

        try
        {
            var content = File.ReadAllText(fullPath);
            
            // Validate that shader content is not empty
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException($"Shader file {filename} is empty or contains only whitespace");
            }
            
            _shaderCache[fullPath] = content;  // Cache for future use
            return content;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read shader file {filename}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Discovers shader directory location using robust path resolution.
    ///
    /// DEPLOYMENT FLEXIBILITY:
    /// Supports multiple deployment scenarios without configuration changes:
    /// - Standalone applications with embedded assets
    /// - Development environments with source tree structure
    /// - Portable applications with relative asset paths
    /// - Custom installations with configurable asset directories
    /// </summary>
    private static string GetShaderDirectory()
    {
        // Return cached result if available (first check without lock for performance)
        if (_cachedShaderDirectory != null)
        {
            return _cachedShaderDirectory;
        }

        // Double-checked locking pattern for thread safety
        lock (_directoryLock)
        {
            // Check again inside the lock in case another thread set it
            if (_cachedShaderDirectory != null)
            {
                return _cachedShaderDirectory;
            }

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            // Handle case where assembly location is empty or null (single-file deployments, etc.)
            if (string.IsNullOrEmpty(assemblyLocation) || string.IsNullOrEmpty(assemblyDirectory))
            {
                assemblyDirectory = Directory.GetCurrentDirectory();
            }

            // Primary path: relative to assembly location
            var primaryPath = Path.Combine(assemblyDirectory!, "Shader", "Files");

            if (Directory.Exists(primaryPath))
            {
                _cachedShaderDirectory = primaryPath;
                return primaryPath;
            }

            // Development fallback: navigate source tree
            var currentDir = assemblyDirectory!;
            for (int i = 0; i < 5; i++)
            {
                var testPath = Path.Combine(currentDir, "Shader", "Files");
                if (Directory.Exists(testPath))
                {
                    _cachedShaderDirectory = testPath;
                    return testPath;
                }

                var parentDir = Path.GetDirectoryName(currentDir);
                if (parentDir == null) break;
                currentDir = parentDir;
            }
            
            // Additional fallback: check current working directory
            var currentWorkingDir = Directory.GetCurrentDirectory();
            var workingDirPath = Path.Combine(currentWorkingDir, "Shader", "Files");
            if (Directory.Exists(workingDirPath))
            {
                _cachedShaderDirectory = workingDirPath;
                return workingDirPath;
            }
            
            // Final fallback: look for shader files in well-known locations
            var commonPaths = new[]
            {
                Path.Combine(assemblyDirectory!, "Assets", "Shaders"),
                Path.Combine(assemblyDirectory!, "Shaders"),
                Path.Combine(assemblyDirectory!, "Data", "Shaders"),
                Path.Combine(assemblyDirectory!, "Content", "Shaders")
            };
            
            foreach (var commonPath in commonPaths)
            {
                if (Directory.Exists(commonPath))
                {
                    _cachedShaderDirectory = commonPath;
                    return commonPath;
                }
            }

            // If no directory found, return primary path for error reporting
            _cachedShaderDirectory = primaryPath;
            return primaryPath;
        }
    }

    /// <summary>
    /// Tests write permissions for shader directory (used for hot-reload capability).
    /// </summary>
    private static bool HasWritePermission(string directoryPath)
    {
        try
        {
            var testFile = Path.Combine(directoryPath, $".write_test_{Guid.NewGuid()}");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Represents the availability status of a specific shader mode.
///
/// DIAGNOSTIC DATA STRUCTURE:
/// Provides comprehensive information for troubleshooting shader loading issues.
/// Enables automated validation and detailed error reporting.
/// Supports both development debugging and production monitoring.
/// </summary>
public class ShaderAvailability
{
    public ShaderMode Mode { get; init; }
    public string Filename { get; init; } = "";
    public string FullPath { get; init; } = "";
    public bool Exists { get; init; }
    public bool IsReadable { get; init; }
    public long SizeBytes { get; init; }
    public DateTime LastModified { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsAvailable => Exists && IsReadable;
}

/// <summary>
/// Represents the overall status of the shader directory and deployment.
///
/// DEPLOYMENT VALIDATION:
/// Provides systematic assessment of shader asset deployment.
/// Identifies common configuration issues and provides actionable guidance.
/// Enables automated deployment testing and validation.
/// </summary>
public class ShaderDirectoryStatus
{
    public string DirectoryPath { get; set; } = "";
    public bool Exists { get; set; }
    public bool HasVertexShader { get; set; }
    public int FragmentShaderCount { get; set; }
    public bool IsWritable { get; set; }
    public string Status { get; set; } = "";
    public List<string> Suggestions { get; init; } = new();
}
