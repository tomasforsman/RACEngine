// ═══════════════════════════════════════════════════════════════════════════════
// File: FileAssetService.cs
// Description: Main implementation of IAssetService for file-based asset loading
// Educational Focus: Service implementation patterns and asset management
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Service implementation patterns in game engine architecture
// - Plugin-based asset loader registration and discovery
// - File system abstraction and path resolution strategies
// - Async programming patterns for non-blocking asset loading
//
// ARCHITECTURAL PATTERNS:
// - Dependency injection compatible service design
// - Plugin architecture for extensible asset type support
// - Caching layer for performance optimization
// - Error handling and logging for production robustness
//
// PERFORMANCE FEATURES:
// - Lazy loading with intelligent caching
// - Async loading for non-blocking operations
// - Memory management through configurable cache limits
// - Batch loading optimization for multiple assets
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.FileSystem.Cache;
using Rac.Assets.FileSystem.Loaders;
using Rac.Assets.Types;
using System.Collections.Concurrent;

namespace Rac.Assets.FileSystem;

/// <summary>
/// File-based asset service with plugin architecture and intelligent caching.
/// 
/// EDUCATIONAL PURPOSE:
/// This implementation demonstrates professional asset service design:
/// - Plugin architecture allows adding new asset types without code changes
/// - Intelligent caching balances memory usage with loading performance
/// - Async support prevents blocking the main thread during asset loading
/// - Path resolution provides flexible asset organization strategies
/// 
/// PLUGIN ARCHITECTURE BENEFITS:
/// - Extensibility: New asset types added by implementing IAssetLoader&lt;T&gt;
/// - Modularity: Each asset type has independent loading logic
/// - Testability: Loaders can be tested in isolation
/// - Performance: Specialized loaders optimize for specific formats
/// 
/// CACHING STRATEGY:
/// - Type-specific caches for different asset categories
/// - LRU eviction prevents memory exhaustion
/// - Configurable memory limits per asset type
/// - Cache warming through preloading for predictable access patterns
/// 
/// PATH RESOLUTION:
/// - Base path configuration for asset root directory
/// - Relative path support for portable asset references
/// - Cross-platform path handling for compatibility
/// - Asset discovery and validation
/// 
/// ASYNC LOADING PIPELINE:
/// - Non-blocking I/O prevents frame rate drops
/// - Parallel loading for multiple assets
/// - Progress tracking for loading screens
/// - Error handling preserves application stability
/// </summary>
public class FileAssetService : IAssetService, IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE SERVICE COMPONENTS
    // Educational note: Dependency injection and modular architecture
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base directory for asset file resolution.
    /// Educational note: Centralizes asset location for easy reorganization.
    /// </summary>
    private readonly string _basePath;

    /// <summary>
    /// Registry of asset loaders by asset type.
    /// Educational note: Type-safe registration enables compile-time verification.
    /// </summary>
    private readonly ConcurrentDictionary<Type, object> _loaders = new();

    /// <summary>
    /// Cache instances by asset type for memory management.
    /// Educational note: Separate caches allow different policies per asset type.
    /// </summary>
    private readonly ConcurrentDictionary<Type, object> _caches = new();

    /// <summary>
    /// Synchronization lock for thread-safe initialization.
    /// Educational note: Ensures single initialization in multi-threaded scenarios.
    /// </summary>
    private readonly object _initLock = new();

    private bool _disposed = false;

    /// <summary>
    /// Initializes a new FileAssetService with the specified base path.
    /// </summary>
    /// <param name="basePath">Base directory for asset file resolution</param>
    /// <exception cref="ArgumentException">Thrown when basePath is null, empty, or doesn't exist</exception>
    public FileAssetService(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));

        // Resolve to absolute path for consistent behavior
        _basePath = Path.GetFullPath(basePath);

        // Create base directory if it doesn't exist
        // Educational note: Graceful handling of missing asset directories
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }

        // Initialize with default loaders
        RegisterDefaultLoaders();
    }

    /// <inheritdoc/>
    public T LoadAsset<T>(string path) where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Asset path cannot be null or empty", nameof(path));

        // Try cache first for performance
        var cache = GetOrCreateCache<T>();
        if (cache.TryGet(path, out var cachedAsset))
        {
            return cachedAsset!;
        }

        // Load from file system
        var asset = LoadAssetFromFile<T>(path);
        
        // Cache the loaded asset
        cache.Store(path, asset);
        
        return asset;
    }

    /// <inheritdoc/>
    public bool TryLoadAsset<T>(string path, out T? asset) where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            asset = LoadAsset<T>(path);
            return true;
        }
        catch
        {
            asset = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<T> LoadAssetAsync<T>(string path) where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Use Task.Run to perform loading on background thread
        // Educational note: Offloads I/O work from main thread
        return await Task.Run(() => LoadAsset<T>(path));
    }

    /// <inheritdoc/>
    public async Task PreloadAssetsAsync(IEnumerable<string> paths)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Group paths by asset type for efficient batch loading
        var pathGroups = paths.GroupBy(GetAssetTypeFromPath);

        // Load each group in parallel
        var loadTasks = pathGroups.Select(async group =>
        {
            await Task.Run(() =>
            {
                foreach (var path in group)
                {
                    try
                    {
                        // Determine asset type and load appropriately
                        LoadAssetByPath(path);
                    }
                    catch
                    {
                        // Continue with other assets even if one fails
                        // Educational note: Robust batch loading doesn't fail entirely
                    }
                }
            });
        });

        await Task.WhenAll(loadTasks);
    }

    /// <inheritdoc/>
    public void UnloadAsset(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(path))
            return;

        // Remove from all caches since we don't know the type
        foreach (var kvp in _caches)
        {
            if (kvp.Value is IAssetCache<object> cache)
            {
                cache.Remove(path);
            }
        }
    }

    /// <inheritdoc/>
    public bool IsAssetLoaded(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Check all caches for the asset
        foreach (var kvp in _caches)
        {
            if (kvp.Value is IAssetCache<object> cache && cache.Contains(path))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public int CachedAssetCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _caches.Values.OfType<IAssetCache<object>>().Sum(cache => cache.Count);
        }
    }

    /// <inheritdoc/>
    public long CacheMemoryUsage
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _caches.Values.OfType<IAssetCache<object>>().Sum(cache => cache.MemoryUsage);
        }
    }

    /// <summary>
    /// Registers an asset loader for the specified asset type.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Plugin registration enables extensible asset systems:
    /// - Type safety through generic constraints
    /// - Runtime registration for dynamic asset support
    /// - Override capability for specialized loaders
    /// - Compile-time verification of loader compatibility
    /// </summary>
    /// <typeparam name="T">Type of asset this loader handles</typeparam>
    /// <param name="loader">Asset loader implementation</param>
    /// <exception cref="ArgumentNullException">Thrown when loader is null</exception>
    public void RegisterLoader<T>(IAssetLoader<T> loader) where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        _loaders[typeof(T)] = loader;
    }

    /// <summary>
    /// Loads an asset from the file system using the appropriate loader.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// Demonstrates the complete asset loading pipeline:
    /// 1. Path resolution and validation
    /// 2. File existence and accessibility checks
    /// 3. Asset loader selection and validation
    /// 4. Stream-based loading for memory efficiency
    /// 5. Comprehensive error handling and reporting
    /// </summary>
    /// <typeparam name="T">Type of asset to load</typeparam>
    /// <param name="relativePath">Relative path to the asset file</param>
    /// <returns>Loaded asset instance</returns>
    private T LoadAssetFromFile<T>(string relativePath) where T : class
    {
        // Resolve full file path
        var fullPath = Path.Combine(_basePath, relativePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Asset file not found: {fullPath}");
        }

        // Get file extension for loader selection
        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        
        // Get appropriate loader
        var loader = GetLoader<T>();
        if (!loader.CanLoad(extension))
        {
            throw new NotSupportedException($"No loader available for asset type {typeof(T).Name} with extension '{extension}'");
        }

        // Load asset using stream for memory efficiency
        try
        {
            using var fileStream = File.OpenRead(fullPath);
            return loader.LoadFromStream(fileStream, relativePath);
        }
        catch (Exception ex) when (!(ex is FileFormatException))
        {
            throw new FileFormatException($"Failed to load asset '{relativePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets or creates a cache instance for the specified asset type.
    /// Educational note: Lazy initialization with thread safety.
    /// </summary>
    private IAssetCache<T> GetOrCreateCache<T>() where T : class
    {
        var assetType = typeof(T);
        
        if (_caches.TryGetValue(assetType, out var existingCache))
        {
            return (IAssetCache<T>)existingCache;
        }

        // Double-checked locking pattern for thread-safe initialization
        lock (_initLock)
        {
            if (_caches.TryGetValue(assetType, out existingCache))
            {
                return (IAssetCache<T>)existingCache;
            }

            var newCache = new MemoryAssetCache<T>();
            _caches[assetType] = newCache;
            return newCache;
        }
    }

    /// <summary>
    /// Gets the registered loader for the specified asset type.
    /// Educational note: Type-safe loader retrieval with error handling.
    /// </summary>
    private IAssetLoader<T> GetLoader<T>() where T : class
    {
        if (_loaders.TryGetValue(typeof(T), out var loader))
        {
            return (IAssetLoader<T>)loader;
        }

        throw new NotSupportedException($"No loader registered for asset type: {typeof(T).Name}");
    }

    /// <summary>
    /// Registers default asset loaders for common asset types.
    /// Educational note: Convenient defaults while maintaining extensibility.
    /// </summary>
    private void RegisterDefaultLoaders()
    {
        RegisterLoader<Texture>(new PngImageLoader());
        RegisterLoader<AudioClip>(new WavAudioLoader());
        RegisterLoader<string>(new PlainTextLoader());
    }

    /// <summary>
    /// Determines asset type from file path extension.
    /// Educational note: Simple heuristic for batch loading optimization.
    /// </summary>
    private static string GetAssetTypeFromPath(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".bmp" => "Texture",
            ".wav" or ".ogg" or ".mp3" => "AudioClip",
            _ => "Text"
        };
    }

    /// <summary>
    /// Loads an asset by path without knowing its type at compile time.
    /// Educational note: Dynamic loading for batch operations.
    /// </summary>
    private void LoadAssetByPath(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        
        try
        {
            switch (extension)
            {
                case ".png":
                    LoadAsset<Texture>(path);
                    break;
                case ".wav":
                    LoadAsset<AudioClip>(path);
                    break;
                default:
                    LoadAsset<string>(path);
                    break;
            }
        }
        catch
        {
            // Silently ignore errors in batch loading
        }
    }

    /// <summary>
    /// Disposes the service and all cached assets.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var cache in _caches.Values.OfType<IDisposable>())
            {
                cache.Dispose();
            }
            
            _caches.Clear();
            _loaders.Clear();
            _disposed = true;
        }
    }
}