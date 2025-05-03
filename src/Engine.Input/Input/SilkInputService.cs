using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;

namespace Engine.Input.Input
{
	public class SilkInputService : IInputService
	{
		private IMouse _mouse;

		// Event to notify left mouse clicks with position in pixels
		public event Action<Vector2D<float>>? OnLeftClick;

		public void Initialize(IWindow window)
		{
			var inp = window.CreateInput();
			_mouse = inp.Mice.Count > 0
				? inp.Mice[0]
				: throw new InvalidOperationException("No mouse found");
			_mouse.MouseDown += OnMouseDown;
		}

		public void Update(double delta)
		{
			// Poll or update input state if needed
		}

		public void Shutdown()
		{
			_mouse.MouseDown -= OnMouseDown;
		}

		private void OnMouseDown(IMouse m, MouseButton b)
		{
			if (b != MouseButton.Left) return;

			var pos = m.Position;
			OnLeftClick?.Invoke(new Vector2D<float>((float)pos.X, (float)pos.Y));
		}
	}
}