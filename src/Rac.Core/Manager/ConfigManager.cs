using Microsoft.Extensions.Configuration;

namespace Rac.Core.Manager;

/// <summary>
/// Manages application configuration loading from INI files and provides strongly-typed access to settings.
/// Loads configuration from config.ini file and binds sections to strongly-typed classes.
/// </summary>
/// <remarks>
/// This class implements the configuration management pattern using Microsoft.Extensions.Configuration
/// to provide flexible and extensible configuration loading. It supports INI file format for
/// user-friendly configuration editing and automatic binding to strongly-typed objects.
/// 
/// The configuration is loaded from the application's base directory and supports optional
/// configuration files (missing files don't cause errors). This allows for graceful
/// degradation when configuration files are absent.
/// 
/// Design Pattern: This implements the Settings pattern with strongly-typed configuration objects.
/// </remarks>
/// <example>
/// <code>
/// // config.ini file content:
/// // [window]
/// // Title=My Game
/// // Size=1920,1080
/// // VSync=true
/// 
/// var configManager = new ConfigManager();
/// var windowTitle = configManager.Window.Title;      // "My Game"
/// var windowSize = configManager.Window.Size;        // "1920,1080"
/// var vSyncEnabled = configManager.Window.VSync;     // true
/// </code>
/// </example>
public class ConfigManager
{
    private readonly IConfigurationRoot _configuration;

    /// <summary>
    /// Initializes a new instance of the ConfigManager class and loads configuration from config.ini.
    /// </summary>
    /// <remarks>
    /// The constructor automatically loads configuration from a config.ini file located in the
    /// application's base directory. If the file doesn't exist, default values are used.
    /// 
    /// Configuration loading process:
    /// 1. Sets base path to the application directory
    /// 2. Adds config.ini file (optional - won't fail if missing)
    /// 3. Builds the configuration root
    /// 4. Binds the [window] section to WindowSettings
    /// </remarks>
    /// <example>
    /// <code>
    /// // This will load config.ini from the application directory
    /// var config = new ConfigManager();
    /// </code>
    /// </example>
    public ConfigManager()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddIniFile("config.ini", true)
            .Build();

        Window = new WindowSettings();
        _configuration.GetSection("window").Bind(Window);
    }

    /// <summary>
    /// Gets the window configuration settings loaded from the [window] section of config.ini.
    /// </summary>
    /// <value>
    /// A WindowSettings instance containing window-specific configuration values.
    /// Properties will be null if not specified in the configuration file.
    /// </value>
    /// <remarks>
    /// This property provides access to window-related settings such as title, size, and VSync.
    /// Values are automatically bound from the INI file's [window] section during construction.
    /// </remarks>
    public WindowSettings Window { get; }
}

/// <summary>
/// Provides strongly-typed configuration settings for window properties loaded from the [window] section of config.ini.
/// Supports common window configuration options with nullable properties for optional settings.
/// </summary>
/// <remarks>
/// This class uses nullable properties to distinguish between explicitly set values and
/// default/unspecified values. When a property is null, it indicates the setting was
/// not specified in the configuration file, allowing the application to use its own defaults.
/// 
/// The Size property uses a string format to allow flexible specification of window dimensions
/// as "width,height" which can be parsed by the application as needed.
/// </remarks>
/// <example>
/// <code>
/// // Example config.ini [window] section:
/// // [window]
/// // Title=My Awesome Game
/// // Size=1920,1080
/// // VSync=true
/// 
/// var settings = configManager.Window;
/// if (settings.Title != null)
/// {
///     windowOptions.Title = settings.Title;
/// }
/// if (settings.Size != null)
/// {
///     var dimensions = settings.Size.Split(',');
///     windowOptions.Size = new Vector2D&lt;int&gt;(int.Parse(dimensions[0]), int.Parse(dimensions[1]));
/// }
/// </code>
/// </example>
public class WindowSettings
{
    /// <summary>
    /// Gets or sets the window title override from configuration.
    /// </summary>
    /// <value>
    /// The window title string, or null if not specified in configuration.
    /// When null, the application should use its default title.
    /// </value>
    /// <example>
    /// <code>
    /// // In config.ini:
    /// // [window]
    /// // Title=My Game Title
    /// 
    /// if (settings.Title != null)
    /// {
    ///     windowOptions.Title = settings.Title;
    /// }
    /// </code>
    /// </example>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the window size override in "width,height" format.
    /// </summary>
    /// <value>
    /// A string representing window dimensions as "width,height" (e.g., "1920,1080"),
    /// or null if not specified in configuration.
    /// </value>
    /// <remarks>
    /// The format allows for easy specification in INI files while requiring parsing
    /// by the application. Common formats include:
    /// - "1920,1080" for Full HD
    /// - "1280,720" for HD
    /// - "800,600" for legacy resolution
    /// </remarks>
    /// <example>
    /// <code>
    /// // In config.ini:
    /// // [window]
    /// // Size=1920,1080
    /// 
    /// if (settings.Size != null)
    /// {
    ///     var parts = settings.Size.Split(',');
    ///     var width = int.Parse(parts[0]);
    ///     var height = int.Parse(parts[1]);
    ///     windowOptions.Size = new Vector2D&lt;int&gt;(width, height);
    /// }
    /// </code>
    /// </example>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets the VSync (vertical synchronization) setting override.
    /// </summary>
    /// <value>
    /// True to enable VSync, false to disable VSync, or null if not specified in configuration.
    /// When null, the application should use its default VSync setting.
    /// </value>
    /// <remarks>
    /// VSync synchronizes frame rendering with the display refresh rate to prevent screen tearing.
    /// Enabling VSync may reduce performance but provides smoother visual output.
    /// Disabling VSync allows for higher frame rates but may cause visual artifacts.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In config.ini:
    /// // [window]
    /// // VSync=true
    /// 
    /// if (settings.VSync.HasValue)
    /// {
    ///     windowOptions.VSync = settings.VSync.Value;
    /// }
    /// </code>
    /// </example>
    public bool? VSync { get; set; }
}
