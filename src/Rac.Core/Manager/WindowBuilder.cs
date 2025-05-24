using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

/// <summary>
///   Fluent builder for constructing and creating a window via IWindowManager.
/// </summary>
public class WindowBuilder
{
	private readonly IWindowManager _manager;
	private WindowOptions _options;

	private WindowBuilder(IWindowManager manager)
	{
		_manager = manager;
		_options = WindowOptions.Default;
	}

	public static WindowBuilder Configure(IWindowManager manager)
	{
		return new WindowBuilder(manager);
	}

	public WindowBuilder WithTitle(string title)
	{
		_options.Title = title;
		return this;
	}

	public WindowBuilder WithSize(int width, int height)
	{
		_options.Size = new Vector2D<int>(width, height);
		return this;
	}

	public WindowBuilder WithVSync(bool enabled = true)
	{
		_options.VSync = enabled;
		return this;
	}

	public WindowBuilder WithState(WindowState state)
	{
		_options.WindowState = state;
		return this;
	}

	public WindowBuilder WithResizable(bool resizable = true)
	{
		_options.WindowBorder = resizable ? WindowBorder.Resizable : WindowBorder.Fixed;
		return this;
	}

	// add more WithXxx(...) methods as needed…

    /// <summary>
    ///   Creates the window with the accumulated options.
    /// </summary>
    public IWindow Create()
	{
		return _manager.CreateWindow(_options);
	}
}