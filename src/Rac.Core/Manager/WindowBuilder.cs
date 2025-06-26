using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

/// <summary>
/// Provides a fluent builder pattern for constructing window configurations.
/// Simplifies window creation by offering chainable methods for setting window properties.
/// </summary>
/// <remarks>
/// The WindowBuilder implements the builder pattern to make window configuration more readable
/// and maintainable. It allows for method chaining to set various window properties before
/// creating the final window instance.
/// 
/// This pattern is particularly useful for complex window configurations where multiple
/// properties need to be set, as it avoids the need for constructors with many parameters
/// and provides a clear, self-documenting API.
/// </remarks>
/// <example>
/// <code>
/// // Create a configured game window
/// var window = WindowBuilder
///     .Configure(windowManager)
///     .WithTitle("My Game")
///     .WithSize(1920, 1080)
///     .WithVSync(true)
///     .WithResizable(false)
///     .WithState(WindowState.Maximized)
///     .Create();
/// 
/// window.Run();
/// </code>
/// </example>
public class WindowBuilder
{
    private readonly IWindowManager _manager;
    private WindowOptions _options;

    private WindowBuilder(IWindowManager manager)
    {
        _manager = manager;
        _options = WindowOptions.Default;
    }

    /// <summary>
    /// Creates a new WindowBuilder instance configured with the specified window manager.
    /// </summary>
    /// <param name="manager">
    /// The window manager that will be used to create the window.
    /// Must not be null.
    /// </param>
    /// <returns>
    /// A new WindowBuilder instance ready for configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="manager"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var builder = WindowBuilder.Configure(windowManager);
    /// </code>
    /// </example>
    public static WindowBuilder Configure(IWindowManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        return new WindowBuilder(manager);
    }

    /// <summary>
    /// Sets the window title that appears in the window's title bar.
    /// </summary>
    /// <param name="title">
    /// The title text to display. If null or empty, a default title may be used.
    /// </param>
    /// <returns>
    /// This WindowBuilder instance for method chaining.
    /// </returns>
    /// <example>
    /// <code>
    /// builder.WithTitle("My Awesome Game");
    /// </code>
    /// </example>
    public WindowBuilder WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    /// <summary>
    /// Sets the initial size of the window's client area.
    /// </summary>
    /// <param name="width">
    /// The width of the window in pixels. Must be greater than 0.
    /// </param>
    /// <param name="height">
    /// The height of the window in pixels. Must be greater than 0.
    /// </param>
    /// <returns>
    /// This WindowBuilder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when width or height is less than or equal to 0.
    /// </exception>
    /// <example>
    /// <code>
    /// // Set to 1080p resolution
    /// builder.WithSize(1920, 1080);
    /// </code>
    /// </example>
    public WindowBuilder WithSize(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be greater than 0", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(height));
            
        _options.Size = new Vector2D<int>(width, height);
        return this;
    }

    /// <summary>
    /// Configures vertical synchronization (VSync) for the window.
    /// </summary>
    /// <param name="enabled">
    /// True to enable VSync (limits frame rate to display refresh rate),
    /// false to disable VSync (allows unlimited frame rate).
    /// Default is true.
    /// </param>
    /// <returns>
    /// This WindowBuilder instance for method chaining.
    /// </returns>
    /// <remarks>
    /// VSync prevents screen tearing by synchronizing frame rendering with the
    /// display's refresh rate, but may introduce input latency. Disable for
    /// competitive gaming or when maximum frame rate is desired.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enable VSync for smooth visuals
    /// builder.WithVSync(true);
    /// 
    /// // Disable VSync for maximum performance
    /// builder.WithVSync(false);
    /// </code>
    /// </example>
    public WindowBuilder WithVSync(bool enabled = true)
    {
        _options.VSync = enabled;
        return this;
    }

    /// <summary>
    /// Sets the initial state of the window (normal, minimized, maximized, or fullscreen).
    /// </summary>
    /// <param name="state">
    /// The desired window state from the WindowState enumeration.
    /// </param>
    /// <returns>
    /// This WindowBuilder instance for method chaining.
    /// </returns>
    /// <example>
    /// <code>
    /// // Start in fullscreen mode
    /// builder.WithState(WindowState.Fullscreen);
    /// 
    /// // Start maximized
    /// builder.WithState(WindowState.Maximized);
    /// </code>
    /// </example>
    public WindowBuilder WithState(WindowState state)
    {
        _options.WindowState = state;
        return this;
    }

    /// <summary>
    /// Configures whether the window can be resized by the user.
    /// </summary>
    /// <param name="resizable">
    /// True to allow window resizing, false to create a fixed-size window.
    /// Default is true.
    /// </param>
    /// <returns>
    /// This WindowBuilder instance for method chaining.
    /// </returns>
    /// <remarks>
    /// Non-resizable windows provide a consistent layout but may not work well
    /// on displays with different resolutions or DPI settings.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a fixed-size window
    /// builder.WithResizable(false);
    /// 
    /// // Allow resizing (default)
    /// builder.WithResizable(true);
    /// </code>
    /// </example>
    public WindowBuilder WithResizable(bool resizable = true)
    {
        _options.WindowBorder = resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        return this;
    }

    // add more WithXxx(...) methods as needed…

    /// <summary>
    /// Creates the window with the accumulated configuration options.
    /// </summary>
    /// <returns>
    /// A configured IWindow instance ready for rendering operations.
    /// The window is created but not yet visible until Run() is called.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when window creation fails due to invalid configuration or system limitations.
    /// </exception>
    /// <example>
    /// <code>
    /// var window = WindowBuilder
    ///     .Configure(windowManager)
    ///     .WithTitle("Game")
    ///     .WithSize(1280, 720)
    ///     .Create();
    ///     
    /// window.Run();
    /// </code>
    /// </example>
    public IWindow Create()
    {
        return _manager.CreateWindow(_options);
    }
}
