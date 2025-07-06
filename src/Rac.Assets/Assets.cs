// ═══════════════════════════════════════════════════════════════════════════════
// File: Assets.cs
// Description: Static facade for simplified asset loading (Layer 1: Beginner access)
// Educational Focus: Facade pattern and progressive complexity design
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Facade pattern for simplifying complex subsystems
// - Static API design for immediate accessibility
// - Progressive complexity with clear upgrade paths
// - Lazy initialization patterns for performance
//
// PROGRESSIVE COMPLEXITY DESIGN:
// - Layer 1 (This): Static methods for beginners (80% of use cases)
// - Layer 2: IAssetService injection for advanced users
// - Layer 3: Custom loaders and cache management
//
// FACADE PATTERN BENEFITS:
// - Hides complexity of asset service configuration
// - Provides immediate success for new developers
// - Maintains consistency with advanced usage patterns
// - Easy to discover and understand API surface
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.FileSystem;
using Rac.Assets.FileSystem.Loaders;
using Rac.Assets.Types;

namespace Rac.Assets;

/// <summary>
/// Static facade providing simplified asset loading for beginners and quick prototyping.
/// 
/// EDUCATIONAL PURPOSE:
/// This facade demonstrates the progressive complexity approach in RACEngine:
/// - Layer 1 (Beginner): Static methods for immediate success
/// - Hides service configuration complexity behind sensible defaults
/// - Provides clear upgrade path to advanced usage patterns
/// - Maintains API consistency across complexity layers
/// 
/// FACADE PATTERN IMPLEMENTATION:
/// - Single point of entry for common asset loading scenarios
/// - Automatic service initialization with default configuration
/// - Thread-safe lazy initialization for performance
/// - Consistent error handling and debugging experience
/// 
/// PROGRESSIVE COMPLEXITY DESIGN:
/// - 80% of asset loading needs covered by static methods
/// - Clear migration path to IAssetService for advanced needs
/// - Same underlying implementation ensures consistent behavior
/// - Educational comments guide users to appropriate complexity level
/// 
/// BEGINNER-FRIENDLY FEATURES:
/// - No configuration required - works out of the box
/// - Intuitive method names that match asset types
/// - Clear error messages for common mistakes
/// - Automatic asset discovery from standard locations
/// 
/// PERFORMANCE CHARACTERISTICS:
/// - Lazy initialization minimizes startup overhead
/// - Automatic caching through underlying service
/// - Thread-safe operations for concurrent access
/// - Memory management through default cache policies
/// </summary>
public static class Assets
{
    /// <summary>
    /// Lazy-initialized asset service for thread-safe singleton pattern.
    /// Educational note: Lazy&lt;T&gt; provides thread-safe initialization without locking overhead.
    /// </summary>
    private static readonly Lazy<IAssetService> _assetService = new(() =>
        AssetServiceBuilder.Create()
            .WithBasePath("assets")
            .Build());

    /// <summary>
    /// Gets the underlying asset service for advanced usage scenarios.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This property provides a clear upgrade path from static usage to service injection:
    /// - Beginners can start with static methods
    /// - Advanced users can access full service capabilities
    /// - Consistent behavior between static and service usage
    /// - Enables testing and dependency injection scenarios
    /// </summary>
    /// <example>
    /// <code>
    /// // Beginner usage (Layer 1)
    /// var texture = Assets.LoadTexture("player.png");
    /// 
    /// // Advanced usage (Layer 2) - access to full service
    /// var service = Assets.Service;
    /// var isLoaded = service.IsAssetLoaded("player.png");
    /// await service.PreloadAssetsAsync(assetPaths);
    /// </code>
    /// </example>
    public static IAssetService Service => _assetService.Value;

