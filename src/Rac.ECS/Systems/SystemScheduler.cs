namespace Rac.ECS.Systems
{
	/// <summary>
	/// Holds and runs a collection of ECS systems each frame.
	/// </summary>
	public sealed class SystemScheduler
	{
		private readonly List<ISystem> _systems = new();

		/// <summary>
		/// Registers a new system to be updated each frame.
		/// </summary>
		/// <param name="system">The ECS system to add.</param>
		public void Add(ISystem system)
		{
			if (system == null) throw new ArgumentNullException(nameof(system));
			_systems.Add(system);
		}

		/// <summary>
		/// Unregisters a system so it no longer runs.
		/// </summary>
		/// <param name="system">The ECS system to remove.</param>
		/// <returns>True if the system was found and removed.</returns>
		public bool Remove(ISystem system)
			=> _systems.Remove(system);

		/// <summary>
		/// Calls <see cref="ISystem.Update"/> on each registered system in order.
		/// </summary>
		/// <param name="delta">Elapsed time in seconds since the last update.</param>
		public void Update(float delta)
		{
			foreach (var system in _systems)
			{
				system.Update(delta);
			}
		}

		/// <summary>
		/// Clears all registered systems.
		/// </summary>
		public void Clear()
			=> _systems.Clear();
	}
}