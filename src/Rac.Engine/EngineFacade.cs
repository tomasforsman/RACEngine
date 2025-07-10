// File: src/Rac.Engine/EngineFacade.cs

using Rac.Assets;
using Rac.Assets.FileSystem;
using Rac.Assets.Types;
using Rac.Audio;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Components;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Camera;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Rac.Engine;

public class EngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;
    private readonly IWindowManager _windowManager;
    private readonly TransformSystem _transformSystem;

    public EngineFacade(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager
    )
    {
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        World = new World();
        Systems = new SystemScheduler(World);
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);

        // Initialize camera manager for dual-camera system
        CameraManager = new CameraManager();

        // Initialize audio service with OpenAL, using null object pattern as fallback
        try
        {
            Audio = new OpenALAudioService();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: OpenALAudioService initialization failed: {ex.Message}. Falling back to NullAudioService.");
            // Fallback to null audio service if OpenAL initialization fails
            Audio = new NullAudioService();
        }

        // Initialize asset service with default configuration
        Assets = AssetServiceBuilder.Create()
            .WithBasePath("Assets")
            .Build();

        // Initialize transform system (required for container operations)
        _transformSystem = new TransformSystem();
        Systems.Add(_transformSystem);

        // Initialize container service with container system integration
        var containerSystem = new ContainerSystem();
        Container = containerSystem;
        Systems.Add(containerSystem);

        // Set up camera system integration
        SetupCameraIntegration();

        // hook up core pipeline
        _inner.OnLoadEvent += () => LoadEvent?.Invoke();
        _inner.OnEcsUpdate += dt =>
        {
            Systems.Update(dt);
            UpdateEvent?.Invoke(dt);
        };
        _inner.OnRenderFrame += dt => 
        {
            // Update camera matrices before rendering
            UpdateCameraMatrices();
            RenderEvent?.Invoke(dt);
        };

        // forward key events
        _inner.OnKeyEvent += (key, evt) => KeyEvent?.Invoke(key, evt);
        
        // forward mouse events
        _inner.OnLeftClick += pos => LeftClickEvent?.Invoke(pos);
        _inner.OnMouseScroll += delta => MouseScrollEvent?.Invoke(delta);
    }

    public IWorld World { get; }
    public SystemScheduler Systems { get; }
    public IRenderer Renderer => _inner.Renderer;
    public IAudioService Audio { get; }
    public IAssetService Assets { get; }
    public ICameraManager CameraManager { get; }
    public IWindowManager WindowManager => _windowManager;
    public IContainerService Container { get; }
    public TransformSystem TransformSystem => _transformSystem;

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    public event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    public event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    public event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

    /// <summary>Fires when the left mouse button is clicked, providing screen coordinates in pixels.</summary>
    public event Action<Vector2D<float>>? LeftClickEvent;

    /// <summary>Fires when the mouse wheel is scrolled, providing scroll delta.</summary>
    public event Action<float>? MouseScrollEvent;

    /// <summary>Register an ECS system.</summary>
    public void AddSystem(ISystem system)
    {
        Systems.Add(system);
    }

    /// <summary>Start the engine loop.</summary>
    public void Run()
    {
        _inner.Run();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new entity in the world.
    /// Convenience method that delegates to the underlying IWorld.
    /// </summary>
    /// <returns>A new Entity with a unique ID.</returns>
    public Entity CreateEntity()
    {
        return World.CreateEntity();
    }

    /// <summary>
    /// Creates a new entity with a specified name.
    /// Convenience method that creates an entity and assigns a NameComponent.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>A new Entity with a unique ID and the specified name</returns>
    public Entity CreateEntity(string name)
    {
        var entity = World.CreateEntity();
        World.SetComponent(entity, new NameComponent(name));
        return entity;
    }

    /// <summary>
    /// Destroys an entity by removing all its components.
    /// This implementation now properly destroys the entity using the enhanced World API.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
        World.DestroyEntity(entity);
    }

    /// <summary>
    /// Gets the total number of entities currently in the world.
    /// This now returns the actual count of living entities.
    /// </summary>
    public int EntityCount 
    { 
        get 
        {
            return World.GetAllEntities().Count();
        } 
    }

    /// <summary>
    /// Finds entities that have the specified tag.
    /// </summary>
    /// <param name="tag">Tag to search for</param>
    /// <returns>Entities that have the specified tag</returns>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag)
    {
        return World.Query<TagComponent>()
            .Where(result => result.Component1.HasTag(tag))
            .Select(result => result.Entity);
    }

    /// <summary>
    /// Finds the first entity with the specified name.
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>Entity with the specified name, or null if not found</returns>
    public Entity? FindEntityByName(string name)
    {
        var result = World.Query<NameComponent>()
            .FirstOrDefault(result => result.Component1.Name == name);
        
        return result.Entity.Id != 0 ? result.Entity : null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Convenience method that delegates to the container service for the most common use case.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    public Entity CreateContainer(string containerName)
    {
        return Container.CreateContainer(containerName);
    }

    /// <summary>
    /// Places an item inside a container at the origin.
    /// Convenience method for the most common placement operation using extension methods.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container)
    {
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem);
    }

    /// <summary>
    /// Places an item inside a container at the specified local position.
    /// Convenience method that provides positioning control using extension methods.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <param name="localPosition">Local position within the container</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container, Vector2D<float> localPosition)
    {
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem, localPosition);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA SYSTEM INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up camera system integration with window events and renderer.
    /// </summary>
    private void SetupCameraIntegration()
    {
        // Update camera system when window is resized
        _windowManager.OnResize += newSize =>
        {
            CameraManager.UpdateViewport(newSize.X, newSize.Y);
        };
    }

    /// <summary>
    /// Updates camera matrices and applies them to renderer.
    /// Called before each render frame to ensure proper transformations.
    /// </summary>
    private void UpdateCameraMatrices()
    {
        var windowSize = _windowManager.Size;
        
        // Ensure cameras have current viewport dimensions
        CameraManager.UpdateViewport(windowSize.X, windowSize.Y);
        
        // Set game camera matrix as default for world rendering
        // Applications can switch to UI camera matrix for UI rendering passes
        Renderer.SetCameraMatrix(CameraManager.GameCamera.CombinedMatrix);
    }

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
    public Texture LoadTexture(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Assets.LoadAsset<Texture>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Texture file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: PNG");
        }
        catch (FormatException ex)
        {
            throw new FormatException(
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
    public AudioClip LoadAudio(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Assets.LoadAsset<AudioClip>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Audio file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: WAV");
        }
        catch (FormatException ex)
        {
            throw new FormatException(
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
    public string LoadShaderSource(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        try
        {
            return Assets.LoadAsset<string>(filename);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException(
                $"Text file '{filename}' not found. " +
                $"Ensure the file exists in the 'assets' directory relative to your application. " +
                $"Supported formats: Text files (.vert, .frag, .glsl, .txt, .json, .xml, etc.)");
        }
        catch (FormatException ex)
        {
            throw new FormatException(
                $"Failed to load text file '{filename}': {ex.Message}. " +
                $"Ensure the file uses a supported text encoding (UTF-8 recommended).", ex);
        }
    }
}
