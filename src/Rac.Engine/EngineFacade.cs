namespace Rac.Engine;

using System;
using Rac.Core.Manager;
using Rac.Input.Service;
using Rac.ECS.Core;
using Rac.ECS.System;
using Rac.Rendering;
using Rac.GameEngine;

/// <summary>
/// High-level façade: composes the core loop, ECS & rendering into one Engine API.
/// </summary>
public class EngineFacade
{
	readonly Rac.GameEngine.Engine _inner;
	public World World { get; }
	public SystemScheduler SystemScheduler { get; }
	public IRenderer Renderer => _inner.Renderer;

	/// <summary>Fires once on init/load (before first UpdateEvent)</summary>
	public event Action? LoadEvent;
	/// <summary>Fires each frame, after ECS systems have run.</summary>
	public event Action<float>? UpdateEvent;
	/// <summary>Fires each frame, before Draw calls.</summary>
	public event Action<float>? RenderEvent;

	public EngineFacade(
		IWindowManager windowManager,
		IInputService inputService,
		ConfigManager configManager
	)
	{
		World           = new World();
		SystemScheduler = new SystemScheduler();
		_inner          = new Rac.GameEngine.Engine(windowManager, inputService, configManager);

		_inner.OnLoadEvent      += () => LoadEvent?.Invoke();
		_inner.OnEcsUpdate      += dt =>
		{
			SystemScheduler.Update(dt);
			UpdateEvent?.Invoke(dt);
		};
		_inner.OnRenderFrame    += dt => RenderEvent?.Invoke(dt);
	}

	/// <summary>Register your ECS systems here.</summary>
	public void AddSystem(ISystem system)
		=> SystemScheduler.Add(system);

	/// <summary>Kick everything off.</summary>
	public void Run()
		=> _inner.Run();
}