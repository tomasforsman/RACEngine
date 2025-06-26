using Rac.ECS.Components;
using Silk.NET.Maths;
using Xunit;

namespace Rac.ECS.Tests.Components;

public class WorldTransformComponentTests
{
    [Fact]
    public void DefaultConstructor_CreatesIdentityTransform()
    {
        // Act
        var worldTransform = new WorldTransformComponent();

        // Assert
        Assert.Equal(Vector2D<float>.Zero, worldTransform.WorldPosition);
        Assert.Equal(0f, worldTransform.WorldRotation);
        Assert.Equal(Vector2D<float>.One, worldTransform.WorldScale);
        Assert.Equal(Matrix4X4<float>.Identity, worldTransform.WorldMatrix);
    }

    [Fact]
    public void ConstructorWithComponents_ComputesCorrectMatrix()
    {
        // Arrange
        var position = new Vector2D<float>(10f, 20f);
        var rotation = MathF.PI / 2f; // 90 degrees
        var scale = new Vector2D<float>(2f, 3f);

        // Act
        var worldTransform = new WorldTransformComponent(position, rotation, scale);

        // Assert
        Assert.Equal(position, worldTransform.WorldPosition);
        Assert.Equal(rotation, worldTransform.WorldRotation);
        Assert.Equal(scale, worldTransform.WorldScale);
        
        // Test matrix by transforming a point
        var testPoint = new Vector4D<float>(1f, 0f, 0f, 1f);
        var transformedPoint = Vector4D.Transform(testPoint, worldTransform.WorldMatrix);
        
        // After scale (2,3), rotation (90°), and translation (10,20):
        // (1,0) -> (2,0) -> (0,2) -> (10,22)
        Assert.Equal(10f, transformedPoint.X, 3);
        Assert.Equal(22f, transformedPoint.Y, 3);
    }

    [Fact]
    public void PositionOnlyConstructor_UsesDefaultRotationAndScale()
    {
        // Arrange
        var position = new Vector2D<float>(100f, 200f);

        // Act
        var worldTransform = new WorldTransformComponent(position);

        // Assert
        Assert.Equal(position, worldTransform.WorldPosition);
        Assert.Equal(0f, worldTransform.WorldRotation);
        Assert.Equal(Vector2D<float>.One, worldTransform.WorldScale);
    }

    [Fact]
    public void TransformPoint_CorrectlyTransformsLocalToWorld()
    {
        // Arrange
        var worldTransform = new WorldTransformComponent(
            new Vector2D<float>(10f, 20f),  // Translation
            MathF.PI / 2f,                  // 90 degree rotation
            new Vector2D<float>(2f, 1f)     // Scale
        );
        var localPoint = new Vector2D<float>(1f, 0f);

        // Act
        var worldPoint = worldTransform.TransformPoint(localPoint);

        // Assert
        // (1,0) scaled by (2,1) = (2,0)
        // (2,0) rotated 90° = (0,2)
        // (0,2) translated by (10,20) = (10,22)
        Assert.Equal(10f, worldPoint.X, 3);
        Assert.Equal(22f, worldPoint.Y, 3);
    }

    [Fact]
    public void TransformDirection_IgnoresTranslation()
    {
        // Arrange
        var worldTransform = new WorldTransformComponent(
            new Vector2D<float>(100f, 200f), // Large translation (should be ignored)
            MathF.PI / 2f,                    // 90 degree rotation
            new Vector2D<float>(2f, 1f)       // Scale
        );
        var localDirection = new Vector2D<float>(1f, 0f);

        // Act
        var worldDirection = worldTransform.TransformDirection(localDirection);

        // Assert
        // (1,0) scaled by (2,1) = (2,0)
        // (2,0) rotated 90° = (0,2)
        // Translation ignored for direction vectors
        Assert.Equal(0f, worldDirection.X, 3);
        Assert.Equal(2f, worldDirection.Y, 3);
    }

    [Fact]
    public void InverseTransformPoint_CorrectlyTransformsWorldToLocal()
    {
        // Arrange
        var worldTransform = new WorldTransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 2f,
            new Vector2D<float>(2f, 1f)
        );
        var worldPoint = new Vector2D<float>(10f, 22f);

        // Act
        var localPoint = worldTransform.InverseTransformPoint(worldPoint);

        // Assert
        // This should reverse the transformation from TransformPoint test
        Assert.Equal(1f, localPoint.X, 3);
        Assert.Equal(0f, localPoint.Y, 3);
    }

    [Fact]
    public void Forward_ReturnsCorrectDirectionVector()
    {
        // Arrange
        var worldTransform = new WorldTransformComponent(
            Vector2D<float>.Zero,
            MathF.PI / 2f,  // 90 degrees
            Vector2D<float>.One
        );

        // Act
        var forward = worldTransform.Forward;

        // Assert
        // Forward at 90° should point up (0,1)
        Assert.Equal(0f, forward.X, 3);
        Assert.Equal(1f, forward.Y, 3);
    }

    [Fact]
    public void Right_ReturnsCorrectDirectionVector()
    {
        // Arrange
        var worldTransform = new WorldTransformComponent(
            Vector2D<float>.Zero,
            MathF.PI / 2f,  // 90 degrees
            Vector2D<float>.One
        );

        // Act
        var right = worldTransform.Right;

        // Assert
        // Right at 90° should point left (-1,0)
        Assert.Equal(-1f, right.X, 3);
        Assert.Equal(0f, right.Y, 3);
    }

    [Fact]
    public void WithPosition_UpdatesPositionAndMatrix()
    {
        // Arrange
        var original = new WorldTransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 4f,
            new Vector2D<float>(2f, 3f)
        );
        var newPosition = new Vector2D<float>(100f, 200f);

        // Act
        var updated = original.WithPosition(newPosition);

        // Assert
        Assert.Equal(newPosition, updated.WorldPosition);
        Assert.Equal(original.WorldRotation, updated.WorldRotation);
        Assert.Equal(original.WorldScale, updated.WorldScale);
        
        // Matrix should be updated too
        Assert.NotEqual(original.WorldMatrix, updated.WorldMatrix);
    }

    [Fact]
    public void WithRotation_UpdatesRotationAndMatrix()
    {
        // Arrange
        var original = new WorldTransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 4f,
            new Vector2D<float>(2f, 3f)
        );
        var newRotation = MathF.PI;

        // Act
        var updated = original.WithRotation(newRotation);

        // Assert
        Assert.Equal(original.WorldPosition, updated.WorldPosition);
        Assert.Equal(newRotation, updated.WorldRotation);
        Assert.Equal(original.WorldScale, updated.WorldScale);
        
        // Matrix should be updated too
        Assert.NotEqual(original.WorldMatrix, updated.WorldMatrix);
    }

    [Fact]
    public void WithScale_UpdatesScaleAndMatrix()
    {
        // Arrange
        var original = new WorldTransformComponent(
            new Vector2D<float>(10f, 20f),
            MathF.PI / 4f,
            new Vector2D<float>(2f, 3f)
        );
        var newScale = new Vector2D<float>(5f, 6f);

        // Act
        var updated = original.WithScale(newScale);

        // Assert
        Assert.Equal(original.WorldPosition, updated.WorldPosition);
        Assert.Equal(original.WorldRotation, updated.WorldRotation);
        Assert.Equal(newScale, updated.WorldScale);
        
        // Matrix should be updated too
        Assert.NotEqual(original.WorldMatrix, updated.WorldMatrix);
    }
}