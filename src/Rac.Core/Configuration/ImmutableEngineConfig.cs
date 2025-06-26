using Microsoft.Extensions.DependencyInjection;

namespace Rac.Core.Configuration;

/// <summary>
/// Provides immutable configuration for engine initialization and feature toggling.
/// Defines which engine subsystems are enabled and manages dependency injection services.
/// </summary>
/// <remarks>
/// This class follows the immutable object pattern to prevent accidental configuration
/// changes after engine initialization. It supports different engine profiles for
/// various deployment scenarios (full game, headless server, custom configuration).
/// 
/// The configuration includes feature flags for major engine subsystems and a service
/// collection for dependency injection. Use the builder pattern methods to create
/// modified configurations while maintaining immutability.
/// 
/// Design Pattern: This implements the Immutable Object pattern combined with the
/// Builder pattern to provide thread-safe configuration management.
/// </remarks>
/// <example>
/// <code>
/// // Create a full game configuration
/// var config = ImmutableEngineConfig.CreateDefault(EngineProfile.FullGame);
/// 
/// // Create a custom configuration with networking disabled
/// var customConfig = config.With(enableNetworking: false);
/// 
/// // Create a headless server configuration
/// var serverConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.Headless);
/// </code>
/// </example>
public sealed class ImmutableEngineConfig
{
    /// <summary>
    /// Gets the engine profile that determines the default feature configuration.
    /// </summary>
    /// <value>
    /// The EngineProfile enum value indicating whether this is a FullGame, Headless, or Custom configuration.
    /// </value>
    public EngineProfile Profile { get; }
    
    /// <summary>
    /// Gets a value indicating whether the graphics rendering subsystem is enabled.
    /// </summary>
    /// <value>
    /// True if graphics rendering, windowing, and visual output should be initialized; otherwise, false.
    /// </value>
    /// <remarks>
    /// When disabled, the engine runs in headless mode without any visual output.
    /// Useful for server applications, automated testing, or console-only tools.
    /// </remarks>
    public bool EnableGraphics { get; }
    
    /// <summary>
    /// Gets a value indicating whether the audio subsystem is enabled.
    /// </summary>
    /// <value>
    /// True if audio playback, sound effects, and music should be initialized; otherwise, false.
    /// </value>
    /// <remarks>
    /// When disabled, all audio operations become no-ops or use null object implementations.
    /// Useful for headless servers or environments without audio hardware.
    /// </remarks>
    public bool EnableAudio { get; }
    
    /// <summary>
    /// Gets a value indicating whether the input handling subsystem is enabled.
    /// </summary>
    /// <value>
    /// True if keyboard, mouse, and gamepad input should be processed; otherwise, false.
    /// </value>
    /// <remarks>
    /// When disabled, input events are not processed and input queries return default values.
    /// Typically disabled for headless servers or automated systems.
    /// </remarks>
    public bool EnableInput { get; }
    
    /// <summary>
    /// Gets a value indicating whether the Entity-Component-System (ECS) is enabled.
    /// </summary>
    /// <value>
    /// True if the ECS world, entities, components, and systems should be initialized; otherwise, false.
    /// </value>
    /// <remarks>
    /// The ECS is the core architectural pattern for game object management.
    /// Rarely disabled except in minimal utility applications.
    /// </remarks>
    public bool EnableECS { get; }
    
    /// <summary>
    /// Gets a value indicating whether networking capabilities are enabled.
    /// </summary>
    /// <value>
    /// True if network communication, multiplayer features, and remote services should be available; otherwise, false.
    /// </value>
    /// <remarks>
    /// Typically enabled for multiplayer games and server applications,
    /// disabled for single-player games and offline tools.
    /// </remarks>
    public bool EnableNetworking { get; }
    
    /// <summary>
    /// Gets the service collection containing registered dependency injection services.
    /// </summary>
    /// <value>
    /// An IServiceCollection containing all registered services for the application.
    /// This collection is immutable and contains a copy of the original services.
    /// </value>
    /// <remarks>
    /// Services are copied during construction to ensure immutability.
    /// Use this collection to build the final IServiceProvider for the application.
    /// </remarks>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the ImmutableEngineConfig class with the specified settings.
    /// </summary>
    /// <param name="profile">The engine profile that determines default feature settings.</param>
    /// <param name="enableGraphics">Whether to enable graphics rendering and windowing.</param>
    /// <param name="enableAudio">Whether to enable audio playback and sound processing.</param>
    /// <param name="enableInput">Whether to enable input device handling.</param>
    /// <param name="enableECS">Whether to enable the Entity-Component-System architecture.</param>
    /// <param name="enableNetworking">Whether to enable networking and multiplayer features.</param>
    /// <param name="services">The service collection for dependency injection. Services will be copied to ensure immutability.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is null.
    /// </exception>
    /// <remarks>
    /// The constructor creates a defensive copy of the services collection to maintain immutability.
    /// This ensures that changes to the original collection do not affect this configuration.
    /// </remarks>
    public ImmutableEngineConfig(
        EngineProfile profile,
        bool enableGraphics,
        bool enableAudio,
        bool enableInput,
        bool enableECS,
        bool enableNetworking,
        IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        Profile = profile;
        EnableGraphics = enableGraphics;
        EnableAudio = enableAudio;
        EnableInput = enableInput;
        EnableECS = enableECS;
        EnableNetworking = enableNetworking;
        Services = new ServiceCollection();
        
        // Copy services to ensure immutability
        foreach (var service in services)
        {
            ((ICollection<ServiceDescriptor>)Services).Add(service);
        }
    }

