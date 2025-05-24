// File: src/Rac.ECS.Component/BoidSpeciesComponent.cs

using Rac.ECS.Components;

namespace Rac.ECS.Component;

/// <summary>
///   Attaches a runtime species identifier to a boid, plus its draw scale.
/// </summary>
public readonly record struct BoidSpeciesComponent(
	/// <summary>Arbitrary identifier (e.g. "White", "Blue", "Red", or "Enemy", "Ally", etc.)</summary>
	string SpeciesId,
	/// <summary>Scale factor for drawing this species.</summary>
	float Scale
) : IComponent;