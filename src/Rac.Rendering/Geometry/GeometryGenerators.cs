// ═══════════════════════════════════════════════════════════════════════════════
// STANDARDIZED GEOMETRY CREATION UTILITIES
// ═══════════════════════════════════════════════════════════════════════════════
//
// This class provides standardized methods for generating common 2D geometric shapes
// with correct texture coordinates and proper vertex layout. All geometry is generated
// centered at the origin with normalized texture coordinates for consistent behavior.
//
// EDUCATIONAL PURPOSE:
// Demonstrates proper geometry construction, UV mapping principles, and vertex layout
// patterns used in modern graphics programming. Each method showcases different
// geometric primitives and their mathematical relationships.
//
// UV COORDINATE SYSTEM:
// Uses traditional [0,1] texture coordinate mapping where:
// - (0,0) represents bottom-left corner
// - (1,1) represents top-right corner
// - Standard OpenGL texture sampling convention
//
// DESIGN PRINCIPLES:
// - Geometry centered at origin for easy transformation
// - Consistent vertex ordering (counter-clockwise)
// - Normalized texture coordinates for predictable texture mapping
// - Educational comments explaining mathematical concepts
//
// ═══════════════════════════════════════════════════════════════════════════════

using Silk.NET.Maths;

namespace Rac.Rendering.Geometry;

