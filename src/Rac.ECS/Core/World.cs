// File: src/Rac.ECS.Core/World.cs

using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
///   The central container for all entities and their components in the ECS.
/// </summary>
public sealed class World
{
	private readonly Dictionary<Type, Dictionary<int, IComponent>> _components = new();
	private int _nextEntityId = 1;

    /// <summary>
    ///   Creates a new entity with a unique identifier.
    /// </summary>
    public Entity CreateEntity()
	{
		return new Entity(_nextEntityId++);
	}

    /// <summary>
    ///   Adds or replaces a component of type <typeparamref name="T" /> on the given <paramref name="entity" />.
    /// </summary>
    public void SetComponent<T>(Entity entity, T component)
		where T : IComponent
	{
		var componentType = typeof(T);
		if (!_components.TryGetValue(componentType, out var pool))
		{
			pool = new Dictionary<int, IComponent>();
			_components[componentType] = pool;
		}

		pool[entity.Id] = component;
	}

    /// <summary>
    ///   Retrieves the first registered component of type <typeparamref name="T" />,
    ///   or throws if none has been set.
    /// </summary>
    public T GetSingleton<T>()
		where T : IComponent
	{
		var componentType = typeof(T);
		if (_components.TryGetValue(componentType, out var pool) && pool.Count > 0) return (T)pool.Values.First();
		throw new InvalidOperationException(
			$"No singleton component of type {componentType.Name} registered."
		);
	}

    /// <summary>
    ///   Queries all entities that have component <typeparamref name="T1" />.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1)> Query<T1>()
		where T1 : IComponent
	{
		var pool1 = GetPool<T1>();

		foreach ((int entityId, var component) in pool1) yield return (new Entity(entityId), (T1)component);
	}

    /// <summary>
    ///   Queries all entities that have both components T1 and T2.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> Query<T1, T2>()
		where T1 : IComponent
		where T2 : IComponent
	{
		var pool1 = GetPool<T1>();
		var pool2 = GetPool<T2>();

		// Iterate the smaller pool for better performance
		var (smallerPool, largerPool) =
			pool1.Count <= pool2.Count ? (pool1, pool2) : (pool2, pool1);

		foreach (var entry in smallerPool)
		{
			int entityId = entry.Key;
			if (pool1.ContainsKey(entityId) && pool2.ContainsKey(entityId))
				yield return (new Entity(entityId), (T1)pool1[entityId], (T2)pool2[entityId]);
		}
	}

    /// <summary>
    ///   Queries all entities that have components T1, T2, and T3.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> Query<
		T1,
		T2,
		T3
	>()
		where T1 : IComponent
		where T2 : IComponent
		where T3 : IComponent
	{
		var pool1 = GetPool<T1>();
		var pool2 = GetPool<T2>();
		var pool3 = GetPool<T3>();

		// Pick the smallest pool to drive iteration
		var smallestPool = new[] { pool1, pool2, pool3 }.OrderBy(p => p.Count).First();

		foreach (var entry in smallestPool)
		{
			int entityId = entry.Key;
			if (
				pool1.ContainsKey(entityId)
				&& pool2.ContainsKey(entityId)
				&& pool3.ContainsKey(entityId)
			)
				yield return (
					new Entity(entityId),
					(T1)pool1[entityId],
					(T2)pool2[entityId],
					(T3)pool3[entityId]
				);
		}
	}

    /// <summary>
    ///   Retrieves (or creates) the dictionary pool for components of type T.
    /// </summary>
    private Dictionary<int, IComponent> GetPool<T>()
		where T : IComponent
	{
		var componentType = typeof(T);
		if (!_components.TryGetValue(componentType, out var pool))
		{
			pool = new Dictionary<int, IComponent>();
			_components[componentType] = pool;
		}

		return pool;
	}

    /// <summary>
    ///   Removes a component of type <typeparamref name="T" /> from the given <paramref name="entity" />.
    ///   Returns true if a component was removed.
    /// </summary>
    public bool RemoveComponent<T>(Entity entity)
		where T : IComponent
	{
		var componentType = typeof(T);
		if (_components.TryGetValue(componentType, out var pool)) return pool.Remove(entity.Id);
		return false;
	}
}