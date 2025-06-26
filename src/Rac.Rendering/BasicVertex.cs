using Silk.NET.Maths;
using Silk.NET.OpenGL;

using System.Runtime.InteropServices;

namespace Rac.Rendering;
// ════════════════════════════════════════════════════════════════════════════════
// VERTEX DATA STRUCTURES
// ════════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Represents the simplest vertex structure containing only position data for basic geometric rendering.
/// This is the most memory-efficient vertex format suitable for simple shapes and procedural effects.
/// </summary>
/// <remarks>
/// The BasicVertex structure is designed for scenarios where only positional information is needed:
/// - Simple geometric shapes (lines, triangles, basic polygons)
/// - Procedural effects that generate color through shaders
/// - Performance-critical rendering with minimal vertex data
/// - Educational examples demonstrating fundamental graphics concepts
/// 
/// Technical Details:
/// - Memory layout: 8 bytes per vertex (2 floats for X,Y coordinates)
/// - OpenGL attribute location 0: position (vec2)
/// - Sequential memory layout for optimal GPU transfer
/// - No color or texture coordinate data included
/// 
/// All BasicVertex instances are automatically converted to FullVertex format during rendering
/// with default white color (1,1,1,1) to maintain consistency in the rendering pipeline.
/// </remarks>
/// <example>
/// <code>
/// // Create a simple triangle
/// var triangle = new BasicVertex[]
/// {
///     new(new Vector2D&lt;float&gt;(-0.5f, -0.5f)),  // Bottom left
///     new(new Vector2D&lt;float&gt;( 0.5f, -0.5f)),  // Bottom right
///     new(new Vector2D&lt;float&gt;( 0.0f,  0.5f))   // Top center
/// };
/// 
/// // Upload to renderer
/// renderer.UpdateVertices(triangle);
/// renderer.Draw();
/// </code>
/// </example>
[StructLayout(LayoutKind.Sequential)]
public struct BasicVertex
{
    /// <summary>
    /// Gets or sets the 2D position of the vertex in local coordinate space.
    /// </summary>
    /// <value>
    /// A Vector2D containing the X and Y coordinates of the vertex position.
    /// Coordinates are typically in the range [-1, 1] for normalized device coordinates,
    /// but can represent any 2D coordinate system as needed by the application.
    /// </value>
    /// <remarks>
    /// The position represents the vertex location before any transformations are applied.
    /// During rendering, this position will be transformed by the current view and projection
    /// matrices to determine the final screen position.
    /// </remarks>
    public Vector2D<float> Position;

    /// <summary>
    /// Initializes a new instance of the BasicVertex struct with the specified position.
    /// </summary>
    /// <param name="position">
    /// The 2D position coordinates for this vertex.
    /// </param>
    /// <example>
    /// <code>
    /// // Create a vertex at the origin
    /// var centerVertex = new BasicVertex(Vector2D&lt;float&gt;.Zero);
    /// 
    /// // Create a vertex at specific coordinates
    /// var cornerVertex = new BasicVertex(new Vector2D&lt;float&gt;(1.0f, 1.0f));
    /// </code>
    /// </example>
    public BasicVertex(Vector2D<float> position)
    {
        Position = position;
    }

    /// <summary>
    /// Gets the vertex layout definition for OpenGL vertex attribute specification.
    /// </summary>
    /// <returns>
    /// A VertexLayout instance defining the structure of BasicVertex data for GPU consumption.
    /// The layout specifies position data at attribute location 0 as a 2-component float vector.
    /// </returns>
    /// <remarks>
    /// This method provides the metadata needed by the rendering system to correctly
    /// interpret BasicVertex data when uploading to GPU buffers. The layout specifies:
    /// - Attribute 0: Position (2 floats, 8 bytes)
    /// - Total stride: 8 bytes per vertex
    /// - No color or texture coordinate attributes
    /// 
    /// The vertex layout is used internally by the renderer for setting up vertex
    /// attribute pointers and ensuring correct data interpretation by shaders.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get layout for setting up vertex attributes
    /// var layout = BasicVertex.GetLayout();
    /// renderer.SetVertexLayout(layout);
    /// </code>
    /// </example>
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
