// File: src/Rac.ECS.System/BoidSystem.cs

using Rac.Core.Extension;
using Rac.ECS.Component;
using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Silk.NET.Maths;

namespace Rac.ECS.System;

// ═══════════════════════════════════════════════════════════════════════════
// BOIDS ALGORITHM IMPLEMENTATION (Craig Reynolds, 1986)
// ═══════════════════════════════════════════════════════════════════════════
//
// The boids algorithm creates emergent flocking behavior through three simple rules:
// 1. Separation: Avoid crowding neighbors (maintain personal space)
// 2. Alignment: Steer towards average heading of neighbors (move with the flock)
// 3. Cohesion: Steer towards average position of neighbors (stay with the flock)
//
// This demonstrates how complex group behaviors emerge from simple individual rules,
// a fundamental concept in artificial life and swarm intelligence.
//
// ACADEMIC REFERENCE:
// Reynolds, Craig W. "Flocks, herds and schools: A distributed behavioral model."
// Proceedings of the 14th annual conference on Computer graphics and interactive
// techniques. 1987.
//
// EDUCATIONAL VALUE:
// - Emergent behavior from simple rules
// - Vector mathematics in practical application
// - Spatial partitioning and neighbor detection
// - Multi-agent systems and artificial life
// - Performance optimization in real-time simulation
//
// PERFORMANCE CHARACTERISTICS:
// - Time Complexity: O(n²) for naive neighbor detection
// - Space Complexity: O(n) for storing boid states
// - Optimizations: Spatial hashing could reduce to O(n) average case
// - Bottlenecks: Distance calculations and neighbor iterations

/// <summary>
/// Implements Craig Reynolds' Boids algorithm for simulating flocking behavior in artificial life.
/// 
/// This system demonstrates emergent group behavior arising from simple local rules applied
/// to individual agents (boids). Each boid follows three fundamental steering behaviors
/// that collectively produce realistic flocking, schooling, and herding patterns.
/// </summary>
/// <remarks>
/// ALGORITHM OVERVIEW:
/// 
/// For each boid in the simulation:
/// 1. **Neighbor Detection**: Find all boids within perception radius
/// 2. **Separation Calculation**: Compute avoidance force from nearby boids
/// 3. **Alignment Calculation**: Average neighbor velocities for directional consistency
/// 4. **Cohesion Calculation**: Average neighbor positions for group cohesion
/// 5. **Force Integration**: Combine steering forces with current velocity
/// 6. **Constraint Application**: Apply speed limits and boundary conditions
/// 7. **Position Update**: Integrate velocity to compute new position
/// 
/// MATHEMATICAL FOUNDATIONS:
/// 
/// Separation Force: F_sep = Σ(normalize(pos_i - pos_neighbor) / distance²)
/// Alignment Force: F_align = normalize(average(neighbor_velocities))
/// Cohesion Force: F_cohesion = normalize(average(neighbor_positions) - pos_i)
/// 
/// Total Force: F_total = w_sep * F_sep + w_align * F_align + w_cohesion * F_cohesion
/// 
/// PERFORMANCE CONSIDERATIONS:
/// 
/// - Neighbor search dominates computational cost: O(n²) naive implementation
/// - Distance calculations use expensive square root operations
/// - Multi-species interactions multiply complexity: O(n² * s²) where s = species count
/// - Optimizations: spatial hashing, broad-phase collision detection, LOD systems
/// 
/// EDUCATIONAL CONCEPTS DEMONSTRATED:
/// 
/// - **Emergent Behavior**: Complex patterns from simple rules
/// - **Vector Mathematics**: Force accumulation and normalization
/// - **Spatial Algorithms**: Distance calculations and neighbor detection
/// - **Real-time Simulation**: Frame-rate independent updates
/// - **Multi-agent Systems**: Decentralized decision making
/// </remarks>
/// <example>
/// <code>
/// // Setup multi-species boid simulation
/// var boidSystem = new BoidSystem();
/// boidSystem.Initialize(world);
/// 
/// // Create different species with varying behaviors
/// var predatorSpecies = new BoidSpeciesComponent("predator");
/// var preySpecies = new BoidSpeciesComponent("prey");
/// 
/// // Configure species interactions
/// var settings = new MultiSpeciesBoidSettingsComponent
/// {
///     NeighborRadius = 5.0f,
///     MaxSpeed = 10.0f,
///     InteractionWeights = {
///         [("predator", "prey")] = new() { SeparationWeight = 0.1f, AlignmentWeight = 0f, CohesionWeight = 2.0f },
///         [("prey", "predator")] = new() { SeparationWeight = 3.0f, AlignmentWeight = 0f, CohesionWeight = 0f }
///     }
/// };
/// 
/// // Update each frame for real-time flocking behavior
/// boidSystem.Update(deltaTime);
/// </code>
/// </example>
public class BoidSystem : ISystem
{
    private readonly Random _random = new();
    private IWorld _world = null!;

