using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.ECS.Component;
/// <summary>
/// The position component of an entity.
/// </summary>
/// <param name="X"></param>
/// <param name="Y"></param>
public readonly record struct PositionComponent(float X, float Y) : IComponent
{
	/// <summary>Implicitly convert to Silk.NET vector for math operations.</summary>
	public static implicit operator Vector2D<float>(PositionComponent position)
			=> new(position.X, position.Y);
	/// <summary>Implicitly convert from Silk.NET vector for math operations.</summary>
		public static implicit operator PositionComponent(Vector2D<float> vector) 
			=> new(vector.X, vector.Y);
		
}
