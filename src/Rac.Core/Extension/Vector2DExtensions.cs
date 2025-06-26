using Silk.NET.Maths;

namespace Rac.Core.Extension;

/// <summary>
/// Provides extension methods for Vector2D&lt;float&gt; to add common vector mathematics operations.
/// Includes normalization and length clamping operations frequently used in game development.
/// </summary>
/// <remarks>
/// These extension methods implement common vector operations that are not included in the
/// base Silk.NET mathematics library. The operations follow standard vector mathematics
/// principles and handle edge cases like zero-length vectors appropriately.
/// 
/// Mathematical Background:
/// - Vector normalization creates a unit vector (length = 1) pointing in the same direction
/// - Length clamping constrains vector magnitude while preserving direction
/// - Both operations are essential for physics simulations, movement systems, and AI
/// </remarks>
/// <example>
/// <code>
/// var velocity = new Vector2D&lt;float&gt;(3.0f, 4.0f);
/// 
/// // Normalize to unit vector (length = 1)
/// var direction = velocity.Normalize(); // Result: (0.6, 0.8)
/// 
/// // Clamp to maximum speed
/// var clampedVelocity = velocity.ClampLength(2.0f); // Result: (1.2, 1.6)
/// </code>
/// </example>
public static class Vector2DExtensions
{
    /// <summary>
    /// Normalizes the vector to create a unit vector with length 1 pointing in the same direction.
    /// </summary>
    /// <param name="v">The vector to normalize.</param>
    /// <returns>
    /// A normalized vector with length 1, or Vector2D.Zero if the input vector has zero length.
    /// </returns>
    /// <remarks>
    /// Vector normalization is fundamental in game development for:
    /// - Direction vectors for movement and physics
    /// - Surface normals for lighting calculations
    /// - AI steering behaviors and pathfinding
    /// 
    /// Mathematical formula: normalized = v / |v| where |v| is the vector magnitude.
    /// Special case: Zero-length vectors return Vector2D.Zero to avoid division by zero.
    /// </remarks>
    /// <example>
    /// <code>
    /// var movement = new Vector2D&lt;float&gt;(6.0f, 8.0f);  // Length = 10
    /// var direction = movement.Normalize();              // Result: (0.6, 0.8)
    /// 
    /// // Use normalized vector for consistent movement speed
    /// var speed = 5.0f;
    /// var velocity = direction * speed;                  // Result: (3.0, 4.0)
    /// </code>
    /// </example>
    public static Vector2D<float> Normalize(this Vector2D<float> v)
    {
        float len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        return len > 0 ? new Vector2D<float>(v.X / len, v.Y / len) : Vector2D<float>.Zero;
    }

    /// <summary>
    /// Clamps the vector length to a maximum value while preserving its direction.
    /// </summary>
    /// <param name="v">The vector to clamp.</param>
    /// <param name="max">
    /// The maximum allowed length. Must be non-negative.
    /// If the vector's current length is less than or equal to this value, the vector is returned unchanged.
    /// </param>
    /// <returns>
    /// A vector pointing in the same direction as the input but with length clamped to the maximum value.
    /// If the input vector has zero length, Vector2D.Zero is returned.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="max"/> is negative.
    /// </exception>
    /// <remarks>
    /// Length clamping is commonly used for:
    /// - Limiting maximum movement speed in games
    /// - Constraining force magnitudes in physics systems
    /// - Implementing speed limits for AI characters
    /// 
    /// The operation preserves the vector's direction while ensuring the magnitude
    /// does not exceed the specified maximum.
    /// </remarks>
    /// <example>
    /// <code>
    /// var velocity = new Vector2D&lt;float&gt;(9.0f, 12.0f);  // Length = 15
    /// var limited = velocity.ClampLength(10.0f);         // Result: (6.0, 8.0), Length = 10
    /// 
    /// var slowVelocity = new Vector2D&lt;float&gt;(2.0f, 1.0f); // Length ≈ 2.24
    /// var unchanged = slowVelocity.ClampLength(5.0f);     // Result: (2.0, 1.0), unchanged
    /// </code>
    /// </example>
    public static Vector2D<float> ClampLength(this Vector2D<float> v, float max)
    {
        if (max < 0)
            throw new ArgumentException("Maximum length cannot be negative", nameof(max));
            
        float len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        return len > max ? new Vector2D<float>(v.X / len * max, v.Y / len * max) : v;
    }
}
