using Microsoft.Extensions.DependencyInjection;

namespace Rac.Core.Configuration;

/// <summary>
/// Immutable configuration object that defines the engine setup.
/// </summary>
public sealed class ImmutableEngineConfig
{
    public EngineProfile Profile { get; }
    public bool EnableGraphics { get; }
    public bool EnableAudio { get; }
    public bool EnableInput { get; }
    public bool EnableECS { get; }
    public bool EnableNetworking { get; }
    public IServiceCollection Services { get; }

    public ImmutableEngineConfig(
        EngineProfile profile,
        bool enableGraphics,
        bool enableAudio,
        bool enableInput,
        bool enableECS,
        bool enableNetworking,
        IServiceCollection services)
    {
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
    /// Creates a new configuration with the specified profile and default settings for that profile.
    /// </summary>
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
    /// Creates a new configuration instance with modified settings.
    /// </summary>
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