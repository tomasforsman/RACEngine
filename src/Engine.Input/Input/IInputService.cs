using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Input.Input
{
    public interface IInputService
    {
        void Initialize(IWindow window);
        void Update(double delta);
        void Shutdown();
        
        event Action<Vector2D<float>>? OnLeftClick;
    }
}
