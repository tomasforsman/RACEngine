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
    /// </summary>
    /// <example>
    /// <code>
    /// // Centered UV mapping for a quad from local coordinates [-0.3, 0.3]
    /// var centerX = 0.0f; // (minX + maxX) / 2 = (-0.3 + 0.3) / 2
    /// var centerY = 0.0f; // (minY + maxY) / 2 = (-0.3 + 0.3) / 2
    /// var texCoordU = (localX - centerX) / rangeX;  // Center at (0,0)
    /// var texCoordV = (localY - centerY) / rangeY;  // Distance-based effects work correctly
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