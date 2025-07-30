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
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Collections.Concurrent;

namespace Rac.Engine;

public class EngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;
    private readonly IWindowManager _windowManager;
    private readonly TransformSystem _transformSystem;
    private readonly ConcurrentDictionary<Type, FileAssetService> _assetServices = new();
    private readonly Dictionary<Type, string> _assetTypePaths = new();
    private string _defaultAssetBasePath;

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

        // Initialize default asset base path
        _defaultAssetBasePath = Path.Combine(AppContext.BaseDirectory, "Assets");

        // Initialize asset service with default configuration
        Assets = AssetServiceBuilder.Create()
            .WithBasePath(_defaultAssetBasePath)
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
    public IAssetService Assets { get; private set; }
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
            return GetAssetServiceForType<Texture>().LoadAsset<Texture>(filename);
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
            return GetAssetServiceForType<AudioClip>().LoadAsset<AudioClip>(filename);
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
            return GetAssetServiceForType<string>().LoadAsset<string>(filename);
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

    // ═══════════════════════════════════════════════════════════════════════════
    // ASSET PATH CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the base path for all asset types (fallback for specific types).
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This method demonstrates flexible asset organization:
    /// - Allows changing the default asset location at runtime
    /// - Provides fallback path for asset types without specific configuration
    /// - Recreates asset services to use the new path
    /// - Maintains backward compatibility with existing code
    /// </summary>
    /// <param name="basePath">Base directory for asset files</param>
    /// <exception cref="ArgumentNullException">Thrown when basePath is null</exception>
    /// <example>
    /// <code>
    /// // Set all assets to load from "GameContent" folder
    /// engine.SetAssetBasePath("GameContent");
    /// 
    /// // Set absolute path
    /// engine.SetAssetBasePath(@"C:\MyGame\Assets");
    /// </code>
    /// </example>
    public void SetAssetBasePath(string basePath)
    {
        if (basePath == null)
            throw new ArgumentNullException(nameof(basePath));

        _defaultAssetBasePath = Path.IsPathRooted(basePath) 
            ? basePath 
            : Path.Combine(AppContext.BaseDirectory, basePath);

        RecreateAssetServices();
    }

    /// <summary>
    /// Sets the base path specifically for audio assets.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Type-specific asset organization enables better project structure:
    /// - Audio files can be organized in dedicated folders
    /// - Different asset types can have different storage strategies
    /// - Fallback to default path ensures compatibility
    /// - Runtime configuration allows flexible deployment
    /// </summary>
    /// <param name="basePath">Base directory for audio asset files</param>
    /// <exception cref="ArgumentNullException">Thrown when basePath is null</exception>
    /// <example>
    /// <code>
    /// // Audio files in "Audio" folder
    /// engine.SetAudioBasePath("Audio");
    /// 
    /// // Then load audio normally
    /// var sound = engine.LoadAudio("jump.wav"); // Loads from Audio/jump.wav
    /// </code>
    /// </example>
    public void SetAudioBasePath(string basePath)
    {
        if (basePath == null)
            throw new ArgumentNullException(nameof(basePath));

        var fullPath = Path.IsPathRooted(basePath) 
            ? basePath 
            : Path.Combine(AppContext.BaseDirectory, basePath);

        _assetTypePaths[typeof(AudioClip)] = fullPath;
        RecreateAssetServiceForType<AudioClip>();
    }

    /// <summary>
    /// Sets the base path specifically for texture assets.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Texture-specific organization demonstrates asset categorization:
    /// - Visual assets often have different storage requirements
    /// - Texture files can be large and benefit from dedicated organization
    /// - Separate paths enable different caching strategies
    /// - Artist-friendly folder structures improve workflow
    /// </summary>
    /// <param name="basePath">Base directory for texture asset files</param>
    /// <exception cref="ArgumentNullException">Thrown when basePath is null</exception>
    /// <example>
    /// <code>
    /// // Texture files in "Textures" folder
    /// engine.SetTextureBasePath("Textures");
    /// 
    /// // Then load textures normally
    /// var texture = engine.LoadTexture("player.png"); // Loads from Textures/player.png
    /// </code>
    /// </example>
    public void SetTextureBasePath(string basePath)
    {
        if (basePath == null)
            throw new ArgumentNullException(nameof(basePath));

        var fullPath = Path.IsPathRooted(basePath) 
            ? basePath 
            : Path.Combine(AppContext.BaseDirectory, basePath);

        _assetTypePaths[typeof(Texture)] = fullPath;
        RecreateAssetServiceForType<Texture>();
    }

    /// <summary>
    /// Sets the base path specifically for text assets (shaders, configs, etc.).
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Text asset organization enables data-driven development:
    /// - Shader files can be organized separately from other assets
    /// - Configuration files can have dedicated storage
    /// - Text assets often have different versioning requirements
    /// - Enables separate processing pipelines for different text types
    /// </summary>
    /// <param name="basePath">Base directory for text asset files</param>
    /// <exception cref="ArgumentNullException">Thrown when basePath is null</exception>
    /// <example>
    /// <code>
    /// // Text files in "Data" folder
    /// engine.SetTextBasePath("Data");
    /// 
    /// // Then load text assets normally
    /// var shader = engine.LoadShaderSource("basic.vert"); // Loads from Data/basic.vert
    /// </code>
    /// </example>
    public void SetTextBasePath(string basePath)
    {
        if (basePath == null)
            throw new ArgumentNullException(nameof(basePath));

        var fullPath = Path.IsPathRooted(basePath) 
            ? basePath 
            : Path.Combine(AppContext.BaseDirectory, basePath);

        _assetTypePaths[typeof(string)] = fullPath;
        RecreateAssetServiceForType<string>();
    }

    /// <summary>
    /// Recreates all asset services to use updated base paths.
    /// Educational note: Ensures all services use current path configuration.
    /// </summary>
    private void RecreateAssetServices()
    {
        // Dispose existing services
        foreach (var service in _assetServices.Values)
        {
            service.Dispose();
        }
        _assetServices.Clear();

        // Recreate main asset service
        if (Assets is IDisposable disposableAssets)
        {
            disposableAssets.Dispose();
        }
        Assets = AssetServiceBuilder.Create()
            .WithBasePath(_defaultAssetBasePath)
            .Build();
    }

    /// <summary>
    /// Recreates the asset service for a specific type.
    /// Educational note: Selective recreation minimizes impact on other asset types.
    /// </summary>
    private void RecreateAssetServiceForType<T>() where T : class
    {
        var type = typeof(T);
        
        if (_assetServices.TryRemove(type, out var existingService))
        {
            existingService.Dispose();
        }

        // The service will be recreated lazily when needed
    }

    /// <summary>
    /// Gets the appropriate asset service for the specified type.
    /// Educational note: Lazy initialization with type-specific path resolution.
    /// </summary>
    private IAssetService GetAssetServiceForType<T>() where T : class
    {
        var type = typeof(T);

        if (_assetServices.TryGetValue(type, out var existingService))
        {
            return existingService;
        }

        // Determine base path for this type
        var basePath = _assetTypePaths.TryGetValue(type, out var specificPath) 
            ? specificPath 
            : _defaultAssetBasePath;

        // Create service for this type
        var service = AssetServiceBuilder.Create()
            .WithBasePath(basePath)
            .Build();

        _assetServices[type] = service;
        return service;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 2D PRIMITIVE DRAWING CONVENIENCE METHODS (LAYER 1: BEGINNER API)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Draws a textured quad on the screen, centered at the specified position.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This method demonstrates 2D primitive rendering with advanced features:
    /// - Dynamic vertex generation eliminates manual vertex array creation
    /// - Encapsulates complex rendering pipeline setup and state management
    /// - Demonstrates UV mapping and texture coordinate systems
    /// - Shows how color tinting can modify texture appearance
    /// - Provides flexibility through optional parameters with sensible defaults
    /// 
    /// RENDERING PIPELINE INTEGRATION:
    /// - Automatically generates six FullVertex instances for two triangles forming a quad
    /// - Sets renderer to textured shader mode for proper texture sampling
    /// - Configures primitive type to triangles for correct geometry interpretation
    /// - Handles texture binding and color state management
    /// - Calls UpdateVertices and Draw in correct sequence
    /// 
    /// UV COORDINATE SYSTEM:
    /// - Default UV coordinates map texture to full quad: (0,0) to (1,1)
    /// - Bottom-left corner maps to UV (0,0), top-right to UV (1,1)
    /// - Custom coordinates enable texture atlasing, sprite sheets, and effects
    /// - Coordinates outside [0,1] range enable texture repetition if supported
    /// </summary>
    /// <param name="centerPosition">The center point of the quad in world coordinates</param>
    /// <param name="size">The width and height of the quad</param>
    /// <param name="texture">The texture to apply to the quad</param>
    /// <param name="colorTint">Optional color tint to apply to the texture (defaults to white for no tinting)</param>
    /// <param name="textureCoordinates">Optional array of 4 UV coordinates (bottom-left, bottom-right, top-left, top-right). If null, defaults to standard (0,0)-(1,1) range</param>
    /// <exception cref="ArgumentNullException">Thrown when texture is null</exception>
    /// <exception cref="ArgumentException">Thrown when size has non-positive values or textureCoordinates array has incorrect length</exception>
    /// <example>
    /// <code>
    /// // Draw a simple textured quad
    /// var texture = engine.LoadTexture("player.png");
    /// engine.DrawTexturedQuad(new Vector2D&lt;float&gt;(0, 0), new Vector2D&lt;float&gt;(100, 100), texture);
    /// 
    /// // Draw with red tint
    /// engine.DrawTexturedQuad(position, size, texture, new Vector4D&lt;float&gt;(1, 0.5f, 0.5f, 1));
    /// 
    /// // Draw using only top-left quarter of texture
    /// var customUVs = new[] {
    ///     new Vector2D&lt;float&gt;(0f, 0f),    // bottom-left
    ///     new Vector2D&lt;float&gt;(0.5f, 0f),  // bottom-right  
    ///     new Vector2D&lt;float&gt;(0f, 0.5f),  // top-left
    ///     new Vector2D&lt;float&gt;(0.5f, 0.5f) // top-right
    /// };
    /// engine.DrawTexturedQuad(position, size, texture, null, customUVs);
    /// </code>
    /// </example>
    public void DrawTexturedQuad(Vector2D<float> centerPosition, Vector2D<float> size, Texture texture, Vector4D<float>? colorTint = null, Vector2D<float>[]? textureCoordinates = null)
    {
        // Input validation
        if (texture == null)
            throw new ArgumentNullException(nameof(texture));
        
        if (size.X <= 0 || size.Y <= 0)
            throw new ArgumentException("Size must have positive values", nameof(size));
        
        if (textureCoordinates != null && textureCoordinates.Length != 4)
            throw new ArgumentException("Texture coordinates array must have exactly 4 elements (bottom-left, bottom-right, top-left, top-right)", nameof(textureCoordinates));

        // Use default UV coordinates if none provided
        var uvs = textureCoordinates ?? new[]
        {
            new Vector2D<float>(0f, 0f), // bottom-left
            new Vector2D<float>(1f, 0f), // bottom-right
            new Vector2D<float>(0f, 1f), // top-left
            new Vector2D<float>(1f, 1f)  // top-right
        };

        // Use white color if no tint provided
        var color = colorTint ?? new Vector4D<float>(1f, 1f, 1f, 1f);

        // Calculate quad corners from center and size
        var halfWidth = size.X * 0.5f;
        var halfHeight = size.Y * 0.5f;

        var bottomLeft = new Vector2D<float>(centerPosition.X - halfWidth, centerPosition.Y - halfHeight);
        var bottomRight = new Vector2D<float>(centerPosition.X + halfWidth, centerPosition.Y - halfHeight);
        var topLeft = new Vector2D<float>(centerPosition.X - halfWidth, centerPosition.Y + halfHeight);
        var topRight = new Vector2D<float>(centerPosition.X + halfWidth, centerPosition.Y + halfHeight);

        // Generate vertices for two triangles forming the quad
        // Triangle 1: bottom-left → bottom-right → top-left
        // Triangle 2: top-right → bottom-right → top-left
        var vertices = new FullVertex[]
        {
            // Triangle 1
            new FullVertex(bottomLeft, uvs[0], color),  // bottom-left
            new FullVertex(bottomRight, uvs[1], color), // bottom-right
            new FullVertex(topLeft, uvs[2], color),     // top-left

            // Triangle 2
            new FullVertex(topRight, uvs[3], color),    // top-right
            new FullVertex(bottomRight, uvs[1], color), // bottom-right
            new FullVertex(topLeft, uvs[2], color)      // top-left
        };

        // Configure renderer for textured rendering
        Renderer.SetShaderMode(ShaderMode.Textured);
        Renderer.SetPrimitiveType(Silk.NET.OpenGL.PrimitiveType.Triangles);
        Renderer.SetTexture(texture);
        Renderer.SetColor(color); // Set global color for potential shader effects

        // Render the quad
        Renderer.UpdateVertices(vertices);
        Renderer.Draw();
    }

    /// <summary>
    /// Draws a solid color quad on the screen, centered at the specified position.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This method demonstrates simple 2D shape rendering:
    /// - Shows how to render geometry without textures using solid colors
    /// - Demonstrates vertex generation for basic geometric shapes
    /// - Illustrates shader mode switching for different rendering techniques
    /// - Provides a foundation for more complex shape rendering systems
    /// 
    /// RENDERING PIPELINE INTEGRATION:
    /// - Generates six FullVertex instances with dummy UV coordinates (not used for solid color)
    /// - Sets renderer to solid color shader mode (non-textured rendering)
    /// - Configures primitive type to triangles for proper shape rendering
    /// - Sets vertex colors to the specified solid color
    /// - Calls UpdateVertices and Draw to complete the rendering
    /// 
    /// USE CASES:
    /// - UI elements (buttons, panels, overlays)
    /// - Debug visualization (collision boxes, grid lines, debug shapes)
    /// - Simple geometric game objects (platforms, obstacles)
    /// - Fallback rendering when textures fail to load
    /// - Performance-critical scenarios where texture sampling is unnecessary
    /// </summary>
    /// <param name="centerPosition">The center point of the quad in world coordinates</param>
    /// <param name="size">The width and height of the quad</param>
    /// <param name="color">The solid color of the quad (RGBA format)</param>
    /// <exception cref="ArgumentException">Thrown when size has non-positive values</exception>
    /// <example>
    /// <code>
    /// // Draw a red square
    /// engine.DrawSolidColorQuad(new Vector2D&lt;float&gt;(0, 0), new Vector2D&lt;float&gt;(50, 50), 
    ///                          new Vector4D&lt;float&gt;(1, 0, 0, 1));
    /// 
    /// // Draw a semi-transparent blue rectangle
    /// engine.DrawSolidColorQuad(position, size, new Vector4D&lt;float&gt;(0, 0, 1, 0.5f));
    /// 
    /// // Draw a white background panel
    /// engine.DrawSolidColorQuad(centerPos, panelSize, new Vector4D&lt;float&gt;(1, 1, 1, 1));
    /// </code>
    /// </example>
    public void DrawSolidColorQuad(Vector2D<float> centerPosition, Vector2D<float> size, Vector4D<float> color)
    {
        // Input validation
        if (size.X <= 0 || size.Y <= 0)
            throw new ArgumentException("Size must have positive values", nameof(size));

        // Calculate quad corners from center and size
        var halfWidth = size.X * 0.5f;
        var halfHeight = size.Y * 0.5f;

        var bottomLeft = new Vector2D<float>(centerPosition.X - halfWidth, centerPosition.Y - halfHeight);
        var bottomRight = new Vector2D<float>(centerPosition.X + halfWidth, centerPosition.Y - halfHeight);
        var topLeft = new Vector2D<float>(centerPosition.X - halfWidth, centerPosition.Y + halfHeight);
        var topRight = new Vector2D<float>(centerPosition.X + halfWidth, centerPosition.Y + halfHeight);

        // Generate vertices with dummy UV coordinates (not used for solid color rendering)
        var dummyUV = new Vector2D<float>(0f, 0f);
        
        var vertices = new FullVertex[]
        {
            // Triangle 1: bottom-left → bottom-right → top-left
            new FullVertex(bottomLeft, dummyUV, color),  // bottom-left
            new FullVertex(bottomRight, dummyUV, color), // bottom-right
            new FullVertex(topLeft, dummyUV, color),     // top-left

            // Triangle 2: top-right → bottom-right → top-left
            new FullVertex(topRight, dummyUV, color),    // top-right
            new FullVertex(bottomRight, dummyUV, color), // bottom-right
            new FullVertex(topLeft, dummyUV, color)      // top-left
        };

        // Configure renderer for solid color rendering
        Renderer.SetShaderMode(ShaderMode.Normal);
        Renderer.SetPrimitiveType(Silk.NET.OpenGL.PrimitiveType.Triangles);
        Renderer.SetColor(color); // Set global color for the shader

        // Render the quad
        Renderer.UpdateVertices(vertices);
        Renderer.Draw();
    }
}
