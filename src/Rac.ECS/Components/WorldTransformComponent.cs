using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Components;

/// <summary>
/// Stores the computed world-space transformation data for an entity.
/// 
/// MATHEMATICAL FOUNDATION:
/// World transforms represent the final position, rotation, and scale of an entity
/// in global coordinate space after combining all parent transformations in the hierarchy.
/// 
/// COMPUTATION FORMULA:
/// WorldMatrix = ParentWorldMatrix * LocalMatrix
/// WorldPosition = WorldMatrix * LocalPosition
/// WorldRotation = ParentWorldRotation + LocalRotation
/// WorldScale = ParentWorldScale * LocalScale
/// 
/// EDUCATIONAL NOTES:
/// This component is typically computed by TransformSystem and should not be
/// modified directly. It represents the "final" transform used by:
/// - Rendering systems for vertex transformation
/// - Physics systems for collision detection
/// - Audio systems for 3D positional audio
/// - Input systems for mouse picking and interaction
/// 
/// COORDINATE SYSTEM:
/// - World space uses global coordinate system with origin at (0,0)
/// - Positive X typically points right, positive Y points up
/// - Rotation is measured in radians, counter-clockwise from positive X axis
/// - Scale factors: 1.0 = original size, 2.0 = double size, 0.5 = half size
/// 
/// PERFORMANCE NOTES:
/// - Cached to avoid recomputing every frame unless hierarchy changes
/// - Updated only when TransformComponent or hierarchy structure changes
/// - Used as read-only data by most systems (rendering, physics, etc.)
/// </summary>
/// <param name="WorldPosition">Final position in world coordinate space</param>
/// <param name="WorldRotation">Final rotation in radians in world space</param>
/// <param name="WorldScale">Final scale factors in world space</param>
/// <param name="WorldMatrix">Complete 4x4 transformation matrix for GPU operations</param>
public readonly record struct WorldTransformComponent(
    Vector2D<float> WorldPosition,
    float WorldRotation,
    Vector2D<float> WorldScale,
    Matrix4X4<float> WorldMatrix
) : IComponent
{
    /// <summary>
    /// Creates a world transform component representing identity (no transformation).
    /// </summary>
    public WorldTransformComponent() : this(
        Vector2D<float>.Zero,
        0f,
        Vector2D<float>.One,
        Matrix4X4<float>.Identity) { }

    /// <summary>
    /// Creates a world transform from individual components, computing the matrix.
    /// </summary>
    /// <param name="position">World position</param>
    /// <param name="rotation">World rotation in radians</param>
    /// <param name="scale">World scale</param>
    public WorldTransformComponent(Vector2D<float> position, float rotation, Vector2D<float> scale) 
        : this(position, rotation, scale, ComputeMatrix(position, rotation, scale)) { }

    /// <summary>
    /// Creates a world transform with only position, using default rotation and scale.
    /// </summary>
    /// <param name="position">World position</param>
    public WorldTransformComponent(Vector2D<float> position) 
        : this(position, 0f, Vector2D<float>.One) { }

    /// <summary>
    /// Computes a 4x4 transformation matrix from position, rotation, and scale.
    /// Matrix order: Scale * Rotation * Translation (applied in that order)
    /// </summary>
    /// <param name="position">World position</param>
    /// <param name="rotation">World rotation in radians</param>
    /// <param name="scale">World scale</param>
    /// <returns>4x4 transformation matrix</returns>
    private static Matrix4X4<float> ComputeMatrix(Vector2D<float> position, float rotation, Vector2D<float> scale)
    {
        var scaleMatrix = Matrix4X4.CreateScale(scale.X, scale.Y, 1f);
        var rotationMatrix = Matrix4X4.CreateRotationZ(rotation);
        var translationMatrix = Matrix4X4.CreateTranslation(position.X, position.Y, 0f);

        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    /// <summary>
    /// Transforms a local point to world space using this transform.
    /// </summary>
    /// <param name="localPoint">Point in local coordinate space</param>
    /// <returns>Point transformed to world coordinate space</returns>
    public Vector2D<float> TransformPoint(Vector2D<float> localPoint)
    {
        // Apply scale, then rotation, then translation
        var scaled = new Vector2D<float>(localPoint.X * WorldScale.X, localPoint.Y * WorldScale.Y);
        var rotated = new Vector2D<float>(
            scaled.X * MathF.Cos(WorldRotation) - scaled.Y * MathF.Sin(WorldRotation),
            scaled.X * MathF.Sin(WorldRotation) + scaled.Y * MathF.Cos(WorldRotation)
        );
        return rotated + WorldPosition;
    }

    /// <summary>
    /// Transforms a direction vector from local to world space (ignores position).
    /// </summary>
    /// <param name="localDirection">Direction in local coordinate space</param>
    /// <returns>Direction transformed to world coordinate space</returns>
    public Vector2D<float> TransformDirection(Vector2D<float> localDirection)
    {
        // Apply scale and rotation, but not translation
        var scaled = new Vector2D<float>(localDirection.X * WorldScale.X, localDirection.Y * WorldScale.Y);
        return new Vector2D<float>(
            scaled.X * MathF.Cos(WorldRotation) - scaled.Y * MathF.Sin(WorldRotation),
            scaled.X * MathF.Sin(WorldRotation) + scaled.Y * MathF.Cos(WorldRotation)
        );
    }

    /// <summary>
    /// Transforms a point from world space to local space using this transform.
    /// </summary>
    /// <param name="worldPoint">Point in world coordinate space</param>
    /// <returns>Point transformed to local coordinate space</returns>
    public Vector2D<float> InverseTransformPoint(Vector2D<float> worldPoint)
    {
        // Reverse the transformation order: remove translation, then rotation, then scale
        var translated = worldPoint - WorldPosition;
        var rotated = new Vector2D<float>(
            translated.X * MathF.Cos(-WorldRotation) - translated.Y * MathF.Sin(-WorldRotation),
            translated.X * MathF.Sin(-WorldRotation) + translated.Y * MathF.Cos(-WorldRotation)
        );
        return new Vector2D<float>(rotated.X / WorldScale.X, rotated.Y / WorldScale.Y);
    }

    /// <summary>
    /// Gets the forward direction vector in world space (rotated positive X axis).
    /// </summary>
    public Vector2D<float> Forward => new(MathF.Cos(WorldRotation), MathF.Sin(WorldRotation));

    /// <summary>
    /// Gets the right direction vector in world space (rotated positive Y axis).
    /// </summary>
    public Vector2D<float> Right => new(-MathF.Sin(WorldRotation), MathF.Cos(WorldRotation));

    /// <summary>
    /// Creates a new world transform with updated position.
    /// </summary>
    /// <param name="newPosition">New world position</param>
    /// <returns>New WorldTransformComponent with updated position and matrix</returns>
    public WorldTransformComponent WithPosition(Vector2D<float> newPosition) =>
        new(newPosition, WorldRotation, WorldScale);

    /// <summary>
    /// Creates a new world transform with updated rotation.
    /// </summary>
    /// <param name="newRotation">New world rotation in radians</param>
    /// <returns>New WorldTransformComponent with updated rotation and matrix</returns>
    public WorldTransformComponent WithRotation(float newRotation) =>
        new(WorldPosition, newRotation, WorldScale);

    /// <summary>
    /// Creates a new world transform with updated scale.
    /// </summary>
    /// <param name="newScale">New world scale</param>
    /// <returns>New WorldTransformComponent with updated scale and matrix</returns>
    public WorldTransformComponent WithScale(Vector2D<float> newScale) =>
        new(WorldPosition, WorldRotation, newScale);
}