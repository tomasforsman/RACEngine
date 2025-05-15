// File: src/Rac.ECS.System/BoidSystem.cs
using System;
using System.Linq;
using Silk.NET.Maths;
using Rac.Core.Extension;
using Rac.ECS.Core;
using Rac.ECS.Component;
using Rac.ECS.Systems;

namespace Rac.ECS.System
{
    public class BoidSystem : ISystem
    {
        private readonly World _world;
        private readonly Random _random = new();

        public BoidSystem(World world) =>
            _world = world ?? throw new ArgumentNullException(nameof(world));

        public void Update(float deltaTime)
        {
            // 1) Fetch global settings
            var settings = _world.GetSingleton<MultiSpeciesBoidSettingsComponent>();

            // 2) Gather all boids and obstacles
            var boidEntries = _world
                .Query<PositionComponent, VelocityComponent, BoidSpeciesComponent>()
                .ToArray();
            var obstacleEntries = _world
                .Query<PositionComponent, ObstacleComponent>()
                .ToArray();

            // 3) Process each boid
            foreach (var (boidEntity, positionComponent, velocityComponent, speciesComponent)
                     in boidEntries)
            {
                Vector2D<float> currentPosition = positionComponent;
                Vector2D<float> currentVelocity = velocityComponent;
                string speciesId                = speciesComponent.SpeciesId;

                // Accumulators for steering
                Vector2D<float> separationForceAccumulator = Vector2D<float>.Zero;
                Vector2D<float> alignmentForceAccumulator  = Vector2D<float>.Zero;
                Vector2D<float> cohesionForceAccumulator   = Vector2D<float>.Zero;
                int neighborCount = 0;

                // 3a) Interactions with other boids
                foreach (var (_, otherPositionComponent, otherVelocityComponent, otherSpeciesComponent) in boidEntries)
                {
                    Vector2D<float> otherPosition = otherPositionComponent;
                    Vector2D<float> otherVelocity = otherVelocityComponent;
                    string otherId                = otherSpeciesComponent.SpeciesId;

                    Vector2D<float> offsetToOther = otherPosition - currentPosition;
                    float           distanceToOther = offsetToOther.Length;
                    if (distanceToOther > settings.NeighborRadius || distanceToOther <= 0f)
                        continue;

                    if (!settings.InteractionWeights.TryGetValue((speciesId, otherId), out var interaction))
                        continue;

                    separationForceAccumulator -= (offsetToOther / (distanceToOther * distanceToOther))
                                                 * interaction.SeparationWeight;
                    alignmentForceAccumulator  += otherVelocity * interaction.AlignmentWeight;
                    cohesionForceAccumulator   += otherPosition * interaction.CohesionWeight;

                    neighborCount++;
                }

                // 3b) Combine same‐species forces with their own weights
                Vector2D<float> flockingForce = Vector2D<float>.Zero;
                if (neighborCount > 0)
                {
                    var separationDirection = separationForceAccumulator.Normalize();
                    var alignmentDirection  = (alignmentForceAccumulator / neighborCount).Normalize();
                    var cohesionDirection   = ((cohesionForceAccumulator / neighborCount) - currentPosition).Normalize();

                    var selfInteraction = settings.InteractionWeights[(speciesId, speciesId)];

                    flockingForce =
                        separationDirection * selfInteraction.SeparationWeight +
                        alignmentDirection  * selfInteraction.AlignmentWeight  +
                        cohesionDirection   * selfInteraction.CohesionWeight;
                }

                // 3c) Avoid obstacles
                foreach (var (_, obstaclePositionComponent, obstacleComponent) in obstacleEntries)
                {
                    Vector2D<float> obstaclePosition = obstaclePositionComponent;
                    float           obstacleRadius   = obstacleComponent.Radius;

                    Vector2D<float> offsetToObstacle = obstaclePosition - currentPosition;
                    float           distanceToObstacle = offsetToObstacle.Length;
                    if (distanceToObstacle < obstacleRadius + 0.1f)
                    {
                        flockingForce -= (offsetToObstacle / (distanceToObstacle * distanceToObstacle))
                                         * settings.ObstacleAvoidanceWeight;
                    }
                }

                // 4) Add jitter for liveliness
                float jitterAngle = (float)(_random.NextDouble() * Math.PI * 2);
                var   jitterVector = new Vector2D<float>(
                    MathF.Cos(jitterAngle),
                    MathF.Sin(jitterAngle)
                ) * settings.JitterStrength;

                // 5) Integrate motion
                Vector2D<float> newVelocity = (currentVelocity + (flockingForce + jitterVector) * deltaTime)
                    .ClampLength(settings.MaxSpeed);
                Vector2D<float> newPosition = currentPosition + newVelocity * deltaTime;

                // 6) Wrap around world bounds
                var min = settings.BoundaryMin;
                var max = settings.BoundaryMax;
                if (newPosition.X > max.X) newPosition.X = min.X;
                else if (newPosition.X < min.X) newPosition.X = max.X;
                if (newPosition.Y > max.Y) newPosition.Y = min.Y;
                else if (newPosition.Y < min.Y) newPosition.Y = max.Y;

                // 7) Commit updated components
                _world.SetComponent(boidEntity, (VelocityComponent)newVelocity);
                _world.SetComponent(boidEntity, (PositionComponent)newPosition);
            }
        }
    }
}
