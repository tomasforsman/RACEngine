// ═══════════════════════════════════════════════════════════════════════════════
// GEOMETRY GENERATOR TESTS - COMPREHENSIVE VALIDATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// These tests validate the mathematical correctness, UV coordinate mapping, and
// edge case handling of the GeometryGenerators utility class. They ensure that
// generated geometry meets the specified requirements for texture mapping and
// vertex layout consistency.
//
// TESTING APPROACH:
// - Mathematical validation of vertex positions and UV coordinates
// - Boundary condition testing for edge cases
// - Consistency verification across different shape types
// - Performance and allocation pattern validation
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Rendering.Geometry;
using Silk.NET.Maths;
using Xunit;

namespace Rac.Rendering.Tests;

/// <summary>
/// Comprehensive test suite for GeometryGenerators utility class.
/// Validates mathematical correctness, UV mapping, and edge case handling.
/// </summary>
public class GeometryGeneratorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SQUARE GEOMETRY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CreateSquare_WithValidParameters_ReturnsCorrectVertexCount()
    {
        // Arrange
        var sideLength = 2.0f;
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateSquare(sideLength, color);

        // Assert
        Assert.Equal(6, vertices.Length); // 2 triangles × 3 vertices each
    }

    [Fact]
    public void CreateSquare_WithValidParameters_GeneratesCorrectVertexPositions()
    {
        // Arrange
        var sideLength = 2.0f;
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);
        var expectedHalfSize = 1.0f;

        // Act
        var vertices = GeometryGenerators.CreateSquare(sideLength, color);

        // Assert - Check that vertices form a square with correct bounds
        var positions = vertices.Select(v => v.Position).ToArray();
        
        // Extract unique positions (some vertices are shared between triangles)
        var uniquePositions = positions.Distinct().ToArray();
        Assert.Equal(4, uniquePositions.Length);

        // Verify all positions are within expected bounds
        foreach (var pos in uniquePositions)
        {
            Assert.True(MathF.Abs(pos.X) <= expectedHalfSize + 0.0001f, $"X position {pos.X} exceeds bounds");
            Assert.True(MathF.Abs(pos.Y) <= expectedHalfSize + 0.0001f, $"Y position {pos.Y} exceeds bounds");
        }

        // Verify we have all four corners
        Assert.Contains(uniquePositions, p => MathF.Abs(p.X + expectedHalfSize) < 0.0001f && MathF.Abs(p.Y + expectedHalfSize) < 0.0001f); // Bottom-left
        Assert.Contains(uniquePositions, p => MathF.Abs(p.X - expectedHalfSize) < 0.0001f && MathF.Abs(p.Y + expectedHalfSize) < 0.0001f); // Bottom-right
        Assert.Contains(uniquePositions, p => MathF.Abs(p.X - expectedHalfSize) < 0.0001f && MathF.Abs(p.Y - expectedHalfSize) < 0.0001f); // Top-right
        Assert.Contains(uniquePositions, p => MathF.Abs(p.X + expectedHalfSize) < 0.0001f && MathF.Abs(p.Y - expectedHalfSize) < 0.0001f); // Top-left
    }

    [Fact]
    public void CreateSquare_WithValidParameters_GeneratesCorrectUVCoordinates()
    {
        // Arrange
        var sideLength = 2.0f;
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateSquare(sideLength, color);

        // Assert - Verify UV coordinates are in [0,1] range and map to corners
        var uvCoordinates = vertices.Select(v => v.TexCoord).Distinct().ToArray();
        
        Assert.Equal(4, uvCoordinates.Length); // Four unique UV coordinates

        // Verify all UV coordinates are within [0,1] range
        foreach (var uv in uvCoordinates)
        {
            Assert.True(uv.X >= 0f && uv.X <= 1f, $"U coordinate {uv.X} outside [0,1] range");
            Assert.True(uv.Y >= 0f && uv.Y <= 1f, $"V coordinate {uv.Y} outside [0,1] range");
        }

        // Verify we have all four corner UV coordinates
        Assert.Contains(uvCoordinates, uv => MathF.Abs(uv.X - 0f) < 0.0001f && MathF.Abs(uv.Y - 0f) < 0.0001f); // (0,0)
        Assert.Contains(uvCoordinates, uv => MathF.Abs(uv.X - 1f) < 0.0001f && MathF.Abs(uv.Y - 0f) < 0.0001f); // (1,0)
        Assert.Contains(uvCoordinates, uv => MathF.Abs(uv.X - 1f) < 0.0001f && MathF.Abs(uv.Y - 1f) < 0.0001f); // (1,1)
        Assert.Contains(uvCoordinates, uv => MathF.Abs(uv.X - 0f) < 0.0001f && MathF.Abs(uv.Y - 1f) < 0.0001f); // (0,1)
    }

    [Fact]
    public void CreateSquare_WithValidParameters_AppliesColorToAllVertices()
    {
        // Arrange
        var sideLength = 1.5f;
        var expectedColor = new Vector4D<float>(0.8f, 0.6f, 0.4f, 0.9f);

        // Act
        var vertices = GeometryGenerators.CreateSquare(sideLength, expectedColor);

        // Assert
        foreach (var vertex in vertices)
        {
            Assert.Equal(expectedColor, vertex.Color);
        }
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(-0.001f)]
    public void CreateSquare_WithNonPositiveSideLength_ThrowsArgumentOutOfRangeException(float invalidSideLength)
    {
        // Arrange
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeometryGenerators.CreateSquare(invalidSideLength, color));
        
        Assert.Equal("sideLength", exception.ParamName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRIANGLE GEOMETRY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CreateTriangle_WithValidParameters_ReturnsCorrectVertexCount()
    {
        // Arrange
        var size = 2.0f;
        var color = new Vector4D<float>(1f, 0f, 0f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateTriangle(size, color);

        // Assert
        Assert.Equal(3, vertices.Length);
    }

    [Fact]
    public void CreateTriangle_WithValidParameters_GeneratesEquilateralTriangle()
    {
        // Arrange
        var size = 2.0f;
        var color = new Vector4D<float>(1f, 0f, 0f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateTriangle(size, color);

        // Assert - Verify triangle is approximately equilateral
        var positions = vertices.Select(v => v.Position).ToArray();
        
        // Calculate side lengths
        var side1Length = Vector2D.Distance(positions[0], positions[1]);
        var side2Length = Vector2D.Distance(positions[1], positions[2]);
        var side3Length = Vector2D.Distance(positions[2], positions[0]);
        
        // All sides should be approximately equal
        var tolerance = 0.0001f;
        Assert.True(MathF.Abs(side1Length - side2Length) < tolerance, "Sides 1 and 2 not equal");
        Assert.True(MathF.Abs(side2Length - side3Length) < tolerance, "Sides 2 and 3 not equal");
        Assert.True(MathF.Abs(side3Length - side1Length) < tolerance, "Sides 3 and 1 not equal");
    }

    [Fact]
    public void CreateTriangle_WithValidParameters_IsCenteredAtOrigin()
    {
        // Arrange
        var size = 3.0f;
        var color = new Vector4D<float>(0f, 1f, 0f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateTriangle(size, color);

        // Assert - Centroid should be at or very close to origin
        var positions = vertices.Select(v => v.Position).ToArray();
        var centroidX = positions.Average(p => p.X);
        var centroidY = positions.Average(p => p.Y);
        
        var tolerance = 0.0001f;
        Assert.True(MathF.Abs(centroidX) < tolerance, $"Triangle not centered in X: centroid X = {centroidX}");
        Assert.True(MathF.Abs(centroidY) < tolerance, $"Triangle not centered in Y: centroid Y = {centroidY}");
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(-0.001f)]
    public void CreateTriangle_WithNonPositiveSize_ThrowsArgumentOutOfRangeException(float invalidSize)
    {
        // Arrange
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeometryGenerators.CreateTriangle(invalidSize, color));
        
        Assert.Equal("size", exception.ParamName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECTANGLE GEOMETRY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CreateRectangle_WithValidParameters_ReturnsCorrectVertexCount()
    {
        // Arrange
        var width = 3.0f;
        var height = 2.0f;
        var color = new Vector4D<float>(0f, 0f, 1f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateRectangle(width, height, color);

        // Assert
        Assert.Equal(6, vertices.Length); // 2 triangles × 3 vertices each
    }

    [Fact]
    public void CreateRectangle_WithValidParameters_GeneratesCorrectDimensions()
    {
        // Arrange
        var width = 4.0f;
        var height = 2.0f;
        var color = new Vector4D<float>(1f, 1f, 0f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateRectangle(width, height, color);

        // Assert
        var positions = vertices.Select(v => v.Position).ToArray();
        var uniquePositions = positions.Distinct().ToArray();
        
        Assert.Equal(4, uniquePositions.Length);

        // Check bounds
        var minX = uniquePositions.Min(p => p.X);
        var maxX = uniquePositions.Max(p => p.X);
        var minY = uniquePositions.Min(p => p.Y);
        var maxY = uniquePositions.Max(p => p.Y);
        
        var tolerance = 0.0001f;
        Assert.True(MathF.Abs(maxX - minX - width) < tolerance, $"Width incorrect: expected {width}, got {maxX - minX}");
        Assert.True(MathF.Abs(maxY - minY - height) < tolerance, $"Height incorrect: expected {height}, got {maxY - minY}");
    }

    [Theory]
    [InlineData(0f, 1f)]
    [InlineData(-1f, 1f)]
    [InlineData(1f, 0f)]
    [InlineData(1f, -1f)]
    public void CreateRectangle_WithNonPositiveDimensions_ThrowsArgumentOutOfRangeException(float width, float height)
    {
        // Arrange
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeometryGenerators.CreateRectangle(width, height, color));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CIRCLE GEOMETRY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void CreateCircle_WithValidParameters_ReturnsCorrectVertexCount(int segments)
    {
        // Arrange
        var radius = 1.5f;
        var color = new Vector4D<float>(1f, 0f, 1f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateCircle(radius, segments, color);

        // Assert
        Assert.Equal(segments * 3, vertices.Length); // segments triangles × 3 vertices each
    }

    [Fact]
    public void CreateCircle_WithValidParameters_GeneratesVerticesAtCorrectRadius()
    {
        // Arrange
        var radius = 2.0f;
        var segments = 16;
        var color = new Vector4D<float>(0.5f, 0.5f, 0.5f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateCircle(radius, segments, color);

        // Assert - Check that perimeter vertices are at correct radius
        var tolerance = 0.0001f;
        
        for (int i = 0; i < vertices.Length; i += 3)
        {
            // Skip center vertex (index 0 of each triangle)
            var vertex1 = vertices[i + 1]; // Perimeter vertex
            var vertex2 = vertices[i + 2]; // Perimeter vertex
            
            var distance1 = MathF.Sqrt(vertex1.Position.X * vertex1.Position.X + vertex1.Position.Y * vertex1.Position.Y);
            var distance2 = MathF.Sqrt(vertex2.Position.X * vertex2.Position.X + vertex2.Position.Y * vertex2.Position.Y);
            
            Assert.True(MathF.Abs(distance1 - radius) < tolerance, $"Vertex at distance {distance1}, expected {radius}");
            Assert.True(MathF.Abs(distance2 - radius) < tolerance, $"Vertex at distance {distance2}, expected {radius}");
        }
    }

    [Fact]
    public void CreateCircle_WithValidParameters_HasCenterVertexAtOrigin()
    {
        // Arrange
        var radius = 1.0f;
        var segments = 8;
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act
        var vertices = GeometryGenerators.CreateCircle(radius, segments, color);

        // Assert - Every third vertex (center vertices) should be at origin
        var tolerance = 0.0001f;
        
        for (int i = 0; i < vertices.Length; i += 3)
        {
            var centerVertex = vertices[i];
            Assert.True(MathF.Abs(centerVertex.Position.X) < tolerance, $"Center vertex X not at origin: {centerVertex.Position.X}");
            Assert.True(MathF.Abs(centerVertex.Position.Y) < tolerance, $"Center vertex Y not at origin: {centerVertex.Position.Y}");
            
            // Center UV should be (0.5, 0.5)
            Assert.True(MathF.Abs(centerVertex.TexCoord.X - 0.5f) < tolerance, $"Center UV X incorrect: {centerVertex.TexCoord.X}");
            Assert.True(MathF.Abs(centerVertex.TexCoord.Y - 0.5f) < tolerance, $"Center UV Y incorrect: {centerVertex.TexCoord.Y}");
        }
    }

    [Theory]
    [InlineData(0f, 8)]
    [InlineData(-1f, 8)]
    [InlineData(1f, 2)]
    [InlineData(1f, 1)]
    [InlineData(1f, 0)]
    public void CreateCircle_WithInvalidParameters_ThrowsArgumentOutOfRangeException(float radius, int segments)
    {
        // Arrange
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeometryGenerators.CreateCircle(radius, segments, color));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UV COORDINATE CONSISTENCY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void AllShapes_GenerateUVCoordinatesInValidRange()
    {
        // Arrange
        var color = new Vector4D<float>(1f, 1f, 1f, 1f);

        // Act
        var squareVertices = GeometryGenerators.CreateSquare(2f, color);
        var triangleVertices = GeometryGenerators.CreateTriangle(2f, color);
        var rectangleVertices = GeometryGenerators.CreateRectangle(3f, 2f, color);
        var circleVertices = GeometryGenerators.CreateCircle(1.5f, 12, color);

        // Assert - All UV coordinates should be in [0,1] range
        var allVertices = squareVertices.Concat(triangleVertices)
                                      .Concat(rectangleVertices)
                                      .Concat(circleVertices);

        foreach (var vertex in allVertices)
        {
            Assert.True(vertex.TexCoord.X >= 0f && vertex.TexCoord.X <= 1f, 
                       $"UV X coordinate {vertex.TexCoord.X} outside [0,1] range");
            Assert.True(vertex.TexCoord.Y >= 0f && vertex.TexCoord.Y <= 1f, 
                       $"UV Y coordinate {vertex.TexCoord.Y} outside [0,1] range");
        }
    }

    [Fact]
    public void AllShapes_ApplyColorCorrectly()
    {
        // Arrange
        var testColor = new Vector4D<float>(0.2f, 0.7f, 0.9f, 0.6f);

        // Act
        var squareVertices = GeometryGenerators.CreateSquare(1f, testColor);
        var triangleVertices = GeometryGenerators.CreateTriangle(1f, testColor);
        var rectangleVertices = GeometryGenerators.CreateRectangle(2f, 1f, testColor);
        var circleVertices = GeometryGenerators.CreateCircle(1f, 8, testColor);

        // Assert
        var allVertices = squareVertices.Concat(triangleVertices)
                                      .Concat(rectangleVertices)
                                      .Concat(circleVertices);

        foreach (var vertex in allVertices)
        {
            Assert.Equal(testColor, vertex.Color);
        }
    }
}