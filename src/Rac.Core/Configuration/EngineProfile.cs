namespace Rac.Core.Configuration;

/// <summary>
/// Defines the engine profile types for different application scenarios.
/// </summary>
public enum EngineProfile
{
    /// <summary>
    /// Full game profile with all engine features enabled including graphics, audio, input, and ECS.
    /// </summary>
    FullGame,

    /// <summary>
    /// Headless profile for server applications or automated testing. Graphics and input are disabled.
    /// </summary>
    Headless,

    /// <summary>
    /// Custom profile allowing manual configuration of engine features.
    /// </summary>
    Custom
}