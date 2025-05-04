using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;
using Engine.Rendering;
using Rac.Core.Manager;
using Rac.Input.Service;
using Rac.Input.State;
using Silk.NET.Input;

namespace Rac.Core
{
    public class GameEngine
    {
        private readonly IWindowManager _windowManager;
        private readonly IInputService _inputService;
        private readonly ConfigManager _configManager;
        private IWindow _window;
        private OpenGLRenderer _renderer;
        private Vector2D<int> _windowSize;
        private float[]? _pendingVertices;
        /// <summary>
        /// Fires every frame with the elapsed time in seconds.
        /// </summary>
        public event Action<float>? OnUpdateFrame;

        public event Action<Vector2D<float>>? OnLeftClick;
        public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;

        public GameEngine(IWindowManager windowManager,
                          IInputService inputService,
                          ConfigManager configManager)
        {
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
            _inputService  = inputService  ?? throw new ArgumentNullException(nameof(inputService));
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        }

        public void Run()
        {
            // Create and configure window via builder and config manager
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

            _window     = builder.Create();
            _windowSize = _window.Size;

            _renderer = new OpenGLRenderer();
            _windowManager.OnResize += size => _renderer.Resize(size);

            _window.Load    += OnLoad;
            _window.Render  += OnRender;
            _window.Update  += OnUpdate;
            _window.Update  += delta => OnUpdateFrame?.Invoke((float)delta);
            _window.Closing += OnClosing;

            _inputService.OnLeftClick += pos   => OnLeftClick?.Invoke(pos);
            _inputService.OnKeyEvent  += (k,e) => OnKeyEvent?.Invoke(k, e);

            _window.Run();
        }

        private void OnLoad()
        {
            _renderer.Initialize(_window);
            if (_pendingVertices is not null)
            {
                _renderer.UpdateVertices(_pendingVertices);
                _pendingVertices = null;
            }
            _inputService.Initialize(_window);
        }

        private void OnRender(double dt) => _renderer.Render(dt);

        private void OnUpdate(double dt) => _inputService.Update(dt);

        private void OnClosing()
        {
            _inputService.Shutdown();
            _renderer.Shutdown();
        }

        public void UpdateVertices(float[] vertices)
        {
            if (_renderer is not null)
                _renderer.UpdateVertices(vertices);
            else
                _pendingVertices = vertices;
        }
    }
}
