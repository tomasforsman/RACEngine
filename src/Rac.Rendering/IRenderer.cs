// File: src/Engine/Rendering/IRenderer.cs

using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
///   Rendering abstraction: supports multiple draw calls per frame with colors.
/// </summary>
public interface IRenderer
{
    /// <summary>Initialize GL state, compile shaders, setup VAO/VBO.</summary>
    void Initialize(IWindow window);

    /// <summary>Clear the color buffer at start of a frame.</summary>
    void Clear();

    /// <summary>Set the RGBA color for subsequent draw calls.</summary>
    void SetColor(Vector4D<float> rgba);

    /// <summary>Set the shader mode for visual effects.</summary>
    void SetShaderMode(ShaderMode mode);

    /// <summary>Upload vertex positions (2D) into the VBO.</summary>
    void UpdateVertices(float[] vertices);

    /// <summary>Issue DrawArrays on the currently bound VAO/VBO.</summary>
    void Draw();

    /// <summary>Finalize frame rendering with post-processing effects.</summary>
    void FinalizeFrame();

    /// <summary>Handle window resize (update viewport & aspect).</summary>
    void Resize(Vector2D<int> newSize);

    /// <summary>Release GL resources.</summary>
    void Shutdown();
}
