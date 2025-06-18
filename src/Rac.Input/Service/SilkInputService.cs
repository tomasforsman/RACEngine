using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Input.Service;

/// <summary>
/// Silk.NET implementation of input service providing keyboard and mouse input handling.
/// Integrates with Silk.NET windowing system to capture and process user input events.
/// </summary>
public class SilkInputService : IInputService
{
    private IMouse? _mouse;

    /// <summary>
    /// Event triggered when the left mouse button is clicked, providing the click position in pixels.
    /// </summary>
    public event Action<Vector2D<float>>? OnLeftClick;

    /// <summary>
    /// Event triggered when the mouse wheel is scrolled, providing the scroll delta.
    /// </summary>
    public event Action<float>? OnMouseScroll;

    /// <summary>
    /// Event triggered when any key is pressed, providing the key.
    /// </summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? PressedKey;

    /// <summary>
    /// Event triggered when keyboard keys are pressed or released.
    /// </summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;

    /// <summary>
    /// Gets the current keyboard key state tracker.
    /// </summary>
    public KeyboardKeyState KeyboardKeyKeyState { get; } = new();

    /// <summary>
    /// Gets the current keyboard key state tracker (alias for compatibility).
    /// </summary>
    public KeyboardKeyState KeyEvent { get; } = new();

    /// <summary>
    /// Initializes the input service with the specified window.
    /// Sets up mouse and keyboard event handlers.
    /// </summary>
    /// <param name="window">The window to capture input from.</param>
    /// <exception cref="InvalidOperationException">Thrown when no mouse is found.</exception>
    public void Initialize(IWindow window)
    {
        var inputContext = window.CreateInput();
        _mouse =
            inputContext.Mice.Count > 0
                ? inputContext.Mice[0]
                : throw new InvalidOperationException("No mouse found");
        _mouse.MouseDown += OnMouseDown;
        _mouse.Scroll += OnMouseScrollWheel;

        // Keyboard
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += (_, key, _) =>
            {
                KeyboardKeyKeyState.KeyDown(key);
                PressedKey?.Invoke(key, KeyboardKeyState.KeyEvent.Pressed);
                OnKeyEvent?.Invoke(key, KeyboardKeyState.KeyEvent.Pressed);
            };
            keyboard.KeyUp += (_, key, _) =>
            {
                KeyboardKeyKeyState.KeyUp(key);
                OnKeyEvent?.Invoke(key, KeyboardKeyState.KeyEvent.Released);
            };
        }
    }

    /// <summary>
    /// Updates the input service state. Currently a no-op as events are handled asynchronously.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    public void Update(double delta)
    {
        // Poll or update input state if needed
    }

    /// <summary>
    /// Shuts down the input service and unregisters event handlers.
    /// </summary>
    public void Shutdown()
    {
        if (_mouse != null)
        {
            _mouse.MouseDown -= OnMouseDown;
            _mouse.Scroll -= OnMouseScrollWheel;
        }
    }

    /// <summary>
    /// Handles mouse button down events, specifically left mouse button clicks.
    /// </summary>
    /// <param name="m">The mouse instance that triggered the event.</param>
    /// <param name="b">The mouse button that was pressed.</param>
    private void OnMouseDown(IMouse m, MouseButton b)
    {
        if (b != MouseButton.Left)
            return;

        var pos = m.Position;
        OnLeftClick?.Invoke(new Vector2D<float>(pos.X, pos.Y));
    }

    /// <summary>
    /// Handles mouse scroll wheel events.
    /// </summary>
    /// <param name="m">The mouse instance that triggered the event.</param>
    /// <param name="wheel">The scroll wheel data containing delta values.</param>
    private void OnMouseScrollWheel(IMouse m, ScrollWheel wheel)
    {
        // Use Y component for vertical scrolling (standard mouse wheel)
        OnMouseScroll?.Invoke(wheel.Y);
    }
}
