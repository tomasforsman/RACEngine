using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Components;

/// <summary>
/// Stores local transformation data relative to the entity's parent in the hierarchy.
/// 
/// COORDINATE SYSTEM AND MATHEMATICAL FOUNDATION:
/// - Position: Local offset from parent's origin (Vector2D for 2D games)
/// - Rotation: Local rotation in radians, applied around local origin
/// - Scale: Local scaling factors applied before rotation and translation
/// - Transform Order: Scale → Rotate → Translate (standard graphics pipeline)
/// 
/// EDUCATIONAL NOTES:
/// Transform matrices follow the standard graphics transformation pipeline:
/// FinalMatrix = Translation * Rotation * Scale
/// 
/// For hierarchical transforms:
/// WorldMatrix = ParentWorldMatrix * LocalMatrix
/// 
/// This component stores only LOCAL values - world-space values are computed
/// by the TransformSystem using the parent hierarchy chain.
/// 
/// USAGE PATTERNS:
/// - Root entities (no parent): local transform = world transform
/// - Child entities: local transform is relative to immediate parent
/// - Nested hierarchies: computed recursively through parent chain
/// </summary>
/// <param name="LocalPosition">Position relative to parent coordinate system (Vector2D for 2D)</param>
/// <param name="LocalRotation">Rotation in radians relative to parent orientation</param>
/// <param name="LocalScale">Scale factors relative to parent scale (1.0 = no scaling)</param>
public readonly record struct TransformComponent(
    Vector2D<float> LocalPosition,
    float LocalRotation,
    Vector2D<float> LocalScale
) : IComponent
{
    /// <summary>
    /// Creates a transform component with default values (identity transform).
    /// </summary>
    public TransformComponent() : this(Vector2D<float>.Zero, 0f, Vector2D<float>.One) { }

    /// <summary>
    /// Creates a transform component with only position, using default rotation and scale.
    /// </summary>
    /// <param name="position">Local position relative to parent</param>
    public TransformComponent(Vector2D<float> position) : this(position, 0f, Vector2D<float>.One) { }

    /// <summary>
    /// Creates a transform component with position and rotation, using default scale.
    /// </summary>
    /// <param name="position">Local position relative to parent</param>
    /// <param name="rotation">Local rotation in radians</param>
    public TransformComponent(Vector2D<float> position, float rotation) : this(position, rotation, Vector2D<float>.One) { }

    /// <summary>
    /// Computes the local transformation matrix for this transform.
    /// Matrix represents: Scale * Rotation * Translation (applied in that order)
    /// </summary>
    /// <returns>4x4 transformation matrix representing this local transform</returns>
    public Matrix4X4<float> GetLocalMatrix()
    {
        // Create individual transformation matrices
        var scaleMatrix = Matrix4X4.CreateScale(LocalScale.X, LocalScale.Y, 1f);
        var rotationMatrix = Matrix4X4.CreateRotationZ(LocalRotation);
        var translationMatrix = Matrix4X4.CreateTranslation(LocalPosition.X, LocalPosition.Y, 0f);

        // Combine in standard order: S * R * T (Scale, then rotate, then translate)
        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    /// <summary>
    /// Creates a transform with modified position while preserving rotation and scale.
    /// </summary>
    /// <param name="newPosition">New local position</param>
    /// <returns>New TransformComponent with updated position</returns>
    public TransformComponent WithPosition(Vector2D<float> newPosition) =>
        this with { LocalPosition = newPosition };

    /// <summary>
    /// Creates a transform with modified rotation while preserving position and scale.
    /// </summary>
    /// <param name="newRotation">New local rotation in radians</param>
    /// <returns>New TransformComponent with updated rotation</returns>
    public TransformComponent WithRotation(float newRotation) =>
        this with { LocalRotation = newRotation };

    /// <summary>
    /// Creates a transform with modified scale while preserving position and rotation.
    /// </summary>
    /// <param name="newScale">New local scale</param>
    /// <returns>New TransformComponent with updated scale</returns>
    public TransformComponent WithScale(Vector2D<float> newScale) =>
        this with { LocalScale = newScale };

    /// <summary>
    /// Creates a transform with uniform scale while preserving position and rotation.
    /// </summary>
    /// <param name="uniformScale">Uniform scale factor applied to both X and Y</param>
    /// <returns>New TransformComponent with updated uniform scale</returns>
    public TransformComponent WithUniformScale(float uniformScale) =>
        this with { LocalScale = new Vector2D<float>(uniformScale, uniformScale) };
}