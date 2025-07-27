// File: src/Engine/Rendering/IRenderer.cs

using Rac.Rendering.Shader;
using Rac.Rendering.Camera;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
/// Rendering service interface providing both simple and advanced rendering functionality.
/// </summary>
public interface IRenderer
{
    /// <summary>Initialize GL state, compile shaders, setup VAO/VBO.</summary>
    void Initialize(IWindow window);

    // Simple rendering methods
    /// <summary>Clear the color buffer at start of a frame.</summary>
    void Clear();

    /// <summary>Set the RGBA color for subsequent draw calls.</summary>
    void SetColor(Vector4D<float> rgba);

    /// <summary>Upload vertex positions (2D) into the VBO.</summary>
    void UpdateVertices(float[] vertices);

    /// <summary>Issue DrawArrays on the currently bound VAO/VBO.</summary>
    void Draw();

    /// <summary>Finalize frame rendering with post-processing effects.</summary>
    void FinalizeFrame();

    // Advanced rendering methods
    /// <summary>Set the camera transformation matrix for vertex transformations.</summary>
    void SetCameraMatrix(Matrix4X4<float> cameraMatrix);

    /// <summary>Set the active camera for subsequent rendering operations.</summary>
    void SetActiveCamera(ICamera camera);

    /// <summary>Set the shader mode for visual effects.</summary>
    void SetShaderMode(ShaderMode mode);

    /// <summary>Set the primitive type for subsequent draw calls (default: Triangles).</summary>
    void SetPrimitiveType(PrimitiveType primitiveType);

    /// <summary>Set the texture for subsequent textured rendering operations.</summary>
    void SetTexture(Rac.Assets.Types.Texture texture);

    /// <summary>Upload vertex data with automatic layout detection and type safety.</summary>
    void UpdateVertices<T>(T[] vertices) where T : unmanaged;

    /// <summary>Upload raw float array with explicit layout specification.</summary>
    void UpdateVertices(float[] vertices, VertexLayout layout);

    /// <summary>Handle window resize (update viewport & aspect).</summary>
    void Resize(Vector2D<int> newSize);

    /// <summary>Release GL resources.</summary>
    void Shutdown();
}
