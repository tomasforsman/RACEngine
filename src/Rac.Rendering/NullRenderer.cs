using System;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
/// Null Object pattern implementation of IRenderer.
/// Provides safe no-op rendering functionality for testing and headless scenarios.
/// </summary>
public class NullRenderer : IRenderer
{
#if DEBUG
    private static bool _warningShown = false;
    
    private static void ShowWarningOnce()
    {
        if (!_warningShown)
        {
            _warningShown = true;
            Console.WriteLine("[DEBUG] Warning: NullRenderer is being used - no graphics will be rendered.");
        }
    }
#endif

    public void Initialize(IWindow window)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // No-op: no GL state to initialize
    }

    public void Clear()
    {
        // No-op: no buffer to clear
    }

    public void SetColor(Vector4D<float> rgba)
    {
        // No-op: no color to set
    }

    public void SetShaderMode(ShaderMode mode)
    {
        // No-op: no shader to set
    }

    public void UpdateVertices(float[] vertices)
    {
        // No-op: no vertices to upload
    }

    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        // No-op: no vertices to upload
    }

    public void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        // No-op: no vertices to upload
    }

    public void Draw()
    {
        // No-op: no drawing to perform
    }

    public void FinalizeFrame()
    {
        // No-op: no frame to finalize
    }

    public void Resize(Vector2D<int> newSize)
    {
        // No-op: no viewport to resize
    }

    public void Shutdown()
    {
        // No-op: no resources to release
    }
}