using Microsoft.Extensions.DependencyInjection;
using Rac.Core.Configuration;

namespace Rac.Core.Builder;

/// <summary>
/// Immutable builder for constructing engine configurations with fluent API.
/// Each method returns a new instance to maintain immutability.
/// </summary>
public sealed class EngineBuilder
{
    private readonly ImmutableEngineConfig _config;

    /// <summary>
    /// Private constructor for immutable builder pattern.
    /// </summary>
    private EngineBuilder(ImmutableEngineConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Creates a new engine builder with the specified profile.
    /// </summary>
    public static EngineBuilder Create(EngineProfile profile)
    {
        var config = ImmutableEngineConfig.CreateDefault(profile);
        return new EngineBuilder(config);
    }

    /// <summary>
    /// Creates a new engine builder from an existing configuration.
    /// </summary>
    public static EngineBuilder FromConfig(ImmutableEngineConfig config)
    {
        return new EngineBuilder(config);
    }

    /// <summary>
    /// Returns a new builder with graphics enabled or disabled.
    /// </summary>
    public EngineBuilder WithGraphics(bool enabled = true)
    {
        var newConfig = _config.With(enableGraphics: enabled);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with audio enabled or disabled.
    /// </summary>
    public EngineBuilder WithAudio(bool enabled = true)
    {
        var newConfig = _config.With(enableAudio: enabled);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with input enabled or disabled.
    /// </summary>
    public EngineBuilder WithInput(bool enabled = true)
    {
        var newConfig = _config.With(enableInput: enabled);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with ECS enabled or disabled.
    /// </summary>
    public EngineBuilder WithECS(bool enabled = true)
    {
        var newConfig = _config.With(enableECS: enabled);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with networking enabled or disabled.
    /// </summary>
    public EngineBuilder WithNetworking(bool enabled = true)
    {
        var newConfig = _config.With(enableNetworking: enabled);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with an additional service registered.
    /// </summary>
    public EngineBuilder AddService<TService>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TService : class
    {
        return AddService<TService, TService>(lifetime);
    }

    /// <summary>
    /// Returns a new builder with an additional service registered with an implementation type.
    /// </summary>
    public EngineBuilder AddService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TService : class
        where TImplementation : class, TService
    {
        var newServices = new ServiceCollection();
        
        // Copy existing services
        foreach (var service in _config.Services)
        {
            ((ICollection<ServiceDescriptor>)newServices).Add(service);
        }
        
        // Add new service directly as ServiceDescriptor
        var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
        ((ICollection<ServiceDescriptor>)newServices).Add(descriptor);
        
        var newConfig = _config.With(services: newServices);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with an additional service registered with a factory.
    /// </summary>
    public EngineBuilder AddService<TService>(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TService : class
    {
        var newServices = new ServiceCollection();
        
        // Copy existing services
        foreach (var service in _config.Services)
        {
            ((ICollection<ServiceDescriptor>)newServices).Add(service);
        }
        
        // Add new service directly as ServiceDescriptor
        var descriptor = new ServiceDescriptor(typeof(TService), factory, lifetime);
        ((ICollection<ServiceDescriptor>)newServices).Add(descriptor);
        
        var newConfig = _config.With(services: newServices);
        return new EngineBuilder(newConfig);
    }

    /// <summary>
    /// Returns a new builder with an additional singleton service.
    /// </summary>
    public EngineBuilder AddSingleton<TService>()
        where TService : class
    {
        return AddService<TService>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Returns a new builder with an additional singleton service with implementation.
    /// </summary>
    public EngineBuilder AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddService<TService, TImplementation>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Returns a new builder with an additional singleton service with factory.
    /// </summary>
    public EngineBuilder AddSingleton<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        return AddService(factory, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Returns a new builder with an additional scoped service.
    /// </summary>
    public EngineBuilder AddScoped<TService>()
        where TService : class
    {
        return AddService<TService>(ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Returns a new builder with an additional scoped service with implementation.
    /// </summary>
    public EngineBuilder AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddService<TService, TImplementation>(ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Returns a new builder with an additional scoped service with factory.
    /// </summary>
    public EngineBuilder AddScoped<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        return AddService(factory, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Returns a new builder with an additional transient service.
    /// </summary>
    public EngineBuilder AddTransient<TService>()
        where TService : class
    {
        return AddService<TService>(ServiceLifetime.Transient);
    }

    /// <summary>
    /// Returns a new builder with an additional transient service with implementation.
    /// </summary>
    public EngineBuilder AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddService<TService, TImplementation>(ServiceLifetime.Transient);
    }

    /// <summary>
    /// Returns a new builder with an additional transient service with factory.
    /// </summary>
    public EngineBuilder AddTransient<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        return AddService(factory, ServiceLifetime.Transient);
    }

    /// <summary>
    /// Builds and returns the final immutable engine configuration.
    /// </summary>
    public ImmutableEngineConfig Build()
    {
        return _config;
    }

    /// <summary>
    /// Builds the configuration and creates a service provider from it.
    /// </summary>
    public IServiceProvider BuildServiceProvider()
    {
        return _config.Services.BuildServiceProvider();
    }
}