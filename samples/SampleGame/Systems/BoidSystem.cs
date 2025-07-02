﻿// File: src/Rac.ECS.System/BoidSystem.cs

using Rac.Core.Extension;
using Rac.ECS.Component;
using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;

namespace Rac.ECS.System;

public class BoidSystem : ISystem
{
    private readonly Random _random = new();
    private IWorld _world = null!;

    /// <summary>
    /// Initializes the system with access to the ECS world.
    /// Called once when the system is registered with the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for entity and component operations.</param>
    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public void Update(float deltaTime)
    {
        // 1) Fetch global settings
        var settings = _world.GetSingleton<MultiSpeciesBoidSettingsComponent>();

        // 2) Gather all boids and obstacles
        var boidEntries = _world
            .Query<WorldTransformComponent, VelocityComponent, BoidSpeciesComponent>()
            .ToArray();
        var obstacleEntries = _world.Query<WorldTransformComponent, ObstacleComponent>().ToArray();

        // 3) Process each boid
        foreach (
            var (boidEntity, worldTransformComponent, velocityComponent, speciesComponent) in boidEntries
        )
        {
            Vector2D<float> currentPosition = worldTransformComponent.WorldPosition;
            Vector2D<float> currentVelocity = velocityComponent;
            string speciesId = speciesComponent.SpeciesId;

            // Accumulators for steering
            var separationForceAccumulator = Vector2D<float>.Zero;
            var alignmentForceAccumulator = Vector2D<float>.Zero;
            var cohesionForceAccumulator = Vector2D<float>.Zero;
            int neighborCount = 0;

            // 3a) Interactions with other boids
            foreach (
                var (
                    _,
                    otherWorldTransformComponent,
                    otherVelocityComponent,
                    otherSpeciesComponent
                ) in boidEntries
            )
            {
                Vector2D<float> otherPosition = otherWorldTransformComponent.WorldPosition;
                Vector2D<float> otherVelocity = otherVelocityComponent;
                string otherId = otherSpeciesComponent.SpeciesId;

                var offsetToOther = otherPosition - currentPosition;
                float distanceToOther = offsetToOther.Length;
                if (distanceToOther > settings.NeighborRadius || distanceToOther <= 0f)
                    continue;

                if (
                    !settings.InteractionWeights.TryGetValue(
                        (speciesId, otherId),
                        out var interaction
                    )
                )
                    continue;

                separationForceAccumulator -=
                    offsetToOther
                    / (distanceToOther * distanceToOther)
                    * interaction.SeparationWeight;
                alignmentForceAccumulator += otherVelocity * interaction.AlignmentWeight;
                cohesionForceAccumulator += otherPosition * interaction.CohesionWeight;

                neighborCount++;
            }

            // 3b) Combine same‐species forces with their own weights
            var flockingForce = Vector2D<float>.Zero;
            if (neighborCount > 0)
            {
                var separationDirection = separationForceAccumulator.Normalize();
                var alignmentDirection = (alignmentForceAccumulator / neighborCount).Normalize();
                var cohesionDirection = (
                    cohesionForceAccumulator / neighborCount - currentPosition
                ).Normalize();

                var selfInteraction = settings.InteractionWeights[(speciesId, speciesId)];

                flockingForce =
                    separationDirection * selfInteraction.SeparationWeight
                    + alignmentDirection * selfInteraction.AlignmentWeight
                    + cohesionDirection * selfInteraction.CohesionWeight;
            }

            // 3c) Avoid obstacles
            foreach (var (_, obstacleWorldTransformComponent, obstacleComponent) in obstacleEntries)
            {
                Vector2D<float> obstaclePosition = obstacleWorldTransformComponent.WorldPosition;
                float obstacleRadius = obstacleComponent.Radius;

                var offsetToObstacle = obstaclePosition - currentPosition;
                float distanceToObstacle = offsetToObstacle.Length;
                if (distanceToObstacle < obstacleRadius + 0.1f)
                    flockingForce -=
                        offsetToObstacle
                        / (distanceToObstacle * distanceToObstacle)
                        * settings.ObstacleAvoidanceWeight;
            }

            // 4) Add jitter for liveliness
            float jitterAngle = (float)(_random.NextDouble() * Math.PI * 2);
            var jitterVector =
                new Vector2D<float>(MathF.Cos(jitterAngle), MathF.Sin(jitterAngle))
                * settings.JitterStrength;

            // 5) Integrate motion
            var newVelocity = (
                currentVelocity + (flockingForce + jitterVector) * deltaTime
            ).ClampLength(settings.MaxSpeed);
            var newPosition = currentPosition + newVelocity * deltaTime;

            // 6) Wrap around world bounds
            var min = settings.BoundaryMin;
            var max = settings.BoundaryMax;
            if (newPosition.X > max.X)
                newPosition.X = min.X;
            else if (newPosition.X < min.X)
                newPosition.X = max.X;
            if (newPosition.Y > max.Y)
                newPosition.Y = min.Y;
            else if (newPosition.Y < min.Y)
                newPosition.Y = max.Y;

            // 7) Commit updated components - update local transform instead of position
            _world.SetComponent(boidEntity, (VelocityComponent)newVelocity);
            
            // Update local transform component with new position (since these are root entities, local = world)
            var currentLocalTransform = boidEntity.GetLocalTransform(_world) ?? new TransformComponent();
            var updatedLocalTransform = currentLocalTransform.WithPosition(newPosition);
            _world.SetComponent(boidEntity, updatedLocalTransform);
        }
    }

    /// <summary>
    /// Cleans up system resources before the system is removed.
    /// Called once when the system is unregistered from the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for final cleanup operations.</param>
    public void Shutdown(IWorld world)
    {
        // No resources to clean up for BoidSystem
        // All component modifications are managed by the ECS world
    }
}
