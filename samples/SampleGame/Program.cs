using Engine.Core;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;

namespace SampleGame
{
	class Program
	{
		static Engine.Core.Engine _engine = new();
		static Vector2D<int> _windowSize = new(800, 600);
		static List<Vector2D<float>> _triangleCenters = new();

		static void Main(string[] args)
		{
			// Subscribe to mouse click event exposed by the engine
			_engine.OnLeftClick += OnLeftClick;

			_engine.Run();
		}

		private static void OnLeftClick(Vector2D<float> mousePos)
		{
			// Convert mouse position (pixels) to NDC (-1 to 1)
			float ndcX = mousePos.X / _windowSize.X * 2f - 1f;
			float ndcY = - (mousePos.Y / _windowSize.Y * 2f - 1f);

			_triangleCenters.Add(new Vector2D<float>(ndcX, ndcY));

			// Build vertex data for all triangles
			var vertices = new List<float>();
			foreach (var c in _triangleCenters)
			{
				vertices.AddRange(new float[]
				{
					c.X - 0.05f, c.Y - 0.05f,
					c.X + 0.05f, c.Y - 0.05f,
					c.X,        c.Y + 0.05f
				});
			}

			// Update the engine's renderer vertex buffer
			_engine.UpdateVertices(vertices.ToArray());
		}
	}
}