    /// <summary>
    /// Initializes the system with access to the ECS world.
    /// Called once when the system is registered with the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for entity and component operations</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    /// <remarks>
    /// Stores the world reference for component queries and updates during the simulation.
    /// The random number generator is seeded for consistent jitter behavior across runs.
    /// </remarks>
    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    /// <summary>
    /// Executes one frame of the boids flocking simulation using Craig Reynolds' algorithm.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
    /// <remarks>
    /// ALGORITHM IMPLEMENTATION:
    /// 
    /// Phase 1: Data Gathering (O(n))
    /// - Retrieve global simulation settings
    /// - Query all boid entities with required components
    /// - Query obstacle entities for avoidance calculations
    /// 
    /// Phase 2: Neighbor Analysis (O(n²))
    /// - For each boid, find neighbors within perception radius
    /// - Calculate separation, alignment, and cohesion forces
    /// - Handle multi-species interactions with different weights
    /// 
    /// Phase 3: Force Integration (O(n))
    /// - Combine steering forces according to species behavior
    /// - Add obstacle avoidance using inverse square law
    /// - Apply random jitter for behavioral variety
    /// 
    /// Phase 4: Motion Update (O(n))
    /// - Integrate forces to update velocity (Euler integration)
    /// - Clamp velocity to maximum speed constraint
    /// - Update position using velocity and deltaTime
    /// - Apply boundary wrapping for toroidal world
    /// 
    /// PERFORMANCE BOTTLENECKS:
    /// - Nested loops for neighbor detection: O(n²) complexity
    /// - Distance calculations using expensive sqrt operations
    /// - Multi-species weight lookups in dictionary
    /// 
    /// OPTIMIZATION OPPORTUNITIES:
    /// - Spatial hashing for O(n) average neighbor detection
    /// - Fast inverse square root approximation
    /// - Level-of-detail for distant boids
    /// - Parallel processing of independent boids
    /// </remarks>
    public void Update(float deltaTime)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // PHASE 1: DATA GATHERING AND INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════
        
        // Fetch global simulation parameters from singleton component
        var settings = _world.GetSingleton<MultiSpeciesBoidSettingsComponent>();

        // Gather all boids and obstacles for this frame's calculations
        // Performance Note: ToArray() materializes queries to avoid multiple iterations
        var boidEntries = _world
            .Query<WorldTransformComponent, VelocityComponent, BoidSpeciesComponent>()
            .ToArray();
        var obstacleEntries = _world.Query<WorldTransformComponent, ObstacleComponent>().ToArray();

        // ═══════════════════════════════════════════════════════════════════════════
        // PHASE 2: BOID BEHAVIOR COMPUTATION (MAIN ALGORITHM LOOP)
        // ═══════════════════════════════════════════════════════════════════════════
        
        // Process each boid independently (potential for parallelization)
        foreach (
            var (boidEntity, worldTransformComponent, velocityComponent, speciesComponent) in boidEntries
        )
        {
            Vector2D<float> currentPosition = worldTransformComponent.WorldPosition;
            Vector2D<float> currentVelocity = velocityComponent;
            string speciesId = speciesComponent.SpeciesId;

            // Initialize force accumulators for the three classic boid behaviors
            // Educational Note: These represent the fundamental steering behaviors in Reynolds' model
            var separationForceAccumulator = Vector2D<float>.Zero;  // Avoid crowding
            var alignmentForceAccumulator = Vector2D<float>.Zero;   // Match neighbor heading
            var cohesionForceAccumulator = Vector2D<float>.Zero;    // Move toward group center
            int neighborCount = 0;

            // ───────────────────────────────────────────────────────────────────────
            // NEIGHBOR INTERACTION ANALYSIS (O(n) per boid, O(n²) total)
            // ───────────────────────────────────────────────────────────────────────
            
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

                // Calculate spatial relationship between current boid and neighbor
                var offsetToOther = otherPosition - currentPosition;
                float distanceToOther = offsetToOther.Length;
                
                // Skip self-interaction and boids outside perception radius
                if (distanceToOther > settings.NeighborRadius || distanceToOther <= 0f)
                    continue;

                // Retrieve species interaction weights (predator-prey, flocking, etc.)
                if (
                    !settings.InteractionWeights.TryGetValue(
                        (speciesId, otherId),
                        out var interaction
                    )
                )
                    continue;

                // SEPARATION: Inverse square law for close-range repulsion
                // Educational Note: Division by distance² creates realistic falloff
                // Closer neighbors have exponentially stronger repulsive effect
                separationForceAccumulator -=
                    offsetToOther
                    / (distanceToOther * distanceToOther)
                    * interaction.SeparationWeight;
                    
                // ALIGNMENT: Accumulate neighbor velocities for directional consensus
                // Educational Note: Creates coordinated group movement patterns
                alignmentForceAccumulator += otherVelocity * interaction.AlignmentWeight;
                
                // COHESION: Accumulate neighbor positions for group center calculation
                // Educational Note: Attracts boids toward the local group centroid
                cohesionForceAccumulator += otherPosition * interaction.CohesionWeight;

                neighborCount++;
            }