    /// <summary>
    /// Creates a new configuration with the specified profile and appropriate default settings.
    /// </summary>
    /// <param name="profile">
    /// The engine profile to use. Each profile has predefined feature combinations:
    /// - FullGame: All features enabled except networking
    /// - Headless: Only ECS and networking enabled (no graphics, audio, or input)
    /// - Custom: All features disabled (manual configuration required)
    /// </param>
    /// <returns>
    /// An ImmutableEngineConfig instance configured with profile-appropriate default settings.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="profile"/> is not a valid EngineProfile value.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a full-featured game configuration
    /// var gameConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.FullGame);
    /// 
    /// // Create a headless server configuration
    /// var serverConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.Headless);
    /// 
    /// // Create a blank configuration for custom setup
    /// var customConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.Custom);
    /// </code>
    /// </example>
    public static ImmutableEngineConfig CreateDefault(EngineProfile profile)
    {
        return profile switch
        {
            EngineProfile.FullGame => new ImmutableEngineConfig(
                profile: EngineProfile.FullGame,
                enableGraphics: true,
                enableAudio: true,
                enableInput: true,
                enableECS: true,
                enableNetworking: false,
                services: new ServiceCollection()),

            EngineProfile.Headless => new ImmutableEngineConfig(
                profile: EngineProfile.Headless,
                enableGraphics: false,
                enableAudio: false,
                enableInput: false,
                enableECS: true,
                enableNetworking: true,
                services: new ServiceCollection()),

            EngineProfile.Custom => new ImmutableEngineConfig(
                profile: EngineProfile.Custom,
                enableGraphics: false,
                enableAudio: false,
                enableInput: false,
                enableECS: false,
                enableNetworking: false,
                services: new ServiceCollection()),

            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unsupported engine profile")
        };
    }

    /// <summary>
    /// Creates a new configuration instance with selectively modified settings while preserving immutability.
    /// </summary>
    /// <param name="profile">The new engine profile, or null to keep the current profile.</param>
    /// <param name="enableGraphics">The new graphics setting, or null to keep the current setting.</param>
    /// <param name="enableAudio">The new audio setting, or null to keep the current setting.</param>
    /// <param name="enableInput">The new input setting, or null to keep the current setting.</param>
    /// <param name="enableECS">The new ECS setting, or null to keep the current setting.</param>
    /// <param name="enableNetworking">The new networking setting, or null to keep the current setting.</param>
    /// <param name="services">The new service collection, or null to keep the current services.</param>
    /// <returns>
    /// A new ImmutableEngineConfig instance with the specified changes applied.
    /// The original instance remains unchanged.
    /// </returns>
    /// <remarks>
    /// This method implements the "with" pattern common in immutable objects.
    /// Only the explicitly provided parameters are changed; all others retain their current values.
    /// If services are provided, they will be copied to maintain immutability.
    /// </remarks>
    /// <example>
    /// <code>
    /// var baseConfig = ImmutableEngineConfig.CreateDefault(EngineProfile.FullGame);
    /// 
    /// // Create a version with networking enabled
    /// var networkConfig = baseConfig.With(enableNetworking: true);
    /// 
    /// // Create a headless version of the current config
    /// var headlessConfig = baseConfig.With(
    ///     enableGraphics: false,
    ///     enableAudio: false,
    ///     enableInput: false);
    /// </code>
    /// </example>
    public ImmutableEngineConfig With(
        EngineProfile? profile = null,
        bool? enableGraphics = null,
        bool? enableAudio = null,
        bool? enableInput = null,
        bool? enableECS = null,
        bool? enableNetworking = null,
        IServiceCollection? services = null)
    {
        return new ImmutableEngineConfig(
            profile: profile ?? Profile,
            enableGraphics: enableGraphics ?? EnableGraphics,
            enableAudio: enableAudio ?? EnableAudio,
            enableInput: enableInput ?? EnableInput,
            enableECS: enableECS ?? EnableECS,
            enableNetworking: enableNetworking ?? EnableNetworking,
            services: services ?? Services);
    }
}