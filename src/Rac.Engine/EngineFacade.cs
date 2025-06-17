// File: src/Rac.Engine/EngineFacade.cs

using Rac.Audio;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Silk.NET.Input;

namespace Rac.Engine;

public class EngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;

    public EngineFacade(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager
    )
    {
        World = new World();
        Systems = new SystemScheduler();
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);

        // Initialize audio service (use null object pattern as fallback)
        Audio = new NullAudioService();

        // hook up core pipeline
        _inner.OnLoadEvent += () => LoadEvent?.Invoke();
        _inner.OnEcsUpdate += dt =>
        {
            Systems.Update(dt);
            UpdateEvent?.Invoke(dt);
        };
        _inner.OnRenderFrame += dt => RenderEvent?.Invoke(dt);

        // forward key events
        _inner.OnKeyEvent += (key, evt) => KeyEvent?.Invoke(key, evt);
    }

    public World World { get; }
    public SystemScheduler Systems { get; }
    public IRenderer Renderer => _inner.Renderer;
    public IAudioService Audio { get; }

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    public event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    public event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    public event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

    /// <summary>Register an ECS system.</summary>
    public void AddSystem(ISystem system)
    {
        Systems.Add(system);
    }

    /// <summary>Start the engine loop.</summary>
    public void Run()
    {
        _inner.Run();
    }
}
