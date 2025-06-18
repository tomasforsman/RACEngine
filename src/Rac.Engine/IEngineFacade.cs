using Rac.Audio;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Camera;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Rac.Engine;

/// <summary>
/// Interface for engine facade providing simplified access to engine services and lifecycle management.
/// </summary>
public interface IEngineFacade
{
    /// <summary>Gets the ECS world instance.</summary>
    World World { get; }

    /// <summary>Gets the system scheduler for managing ECS systems.</summary>
    SystemScheduler Systems { get; }

    /// <summary>Gets the renderer for graphics operations.</summary>
    IRenderer Renderer { get; }

    /// <summary>Gets the audio service for sound and music playback.</summary>
    IAudioService Audio { get; }

    /// <summary>Gets the camera manager for dual-camera system (game world and UI).</summary>
    ICameraManager CameraManager { get; }

    /// <summary>Gets the window manager for window operations and size information.</summary>
    IWindowManager WindowManager { get; }

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

    /// <summary>Fires when the left mouse button is clicked, providing screen coordinates in pixels.</summary>
    event Action<Vector2D<float>>? LeftClickEvent;

    /// <summary>Register an ECS system.</summary>
    void AddSystem(ISystem system);

    /// <summary>Start the engine loop.</summary>
    void Run();
}