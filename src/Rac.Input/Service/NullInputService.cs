using System;
using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Input.Service;

/// <summary>
/// Null Object pattern implementation of IInputService.
/// Provides safe no-op input functionality for testing and headless scenarios.
/// </summary>
public class NullInputService : IInputService
{
#if DEBUG
    private static bool _warningShown = false;
    
    private static void ShowWarningOnce()
    {
        if (!_warningShown)
        {
            _warningShown = true;
            Console.WriteLine("[DEBUG] Warning: NullInputService is being used - no input will be processed.");
        }
    }
#endif

    /// <summary>Gets empty keyboard state (no keys pressed).</summary>
    public KeyboardKeyState KeyboardKeyKeyState { get; } = new();

    /// <summary>Gets empty key event state.</summary>
    public KeyboardKeyState KeyEvent { get; } = new();

    /// <summary>Initialize input service (no-op).</summary>
    public void Initialize(IWindow window)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // No-op: no input to initialize
    }

    /// <summary>Update input state (no-op).</summary>
    public void Update(double delta)
    {
        // No-op: no input to update
    }

    /// <summary>Shutdown input service (no-op).</summary>
    public void Shutdown()
    {
        // No-op: no resources to cleanup
    }

    /// <summary>Left mouse click event (never fires).</summary>
    public event Action<Vector2D<float>>? OnLeftClick;

    /// <summary>Mouse scroll event (never fires).</summary>
    public event Action<float>? OnMouseScroll;

    /// <summary>Key pressed event (never fires).</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? PressedKey;

    /// <summary>Key event (never fires).</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;
}