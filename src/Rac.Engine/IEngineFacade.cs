using Rac.Assets;
using Rac.Assets.Types;
using Rac.Audio;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Camera;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Rac.Engine;

/// <summary>
/// Interface for engine facade providing simplified access to engine services and lifecycle management.
/// </summary>
public interface IEngineFacade
{
    /// <summary>Gets the ECS world instance.</summary>
    IWorld World { get; }

    /// <summary>Gets the system scheduler for managing ECS systems.</summary>
    SystemScheduler Systems { get; }

    /// <summary>Gets the renderer for graphics operations.</summary>
    IRenderer Renderer { get; }

    /// <summary>Gets the audio service for sound and music playback.</summary>
    IAudioService Audio { get; }

    /// <summary>Gets the asset service for loading textures, audio, and other game assets.</summary>
    IAssetService Assets { get; }

    /// <summary>Gets the camera manager for dual-camera system (game world and UI).</summary>
    ICameraManager CameraManager { get; }

    /// <summary>Gets the window manager for window operations and size information.</summary>
    IWindowManager WindowManager { get; }

    /// <summary>Gets the container service for entity container management and operations.</summary>
    IContainerService Container { get; }
    
    /// <summary>Gets the transform system for direct access to transform operations and extension methods.</summary>
    TransformSystem TransformSystem { get; }

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

    /// <summary>Fires when the left mouse button is clicked, providing screen coordinates in pixels.</summary>
    event Action<Vector2D<float>>? LeftClickEvent;

    /// <summary>Fires when the mouse wheel is scrolled, providing scroll delta.</summary>
    event Action<float>? MouseScrollEvent;

    /// <summary>Register an ECS system.</summary>
    void AddSystem(ISystem system);

    /// <summary>Start the engine loop.</summary>
    void Run();

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new entity in the world.
    /// Convenience method that delegates to the underlying IWorld.
    /// </summary>
    /// <returns>A new Entity with a unique ID.</returns>
    Entity CreateEntity();

    /// <summary>
    /// Creates a new entity with a specified name.
    /// Convenience method that creates an entity and assigns a NameComponent.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>A new Entity with a unique ID and the specified name</returns>
    Entity CreateEntity(string name);

    /// <summary>
    /// Destroys an entity and removes it from the world.
    /// Note: Current implementation removes all components - entity destruction will be enhanced in future versions.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    void DestroyEntity(Entity entity);

    /// <summary>
    /// Gets the total number of entities currently in the world.
    /// Note: This is a convenience property - actual count may vary based on implementation.
    /// </summary>
    int EntityCount { get; }

    /// <summary>
    /// Finds entities that have the specified tag.
    /// </summary>
    /// <param name="tag">Tag to search for</param>
    /// <returns>Entities that have the specified tag</returns>
    IEnumerable<Entity> GetEntitiesWithTag(string tag);

    /// <summary>
    /// Finds the first entity with the specified name.
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>Entity with the specified name, or null if not found</returns>
    Entity? FindEntityByName(string name);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Convenience method that delegates to the container service.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    Entity CreateContainer(string containerName);

    /// <summary>
    /// Places an item inside a container at the origin.
    /// Convenience method for the most common placement operation.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    void PlaceInContainer(Entity item, Entity container);

    /// <summary>
    /// Places an item inside a container at the specified local position.
    /// Convenience method that provides positioning control.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <param name="localPosition">Local position within the container</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    void PlaceInContainer(Entity item, Entity container, Vector2D<float> localPosition);

    // ═══════════════════════════════════════════════════════════════════════════
    // ASSET LOADING CONVENIENCE METHODS (LAYER 1: BEGINNER API)
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
    /// <exception cref="FormatException">Thrown when file format is invalid or unsupported</exception>
    /// <example>
    /// <code>
    /// // Load player sprite texture
    /// var playerTexture = engine.LoadTexture("sprites/player.png");
    /// 
    /// // Load UI button texture
    /// var buttonTexture = engine.LoadTexture("ui/button.png");
    /// 
    /// // Use with renderer
    /// renderer.DrawSprite(playerTexture, position, size);
    /// </code>
    /// </example>
    Texture LoadTexture(string filename);

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
    /// <exception cref="FormatException">Thrown when file format is invalid or unsupported</exception>
    /// <example>
    /// <code>
    /// // Load sound effect
    /// var jumpSound = engine.LoadAudio("sfx/jump.wav");
    /// 
    /// // Load background music
    /// var bgMusic = engine.LoadAudio("music/level1.wav");
    /// 
    /// // Use with audio system
    /// engine.Audio.PlaySound(jumpSound, volume: 0.8f);
    /// engine.Audio.PlayMusic(bgMusic, loop: true);
    /// </code>
    /// </example>
    AudioClip LoadAudio(string filename);

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
    /// <exception cref="FormatException">Thrown when text encoding is invalid</exception>
    /// <example>
    /// <code>
    /// // Load vertex shader source
    /// var vertexShader = engine.LoadShaderSource("shaders/basic.vert");
    /// 
    /// // Load fragment shader source  
    /// var fragmentShader = engine.LoadShaderSource("shaders/basic.frag");
    /// 
    /// // Load configuration file
    /// var config = engine.LoadShaderSource("config.json");
    /// 
    /// // Use with graphics system
    /// var shader = graphics.CreateShader(vertexShader, fragmentShader);
    /// </code>
    /// </example>
    string LoadShaderSource(string filename);
}