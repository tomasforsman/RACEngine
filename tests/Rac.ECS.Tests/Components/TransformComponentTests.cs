using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Components;

public class TransformComponentTests
{
    [Fact]
    public void DefaultConstructor_CreatesIdentityTransform()
    {
        // Act
        var transform = new TransformComponent();

        // Assert
        Assert.Equal(Vector2D<float>.Zero, transform.LocalPosition);
        Assert.Equal(0f, transform.LocalRotation);
        Assert.Equal(Vector2D<float>.One, transform.LocalScale);
    }

    [Fact]
    public void PositionOnlyConstructor_UsesDefaultRotationAndScale()
    {
        // Arrange
        var position = new Vector2D<float>(10f, 20f);

        // Act
        var transform = new TransformComponent(position);

        // Assert
        Assert.Equal(position, transform.LocalPosition);
        Assert.Equal(0f, transform.LocalRotation);
        Assert.Equal(Vector2D<float>.One, transform.LocalScale);
    }

    [Fact]
    public void PositionAndRotationConstructor_UsesDefaultScale()
    {
        // Arrange
        var position = new Vector2D<float>(10f, 20f);
        var rotation = MathF.PI / 4f; // 45 degrees

        // Act
        var transform = new TransformComponent(position, rotation);

        // Assert
        Assert.Equal(position, transform.LocalPosition);
        Assert.Equal(rotation, transform.LocalRotation);
        Assert.Equal(Vector2D<float>.One, transform.LocalScale);
    }

    [Fact]
    public void GetLocalMatrix_ReturnsCorrectTransformationMatrix()
    {
        // Arrange
        var position = new Vector2D<float>(10f, 20f);
        var rotation = MathF.PI / 2f; // 90 degrees
        var scale = new Vector2D<float>(2f, 3f);
        var transform = new TransformComponent(position, rotation, scale);

        // Act
        var matrix = transform.GetLocalMatrix();

        // Assert - Check if matrix correctly transforms a point
        var testPoint = new Vector4D<float>(1f, 0f, 0f, 1f); // Unit vector along X
        var transformedPoint = Vector4D.Transform(testPoint, matrix);
        
        // After scale (2,3), rotation (90°), and translation (10,20):
        // (1,0) -> (2,0) -> (0,2) -> (10,22)
        Assert.Equal(10f, transformedPoint.X, 3); // Allow small floating point errors
        Assert.Equal(22f, transformedPoint.Y, 3);
    }

    [Fact]
    public void WithPosition_UpdatesPositionPreservesOthers()
    {
        // Arrange
        var original = new TransformComponent(
            new Vector2D<float>(10f, 20f), 
            MathF.PI / 4f, 
            new Vector2D<float>(2f, 3f));
        var newPosition = new Vector2D<float>(100f, 200f);

        // Act
        var updated = original.WithPosition(newPosition);

        // Assert
        Assert.Equal(newPosition, updated.LocalPosition);
        Assert.Equal(original.LocalRotation, updated.LocalRotation);
        Assert.Equal(original.LocalScale, updated.LocalScale);
    }

    [Fact]
    public void WithRotation_UpdatesRotationPreservesOthers()
    {
        // Arrange
        var original = new TransformComponent(
            new Vector2D<float>(10f, 20f), 
            MathF.PI / 4f, 
            new Vector2D<float>(2f, 3f));
        var newRotation = MathF.PI;

        // Act
        var updated = original.WithRotation(newRotation);

        // Assert
        Assert.Equal(original.LocalPosition, updated.LocalPosition);
        Assert.Equal(newRotation, updated.LocalRotation);
        Assert.Equal(original.LocalScale, updated.LocalScale);
    }

    [Fact]
    public void WithScale_UpdatesScalePreservesOthers()
    {
        // Arrange
        var original = new TransformComponent(
            new Vector2D<float>(10f, 20f), 
            MathF.PI / 4f, 
            new Vector2D<float>(2f, 3f));
        var newScale = new Vector2D<float>(5f, 6f);

        // Act
        var updated = original.WithScale(newScale);

        // Assert
        Assert.Equal(original.LocalPosition, updated.LocalPosition);
        Assert.Equal(original.LocalRotation, updated.LocalRotation);
        Assert.Equal(newScale, updated.LocalScale);
    }

    [Fact]
    public void WithUniformScale_UpdatesScaleUniformly()
    {
        // Arrange
        var original = new TransformComponent();
        var uniformScale = 2.5f;

        // Act
        var updated = original.WithUniformScale(uniformScale);

        // Assert
        Assert.Equal(new Vector2D<float>(uniformScale, uniformScale), updated.LocalScale);
    }
}