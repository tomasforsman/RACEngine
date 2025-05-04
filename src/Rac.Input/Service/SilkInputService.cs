using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Input.Service;

public class SilkInputService : IInputService
{
	private IMouse _mouse;
	private readonly KeyboardKeyState _keyboardKeyState = new();

	// Event to notify left mouse clicks with position in pixels
	// public event Action<Vector2D<float>>? OnLeftClick;
	
	
	public event Action<Key, KeyboardKeyState.KeyEvent>? PressedKey;

	public event Action<Vector2D<float>>? OnLeftClick;
	public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;
	public KeyboardKeyState KeyboardKeyKeyState => _keyboardKeyState;
	public KeyboardKeyState KeyEvent { get; }

	public void Initialize(IWindow window)
	{
		var inputContext = window.CreateInput();
		_mouse = inputContext.Mice.Count > 0
			? inputContext.Mice[0]
			: throw new InvalidOperationException("No mouse found");
		_mouse.MouseDown += OnMouseDown;
			
		// Keyboard
		foreach (var keyboard in inputContext.Keyboards)
		{
			keyboard.KeyDown += (_, key, _) =>
			{
				_keyboardKeyState.KeyDown(key);
				OnKeyEvent?.Invoke(key, KeyboardKeyState.KeyEvent.Pressed);
			};
			keyboard.KeyUp += (_, key, _) =>
			{
				_keyboardKeyState.KeyUp(key);
				OnKeyEvent?.Invoke(key, KeyboardKeyState.KeyEvent.Released);
			};
		}

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