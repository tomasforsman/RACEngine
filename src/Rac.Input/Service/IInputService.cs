using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Input.Service;

public interface IInputService
{
    /// <summary>Realtime polling of which keys are pressed.</summary>
    KeyboardKeyState KeyboardKeyKeyState { get; }

    /// <summary>Realtime polling of which keys are currently down.</summary>
    KeyboardKeyState KeyEvent { get; }

    void Initialize(IWindow window);
    void Update(double delta);
    void Shutdown();

    /// <summary>
    ///   Occurs when the left mouse button is pressed, providing the position in pixels.
    /// </summary>
    event Action<Vector2D<float>>? OnLeftClick;

    /// <summary>
    ///   Occurs when the mouse wheel is scrolled, providing the scroll delta.
    /// </summary>
    event Action<float>? OnMouseScroll;

    /// <summary>
    ///   Occurs when any key is pressed, providing the <see cref="Silk.NET.Input.Key" />.
    /// </summary>
    event Action<Key, KeyboardKeyState.KeyEvent>? PressedKey;

    /// <summary>
    ///   Fires on any key press or release.
    /// </summary>
    event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;
}
