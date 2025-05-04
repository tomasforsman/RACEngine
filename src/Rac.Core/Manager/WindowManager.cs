using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;

namespace Rac.Core.Manager
{
	public class WindowManager : IWindowManager
	{
		// Null‐forgiveness initializer so the compiler knows we'll set this in CreateWindow()
		private IWindow _window = null!;

		public Vector2D<int> Size { get; private set; }
		public float AspectRatio => Size.Y / (float)Size.X;
		public event Action<Vector2D<int>>? OnResize;
		public IWindow NativeWindow => _window;

		public IWindow CreateWindow(WindowOptions options)
		{
			_window = Window.Create(options);
			Size    = _window.Size;
			_window.Resize += HandleResize;
			return _window;
		}

		private void HandleResize(Vector2D<int> newSize)
		{
			Size = newSize;
			OnResize?.Invoke(newSize);
		}
	}
}