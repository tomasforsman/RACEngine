// File: src/Rac.Core/GameEngine.cs
using System;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Rac.Core.Manager;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;

namespace Rac.GameEngine;

/// <summary>
/// Central game loop: drives ECS updates and multi‐pass rendering.
/// </summary>
public class Engine
{
    private readonly IWindowManager _windowManager;
    private readonly IInputService _inputService;
    private readonly ConfigManager _configManager;
    private IWindow _window = null!;
    private OpenGLRenderer _renderer = null!;
    private float[]? _pendingVertices;

    /// <summary>Fires once after the GL context and window are ready.</summary>
    public event Action? OnLoadEvent;

    /// <summary>Fires each frame before rendering: run your ECS systems here.</summary>
    public event Action<float>? OnEcsUpdate;

    /// <summary>Fires during the render pass: issue SetColor/UpdateVertices/Draw calls here.</summary>
    public event Action<float>? OnRenderFrame;

    public event Action<Vector2D<float>>? OnLeftClick;
    public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;

    /// <summary>Expose the renderer so samples can call SetColor/Draw directly.</summary>
    public IRenderer Renderer => _renderer;

    /// <summary>Expose window manager for resize/native‐window hooks.</summary>
    public IWindowManager WindowManager => _windowManager;

    public Engine(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager)
    {
        _windowManager = windowManager;
        _inputService  = inputService;
        _configManager = configManager;
    }

    public void Run()
    {
        var builder  = WindowBuilder.Configure(_windowManager);
        var settings = _configManager.Window;

        if (!string.IsNullOrEmpty(settings.Title))
            builder = builder.WithTitle(settings.Title);

        if (!string.IsNullOrEmpty(settings.Size))
        {
            var parts = settings.Size.Split(',');
            if (parts.Length == 2
                && int.TryParse(parts[0], out var w)
                && int.TryParse(parts[1], out var h))
            {
                builder = builder.WithSize(w, h);
            }
        }

        if (settings.VSync.HasValue)
            builder = builder.WithVSync(settings.VSync.Value);

        _window = builder.Create();

        _renderer = new OpenGLRenderer();
        _windowManager.OnResize += newSize => _renderer.Resize(newSize);

        // Load: initialize GL, pending vertices, then notify sample
        _window.Load += () =>
        {
            _renderer.Initialize(_window);
            if (_pendingVertices is not null)
            {
                _renderer.UpdateVertices(_pendingVertices);
                _pendingVertices = null;
            }
            _inputService.Initialize(_window);
            OnLoadEvent?.Invoke();
        };

        // Render pass: clear once, let sample draw, then flush
        _window.Render += dt =>
        {
            _renderer.Clear();
            OnRenderFrame?.Invoke((float)dt);
            _renderer.Draw();
        };

        // Update pass: input + ECS
        _window.Update += OnUpdate;
        _window.Update += delta => OnEcsUpdate?.Invoke((float)delta);

        _window.Closing += () =>
        {
            _inputService.Shutdown();
            _renderer.Shutdown();
        };

        _inputService.OnLeftClick += pos => OnLeftClick?.Invoke(pos);
        _inputService.OnKeyEvent  += (k, e) => OnKeyEvent?.Invoke(k, e);

        _window.Run();
    }

    private void OnUpdate(double delta)
        => _inputService.Update(delta);

    /// <summary>
    /// Upload vertex data to GPU (buffer if not ready).
    /// </summary>
    public void UpdateVertices(float[] vertices)
    {
        if (_renderer != null)
            _renderer.UpdateVertices(vertices);
        else
            _pendingVertices = vertices;
    }
}
