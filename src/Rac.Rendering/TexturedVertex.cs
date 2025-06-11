using System.Runtime.InteropServices;

using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Rac.Rendering;

/// <summary>
/// Enhanced vertex with position and texture coordinates for gradient effects
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TexturedVertex
{
    public Vector2D<float> Position;
    public Vector2D<float> TexCoord;

    public TexturedVertex(Vector2D<float> position, Vector2D<float> texCoord)
    {
        Position = position;
        TexCoord = texCoord;
    }

    public static VertexLayout GetLayout() => new(
        new[]
        {
            new VertexAttribute(0, 2, VertexAttribPointerType.Float, false, 0),
            new VertexAttribute(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2)
        },
        sizeof(float) * 4
    );
}