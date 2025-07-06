// ═══════════════════════════════════════════════════════════════════════════════
// File: IAssetService.cs
// Description: Core asset service interface for loading and managing game assets
// Educational Focus: Service interface design and asset management patterns
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Service interface patterns in game engine architecture
// - Generic type constraints for type-safe asset loading
// - Async programming patterns for non-blocking asset loading
// - Cache management strategies for performance optimization
//
// ARCHITECTURAL BENEFITS:
// - Dependency injection compatibility for modular design
// - Generic type system ensures compile-time type safety
// - Async support prevents frame rate drops during asset loading
// - Cache management enables memory optimization and preloading
//
// PROGRESSIVE COMPLEXITY SUPPORT:
// - Simple LoadAsset<T> for basic usage (80% of scenarios)
// - TryLoadAsset<T> for error handling without exceptions
// - LoadAssetAsync<T> for advanced non-blocking scenarios
// - Cache management for performance-critical applications
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.Types;

namespace Rac.Assets;

/// <summary>
/// Core service interface for loading and managing game assets with progressive complexity support.
/// 
/// EDUCATIONAL PURPOSE:
/// This interface demonstrates modern game engine asset management, featuring:
/// - Generic type system for compile-time type safety
/// - Async programming patterns for smooth gameplay
/// - Cache management for memory optimization
/// - Error handling patterns for robust applications
/// 
/// PROGRESSIVE COMPLEXITY DESIGN:
/// Layer 1 (Basic): LoadAsset&lt;T&gt;(path) - Simple loading with exceptions
/// Layer 2 (Safe): TryLoadAsset&lt;T&gt;(path, out asset) - Error handling without exceptions
/// Layer 3 (Advanced): LoadAssetAsync&lt;T&gt;(path) - Non-blocking async loading
/// Layer 4 (Performance): Cache management for memory optimization
/// 
/// SUPPORTED ASSET TYPES:
/// - Texture: Image assets (PNG, JPEG, etc.)
/// - AudioClip: Audio assets (WAV, OGG, etc.)
/// - string: Text assets (shaders, configs, etc.)
/// - Custom types: Extensible through asset loader registration
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Caching prevents redundant file I/O operations
/// - Async loading prevents blocking the main thread
/// - Memory management through asset disposal and cache control
/// - Batch loading reduces file system overhead
/// </summary>
public interface IAssetService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // BASIC LOADING OPERATIONS (Layer 1: Simple usage)
    // Educational note: These methods cover 80% of asset loading scenarios
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads an asset of the specified type from the given path.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// This method demonstrates the most common asset loading pattern:
    /// 1. Resolve the full file path from the relative path
    /// 2. Check cache for existing loaded asset
    /// 3. If not cached, load from file using appropriate loader
    /// 4. Store in cache for future requests
    /// 5. Return the loaded asset
    /// </summary>
    /// <typeparam name="T">Type of asset to load (Texture, AudioClip, string, etc.)</typeparam>
    /// <param name="path">Relative path to the asset file</param>
    /// <returns>The loaded asset instance</returns>
    /// <exception cref="FileNotFoundException">Thrown when the asset file doesn't exist</exception>
    /// <exception cref="NotSupportedException">Thrown when the asset type isn't supported</exception>
    /// <exception cref="InvalidDataException">Thrown when the file format is invalid</exception>
    /// <example>
    /// <code>
    /// var playerTexture = assetService.LoadAsset&lt;Texture&gt;("sprites/player.png");
    /// var jumpSound = assetService.LoadAsset&lt;AudioClip&gt;("audio/jump.wav");
    /// var shaderCode = assetService.LoadAsset&lt;string&gt;("shaders/basic.vert");
    /// </code>
    /// </example>
    T LoadAsset<T>(string path) where T : class;

    /// <summary>
    /// Attempts to load an asset without throwing exceptions on failure.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This pattern demonstrates safe asset loading for scenarios where:
    /// - Missing assets are acceptable (optional content)
    /// - Performance-critical code can't handle exceptions
    /// - Graceful degradation is preferred over crashes
    /// </summary>
    /// <typeparam name="T">Type of asset to load</typeparam>
    /// <param name="path">Relative path to the asset file</param>
    /// <param name="asset">Output parameter containing the loaded asset, or null if loading failed</param>
    /// <returns>True if the asset was loaded successfully, false otherwise</returns>
    /// <example>
    /// <code>
    /// if (assetService.TryLoadAsset&lt;Texture&gt;("optional/decoration.png", out var decoration))
    /// {
    ///     // Use optional decoration
    ///     renderer.DrawSprite(decoration, position, size);
    /// }
    /// else
    /// {
    ///     // Fallback to default appearance
    ///     renderer.DrawRect(position, size, Color.Gray);
    /// }
    /// </code>
    /// </example>
    bool TryLoadAsset<T>(string path, out T? asset) where T : class;

    // ═══════════════════════════════════════════════════════════════════════════
    // ASYNC LOADING OPERATIONS (Layer 2: Advanced usage)
    // Educational note: Prevents frame rate drops during asset loading
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Asynchronously loads an asset without blocking the main thread.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Async asset loading is crucial for maintaining smooth gameplay:
    /// - Large assets (textures, audio) can take significant time to load
    /// - Blocking the main thread causes frame rate drops and stuttering
    /// - Async loading allows the game to continue running during I/O operations
    /// - Essential for streaming and level loading systems
    /// </summary>
    /// <typeparam name="T">Type of asset to load</typeparam>
    /// <param name="path">Relative path to the asset file</param>
    /// <returns>Task containing the loaded asset when complete</returns>
    /// <example>
    /// <code>
    /// // Load level assets without blocking gameplay
    /// var backgroundTask = assetService.LoadAssetAsync&lt;Texture&gt;("levels/background.png");
    /// var musicTask = assetService.LoadAssetAsync&lt;AudioClip&gt;("music/level1.wav");
    /// 
    /// // Continue with game logic while loading
    /// UpdateGameplay(deltaTime);
    /// 
    /// // Use assets when ready
    /// var background = await backgroundTask;
    /// var music = await musicTask;
    /// </code>
    /// </example>
    Task<T> LoadAssetAsync<T>(string path) where T : class;

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHE MANAGEMENT OPERATIONS (Layer 3: Performance optimization)
    // Educational note: Memory management and performance optimization
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Preloads multiple assets into cache for faster access.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Preloading is an important optimization technique:
    /// - Load assets during loading screens to hide I/O latency
    /// - Batch loading is more efficient than individual requests
    /// - Prevents frame rate drops during gameplay
    /// - Essential for streaming and just-in-time loading systems
    /// </summary>
    /// <param name="paths">Collection of asset paths to preload</param>
    /// <returns>Task that completes when all assets are loaded</returns>
    /// <example>
    /// <code>
    /// // Preload all level assets during loading screen
    /// var levelAssets = new[]
    /// {
    ///     "sprites/player.png",
    ///     "sprites/enemies.png",
    ///     "audio/background.wav"
    /// };
    /// await assetService.PreloadAssetsAsync(levelAssets);
    /// </code>
    /// </example>
    Task PreloadAssetsAsync(IEnumerable<string> paths);

    /// <summary>
    /// Removes an asset from the cache to free memory.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Memory management is critical in game development:
    /// - Assets consume significant memory (textures, audio)
    /// - Unused assets should be unloaded to prevent memory pressure
    /// - Important for level transitions and dynamic content
    /// - Balances memory usage with loading performance
    /// </summary>
    /// <param name="path">Path of the asset to unload</param>
    /// <example>
    /// <code>
    /// // Unload previous level assets when transitioning
    /// assetService.UnloadAsset("levels/level1_background.png");
    /// assetService.UnloadAsset("music/level1_theme.wav");
    /// </code>
    /// </example>
    void UnloadAsset(string path);

    /// <summary>
    /// Checks if an asset is currently loaded in cache.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Cache status checking enables intelligent loading decisions:
    /// - Avoid redundant loading operations
    /// - Implement smart preloading strategies
    /// - Debug memory usage and cache behavior
    /// - Optimize asset streaming systems
    /// </summary>
    /// <param name="path">Path of the asset to check</param>
    /// <returns>True if the asset is currently cached, false otherwise</returns>
    bool IsAssetLoaded(string path);

    /// <summary>
    /// Gets the current number of cached assets.
    /// Educational note: Useful for memory profiling and cache optimization.
    /// </summary>
    int CachedAssetCount { get; }

    /// <summary>
    /// Gets the total memory usage of cached assets in bytes.
    /// Educational note: Essential for memory management and optimization.
    /// </summary>
    long CacheMemoryUsage { get; }
}