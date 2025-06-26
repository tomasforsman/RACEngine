using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

/// <summary>
/// Manages the lifecycle and configuration of application windows.
/// Provides a concrete implementation of window management using Silk.NET windowing system.
/// </summary>
/// <remarks>
/// This class implements the IWindowManager interface and provides window creation,
/// resize handling, and state management for cross-platform applications.
/// 
/// The WindowManager follows the facade pattern to simplify window operations and
/// provides event-driven resize handling for responsive layouts. It maintains
/// window state including size and aspect ratio calculations.
/// 
/// Thread Safety: This class is not thread-safe. Window operations should be
/// performed on the main UI thread.
/// </remarks>
/// <example>
/// <code>
/// // Basic window creation
/// var windowManager = new WindowManager();
/// var options = WindowOptions.Default;
/// options.Title = "My Application";
/// options.Size = new Vector2D&lt;int&gt;(1280, 720);
/// 
/// var window = windowManager.CreateWindow(options);
/// 
/// // Subscribe to resize events
/// windowManager.OnResize += (newSize) => {
///     Console.WriteLine($"Window resized to {newSize.X}x{newSize.Y}");
/// };
/// 
/// window.Run();
/// </code>
/// </example>
public class WindowManager : IWindowManager
{
    // Null‐forgiveness initializer so the compiler knows we'll set this in CreateWindow()

    /// <summary>
    /// Gets the current client area size of the window in pixels.
    /// </summary>
    /// <value>
    /// A Vector2D representing the width and height of the window's drawable area.
    /// Updated automatically when the window is resized.
    /// </value>
    public Vector2D<int> Size { get; private set; }
    
    /// <summary>
    /// Gets the aspect ratio of the window calculated as height divided by width.
    /// </summary>
    /// <value>
    /// A floating-point value representing the height-to-width ratio.
    /// Recalculated automatically when the window size changes.
    /// </value>
    public float AspectRatio => Size.Y / (float)Size.X;
    
    /// <summary>
    /// Occurs when the window is resized by the user or programmatically.
    /// </summary>
    /// <remarks>
    /// This event is raised after the Size property has been updated.
    /// Use this to update viewport settings or UI layouts.
    /// </remarks>
    public event Action<Vector2D<int>>? OnResize;
    
    /// <summary>
    /// Gets the underlying Silk.NET window instance for advanced operations.
    /// </summary>
    /// <value>
    /// The native IWindow instance providing direct access to windowing features.
    /// </value>
    public IWindow NativeWindow { get; private set; } = null!;

    /// <summary>
    /// Creates a new window with the specified configuration options.
    /// </summary>
    /// <param name="options">
    /// Window configuration including size, title, and display settings.
    /// Must contain valid window parameters.
    /// </param>
    /// <returns>
    /// A configured IWindow instance ready for rendering operations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when window creation fails.
    /// </exception>
    /// <example>
    /// <code>
    /// var options = WindowOptions.Default;
    /// options.Title = "Game Window";
    /// options.Size = new Vector2D&lt;int&gt;(1920, 1080);
    /// 
    /// var window = CreateWindow(options);
    /// </code>
    /// </example>
    public IWindow CreateWindow(WindowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
            
        NativeWindow = Window.Create(options);
        Size = NativeWindow.Size;
        NativeWindow.Resize += HandleResize;
        return NativeWindow;
    }

    /// <summary>
    /// Handles window resize events and updates internal state.
    /// </summary>
    /// <param name="newSize">The new window size in pixels.</param>
    private void HandleResize(Vector2D<int> newSize)
    {
        Size = newSize;
        OnResize?.Invoke(newSize);
    }
}
