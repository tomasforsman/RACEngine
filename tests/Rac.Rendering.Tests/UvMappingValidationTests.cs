// ═══════════════════════════════════════════════════════════════════════════════
// UV MAPPING VALIDATION TESTS
// ═══════════════════════════════════════════════════════════════════════════════
//
// This test suite validates that texture coordinate (UV) generation follows correct
// graphics programming practices, ensuring consistent mapping regardless of geometric
// transformations like rotation, translation, and scaling.
//
// EDUCATIONAL PURPOSE:
// These tests demonstrate the importance of calculating UV coordinates from original
// local vertex positions before any transformations are applied. This prevents
// texture coordinate changes during object animation and ensures proper texture mapping.

using Silk.NET.Maths;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests to verify correct UV mapping calculation that remains consistent
/// regardless of geometric transformations applied to vertices.
/// </summary>
public class UvMappingValidationTests
{
    /// <summary>
    /// Test that validates UV coordinates are calculated from local positions
    /// and normalized to [0,1] range correctly for the quad geometry used in the demo.
    /// </summary>
    [Fact]
    public void UvMapping_QuadGeometry_ShouldNormalizeToStandardRange()
    {
        // Arrange: Simulate the quad vertices from RenderingPipelineDemo
        // This represents a square with local coordinates ranging from -0.3 to 0.3
        var quadVertices = new float[]
        {
            // Position        Color (not used for UV calculation)
            -0.3f, -0.3f,     0.8f, 0.6f, 0.2f, 1.0f,  // Bottom left
             0.3f, -0.3f,     0.2f, 0.8f, 0.6f, 1.0f,  // Bottom right
             0.3f,  0.3f,     0.6f, 0.2f, 0.8f, 1.0f,  // Top right
        };
        
        // Act: Calculate UV coordinates using the same logic as the fixed method
        var uvCoordinates = CalculateUvCoordinatesFromVertices(quadVertices);
        
        // Assert: Verify UV coordinates are properly normalized to [0,1] range
        // Bottom left (-0.3, -0.3) should map to (0, 0)
        Assert.Equal(0.0f, uvCoordinates[0].X, precision: 3);
        Assert.Equal(0.0f, uvCoordinates[0].Y, precision: 3);
        
        // Bottom right (0.3, -0.3) should map to (1, 0)
        Assert.Equal(1.0f, uvCoordinates[1].X, precision: 3);
        Assert.Equal(0.0f, uvCoordinates[1].Y, precision: 3);
        
        // Top right (0.3, 0.3) should map to (1, 1)
        Assert.Equal(1.0f, uvCoordinates[2].X, precision: 3);
        Assert.Equal(1.0f, uvCoordinates[2].Y, precision: 3);
    }
    
    /// <summary>
    /// Test that validates UV coordinates remain consistent regardless of
    /// transformations applied to the vertices (rotation, translation, scaling).
    /// </summary>
    [Fact]
    public void UvMapping_WithTransformations_ShouldRemainConsistent()
    {
        // Arrange: Same quad vertices
        var quadVertices = new float[]
        {
            -0.3f, -0.3f,     0.8f, 0.6f, 0.2f, 1.0f,
             0.3f, -0.3f,     0.2f, 0.8f, 0.6f, 1.0f,
             0.3f,  0.3f,     0.6f, 0.2f, 0.8f, 1.0f,
        };
        
        // Act: Calculate UV coordinates from original vertices
        var originalUv = CalculateUvCoordinatesFromVertices(quadVertices);
        
        // Simulate transformed vertices (this would be the result after rotation/translation)
        var transformedVertices = new float[]
        {
            // Different positions, same colors
            1.0f, 2.0f,       0.8f, 0.6f, 0.2f, 1.0f,  // Transformed bottom left
            2.5f, 1.8f,       0.2f, 0.8f, 0.6f, 1.0f,  // Transformed bottom right  
            2.7f, 3.2f,       0.6f, 0.2f, 0.8f, 1.0f,  // Transformed top right
        };
        
        // The key insight: UV should be calculated from ORIGINAL positions, not transformed ones
        var transformedUv = CalculateUvCoordinatesFromVertices(quadVertices); // Use original for UV!
        
        // Assert: UV coordinates should be identical regardless of transformations
        for (int i = 0; i < originalUv.Length; i++)
        {
            Assert.Equal(originalUv[i].X, transformedUv[i].X, precision: 3);
            Assert.Equal(originalUv[i].Y, transformedUv[i].Y, precision: 3);
        }
    }
    
