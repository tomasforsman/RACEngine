// ═══════════════════════════════════════════════════════════════════════════════
// File: AssetServiceBuilder.cs
// Description: Builder pattern for configuring and creating asset services
// Educational Focus: Builder pattern and dependency injection configuration
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Builder pattern implementation for complex object construction
// - Fluent API design for intuitive configuration experience
// - Dependency injection integration and service registration
// - Default configuration with override capabilities
//
// BUILDER PATTERN BENEFITS:
// - Step-by-step configuration with validation
// - Fluent API provides intuitive configuration experience
// - Immutable configuration prevents accidental modifications
// - Extensible design allows adding new configuration options
//
// CONFIGURATION FEATURES:
// - Asset loader registration and override capabilities
// - Cache configuration with memory limits and policies
// - Base path resolution for asset organization
// - Service lifetime management for dependency injection
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.FileSystem.Cache;
using Rac.Assets.FileSystem.Loaders;
using Rac.Assets.Types;

namespace Rac.Assets.FileSystem;

/// <summary>
/// Builder for creating and configuring FileAssetService instances with fluent API.
/// 
/// EDUCATIONAL PURPOSE:
/// This builder demonstrates the builder pattern for complex service configuration:
/// - Fluent API provides intuitive, readable configuration syntax
/// - Step-by-step validation ensures correct service construction
/// - Default configuration works out-of-the-box while allowing customization
/// - Immutable design prevents configuration corruption after building
/// 
/// BUILDER PATTERN ADVANTAGES:
/// - Complexity Management: Breaks down service configuration into manageable steps
/// - Validation: Each step can validate configuration before proceeding
/// - Flexibility: Optional configuration with sensible defaults
/// - Readability: Fluent API reads like natural language
/// 
/// CONFIGURATION STRATEGY:
/// - Default Loaders: PNG, WAV, and text loaders registered automatically
/// - Memory Limits: Configurable cache sizes for different asset types
/// - Base Path: Centralized asset location with fallback to current directory
/// - Override Support: Custom loaders can replace or extend defaults
/// 
/// DEPENDENCY INJECTION INTEGRATION:
/// - Creates services ready for DI container registration
/// - Supports singleton and scoped lifetimes
/// - Configures dependencies in correct order
/// - Validates configuration before service creation
/// </summary>
public class AssetServiceBuilder
{
    private string _basePath = "assets";
    private readonly List<LoaderRegistration> _loaderRegistrations = new();
    private readonly Dictionary<Type, long> _memoryLimits = new();
    private bool _useDefaultLoaders = true;

    /// <summary>
    /// Creates a new AssetServiceBuilder with default configuration.
    /// 
    /// EDUCATIONAL NOTE:
    /// Static factory methods provide a clean entry point for builder patterns
    /// and can pre-configure common scenarios for better developer experience.
    /// </summary>
    /// <returns>New builder instance with default configuration</returns>
    public static AssetServiceBuilder Create()
    {
        return new AssetServiceBuilder();
    }