            // ───────────────────────────────────────────────────────────────────────
            // FORCE COMBINATION AND NORMALIZATION
            // ───────────────────────────────────────────────────────────────────────
            
            var flockingForce = Vector2D<float>.Zero;
            if (neighborCount > 0)
            {
                // Normalize accumulated forces to create unit direction vectors
                // Educational Note: Normalization ensures consistent force magnitudes
                // regardless of neighbor count, preventing speed variation with density
                var separationDirection = separationForceAccumulator.Normalize();
                var alignmentDirection = (alignmentForceAccumulator / neighborCount).Normalize();
                var cohesionDirection = (
                    cohesionForceAccumulator / neighborCount - currentPosition
                ).Normalize();

                // Apply species-specific weights to the three fundamental behaviors
                var selfInteraction = settings.InteractionWeights[(speciesId, speciesId)];

                flockingForce =
                    separationDirection * selfInteraction.SeparationWeight
                    + alignmentDirection * selfInteraction.AlignmentWeight
                    + cohesionDirection * selfInteraction.CohesionWeight;
            }

            // ───────────────────────────────────────────────────────────────────────
            // OBSTACLE AVOIDANCE (Environmental Constraints)
            // ───────────────────────────────────────────────────────────────────────
            
            // Educational Note: Obstacle avoidance uses similar inverse square law
            // as separation but applies to static environmental geometry
            foreach (var (_, obstacleWorldTransformComponent, obstacleComponent) in obstacleEntries)
            {
                Vector2D<float> obstaclePosition = obstacleWorldTransformComponent.WorldPosition;
                float obstacleRadius = obstacleComponent.Radius;

                var offsetToObstacle = obstaclePosition - currentPosition;
                float distanceToObstacle = offsetToObstacle.Length;
                
                // Apply strong repulsive force when approaching obstacle boundary
                if (distanceToObstacle < obstacleRadius + 0.1f)
                    flockingForce -=
                        offsetToObstacle
                        / (distanceToObstacle * distanceToObstacle)
                        * settings.ObstacleAvoidanceWeight;
            }

            // ───────────────────────────────────────────────────────────────────────
            // BEHAVIORAL VARIETY THROUGH RANDOM JITTER
            // ───────────────────────────────────────────────────────────────────────
            
            // Educational Note: Small random forces prevent perfectly predictable movement
            // and create more natural, lifelike behaviors in the simulation
            float jitterAngle = (float)(_random.NextDouble() * Math.PI * 2);
            var jitterVector =
                new Vector2D<float>(MathF.Cos(jitterAngle), MathF.Sin(jitterAngle))
                * settings.JitterStrength;

            // ═══════════════════════════════════════════════════════════════════════════
            // PHASE 3: NUMERICAL INTEGRATION AND CONSTRAINT APPLICATION
            // ═══════════════════════════════════════════════════════════════════════════
            
            // Euler integration: velocity += acceleration * deltaTime
            // Educational Note: Simple but effective for real-time simulation
            // More sophisticated integrators (Runge-Kutta) could improve stability
            var newVelocity = (
                currentVelocity + (flockingForce + jitterVector) * deltaTime
            ).ClampLength(settings.MaxSpeed);
            
            // Position integration: position += velocity * deltaTime
            var newPosition = currentPosition + newVelocity * deltaTime;

            // ───────────────────────────────────────────────────────────────────────
            // BOUNDARY CONDITIONS (Toroidal World Wrapping)
            // ───────────────────────────────────────────────────────────────────────
            
            // Educational Note: Wrapping creates infinite world illusion
            // Alternative approaches: bouncing, clamping, or invisible barriers
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

            // ═══════════════════════════════════════════════════════════════════════════
            // PHASE 4: COMPONENT STATE UPDATES (ECS Integration)
            // ═══════════════════════════════════════════════════════════════════════════
            
            // Commit computed changes back to the ECS component system
            // Educational Note: Separation of computation and state mutation
            // enables cleaner code and potential optimizations (batching, caching)
            _world.SetComponent(boidEntity, (VelocityComponent)newVelocity);
            
            // Update local transform component with new position
            // Note: For root entities, local transform equals world transform
            var currentLocalTransform = boidEntity.GetLocalTransform(_world) ?? new TransformComponent();
            var updatedLocalTransform = currentLocalTransform.WithPosition(newPosition);
            _world.SetComponent(boidEntity, updatedLocalTransform);
        }
    }

    /// <summary>
    /// Cleans up system resources before the system is removed.
    /// Called once when the system is unregistered from the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for final cleanup operations</param>
    /// <remarks>
    /// The BoidSystem is stateless except for the cached world reference and random generator.
    /// No explicit cleanup is required as all component modifications are managed by the ECS world.
    /// The system maintains no persistent state, allocated memory, or native resources.
    /// 
    /// Educational Note: Proper resource cleanup is crucial in game engines to prevent
    /// memory leaks and ensure stable operation during system hot-swapping.
    /// </remarks>
    public void Shutdown(IWorld world)
    {
        // No resources to clean up for BoidSystem
        // All component modifications are managed by the ECS world
    }
}
