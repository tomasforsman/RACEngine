using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Core.Manager;

public interface IWindowManager
{
    /// <summary>Current client‐area size of the window.</summary>
    Vector2D<int> Size { get; }

    /// <summary>Height/Width for aspect‐ratio corrections.</summary>
    float AspectRatio { get; }

    /// <summary>The low‐level Silk.NET window.</summary>
    IWindow NativeWindow { get; }

    /// <summary>Fired whenever the window is resized.</summary>
    event Action<Vector2D<int>>? OnResize;

    /// <summary>Create a window with the provided options.</summary>
    IWindow CreateWindow(WindowOptions options);
}
