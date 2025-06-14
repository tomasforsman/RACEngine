using System.Runtime.InteropServices;

using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Rac.Rendering;

/// <summary>
/// Full-featured vertex for advanced effects with per-vertex colors.
/// This is the standard internal format - all other vertex types are converted to this format
/// with default color (1,1,1,1) to ensure consistent, explicit color handling.
/// 
/// Use FullVertex directly when you need:
/// - Explicit per-vertex colors
/// - Transparency effects (alpha < 1.0)
/// - Advanced visual effects requiring color variation
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FullVertex
{
    public Vector2D<float> Position;
    public Vector2D<float> TexCoord;
    public Vector4D<float> Color;

    public FullVertex(Vector2D<float> position, Vector2D<float> texCoord, Vector4D<float> color)
    {
        Position = position;
        TexCoord = texCoord;
        Color = color;
    }

    public static VertexLayout GetLayout() => new(
        new[]
        {
            new VertexAttribute(0, 2, VertexAttribPointerType.Float, false, 0),
            new VertexAttribute(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2),
            new VertexAttribute(2, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4)
        },
        sizeof(float) * 8
    );
}