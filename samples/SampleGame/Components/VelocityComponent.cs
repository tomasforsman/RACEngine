using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Component;

public readonly record struct VelocityComponent(float VelocityX, float VelocityY) : IComponent
{
	/// <summary>Implicitly convert to Silk.NET vector for math operations.</summary>
	public static implicit operator Vector2D<float>(VelocityComponent velocity)
	{
		return new Vector2D<float>(velocity.VelocityX, velocity.VelocityY);
	}

	/// <summary>Implicitly convert from Silk.NET vector for math operations.</summary>
	public static implicit operator VelocityComponent(Vector2D<float> vector)
	{
		return new VelocityComponent(vector.X, vector.Y);
	}
}