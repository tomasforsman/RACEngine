// File: src/SampleGame/BoidSample.cs
using System;
using System.Collections.Generic;
using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.ECS.Component;
using Rac.ECS.Core;
using Rac.ECS.System;
using Silk.NET.Maths;
using Rac.Engine;
using Rac.GameEngine;
using Rac.Input.Service;

namespace SampleGame
{
    /// <summary>
    /// Demo: fully dynamic, data‐driven multi‐species Boids with runtime obstacles.
    /// </summary>
    public static class BoidSample
    {
        public static void Run(string[] args)
        {
            // ─── Engine Setup ────────────────────────────────────────
            var windowManager       = new WindowManager();
            var inputService        = new SilkInputService();
            var configurationManager = new ConfigManager();
            var gameEngine          = new EngineFacade(windowManager, inputService, configurationManager);

            // ─── ECS Setup ───────────────────────────────────────────
            var world = gameEngine.World;
            gameEngine.AddSystem(new BoidSystem(world));
            

            // Data‐driven species identifiers and scales/colors
            var speciesIdentifiers = new List<string> { "White", "Blue", "Red" };
            var speciesScales = new Dictionary<string, float>
            {
                ["White"] = 0.5f,
                ["Blue"]  = 0.8f,
                ["Red"]   = 1.2f
            };
            var speciesColors = new Dictionary<string, Vector4D<float>>
            {
                ["White"] = new Vector4D<float>(1f, 1f, 1f, 1f),
                ["Blue"]  = new Vector4D<float>(0f, 0f, 1f, 1f),
                ["Red"]   = new Vector4D<float>(1f, 0f, 0f, 1f)
            };

            // Update settings when window loads or resizes
            gameEngine.LoadEvent      += () => UpdateBoidSettings(windowManager.Size);
            windowManager.OnResize      += newSize => UpdateBoidSettings(newSize);

            gameEngine.UpdateEvent    += deltaTime =>
            {
            };

            gameEngine.RenderEvent    += deltaTime =>
            {
                // Draw all species
                foreach (var speciesId in speciesIdentifiers)
                {
                    DrawSpecies(speciesId);
                }

                // Draw obstacles
                DrawObstacles(new Vector4D<float>(0.5f, 0.5f, 0.5f, 1f));
            };


            // Start the engine (fires OnLoadEvent → initial bounds/settings)
            gameEngine.Run();

            // ─── Local Helpers ──────────────────────────────────────

            void UpdateBoidSettings(Vector2D<int> windowSize)
            {
                const float safeZoneRatio = 0.9f;
                float       aspectRatio   = windowSize.Y / (float)windowSize.X;

                var boundaryMin = new Vector2D<float>(
                    -safeZoneRatio,
                    -safeZoneRatio * aspectRatio
                );
                var boundaryMax = new Vector2D<float>(
                     safeZoneRatio,
                     safeZoneRatio * aspectRatio
                );

                var interactionMap = new Dictionary<(string Self, string Other), SpeciesInteraction>();
                foreach (var selfId in speciesIdentifiers)
                {
                    foreach (var otherId in speciesIdentifiers)
                    {
                        if (selfId == otherId)
                        {
                            interactionMap[(selfId, otherId)] = new SpeciesInteraction(
                                SeparationWeight: 1.0f,
                                AlignmentWeight:  1.0f,
                                CohesionWeight:   1.0f
                            );
                        }
                        else if (selfId == "White" && (otherId == "Blue" || otherId == "Red"))
                        {
                            interactionMap[(selfId, otherId)] = new SpeciesInteraction(1.5f, 0f, 0f);
                        }
                        else if (selfId == "Blue" && otherId == "White")
                        {
                            interactionMap[(selfId, otherId)] = new SpeciesInteraction(0f, 0f, 1.2f);
                        }
                        else if (selfId == "Red" && (otherId == "White" || otherId == "Blue"))
                        {
                            interactionMap[(selfId, otherId)] = new SpeciesInteraction(0f, 0f, 1.2f);
                        }
                        else
                        {
                            interactionMap[(selfId, otherId)] = new SpeciesInteraction(0f, 0f, 0f);
                        }
                    }
                }

                var boidSettings = new MultiSpeciesBoidSettingsComponent(
                    NeighborRadius:          0.4f,
                    JitterStrength:          0.02f,
                    MaxSpeed:                0.2f,
                    BoundaryMin:             boundaryMin,
                    BoundaryMax:             boundaryMax,
                    InteractionWeights:      interactionMap,
                    ObstacleAvoidanceWeight: 1.5f
                );

                world.SetComponent(
                    world.CreateEntity(),
                    boidSettings
                );
            }

            void SpawnAllSpecies()
            {
                var random = new Random();
                var spawnCounts = new Dictionary<string, int>
                {
                    ["White"] = 30,
                    ["Blue"]  = 20,
                    ["Red"]   = 10
                };

                foreach (var speciesId in speciesIdentifiers)
                {
                    int    count = spawnCounts[speciesId];
                    float  scale = speciesScales[speciesId];

                    for (int i = 0; i < count; i++)
                    {
                        var entity = world.CreateEntity();
                        world.SetComponent(entity, new PositionComponent(
                            X: (float)(random.NextDouble() * 2.0 - 1.0),
                            Y: (float)(random.NextDouble() * 2.0 - 1.0)
                        ));
                        world.SetComponent(entity, new VelocityComponent(
                            VelocityX: 0f,
                            VelocityY: 0f
                        ));
                        world.SetComponent(entity, new BoidSpeciesComponent(
                            SpeciesId: speciesId,
                            Scale:     scale
                        ));
                    }
                }
            }

            void SpawnObstacles()
            {
                var entity = world.CreateEntity();
                world.SetComponent(entity, new PositionComponent(0f, 0f));
                world.SetComponent(entity, new ObstacleComponent(Radius: 0.2f));
            }

            // Note: DrawSpecies now takes exactly one argument
            void DrawSpecies(string speciesFilter)
            {
                var modelTriangle = new[]
                {
                    new Vector2D<float>(-0.008f, -0.008f),
                    new Vector2D<float>( 0.008f, -0.008f),
                    new Vector2D<float>( 0.000f,  0.03f)
                };

                var vertexBuffer = new List<float>();

                foreach (var (_, positionComponent, velocityComponent, speciesComponent)
                         in world.Query<PositionComponent, VelocityComponent, BoidSpeciesComponent>())
                {
                    if (speciesComponent.SpeciesId != speciesFilter)
                        continue;

                    // Convert and normalize
                    var worldPosition      = (Vector2D<float>)positionComponent;
                    var rawVelocity        = (Vector2D<float>)velocityComponent;
                    var normalizedVelocity = rawVelocity.Normalize();

                    // Compute heading
                    float headingAngle = MathF.Atan2(normalizedVelocity.Y, normalizedVelocity.X)
                                         - MathF.PI / 2f;
                    float cosAngle     = MathF.Cos(headingAngle);
                    float sinAngle     = MathF.Sin(headingAngle);
                    float scaleFactor  = speciesComponent.Scale;

                    foreach (var offset in modelTriangle)
                    {
                        var scaledOffset  = offset * scaleFactor;
                        var rotatedOffset = new Vector2D<float>(
                            scaledOffset.X * cosAngle - scaledOffset.Y * sinAngle,
                            scaledOffset.X * sinAngle + scaledOffset.Y * cosAngle
                        );
                        var finalPosition = worldPosition + rotatedOffset;
                        vertexBuffer.Add(finalPosition.X);
                        vertexBuffer.Add(finalPosition.Y);
                    }
                }

                if (vertexBuffer.Count == 0)
                    return;

                var color = speciesColors[speciesFilter];
                gameEngine.Renderer.SetColor(color);
                gameEngine.Renderer.UpdateVertices(vertexBuffer.ToArray());
                gameEngine.Renderer.Draw();
            }

            // Note: DrawObstacles takes exactly one argument
            void DrawObstacles(Vector4D<float> drawColor)
            {
                const int segmentCount = 16;
                var vertexBuffer = new List<float>();

                foreach (var (_, positionComponent, obstacleComponent)
                         in world.Query<PositionComponent, ObstacleComponent>())
                {
                    var center = (Vector2D<float>)positionComponent;
                    float radius = obstacleComponent.Radius;

                    for (int i = 0; i < segmentCount; i++)
                    {
                        float angle0 = 2 * MathF.PI * i / segmentCount;
                        float angle1 = 2 * MathF.PI * (i + 1) / segmentCount;

                        var point0 = center + new Vector2D<float>(
                            radius * MathF.Cos(angle0),
                            radius * MathF.Sin(angle0)
                        );
                        var point1 = center + new Vector2D<float>(
                            radius * MathF.Cos(angle1),
                            radius * MathF.Sin(angle1)
                        );

                        vertexBuffer.Add(center.X);
                        vertexBuffer.Add(center.Y);
                        vertexBuffer.Add(point0.X);
                        vertexBuffer.Add(point0.Y);
                        vertexBuffer.Add(point1.X);
                        vertexBuffer.Add(point1.Y);
                    }
                }

                if (vertexBuffer.Count == 0)
                    return;

                gameEngine.Renderer.SetColor(drawColor);
                gameEngine.Renderer.UpdateVertices(vertexBuffer.ToArray());
                gameEngine.Renderer.Draw();
            }
        }
    }


}
