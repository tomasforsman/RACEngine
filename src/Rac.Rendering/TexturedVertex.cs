using System.Runtime.InteropServices;

using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Rac.Rendering;

/// <summary>
/// Enhanced vertex with position and texture coordinates for gradient effects.
/// 
/// EDUCATIONAL PURPOSE:
/// This vertex type demonstrates basic texture mapping concepts without per-vertex color data.
/// When used with the rendering pipeline, it automatically receives default color (1,1,1,1)
/// and focuses purely on position and texture coordinate relationships.
/// 
/// USE CASES:
/// - Simple textured geometry without color variation
/// - Gradient effects using texture sampling
/// - Educational texture mapping demonstrations
/// - Performance-optimized rendering with minimal vertex data
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TexturedVertex
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
    /// 
    /// TEXTURED VERTEX SPECIFICS:
    /// - When converted to FullVertex, receives default color (1,1,1,1)
    /// - Optimal for gradient effects that rely purely on texture data
    /// - Reduced memory footprint compared to FullVertex
    /// </summary>
    /// <example>
    /// <code>
    /// // Standard UV mapping for a quad from local coordinates [-0.3, 0.3]
    /// var texCoordU = (localX + 0.3f) / 0.6f;  // Maps [-0.3, 0.3] to [0, 1]
    /// var texCoordV = (localY + 0.3f) / 0.6f;  // Maps [-0.3, 0.3] to [0, 1]
    /// var vertex = new TexturedVertex(position, new Vector2D&lt;float&gt;(texCoordU, texCoordV));
    /// </code>
    /// </example>
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