/// <summary>
/// Static utility class for generating common 2D geometric shapes with standardized
/// texture coordinates and vertex layouts.
/// 
/// GEOMETRIC PRINCIPLES:
/// All shapes are generated centered at the origin (0,0) to enable easy transformation
/// through translation, rotation, and scaling operations. This design pattern follows
/// standard graphics programming practices where geometry is defined in local space
/// and then transformed to world space during rendering.
/// 
/// TEXTURE COORDINATE MAPPING:
/// Uses traditional [0,1] UV coordinate space where (0,0) represents the bottom-left
/// corner and (1,1) represents the top-right corner. This convention aligns with
/// OpenGL texture sampling standards and enables predictable texture application.
/// 
/// VERTEX ORDERING:
/// All polygons use counter-clockwise vertex ordering, which is the standard for
/// front-facing polygons in OpenGL. This ensures proper backface culling and
/// consistent rendering behavior.
/// 
/// PERFORMANCE CONSIDERATIONS:
/// Generated vertex arrays are allocated fresh for each call. For high-performance
/// applications requiring frequent geometry generation, consider caching results
/// or implementing object pooling patterns.
/// </summary>
public static class GeometryGenerators
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SQUARE GEOMETRY GENERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a square geometry centered at the origin with specified side length and color.
    /// 
    /// MATHEMATICAL SPECIFICATION:
    /// - Square extends from -sideLength/2 to +sideLength/2 in both X and Y directions
    /// - Vertices are ordered counter-clockwise starting from bottom-left
    /// - Two triangles form the square with shared diagonal edge
    /// 
    /// TEXTURE COORDINATE MAPPING:
    /// - Bottom-left: (0,0) - minimum texture coordinate
    /// - Bottom-right: (1,0) - maximum U, minimum V
    /// - Top-right: (1,1) - maximum texture coordinate  
    /// - Top-left: (0,1) - minimum U, maximum V
    /// 
    /// TRIANGLE DECOMPOSITION:
    /// Triangle 1: Bottom-left → Bottom-right → Top-right
    /// Triangle 2: Bottom-left → Top-right → Top-left
    /// This decomposition minimizes texture coordinate interpolation artifacts.
    /// </summary>
    /// <param name="sideLength">Length of each side of the square in world units</param>
    /// <param name="color">RGBA color applied to all vertices of the square</param>
    /// <returns>Array of 6 vertices (2 triangles) forming a square with proper UV mapping</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sideLength is negative or zero</exception>
    /// <example>
    /// <code>
    /// // Create a white square with 2.0 unit sides
    /// var square = GeometryGenerators.CreateSquare(2.0f, new Vector4D&lt;float&gt;(1f, 1f, 1f, 1f));
    /// renderer.UpdateVertices(square);
    /// renderer.Draw();
    /// 
    /// // Create a semi-transparent red square
    /// var redSquare = GeometryGenerators.CreateSquare(1.5f, new Vector4D&lt;float&gt;(1f, 0f, 0f, 0.5f));
    /// </code>
    /// </example>
    public static FullVertex[] CreateSquare(float sideLength, Vector4D<float> color)
    {
        if (sideLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sideLength), sideLength, "Side length must be positive");

        // Calculate half-extents for centering at origin
        var halfSize = sideLength * 0.5f;
        
        // Define square vertices in counter-clockwise order
        // Mathematical derivation: square extends from [-halfSize, -halfSize] to [+halfSize, +halfSize]
        var bottomLeft = new Vector2D<float>(-halfSize, -halfSize);
        var bottomRight = new Vector2D<float>(halfSize, -halfSize);
        var topRight = new Vector2D<float>(halfSize, halfSize);
        var topLeft = new Vector2D<float>(-halfSize, halfSize);
        
        // UV coordinates for traditional [0,1] texture mapping
        // Bottom-left corner maps to (0,0), top-right corner maps to (1,1)
        var uvBottomLeft = new Vector2D<float>(0f, 0f);
        var uvBottomRight = new Vector2D<float>(1f, 0f);
        var uvTopRight = new Vector2D<float>(1f, 1f);
        var uvTopLeft = new Vector2D<float>(0f, 1f);
        
        // Create vertex array for two triangles forming the square
        // Triangle winding order ensures proper front-face orientation
        return new FullVertex[]
        {
            // First triangle: Bottom-left → Bottom-right → Top-right
            new(bottomLeft, uvBottomLeft, color),
            new(bottomRight, uvBottomRight, color),
            new(topRight, uvTopRight, color),
            
            // Second triangle: Bottom-left → Top-right → Top-left
            new(bottomLeft, uvBottomLeft, color),
            new(topRight, uvTopRight, color),
            new(topLeft, uvTopLeft, color)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRIANGLE GEOMETRY GENERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an equilateral triangle geometry centered at the origin with specified size and color.
    /// 
    /// MATHEMATICAL SPECIFICATION:
    /// - Triangle is equilateral with all sides equal and all angles 60 degrees
    /// - Centered at origin with one vertex pointing upward (positive Y)
    /// - Height calculated using equilateral triangle formula: height = size * sqrt(3) / 2
    /// 
    /// GEOMETRIC PROPERTIES:
    /// - Top vertex: (0, height * 2/3) - positioned above center
    /// - Bottom-left vertex: (-size/2, -height/3) - positioned below and left of center
    /// - Bottom-right vertex: (size/2, -height/3) - positioned below and right of center
    /// 
    /// TEXTURE COORDINATE MAPPING:
    /// UV coordinates are mapped to triangle's bounding rectangle for consistent texture application:
    /// - Utilizes full [0,1] UV space for maximum texture resolution
    /// - Triangle vertices map to corners of UV space for predictable texture stretching
    /// </summary>
    /// <param name="size">Base width of the equilateral triangle in world units</param>
    /// <param name="color">RGBA color applied to all vertices of the triangle</param>
    /// <returns>Array of 3 vertices forming an equilateral triangle with proper UV mapping</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is negative or zero</exception>
    /// <example>
    /// <code>
    /// // Create a blue triangle with 1.5 unit base
    /// var triangle = GeometryGenerators.CreateTriangle(1.5f, new Vector4D&lt;float&gt;(0f, 0f, 1f, 1f));
    /// renderer.UpdateVertices(triangle);
    /// renderer.Draw();
    /// </code>
    /// </example>
    public static FullVertex[] CreateTriangle(float size, Vector4D<float> color)
    {
        if (size <= 0f)
            throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be positive");

        // Calculate triangle geometry using equilateral triangle mathematics
        // Height formula for equilateral triangle: h = side * sqrt(3) / 2
        var height = size * MathF.Sqrt(3f) * 0.5f;
        
        // Position vertices to center triangle at origin
        // Top vertex positioned at 2/3 of height above center for proper centering
        var topVertex = new Vector2D<float>(0f, height * 2f / 3f);
        var bottomLeft = new Vector2D<float>(-size * 0.5f, -height / 3f);
        var bottomRight = new Vector2D<float>(size * 0.5f, -height / 3f);
        
        // UV mapping for triangle - map to bounding rectangle
        var uvTop = new Vector2D<float>(0.5f, 1f);        // Top center of texture
        var uvBottomLeft = new Vector2D<float>(0f, 0f);   // Bottom-left of texture
        var uvBottomRight = new Vector2D<float>(1f, 0f);  // Bottom-right of texture
        
        // Create triangle with counter-clockwise vertex ordering
        return new FullVertex[]
        {
            new(bottomLeft, uvBottomLeft, color),
            new(bottomRight, uvBottomRight, color),
            new(topVertex, uvTop, color)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECTANGLE GEOMETRY GENERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a rectangle geometry centered at the origin with specified dimensions and color.
    /// 
    /// MATHEMATICAL SPECIFICATION:
    /// - Rectangle extends from -width/2 to +width/2 in X direction
    /// - Rectangle extends from -height/2 to +height/2 in Y direction
    /// - Two triangles form the rectangle with shared diagonal edge
    /// 
    /// TEXTURE COORDINATE MAPPING:
    /// Full [0,1] UV space mapping preserving aspect ratio:
    /// - Corner mapping ensures texture appears correctly without distortion
    /// - UV coordinates directly correspond to geometric corners
    /// 
    /// ASPECT RATIO CONSIDERATIONS:
    /// Rectangle dimensions directly control texture aspect ratio - textures will
    /// scale proportionally to the rectangle's width-to-height ratio.
    /// </summary>
    /// <param name="width">Width of the rectangle in world units</param>
    /// <param name="height">Height of the rectangle in world units</param>
    /// <param name="color">RGBA color applied to all vertices of the rectangle</param>
    /// <returns>Array of 6 vertices (2 triangles) forming a rectangle with proper UV mapping</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height is negative or zero</exception>
    /// <example>
    /// <code>
    /// // Create a green rectangle 3 units wide by 2 units tall
    /// var rectangle = GeometryGenerators.CreateRectangle(3f, 2f, new Vector4D&lt;float&gt;(0f, 1f, 0f, 1f));
    /// renderer.UpdateVertices(rectangle);
    /// renderer.Draw();
    /// </code>
    /// </example>
    public static FullVertex[] CreateRectangle(float width, float height, Vector4D<float> color)
    {
        if (width <= 0f)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive");
        if (height <= 0f)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive");

        // Calculate half-extents for centering at origin
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        
        // Define rectangle vertices in counter-clockwise order
        var bottomLeft = new Vector2D<float>(-halfWidth, -halfHeight);
        var bottomRight = new Vector2D<float>(halfWidth, -halfHeight);
        var topRight = new Vector2D<float>(halfWidth, halfHeight);
        var topLeft = new Vector2D<float>(-halfWidth, halfHeight);
        
        // UV coordinates for traditional [0,1] texture mapping
        var uvBottomLeft = new Vector2D<float>(0f, 0f);
        var uvBottomRight = new Vector2D<float>(1f, 0f);
        var uvTopRight = new Vector2D<float>(1f, 1f);
        var uvTopLeft = new Vector2D<float>(0f, 1f);
        
        // Create vertex array for two triangles forming the rectangle
        return new FullVertex[]
        {
            // First triangle: Bottom-left → Bottom-right → Top-right
            new(bottomLeft, uvBottomLeft, color),
            new(bottomRight, uvBottomRight, color),
            new(topRight, uvTopRight, color),
            
            // Second triangle: Bottom-left → Top-right → Top-left
            new(bottomLeft, uvBottomLeft, color),
            new(topRight, uvTopRight, color),
            new(topLeft, uvTopLeft, color)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CIRCLE GEOMETRY GENERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a circle geometry approximated by triangular segments centered at the origin.
    /// 
    /// MATHEMATICAL SPECIFICATION:
    /// - Circle approximated using triangular segments radiating from center
    /// - Each segment forms a triangle with center vertex and two perimeter vertices
    /// - Angular spacing between vertices: 2π / segments
    /// - Vertex positions calculated using polar coordinates: (r*cos(θ), r*sin(θ))
    /// 
    /// GEOMETRIC APPROXIMATION:
    /// Higher segment counts produce smoother circular appearance but increase vertex count.
    /// Recommended segment counts:
    /// - 8 segments: Octagon (low detail, good performance)
    /// - 16 segments: Adequate circle approximation for most uses
    /// - 32+ segments: High-quality smooth circle (higher vertex cost)
    /// 
    /// TEXTURE COORDINATE MAPPING:
    /// UV coordinates map circle to square texture space:
    /// - Center vertex: (0.5, 0.5) - texture center
    /// - Perimeter vertices: Mapped to circle inscribed in [0,1] UV square
    /// - Preserves circular texture mapping for radial textures
    /// </summary>
    /// <param name="radius">Radius of the circle in world units</param>
    /// <param name="segments">Number of triangular segments to approximate the circle (minimum 3)</param>
    /// <param name="color">RGBA color applied to all vertices of the circle</param>
    /// <returns>Array of vertices forming triangular segments that approximate a circle</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when radius is negative/zero or segments is less than 3</exception>
    /// <example>
    /// <code>
    /// // Create a yellow circle with radius 1.0 and 16 segments
    /// var circle = GeometryGenerators.CreateCircle(1f, 16, new Vector4D&lt;float&gt;(1f, 1f, 0f, 1f));
    /// renderer.UpdateVertices(circle);
    /// renderer.Draw();
    /// 
    /// // Create a high-detail purple circle
    /// var detailedCircle = GeometryGenerators.CreateCircle(0.8f, 32, new Vector4D&lt;float&gt;(0.5f, 0f, 1f, 1f));
    /// </code>
    /// </example>
    public static FullVertex[] CreateCircle(float radius, int segments, Vector4D<float> color)
    {
        if (radius <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be positive");
        if (segments < 3)
            throw new ArgumentOutOfRangeException(nameof(segments), segments, "Segments must be at least 3");

        // Calculate angular step between vertices
        // Full circle (2π radians) divided by number of segments
        var angleStep = 2f * MathF.PI / segments;
        
        // Prepare vertex array: 3 vertices per triangle, one triangle per segment
        var vertices = new FullVertex[segments * 3];
        
        // Center vertex properties (shared by all triangles)
        var centerPosition = new Vector2D<float>(0f, 0f);
        var centerUV = new Vector2D<float>(0.5f, 0.5f);  // Center of texture space
        
        for (int i = 0; i < segments; i++)
        {
            // Calculate angles for current and next vertex
            var currentAngle = i * angleStep;
            var nextAngle = (i + 1) * angleStep;
            
            // Calculate positions using polar coordinates
            // x = r * cos(θ), y = r * sin(θ)
            var currentPos = new Vector2D<float>(
                radius * MathF.Cos(currentAngle),
                radius * MathF.Sin(currentAngle)
            );
            var nextPos = new Vector2D<float>(
                radius * MathF.Cos(nextAngle),
                radius * MathF.Sin(nextAngle)
            );
            
            // Map perimeter positions to UV coordinates
            // Scale and offset from [-radius, radius] to [0, 1]
            var currentUV = new Vector2D<float>(
                (currentPos.X / radius + 1f) * 0.5f,
                (currentPos.Y / radius + 1f) * 0.5f
            );
            var nextUV = new Vector2D<float>(
                (nextPos.X / radius + 1f) * 0.5f,
                (nextPos.Y / radius + 1f) * 0.5f
            );
            
            // Create triangle: center → current → next (counter-clockwise)
            var triangleStart = i * 3;
            vertices[triangleStart] = new FullVertex(centerPosition, centerUV, color);
            vertices[triangleStart + 1] = new FullVertex(currentPos, currentUV, color);
            vertices[triangleStart + 2] = new FullVertex(nextPos, nextUV, color);
        }
        
        return vertices;
    }
}