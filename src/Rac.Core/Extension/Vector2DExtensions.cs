using Silk.NET.Maths;
using System;

namespace Rac.Core.Extension;

public static class Vector2DExtensions
{
	public static Vector2D<float> Normalize(this Vector2D<float> v)
	{
		var len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
		return len > 0
			? new Vector2D<float>(v.X / len, v.Y / len)
			: Vector2D<float>.Zero;
	}

	public static Vector2D<float> ClampLength(this Vector2D<float> v, float max)
	{
		var len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
		return len > max
			? new Vector2D<float>(v.X / len * max, v.Y / len * max)
			: v;
	}
}