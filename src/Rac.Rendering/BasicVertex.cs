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
// VERTEX COLOR BEHAVIOR:
// All vertex types now provide consistent color handling:
// - BasicVertex and TexturedVertex automatically get default color (1,1,1,1) - fully opaque white
// - FullVertex preserves explicit colors, including transparency (alpha < 1.0)
// - No more fragile alpha checks - transparency is explicit and intentional

// Example 1: Basic triangle with position-only vertices (automatically gets default color)
float[] basicTriangle = { -0.5f, -0.5f, 0.5f, -0.5f, 0.0f, 0.5f };
renderer.SetShaderMode(ShaderMode.Normal);
renderer.UpdateVertices(basicTriangle);  // Internally converted to include color (1,1,1,1)
renderer.Draw();

// Example 2: Type-safe basic vertices (automatically gets default color)
var basicVertices = new BasicVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f)),
    new(new Vector2D<float>( 0.5f, -0.5f)),
    new(new Vector2D<float>( 0.0f,  0.5f))
};
renderer.SetShaderMode(ShaderMode.Normal);
renderer.UpdateVertices(basicVertices);  // Internally converted to include color (1,1,1,1)
renderer.Draw();

// Example 3: Textured vertices for gradient effects (automatically gets default color)
var texturedVertices = new TexturedVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(-1f, -1f)),
    new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>( 1f, -1f)),
    new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>( 0f,  1f))
};
renderer.SetShaderMode(ShaderMode.SoftGlow);
renderer.UpdateVertices(texturedVertices);  // Internally converted to include color (1,1,1,1)
renderer.Draw();

// Example 4: Raw float array with explicit layout (automatically gets default color)
float[] texturedFloats = {
    -0.5f, -0.5f, -1f, -1f,   // position + texcoord
     0.5f, -0.5f,  1f, -1f,
     0.0f,  0.5f,  0f,  1f
};
renderer.SetShaderMode(ShaderMode.SoftGlow);
renderer.UpdateVertices(texturedFloats, TexturedVertex.GetLayout());  // Internally converted to include color (1,1,1,1)
renderer.Draw();

// Example 5: Full vertices with explicit per-vertex colors (including transparency)
var fullVertices = new FullVertex[]
{
    new(new Vector2D<float>(-0.5f, -0.5f), new Vector2D<float>(-1f, -1f), new Vector4D<float>(1f, 0f, 0f, 1f)),    // Red, fully opaque
    new(new Vector2D<float>( 0.5f, -0.5f), new Vector2D<float>( 1f, -1f), new Vector4D<float>(0f, 1f, 0f, 0.5f)),  // Green, 50% transparent
    new(new Vector2D<float>( 0.0f,  0.5f), new Vector2D<float>( 0f,  1f), new Vector4D<float>(0f, 0f, 1f, 0.8f))   // Blue, 80% opaque
};
renderer.UpdateVertices(fullVertices);  // Colors preserved exactly as specified
renderer.Draw();

// Example 6: Post-processing with bloom effects
renderer.SetShaderMode(ShaderMode.Bloom);
renderer.Clear();
// ... render bright objects ...
renderer.FinalizeFrame();  // Applies bloom post-processing
*/
