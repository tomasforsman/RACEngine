using System;
using Rac.Rendering.Shader;
using Rac.Rendering.Camera;
using Rac.Assets.Types;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
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

    public void SetTexture(Assets.Types.Texture texture)
    {
        // No-op: no texture to set
    }

    public void SetCameraMatrix(Matrix4X4<float> cameraMatrix)
    {
        // No-op: no camera matrix to set
    }

    public void SetActiveCamera(ICamera camera)
    {
        // No-op: no camera to set
    }

    public void SetShaderMode(ShaderMode mode)
    {
        // No-op: no shader to set
    }

    public void SetPrimitiveType(PrimitiveType primitiveType)
    {
        // No-op: no primitive type to set
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