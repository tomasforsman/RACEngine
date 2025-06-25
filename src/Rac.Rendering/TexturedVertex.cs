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
    /// TEXTURE COORDINATE SYSTEM FOR PROCEDURAL EFFECTS:
    /// - Coordinates are centered around (0,0) representing the geometry center
    /// - Values typically range from approximately [-0.5, 0.5] for normalized geometry
    /// - Distance from center = length(UV) used for procedural distance-based effects
    /// - Compatible with OpenGL texture sampling when offset to [0,1] range if needed
    /// 
    /// UV MAPPING BEST PRACTICES:
    /// - Calculate from original local vertex positions before transformations
    /// - Center coordinates around (0,0): U = (localX - centerX) / rangeX
    /// - Ensure consistency regardless of object rotation, translation, or scaling
    /// - Centered coordinates enable proper distance-based procedural effects
    /// 
    /// GRAPHICS PIPELINE INTEGRATION:
    /// - Used for distance calculations in procedural fragment shaders
    /// - Can be transformed to [0,1] range for traditional texture sampling
    /// - Essential for proper effect calculations in procedural rendering
    /// 
    /// TEXTURED VERTEX SPECIFICS:
    /// - When converted to FullVertex, receives default color (1,1,1,1)
    /// - Optimal for procedural effects that rely on geometry-centered coordinates
    /// - Reduced memory footprint compared to FullVertex
    /// </summary>
    /// <example>
    /// <code>
    /// // Centered UV mapping for a quad from local coordinates [-0.3, 0.3]
    /// var centerX = 0.0f; // (minX + maxX) / 2 = (-0.3 + 0.3) / 2
    /// var centerY = 0.0f; // (minY + maxY) / 2 = (-0.3 + 0.3) / 2
    /// var texCoordU = (localX - centerX) / rangeX;  // Center at (0,0)
    /// var texCoordV = (localY - centerY) / rangeY;  // Distance-based effects work correctly
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