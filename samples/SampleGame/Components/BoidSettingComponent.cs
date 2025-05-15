// File: src/Rac.ECS.Component/BoidSettingsComponent.cs


using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.ECS.Component
{
	/// <summary>
	/// Global parameters for the Boids algorithm.
	/// </summary>
	public readonly record struct BoidSettingsComponent(
		/// <summary>
		/// Radius within which other boids are considered neighbors.
		/// </summary>
		float NeighborRadius,

		/// <summary>
		/// Weight multiplier for the separation steering force.
		/// </summary>
		float SeparationWeight,

		/// <summary>
		/// Weight multiplier for the alignment steering force.
		/// </summary>
		float AlignmentWeight,

		/// <summary>
		/// Weight multiplier for the cohesion steering force.
		/// </summary>
		float CohesionWeight,

		/// <summary>
		/// Maximum speed any boid can reach.
		/// </summary>
		float MaxSpeed,

		/// <summary>
		/// Strength of the random jitter applied each frame to avoid perfect stasis.
		/// </summary>
		float JitterStrength,

		/// <summary>
		/// Minimum X/Y coordinate value for world wrapping (inclusive).
		/// </summary>
		Vector2D<float> BoundaryMin,

		/// <summary>
		/// Maximum X/Y coordinate value for world wrapping (inclusive).
		/// </summary>
		Vector2D<float> BoundaryMax
		
		
	) : IComponent;
}