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
    
    /// <summary>
    /// Texture coordinates (UV mapping) for sampling textures during rendering.
    /// 
    /// TEXTURE COORDINATE SYSTEM:
    /// - Values typically range from [0,1] representing normalized texture space
    /// - (0,0) corresponds to bottom-left corner of the texture
    /// - (1,1) corresponds to top-right corner of the texture  
    /// - U component (X) represents horizontal texture position
    /// - V component (Y) represents vertical texture position
    /// 
    /// UV MAPPING BEST PRACTICES:
    /// - Calculate from original local vertex positions before transformations
    /// - Normalize coordinates to [0,1] range: U = (localX - minX) / (maxX - minX)
    /// - Ensure consistency regardless of object rotation, translation, or scaling
    /// - Values outside [0,1] create tiling/wrapping effects based on texture settings
    /// 
    /// GRAPHICS PIPELINE INTEGRATION:
    /// - Passed to fragment shaders for texture sampling (texture2D, sampler2D)
    /// - Used with OpenGL texture coordinate interpolation across triangles
    /// - Essential for proper texture mapping in 2D and 3D rendering
    /// </summary>
    /// <example>
    /// <code>
    /// // Standard UV mapping for a quad from local coordinates [-0.5, 0.5]
    /// var texCoordU = (localX + 0.5f) / 1.0f;  // Maps [-0.5, 0.5] to [0, 1]
    /// var texCoordV = (localY + 0.5f) / 1.0f;  // Maps [-0.5, 0.5] to [0, 1]
    /// var vertex = new FullVertex(position, new Vector2D&lt;float&gt;(texCoordU, texCoordV), color);
    /// </code>
    /// </example>
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