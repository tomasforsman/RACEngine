// File: samples/SampleGame/BoidSample.cs

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.ECS.Component;
using Rac.ECS.System;
using Rac.Engine;
using Rac.Input.Service;
using Silk.NET.Maths;

namespace SampleGame;

public static class BoidSample
{
	public static void Run(string[] args)
	{
		// ─── Engine Setup ────────────────────────────────────────
		var windowManager = new WindowManager();
		var inputService = new SilkInputService();
		var configurationManager = new ConfigManager();
		var engine = new EngineFacade(windowManager, inputService, configurationManager);

		// ─── ECS Setup ───────────────────────────────────────────
		var world = engine.World;
		engine.AddSystem(new BoidSystem(world));
		var settingsEntity = world.CreateEntity();

		// ─── Species Data ────────────────────────────────────────
		string[] speciesIds = new[] { "White", "Blue", "Red" };
		var speciesScales = new Dictionary<string, float>
		{
			["White"] = 0.5f,
			["Blue"] = 0.8f,
			["Red"] = 1.2f
		};
		var speciesColors = new Dictionary<string, Vector4D<float>>
		{
			["White"] = new(1f, 1f, 1f, 1f),
			["Blue"] = new(0f, 0f, 1f, 1f),
			["Red"] = new(1f, 0f, 0f, 1f)
		};

		// ─── Settings on load/resize ────────────────────────────
		engine.LoadEvent += () => UpdateBoidSettings(windowManager.Size);
		windowManager.OnResize += newSize => UpdateBoidSettings(newSize);

		// ─── Initial Spawn ──────────────────────────────────────
		SpawnAllSpecies();
		SpawnObstacles();

		// ─── Per-frame hooks ─────────────────────────────────────
		engine.UpdateEvent += deltaSeconds =>
		{
			// ECS systems already run in the façade before this.
			// Any extra per-frame logic would go here.
		};

		engine.RenderEvent += deltaSeconds =>
		{
			foreach (string id in speciesIds)
				DrawSpecies(id);
			DrawObstacles(new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f));
		};

		// ─── Start! ──────────────────────────────────────────────
		engine.Run();

		// ─── Local helpers ──────────────────────────────────────

		void UpdateBoidSettings(Vector2D<int> windowSize)
		{
			const float safeZoneRatio = 0.9f;
			float aspectRatio = windowSize.Y / (float)windowSize.X;

			var boundaryMin = new Vector2D<float>(-safeZoneRatio, -safeZoneRatio * aspectRatio);
			var boundaryMax = new Vector2D<float>(safeZoneRatio, safeZoneRatio * aspectRatio);

			var interactionMap = new Dictionary<(string Self, string Other), SpeciesInteraction>();
			foreach (string selfId in speciesIds)
			foreach (string otherId in speciesIds)
				interactionMap[(selfId, otherId)] =
					selfId == otherId
						? new SpeciesInteraction(1f, 1f, 1f)
						: (selfId, otherId) switch
						{
							("White", "Blue") or ("White", "Red") => new SpeciesInteraction(
								1.5f,
								0f,
								0f
							),
							("Blue", "White") => new SpeciesInteraction(0f, 0f, 1.2f),
							("Red", "White") or ("Red", "Blue") => new SpeciesInteraction(
								0f,
								0f,
								1.2f
							),
							_ => new SpeciesInteraction(0f, 0f, 0f)
						};

			var boidSettings = new MultiSpeciesBoidSettingsComponent(
				0.4f,
				0.02f,
				0.2f,
				boundaryMin,
				boundaryMax,
				interactionMap,
				1.5f
			);

			world.SetComponent(settingsEntity, boidSettings);
		}

		void SpawnAllSpecies()
		{
			var random = new Random();
			var spawnCounts = new Dictionary<string, int>
			{
				["White"] = 30,
				["Blue"] = 20,
				["Red"] = 10
			};

			foreach (string id in speciesIds)
			{
				float scale = speciesScales[id];
				int count = spawnCounts[id];

				for (int i = 0; i < count; i++)
				{
					var e = world.CreateEntity();
					world.SetComponent(
						e,
						new PositionComponent(
							(float)(random.NextDouble() * 2.0 - 1.0),
							(float)(random.NextDouble() * 2.0 - 1.0)
						)
					);
					world.SetComponent(e, new VelocityComponent(0f, 0f));
					world.SetComponent(e, new BoidSpeciesComponent(id, scale));
				}
			}
		}

		void SpawnObstacles()
		{
			var e = world.CreateEntity();
			world.SetComponent(e, new PositionComponent(0f, 0f));
			world.SetComponent(e, new ObstacleComponent(0.2f));
		}

		void DrawSpecies(string filterId)
		{
			var triangle = new[]
			{
				new Vector2D<float>(-0.008f, -0.008f),
				new Vector2D<float>(0.008f, -0.008f),
				new Vector2D<float>(0.000f, 0.03f)
			};
			var verts = new List<float>();

			foreach (
				var (_, pos, vel, spec) in world.Query<
					PositionComponent,
					VelocityComponent,
					BoidSpeciesComponent
				>()
			)
			{
				if (spec.SpeciesId != filterId)
					continue;

				var wp = (Vector2D<float>)pos;
				var normV = ((Vector2D<float>)vel).Normalize();
				float angle = MathF.Atan2(normV.Y, normV.X) - MathF.PI / 2f;
				float cosA = MathF.Cos(angle);
				float sinA = MathF.Sin(angle);
				float scale = spec.Scale;

				foreach (var off in triangle)
				{
					var s = off * scale;
					var r = new Vector2D<float>(s.X * cosA - s.Y * sinA, s.X * sinA + s.Y * cosA);
					var p = wp + r;
					verts.Add(p.X);
					verts.Add(p.Y);
				}
			}

			if (verts.Count == 0)
				return;

			engine.Renderer.SetColor(speciesColors[filterId]);
			engine.Renderer.UpdateVertices(verts.ToArray());
			engine.Renderer.Draw();
		}

		void DrawObstacles(Vector4D<float> color)
		{
			const int segments = 16;
			var verts = new List<float>();

			foreach (var (_, pos, obs) in world.Query<PositionComponent, ObstacleComponent>())
			{
				var center = (Vector2D<float>)pos;
				float r = obs.Radius;

				for (int i = 0; i < segments; i++)
				{
					float a0 = 2 * MathF.PI * i / segments;
					float a1 = 2 * MathF.PI * (i + 1) / segments;

					var p0 = center + new Vector2D<float>(r * MathF.Cos(a0), r * MathF.Sin(a0));
					var p1 = center + new Vector2D<float>(r * MathF.Cos(a1), r * MathF.Sin(a1));

					verts.Add(center.X);
					verts.Add(center.Y);
					verts.Add(p0.X);
					verts.Add(p0.Y);
					verts.Add(p1.X);
					verts.Add(p1.Y);
				}
			}

			if (verts.Count == 0)
				return;

			engine.Renderer.SetColor(color);
			engine.Renderer.UpdateVertices(verts.ToArray());
			engine.Renderer.Draw();
		}
	}
}