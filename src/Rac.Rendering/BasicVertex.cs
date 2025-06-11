using Silk.NET.Maths;
using Silk.NET.OpenGL;

using System.Runtime.InteropServices;

namespace Rac.Rendering;
// ════════════════════════════════════════════════════════════════════════════════
// VERTEX DATA STRUCTURES
// ════════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Simple vertex with position only for basic effects
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BasicVertex
{
    public Vector2D<float> Position;

    public BasicVertex(Vector2D<float> position)
    {
        Position = position;
    }

    public static VertexLayout GetLayout() => new(
        new[]
        {
            new VertexAttribute(0, 2, VertexAttribPointerType.Float, false, 0)
        },
        sizeof(float) * 2
    );
}


// ════════════════════════════════════════════════════════════════════════════════
// USAGE EXAMPLES
// ════════════════════════════════════════════════════════════════════════════════

/*
// Example 1: Basic triangle with position-only vertices
float[] basicTriangle = { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };
renderer.SetShaderMode(ShaderMode.Normal);
renderer.UpdateVertices(basicTriangle);
renderer.Draw();

// Example 2: Type-safe basic vertices
var basicVertices = new BasicVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f)),
    new(new Vector2D<float>( 0.5f, -0.5f)),
    new(new Vector2D<float>( 0.0f,  0.5f))
};
renderer.SetShaderMode(ShaderMode.Normal);
renderer.UpdateVertices(basicVertices);
renderer.Draw();

// Example 3: Textured vertices for gradient effects
var texturedVertices = new TexturedVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(-1f, -1f)),
    new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>( 1f, -1f)),
    new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>( 0f,  1f))
};
renderer.SetShaderMode(ShaderMode.SoftGlow);
renderer.UpdateVertices(texturedVertices);
renderer.Draw();

// Example 4: Raw float array with explicit layout for textured rendering
float[] texturedFloats = {
    -0.5f, -0.5f, -1f, -1f,   // position + texcoord
     0.5f, -0.5f,  1f, -1f,
     0.0f,  0.5f,  0f,  1f
};
renderer.SetShaderMode(ShaderMode.SoftGlow);
renderer.UpdateVertices(texturedFloats, TexturedVertex.GetLayout());
renderer.Draw();

// Example 5: Full vertices with per-vertex colors
var fullVertices = new FullVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(-1f, -1f), new Vector4D<float>(1f, 0f, 0f, 1f)),
    new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>( 1f, -1f), new Vector4D<float>(0f, 1f, 0f, 1f)),
    new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>( 0f,  1f), new Vector4D<float>(0f, 0f, 1f, 1f))
};
renderer.UpdateVertices(fullVertices);
renderer.Draw();

// Example 6: Post-processing with bloom effects
renderer.SetShaderMode(ShaderMode.Bloom);
renderer.Clear();
// ... render bright objects ...
renderer.FinalizeFrame();  // Applies bloom post-processing
*/
