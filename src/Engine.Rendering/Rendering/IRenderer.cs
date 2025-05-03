using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Rendering
{
    public interface IRenderer
    {
        void Initialize(IWindow window);
        void Render(double delta);
        void Resize(Vector2D<int> newSize);
        void Shutdown();
        void UpdateVertices(float[] vertices);
    }
}
