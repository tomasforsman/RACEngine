using Rac.ECS.Core;

namespace Rac.ECS.Component
{
	/// <summary>Marks an entity as a circular obstacle with given radius.</summary>
	public readonly record struct ObstacleComponent(
		/// <summary>Radius of the obstacle circle in world units.</summary>
		float Radius
	) : IComponent;
}