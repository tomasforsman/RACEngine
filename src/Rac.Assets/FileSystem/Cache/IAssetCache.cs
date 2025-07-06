// ═══════════════════════════════════════════════════════════════════════════════
// File: IAssetCache.cs
// Description: Interface for asset caching with memory management capabilities
// Educational Focus: Caching strategies and memory optimization in game engines
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Caching patterns in game development and performance optimization
// - Memory management strategies for large assets
// - Cache eviction policies and memory pressure handling
// - Thread safety considerations for concurrent asset access
//
// CACHING BENEFITS:
// - Eliminates redundant file I/O operations
// - Reduces loading times for frequently accessed assets
// - Enables asset preloading and batch operations
// - Provides memory usage control and optimization
//
// DESIGN PATTERNS:
// - Generic interface allows different cache implementations
// - Memory tracking enables intelligent eviction decisions
// - Thread safety support for multi-threaded asset loading
// - Disposal pattern for proper resource cleanup
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace Rac.Assets.FileSystem.Cache;

/// <summary>
/// Interface for asset caching with memory management and performance optimization.
/// 
/// EDUCATIONAL PURPOSE:
/// This interface demonstrates caching concepts crucial for game engine performance:
/// - Memory-efficient asset storage and retrieval
/// - Cache eviction strategies for memory pressure management
/// - Thread safety for concurrent asset access
/// - Performance monitoring and cache effectiveness metrics
/// 
/// CACHING STRATEGIES IN GAMES:
/// - LRU (Least Recently Used): Evict oldest accessed items
/// - LFU (Least Frequently Used): Evict least accessed items
/// - Size-based: Evict when memory threshold is reached
/// - Time-based: Evict after specific time periods
/// 
/// MEMORY MANAGEMENT CONSIDERATIONS:
/// - Large assets (textures, audio) consume significant memory
/// - Cache size limits prevent out-of-memory conditions
/// - Weak references allow garbage collection when needed
/// - Asset disposal ensures proper cleanup of unmanaged resources
/// 
/// PERFORMANCE CHARACTERISTICS:
/// - Cache hits eliminate file I/O operations (100-1000x faster)
/// - Memory access is orders of magnitude faster than disk access
/// - Thread safety may introduce slight overhead but enables parallelism
/// - Cache monitoring helps optimize loading strategies
/// </summary>
/// <typeparam name="T">Type of assets stored in this cache</typeparam>
public interface IAssetCache<T> : IDisposable where T : class
{
    // ═══════════════════════════════════════════════════════════════════════════
    // BASIC CACHE OPERATIONS
    // Educational note: Core functionality for asset storage and retrieval
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to retrieve an asset from the cache.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Cache retrieval is the primary performance optimization in asset systems:
    /// - Eliminates file system access for cached assets
    /// - Provides immediate access to loaded assets
    /// - Updates access tracking for LRU eviction strategies
    /// - Thread-safe operation for concurrent asset requests
    /// </summary>
    /// <param name="key">Unique identifier for the asset (typically file path)</param>
    /// <param name="asset">Retrieved asset if found, null otherwise</param>
    /// <returns>True if asset was found in cache, false otherwise</returns>
    bool TryGet(string key, out T? asset);

    /// <summary>
    /// Stores an asset in the cache with the specified key.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Cache storage strategies affect both performance and memory usage:
    /// - Overwrites existing entries with same key
    /// - May trigger eviction if cache is full
    /// - Updates access time for LRU tracking
    /// - Thread-safe operation for concurrent asset loading
    /// </summary>
    /// <param name="key">Unique identifier for the asset</param>
    /// <param name="asset">Asset to store in the cache</param>
    /// <exception cref="ArgumentNullException">Thrown when key or asset is null</exception>
    void Store(string key, T asset);

    /// <summary>
    /// Removes an asset from the cache.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Manual cache removal enables memory management and asset lifecycle control:
    /// - Frees memory for unused assets
    /// - Allows cache invalidation for updated assets
    /// - Supports level transitions and dynamic content loading
    /// - Thread-safe operation for concurrent asset management
    /// </summary>
    /// <param name="key">Key of the asset to remove</param>
    /// <returns>True if asset was removed, false if not found</returns>
    bool Remove(string key);

    /// <summary>
    /// Checks if an asset is currently cached.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Cache status checking enables intelligent loading decisions:
    /// - Avoids redundant loading operations
    /// - Supports preloading strategies and batch operations
    /// - Enables cache warming for predictable access patterns
    /// - Provides debugging and profiling information
    /// </summary>
    /// <param name="key">Key of the asset to check</param>
    /// <returns>True if asset is cached, false otherwise</returns>
    bool Contains(string key);

    /// <summary>
    /// Removes all assets from the cache.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Cache clearing is important for memory management and level transitions:
    /// - Frees all cached memory at once
    /// - Useful for level transitions and major state changes
    /// - Ensures clean slate for new asset loading phases
    /// - Disposes assets properly if they implement IDisposable
    /// </summary>
    void Clear();

    // ═══════════════════════════════════════════════════════════════════════════
    // MEMORY MANAGEMENT AND MONITORING
    // Educational note: Essential for optimization and debugging
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current number of cached assets.
    /// Educational note: Useful for monitoring cache effectiveness and memory usage.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the estimated memory usage of cached assets in bytes.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Memory tracking is crucial for performance optimization:
    /// - Enables memory-based eviction policies
    /// - Helps identify memory usage patterns
    /// - Supports cache size limit enforcement
    /// - Provides profiling data for optimization
    /// </summary>
    long MemoryUsage { get; }

    /// <summary>
    /// Gets or sets the maximum memory usage allowed for this cache in bytes.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Memory limits prevent out-of-memory conditions:
    /// - Triggers automatic eviction when exceeded
    /// - Allows different limits for different asset types
    /// - Enables memory budget allocation across systems
    /// - Supports dynamic adjustment based on available memory
    /// </summary>
    long MaxMemoryUsage { get; set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHE PERFORMANCE METRICS
    // Educational note: Essential for optimization and tuning
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of cache requests (hits + misses).
    /// Educational note: Combined with hit count, provides cache hit ratio for performance analysis.
    /// </summary>
    long TotalRequests { get; }

    /// <summary>
    /// Gets the number of successful cache retrievals.
    /// Educational note: High hit count indicates effective caching strategy.
    /// </summary>
    long CacheHits { get; }

    /// <summary>
    /// Gets the cache hit ratio as a percentage (0.0 to 1.0).
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Hit ratio is the primary metric for cache effectiveness:
    /// - Values above 0.8 (80%) indicate excellent cache performance
    /// - Values below 0.5 (50%) suggest caching strategy needs optimization
    /// - Helps identify which assets benefit most from caching
    /// - Guides cache size and eviction policy tuning
    /// </summary>
    double HitRatio { get; }

    /// <summary>
    /// Resets all performance counters to zero.
    /// Educational note: Useful for measuring cache performance over specific time periods.
    /// </summary>
    void ResetCounters();
}