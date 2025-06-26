using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

/// <summary>
/// Provides window management abstraction for cross-platform application windowing.
/// Manages window lifecycle, configuration, and provides event-driven resize handling.
/// </summary>
/// <remarks>
/// This interface abstracts platform-specific windowing systems using the Silk.NET library.
/// It supports various windowing platforms including Windows, Linux, and macOS through
/// a unified API that handles window creation, sizing, and event management.
/// 
/// The window manager follows the facade pattern to simplify complex windowing operations
/// and provides responsive layout handling through resize events.
/// </remarks>
/// <example>
/// <code>
/// // Create and configure a window
/// var windowManager = new WindowManager();
/// var options = WindowOptions.Default;
/// options.Title = "My Game";
/// options.Size = new Vector2D&lt;int&gt;(1920, 1080);
/// 
/// var window = windowManager.CreateWindow(options);
/// window.Run();
/// </code>
/// </example>
public interface IWindowManager
{
    /// <summary>
    /// Gets the current client area size of the window in pixels.
    /// </summary>
    /// <value>
    /// A Vector2D representing the width and height of the window's drawable area, 
    /// excluding title bars, borders, and other non-client elements.
    /// </value>
    /// <remarks>
    /// This size represents the actual rendering area available for graphics operations.
    /// The size is updated automatically when the window is resized and triggers the OnResize event.
    /// </remarks>
    Vector2D<int> Size { get; }

    /// <summary>
    /// Gets the aspect ratio of the window calculated as height divided by width.
    /// </summary>
    /// <value>
    /// A floating-point value representing the height-to-width ratio.
    /// Common values include 0.5625 (16:9), 0.75 (4:3), and 1.0 (square).
    /// </value>
    /// <remarks>
    /// This property is commonly used for perspective projection calculations and 
    /// maintaining proper proportions in 3D rendering pipelines.
    /// </remarks>
    float AspectRatio { get; }

    /// <summary>
    /// Gets the underlying Silk.NET window instance for advanced operations.
    /// </summary>
    /// <value>
    /// The native IWindow instance that provides direct access to low-level windowing operations.
    /// </value>
    /// <remarks>
    /// Use this property when you need direct access to Silk.NET windowing features
    /// that are not exposed through the IWindowManager abstraction.
    /// Handle with care as direct manipulation may interfere with the manager's state.
    /// </remarks>
    IWindow NativeWindow { get; }

    /// <summary>
    /// Occurs when the window is resized by the user or programmatically.
    /// </summary>
    /// <remarks>
    /// This event is raised after the Size property has been updated with the new dimensions.
    /// Event handlers should not perform long-running operations as they may block the UI thread.
    /// Use this event to update viewport settings, projection matrices, or UI layouts.
    /// </remarks>
    event Action<Vector2D<int>>? OnResize;

    /// <summary>
    /// Creates a new window with the specified configuration options.
    /// </summary>
    /// <param name="options">
    /// Window configuration including size, title, display settings, and behavior options.
    /// Must not be null and should contain valid window parameters.
    /// </param>
    /// <returns>
    /// A configured IWindow instance ready for rendering operations and event handling.
    /// The window is created but not yet visible until Run() is called.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when window creation fails due to invalid options or system limitations.
    /// </exception>
    /// <example>
    /// <code>
    /// var options = WindowOptions.Default;
    /// options.Title = "Game Window";
    /// options.Size = new Vector2D&lt;int&gt;(1920, 1080);
    /// options.VSync = true;
    /// 
    /// var window = windowManager.CreateWindow(options);
    /// </code>
    /// </example>
    IWindow CreateWindow(WindowOptions options);
}
