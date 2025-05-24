using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

public class WindowManager : IWindowManager
{
    // Null‐forgiveness initializer so the compiler knows we'll set this in CreateWindow()

    public Vector2D<int> Size { get; private set; }
    public float AspectRatio => Size.Y / (float)Size.X;
    public event Action<Vector2D<int>>? OnResize;
    public IWindow NativeWindow { get; private set; } = null!;

    public IWindow CreateWindow(WindowOptions options)
    {
        NativeWindow = Window.Create(options);
        Size = NativeWindow.Size;
        NativeWindow.Resize += HandleResize;
        return NativeWindow;
    }

    private void HandleResize(Vector2D<int> newSize)
    {
        Size = newSize;
        OnResize?.Invoke(newSize);
    }
}
