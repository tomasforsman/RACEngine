using Silk.NET.Input;

namespace Rac.Input.State;

public class KeyboardKeyState
{
    /// <summary>
    ///   Key event that just occurred.
    /// </summary>
    public enum KeyEvent
	{
		Pressed,
		Released
	}

	private readonly HashSet<Key> keysDown = new();

	public void KeyDown(Key key)
	{
		keysDown.Add(key);
	}

	public void KeyUp(Key key)
	{
		keysDown.Remove(key);
	}

	public bool IsKeyDown(Key key)
	{
		return keysDown.Contains(key);
	}

	public void Reset()
	{
		keysDown.Clear();
	}
}