    // ═══════════════════════════════════════════════════════════════════════════
    // LAYER 1: BEGINNER API - STATIC METHODS FOR IMMEDIATE SUCCESS
    // Educational note: These methods cover 80% of asset loading scenarios
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads a texture from the specified file path.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This method demonstrates the simplest possible asset loading experience:
    /// - No configuration required - works immediately
    /// - Intuitive naming matches the asset type being loaded
    /// - Automatic caching for performance optimization
    /// - Clear error messages guide troubleshooting
    /// 
    /// BEGINNER-FRIENDLY DESIGN:
    /// - Method name clearly indicates what it does and returns
    /// - Single parameter keeps API surface minimal
    /// - Throws descriptive exceptions for common issues
    /// - Works with relative paths from standard "assets" directory
    /// 
    /// PERFORMANCE BENEFITS:
    /// - Automatic caching prevents redundant file loading
    /// - Lazy service initialization minimizes startup time
    /// - Efficient PNG decoding through optimized loaders
    /// - Memory management through automatic cache policies
    /// </summary>
    /// <param name="filename">Relative path to texture file (e.g., "player.png", "ui/button.png")</param>
    /// <returns>Loaded texture ready for rendering</returns>
    /// <exception cref="FileNotFoundException">Thrown when texture file doesn't exist</exception>
    /// <exception cref="ArgumentException">Thrown when filename is null or empty</exception>
    /// <exception cref="FileFormatException">Thrown when file format is invalid or unsupported</exception>
    /// <example>
    /// <code>
    /// // Load player sprite texture
    /// var playerTexture = Assets.LoadTexture("sprites/player.png");
    /// 
    /// // Load UI button texture
    /// var buttonTexture = Assets.LoadTexture("ui/button.png");
    /// 
    /// // Use with renderer
    /// renderer.DrawSprite(playerTexture, position, size);
    /// </code>
    /// </example>
    public static Texture LoadTexture(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Service.LoadAsset<Texture>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Texture file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: PNG");
        }
        catch (FileFormatException ex)
        {
            throw new FileFormatException(
                $"Failed to load texture '{filename}': {ex.Message}. " +
                $"Ensure the file is a valid PNG image.", ex);
        }
    }

    /// <summary>
    /// Loads an audio clip from the specified file path.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Audio loading demonstrates handling of binary asset types:
    /// - Different asset type but same simple loading pattern
    /// - Automatic format detection and validation
    /// - Memory-efficient loading for various audio sizes
    /// - Consistent error handling across asset types
    /// 
    /// AUDIO ASSET CHARACTERISTICS:
    /// - WAV format provides uncompressed, high-quality audio
    /// - Suitable for sound effects requiring immediate playback
    /// - Larger file sizes than compressed formats (MP3, OGG)
    /// - Universal compatibility across audio systems
    /// 
    /// PERFORMANCE CONSIDERATIONS:
    /// - Audio assets can be large - caching is beneficial
    /// - Uncompressed format enables immediate playback
    /// - Memory usage scales with audio length and quality
    /// - Consider audio compression for music vs. sound effects
    /// </summary>
    /// <param name="filename">Relative path to audio file (e.g., "jump.wav", "music/theme.wav")</param>
    /// <returns>Loaded audio clip ready for playback</returns>
    /// <exception cref="FileNotFoundException">Thrown when audio file doesn't exist</exception>
    /// <exception cref="ArgumentException">Thrown when filename is null or empty</exception>
    /// <exception cref="FileFormatException">Thrown when file format is invalid or unsupported</exception>
    /// <example>
    /// <code>
    /// // Load sound effect
    /// var jumpSound = Assets.LoadAudio("sfx/jump.wav");
    /// 
    /// // Load background music
    /// var bgMusic = Assets.LoadAudio("music/level1.wav");
    /// 
    /// // Use with audio system
    /// audioService.PlaySound(jumpSound, volume: 0.8f);
    /// audioService.PlayMusic(bgMusic, loop: true);
    /// </code>
    /// </example>
    public static AudioClip LoadAudio(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Service.LoadAsset<AudioClip>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Audio file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: WAV");
        }
        catch (FileFormatException ex)
        {
            throw new FileFormatException(
                $"Failed to load audio '{filename}': {ex.Message}. " +
                $"Ensure the file is a valid WAV audio file.", ex);
        }
    }

    /// <summary>
    /// Loads shader source code from the specified file path.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Text asset loading demonstrates versatility of the asset system:
    /// - Same loading pattern works for different data types
    /// - Text assets enable data-driven game development
    /// - Automatic encoding detection for cross-platform compatibility
    /// - Foundation for more complex asset types (JSON, XML, scripts)
    /// 
    /// SHADER DEVELOPMENT WORKFLOW:
    /// - External shader files enable live editing during development
    /// - Version control friendly (text-based, diff-friendly)
    /// - Shader compilation happens at runtime for flexibility
    /// - Easy to share and modify shader effects
    /// 
    /// TEXT ASSET USE CASES:
    /// - Shader source code (vertex, fragment, compute shaders)
    /// - Configuration files (JSON, XML, INI formats)
    /// - Game scripts (Lua, JavaScript, custom scripting languages)
    /// - Localization data (text strings, dialog, UI text)
    /// </summary>
    /// <param name="filename">Relative path to text file (e.g., "basic.vert", "config.json")</param>
    /// <returns>Loaded text content as string</returns>
    /// <exception cref="FileNotFoundException">Thrown when text file doesn't exist</exception>
    /// <exception cref="ArgumentException">Thrown when filename is null or empty</exception>
    /// <exception cref="FileFormatException">Thrown when text encoding is invalid</exception>
    /// <example>
    /// <code>
    /// // Load vertex shader source
    /// var vertexShader = Assets.LoadShaderSource("shaders/basic.vert");
    /// 
    /// // Load fragment shader source  
    /// var fragmentShader = Assets.LoadShaderSource("shaders/basic.frag");
    /// 
    /// // Load configuration file
    /// var config = Assets.LoadShaderSource("config.json");
    /// 
    /// // Use with graphics system
    /// var shader = graphics.CreateShader(vertexShader, fragmentShader);
    /// </code>
    /// </example>
    public static string LoadShaderSource(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Service.LoadAsset<string>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Text file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: Text files (.vert, .frag, .glsl, .txt, .json, .xml, etc.)");
        }
        catch (FileFormatException ex)
        {
            throw new FileFormatException(
                $"Failed to load text file '{filename}': {ex.Message}. " +
                $"Ensure the file uses a supported text encoding (UTF-8 recommended).", ex);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MIGRATION HELPERS - GUIDANCE FOR ADVANCING TO LAYER 2
    // Educational note: Help developers understand when to upgrade
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets helpful information about when to migrate from static usage to service injection.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Provides clear guidance on progressive complexity migration:
    /// - Identifies scenarios where advanced features are beneficial
    /// - Explains the benefits of each complexity layer
    /// - Provides concrete migration examples
    /// - Helps developers make informed architecture decisions
    /// </summary>
    public static string MigrationGuidance => 
        """
        WHEN TO MIGRATE FROM STATIC ASSETS TO IASSETSERVICE:
        
        Consider using IAssetService injection when you need:
        - Async asset loading (LoadAssetAsync) for large assets
        - Asset preloading and batch operations (PreloadAssetsAsync)
        - Custom asset loaders for specialized formats
        - Memory management and cache control
        - Unit testing with mock asset services
        - Dependency injection and modular architecture
        
        MIGRATION EXAMPLE:
        // Current static usage (Layer 1)
        var texture = Assets.LoadTexture("player.png");
        
        // Migrated service usage (Layer 2)
        public class GameSystem {
            public GameSystem(IAssetService assets) => _assets = assets;
            public async Task LoadLevelAsync() {
                var texture = await _assets.LoadAssetAsync<Texture>("player.png");
            }
        }
        
        CONFIGURATION EXAMPLE:
        services.AddSingleton<IAssetService>(provider =>
            AssetServiceBuilder.Create()
                .WithBasePath("game_content")
                .WithMemoryLimit<Texture>(100 * 1024 * 1024)
                .Build());
        """;
}