// File: src/Rac.ECS.Component/MultiSpeciesBoidSettingsComponent.cs

using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.ECS.Component;

/// <summary>
///   All tunable parameters for a multi‐species Boids simulation.
///   InteractionWeights maps (SelfSpecies, OtherSpecies) → per‐neighbor weights.
/// </summary>
public sealed record class MultiSpeciesBoidSettingsComponent(
    /// <summary>How far to look for neighbors (world units).</summary>
    float NeighborRadius,
    /// <summary>Random jitter magnitude to keep things lively.</summary>
    float JitterStrength,
    /// <summary>Maximum boid speed (world units/sec).</summary>
    float MaxSpeed,
    /// <summary>World‐wrap bounds.</summary>
    Vector2D<float> BoundaryMin,
    Vector2D<float> BoundaryMax,
    /// <summary>
    /// Map from (SelfSpeciesId, OtherSpeciesId) → interaction weights.
    /// </summary>
    Dictionary<(string Self, string Other), SpeciesInteraction> InteractionWeights,
    /// <summary>Steering weight to avoid obstacles.</summary>
    float ObstacleAvoidanceWeight
) : IComponent;

/// <summary>
///   How one boid steers in response to a single neighbor of another species.
/// </summary>
public readonly record struct SpeciesInteraction(
    float SeparationWeight,
    float AlignmentWeight,
    float CohesionWeight
);
