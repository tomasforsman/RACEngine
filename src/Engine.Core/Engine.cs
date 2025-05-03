using Engine.Input.Input;
using Engine.Rendering;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;

namespace Engine.Core
{
    public class Engine
    {
        private IWindow _window;
        private OpenGLRenderer _renderer; // concrete type to access UpdateVertices
        private SilkInputService _input;  // concrete type to access event

        private Vector2D<int> _windowSize;

        // Expose mouse click event for the game to subscribe
        public event Action<Vector2D<float>>? OnLeftClick;

        public void Run()
        {
            var opts = WindowOptions.Default;
            opts.Title = "My Silk.NET Engine";
            opts.Size = new Vector2D<int>(800, 600);
            opts.VSync = true;

            _window = Window.Create(opts);
            _windowSize = opts.Size;

            _renderer = new OpenGLRenderer();
            _input = new SilkInputService();

            _window.Load    += OnLoad;
            _window.Render  += OnRender;
            _window.Update  += OnUpdate;
            _window.Resize  += OnResize;
            _window.Closing += OnClosing;

            // Forward input mouse clicks to external subscribers
            _input.OnLeftClick += pos => OnLeftClick?.Invoke(pos);

            _window.Run();
        }

        private void OnLoad()
        {
            _renderer.Initialize(_window);
            _input.Initialize(_window);
        }

        private void OnRender(double dt)
        {
            _renderer.Render(dt);
        }

        private void OnUpdate(double dt)
        {
            _input.Update(dt);
        }

        private void OnResize(Vector2D<int> size)
        {
            _windowSize = size;
            _renderer.Resize(size);
        }

        private void OnClosing()
        {
            _input.Shutdown();
            _renderer.Shutdown();
        }

        // Allow the game to update vertex data in the renderer
        public void UpdateVertices(float[] vertices)
        {
            _renderer.UpdateVertices(vertices);
        }
    }
}
