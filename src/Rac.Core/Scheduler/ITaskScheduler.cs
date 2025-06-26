namespace Rac.Core.Scheduler;

/// <summary>
/// Defines the interface for task scheduling and execution management in the game engine.
/// Provides abstraction for scheduling work to be executed at specific times or intervals.
/// </summary>
/// <remarks>
/// This interface will provide the foundation for game engine task scheduling including:
/// - Frame-based task execution
/// - Time-delayed task execution  
/// - Recurring task scheduling
/// - Priority-based task queuing
/// - Coroutine-style async task management
/// 
/// The task scheduler is essential for game engines to manage updates, rendering,
/// physics calculations, and other time-sensitive operations efficiently.
/// 
/// Implementation Status: This interface is currently a placeholder and will be
/// implemented in future engine iterations.
/// </remarks>
/// <example>
/// <code>
/// // Future usage example:
/// // Schedule a task to run every frame
/// // taskScheduler.ScheduleRepeating(UpdateEnemyAI, TimeSpan.Zero);
/// 
/// // Schedule a delayed task
/// // taskScheduler.ScheduleDelayed(SpawnBoss, TimeSpan.FromSeconds(30));
/// 
/// // Schedule a high-priority task
/// // taskScheduler.ScheduleImmediate(HandlePlayerInput, TaskPriority.High);
/// </code>
/// </example>
public interface ITaskScheduler
{
    // TODO: implement ITaskScheduler
}
