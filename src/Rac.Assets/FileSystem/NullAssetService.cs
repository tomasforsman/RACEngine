// ═══════════════════════════════════════════════════════════════════════════════
// File: NullAssetService.cs
// Description: Null object implementation of IAssetService for testing/fallback
// Educational Focus: Null object pattern and graceful degradation
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Null object pattern for graceful system degradation
// - Defensive programming and error handling strategies
// - Testing patterns and dependency injection support
// - Fallback mechanisms in game engine architecture
//
// DESIGN BENEFITS:
// - Eliminates null reference checks in client code
// - Provides predictable behavior when assets are unavailable
// - Enables testing without file system dependencies
// - Supports graceful degradation during development
//
// NULL OBJECT PATTERN:
// - Implements interface with safe, predictable no-op behavior
// - Returns appropriate default values for all operations
// - Logs warnings in debug builds for development feedback
// - Never throws exceptions, ensuring application stability
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.Types;

namespace Rac.Assets.FileSystem;

/// <summary>
/// Null object implementation of IAssetService that provides safe fallback behavior.
/// 
/// EDUCATIONAL PURPOSE:
/// This class demonstrates the null object pattern, a fundamental design pattern in robust systems:
/// - Eliminates need for null checks in client code
/// - Provides predictable, safe behavior when asset loading fails
/// - Enables graceful degradation during development and testing
/// - Supports dependency injection scenarios where assets are optional
/// 
/// NULL OBJECT PATTERN BENEFITS:
/// - Code Simplification: No null checks needed in game logic
/// - Fail-Safe Behavior: Application continues running even without assets
/// - Testing Support: Easy to test game logic without file dependencies
/// - Development Workflow: Game runs even with missing asset files
/// 
/// USAGE SCENARIOS:
/// - Development Mode: Continue development without all assets ready
/// - Testing: Unit tests don't require asset files on disk
/// - Demo Mode: Show application structure without content
/// - Fallback: When asset loading fails, continue with minimal functionality
/// 
/// EDUCATIONAL CONSIDERATIONS:
/// - Debug warnings help developers identify missing asset configurations
/// - Predictable return values prevent crashes but indicate problems
/// - Async methods return completed tasks for consistent behavior
/// - Memory usage remains minimal (no actual assets loaded)
/// </summary>
public class NullAssetService : IAssetService
{
    private const string WarningMessage = "NullAssetService is being used - no assets will be loaded. " +
                                         "Configure a proper asset service for production use.";

    /// <summary>
    /// Static flag to ensure warning is only logged once per application run.
    /// Educational note: Prevents log spam while still alerting developers.
    /// </summary>
    private static bool _warningLogged = false;

    /// <summary>
    /// Initializes a new instance of the NullAssetService.
    /// Logs a debug warning to alert developers that no assets will be loaded.
    /// </summary>
    public NullAssetService()
    {
        // Log warning once per application run
        if (!_warningLogged)
        {
            _warningLogged = true;
            
            // Only log in debug builds to avoid noise in production
            #if DEBUG
            Console.WriteLine($"[DEBUG] Warning: {WarningMessage}");
            #endif
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// Always returns null but doesn't throw exceptions, allowing game logic to continue.
    /// The null return indicates asset loading failure, which client code should handle gracefully.
    /// </remarks>
    public T LoadAsset<T>(string path) where T : class
    {
        // In a null object pattern, we return appropriate "empty" values
        // For reference types, null is appropriate as it clearly indicates no asset
        throw new FileNotFoundException($"NullAssetService cannot load assets. Asset '{path}' is not available.");
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// Always returns false, indicating asset loading failed.
    /// This is the safe, predictable behavior that allows continued execution.
    /// </remarks>
    public bool TryLoadAsset<T>(string path, out T? asset) where T : class
    {
        asset = null;
        return false;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// Returns a completed task with null result for consistent async behavior.
    /// This prevents deadlocks and maintains async/await compatibility.
    /// </remarks>
    public Task<T> LoadAssetAsync<T>(string path) where T : class
    {
        // Return a completed task with exception to maintain consistency with LoadAsset
        return Task.FromException<T>(new FileNotFoundException($"NullAssetService cannot load assets. Asset '{path}' is not available."));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// Returns completed task immediately since no assets are loaded.
    /// Maintains async method contracts without actual work.
    /// </remarks>
    public Task PreloadAssetsAsync(IEnumerable<string> paths)
    {
        // No-op: return completed task since we don't load anything
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// No-op since no assets are cached in the null implementation.
    /// Safe to call, maintains interface contract.
    /// </remarks>
    public void UnloadAsset(string path)
    {
        // No-op: nothing to unload in null implementation
    }

    /// <inheritdoc/>
    /// <remarks>
    /// EDUCATIONAL IMPLEMENTATION:
    /// Always returns false since no assets are ever loaded.
    /// Consistent with the null object pattern behavior.
    /// </remarks>
    public bool IsAssetLoaded(string path)
    {
        return false;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns 0 since no assets are cached.
    /// </remarks>
    public int CachedAssetCount => 0;

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns 0 since no memory is used for assets.
    /// </remarks>
    public long CacheMemoryUsage => 0;
}