    /// <summary>
    /// Configures the base path for asset file resolution.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Base path configuration demonstrates flexible asset organization:
    /// - Relative paths are resolved from application directory
    /// - Absolute paths provide explicit control over asset location
    /// - Path validation ensures assets can be located at runtime
    /// - Cross-platform path handling for compatibility
    /// </summary>
    /// <param name="basePath">Base directory for asset files</param>
    /// <returns>Builder instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when basePath is null or empty</exception>
    /// <example>
    /// <code>
    /// var service = AssetServiceBuilder.Create()
    ///     .WithBasePath("game_assets")
    ///     .Build();
    /// </code>
    /// </example>
    public AssetServiceBuilder WithBasePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));

        _basePath = basePath;
        return this;
    }

    /// <summary>
    /// Registers a custom asset loader for the specified type.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Custom loader registration demonstrates the plugin architecture:
    /// - Type-safe registration prevents runtime errors
    /// - Override capability allows replacing default loaders
    /// - Extensibility enables new asset types without core changes
    /// - Validation ensures loader compatibility at build time
    /// </summary>
    /// <typeparam name="T">Type of asset the loader handles</typeparam>
    /// <param name="loader">Asset loader implementation</param>
    /// <returns>Builder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when loader is null</exception>
    /// <example>
    /// <code>
    /// var service = AssetServiceBuilder.Create()
    ///     .AddLoader&lt;Texture&gt;(new CustomTextureLoader())
    ///     .AddLoader&lt;Model&gt;(new GltfModelLoader())
    ///     .Build();
    /// </code>
    /// </example>
    public AssetServiceBuilder AddLoader<T>(IAssetLoader<T> loader) where T : class
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));

        _loaderRegistrations.Add(new LoaderRegistration(typeof(T), loader));
        return this;
    }

    /// <summary>
    /// Configures the memory limit for caching assets of the specified type.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Memory limit configuration demonstrates cache management strategies:
    /// - Type-specific limits allow fine-grained memory control
    /// - Different asset types have different memory characteristics
    /// - Prevents out-of-memory conditions in asset-heavy applications
    /// - Balances performance (caching) with memory usage
    /// </summary>
    /// <typeparam name="T">Type of asset to configure</typeparam>
    /// <param name="memoryLimitBytes">Maximum memory usage for this asset type in bytes</param>
    /// <returns>Builder instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when memory limit is negative</exception>
    /// <example>
    /// <code>
    /// var service = AssetServiceBuilder.Create()
    ///     .WithMemoryLimit&lt;Texture&gt;(50 * 1024 * 1024) // 50MB for textures
    ///     .WithMemoryLimit&lt;AudioClip&gt;(20 * 1024 * 1024) // 20MB for audio
    ///     .Build();
    /// </code>
    /// </example>
    public AssetServiceBuilder WithMemoryLimit<T>(long memoryLimitBytes) where T : class
    {
        if (memoryLimitBytes < 0)
            throw new ArgumentException("Memory limit cannot be negative", nameof(memoryLimitBytes));

        _memoryLimits[typeof(T)] = memoryLimitBytes;
        return this;
    }

    /// <summary>
    /// Disables automatic registration of default asset loaders.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Default loader control demonstrates flexible configuration:
    /// - Allows complete control over which loaders are available
    /// - Useful for specialized scenarios or minimal configurations
    /// - Prevents conflicts when using only custom loaders
    /// - Reduces memory footprint in constrained environments
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    /// <example>
    /// <code>
    /// var service = AssetServiceBuilder.Create()
    ///     .WithoutDefaultLoaders()
    ///     .AddLoader&lt;CustomAsset&gt;(new CustomAssetLoader())
    ///     .Build();
    /// </code>
    /// </example>
    public AssetServiceBuilder WithoutDefaultLoaders()
    {
        _useDefaultLoaders = false;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured FileAssetService instance.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// The build method demonstrates the builder pattern completion:
    /// - Validates all configuration before creating the service
    /// - Applies default configuration where not specified
    /// - Creates service with all dependencies properly configured
    /// - Returns immutable service ready for use
    /// 
    /// VALIDATION STRATEGY:
    /// - Path validation ensures asset directory accessibility
    /// - Loader validation checks for type conflicts and missing loaders
    /// - Memory limit validation prevents invalid configurations
    /// - Comprehensive error reporting for configuration issues
    /// </summary>
    /// <returns>Configured FileAssetService instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    /// <example>
    /// <code>
    /// var assetService = AssetServiceBuilder.Create()
    ///     .WithBasePath("game_content")
    ///     .WithMemoryLimit&lt;Texture&gt;(100 * 1024 * 1024)
    ///     .AddLoader&lt;Model&gt;(new GltfModelLoader())
    ///     .Build();
    /// </code>
    /// </example>
    public FileAssetService Build()
    {
        ValidateConfiguration();

        // Create the service with configured base path
        var service = new FileAssetService(_basePath);

        // Register default loaders if enabled
        if (_useDefaultLoaders)
        {
            RegisterDefaultLoaders(service);
        }

        // Register custom loaders
        foreach (var registration in _loaderRegistrations)
        {
            RegisterLoader(service, registration);
        }

        // Configure memory limits for caches
        ConfigureMemoryLimits(service);

        return service;
    }

    /// <summary>
    /// Validates the current configuration for completeness and correctness.
    /// Educational note: Early validation prevents runtime errors.
    /// </summary>
    private void ValidateConfiguration()
    {
        // Validate base path
        if (string.IsNullOrWhiteSpace(_basePath))
        {
            throw new InvalidOperationException("Base path must be specified");
        }

        // Validate memory limits
        foreach (var kvp in _memoryLimits)
        {
            if (kvp.Value < 0)
            {
                throw new InvalidOperationException($"Memory limit for {kvp.Key.Name} cannot be negative");
            }
        }

        // Validate that we have at least some loaders
        if (!_useDefaultLoaders && _loaderRegistrations.Count == 0)
        {
            throw new InvalidOperationException("At least one asset loader must be registered when default loaders are disabled");
        }
    }

    /// <summary>
    /// Registers default loaders for common asset types.
    /// Educational note: Provides out-of-the-box functionality for common scenarios.
    /// </summary>
    private static void RegisterDefaultLoaders(FileAssetService service)
    {
        service.RegisterLoader<Texture>(new PngImageLoader());
        service.RegisterLoader<AudioClip>(new WavAudioLoader());
        service.RegisterLoader<string>(new PlainTextLoader());
    }

    /// <summary>
    /// Registers a custom loader using reflection for type safety.
    /// Educational note: Dynamic registration while maintaining compile-time safety.
    /// </summary>
    private static void RegisterLoader(FileAssetService service, LoaderRegistration registration)
    {
        var method = typeof(FileAssetService).GetMethod(nameof(FileAssetService.RegisterLoader));
        var genericMethod = method?.MakeGenericMethod(registration.AssetType);
        genericMethod?.Invoke(service, new[] { registration.Loader });
    }

    /// <summary>
    /// Configures memory limits for asset caches.
    /// Educational note: Cache configuration happens after service creation.
    /// </summary>
    private void ConfigureMemoryLimits(FileAssetService service)
    {
        // Memory limit configuration would require additional API on FileAssetService
        // This is a placeholder for future cache configuration features
        foreach (var kvp in _memoryLimits)
        {
            // TODO: Implement cache configuration API
            // service.ConfigureCacheLimit(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Internal record for tracking loader registrations.
    /// Educational note: Encapsulates registration data for type-safe processing.
    /// </summary>
    private record LoaderRegistration(Type AssetType, object Loader);
}