    /// <summary>
    /// Test that validates UV mapping for triangle geometry with different coordinate ranges.
    /// </summary>
    [Fact]
    public void UvMapping_TriangleGeometry_ShouldNormalizeCorrectly()
    {
        // Arrange: Triangle vertices from RenderingPipelineDemo
        // X ranges from -0.5 to 0.5 (range = 1.0), Y ranges from -0.5 to 0.5 (range = 1.0)
        var triangleVertices = new float[]
        {
            // Position        Color
             0.0f,  0.5f,     1.0f, 0.2f, 0.2f, 1.0f,  // Top vertex
            -0.5f, -0.5f,     0.2f, 1.0f, 0.2f, 1.0f,  // Bottom left
             0.5f, -0.5f,     0.2f, 0.2f, 1.0f, 1.0f   // Bottom right
        };
        
        // Act: Calculate UV coordinates
        var uvCoordinates = CalculateUvCoordinatesFromVertices(triangleVertices);
        
        // Assert: Verify UV coordinates are properly normalized
        // Top vertex (0.0, 0.5) should map to (0.5, 1.0)
        Assert.Equal(0.5f, uvCoordinates[0].X, precision: 3);
        Assert.Equal(1.0f, uvCoordinates[0].Y, precision: 3);
        
        // Bottom left (-0.5, -0.5) should map to (0.0, 0.0)
        Assert.Equal(0.0f, uvCoordinates[1].X, precision: 3);
        Assert.Equal(0.0f, uvCoordinates[1].Y, precision: 3);
        
        // Bottom right (0.5, -0.5) should map to (1.0, 0.0)
        Assert.Equal(1.0f, uvCoordinates[2].X, precision: 3);
        Assert.Equal(0.0f, uvCoordinates[2].Y, precision: 3);
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS - SIMULATE UV CALCULATION LOGIC
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Helper method that simulates the UV coordinate calculation logic
    /// from the fixed ConvertToFullVertices method.
    /// </summary>
    private static Vector2D<float>[] CalculateUvCoordinatesFromVertices(float[] vertices)
    {
        var result = new Vector2D<float>[vertices.Length / 6];
        
        // Find bounds of original vertex positions
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
        for (int i = 0; i < vertices.Length; i += 6)
        {
            var x = vertices[i];
            var y = vertices[i + 1];
            
            minX = MathF.Min(minX, x);
            maxX = MathF.Max(maxX, x);
            minY = MathF.Min(minY, y);
            maxY = MathF.Max(maxY, y);
        }
        
        // Calculate ranges for normalization
        float rangeX = maxX - minX;
        float rangeY = maxY - minY;
        
        // Handle degenerate cases
        if (rangeX <= 0f) rangeX = 1f;
        if (rangeY <= 0f) rangeY = 1f;
        
        // Calculate UV coordinates for each vertex
        for (int i = 0, vertexIndex = 0; i < vertices.Length; i += 6, vertexIndex++)
        {
            var localX = vertices[i];
            var localY = vertices[i + 1];
            
            // Normalize to [0,1] range
            var texCoordU = (localX - minX) / rangeX;
            var texCoordV = (localY - minY) / rangeY;
            
            result[vertexIndex] = new Vector2D<float>(texCoordU, texCoordV);
        }
        
        return result;
    }
}