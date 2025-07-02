using System.Reflection;

namespace Rac.ECS.Systems;

/// <summary>
/// Internal helper class for resolving system dependencies and creating execution order.
/// Implements topological sorting algorithm to handle RunAfter attribute dependencies.
/// </summary>
/// <remarks>
/// TOPOLOGICAL SORTING ALGORITHM:
/// This implementation uses Kahn's algorithm for topological sorting:
/// 1. Calculate in-degree for each node (number of dependencies)
/// 2. Start with nodes that have no dependencies (in-degree = 0)
/// 3. Remove nodes from graph and update in-degrees of dependent nodes
/// 4. Repeat until all nodes are processed or a cycle is detected
/// 
/// EDUCATIONAL NOTES:
/// - Topological sorting is used in build systems (like Make, MSBuild)
/// - Task scheduling systems use similar algorithms for dependency resolution
/// - The algorithm has O(V + E) time complexity where V = systems, E = dependencies
/// - Cycle detection is essential for preventing infinite loops in dependency chains
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Dependency resolution is performed once when systems are added/removed
/// - Runtime execution uses pre-computed order for optimal performance
/// - Memory usage is proportional to number of systems and dependencies
/// </remarks>
internal static class SystemDependencyResolver
{
    /// <summary>
    /// Resolves system dependencies and returns systems in execution order.
    /// Uses topological sorting to handle RunAfter attribute dependencies.
    /// </summary>
    /// <param name="systems">Collection of systems to order by dependencies.</param>
    /// <returns>Systems ordered by their dependencies.</returns>
    /// <exception cref="InvalidOperationException">Thrown when circular dependencies are detected.</exception>
    public static List<ISystem> ResolveDependencies(IEnumerable<ISystem> systems)
    {
        var systemList = systems.ToList();
        if (systemList.Count <= 1)
            return systemList;

        // Build dependency graph
        var dependencyGraph = BuildDependencyGraph(systemList);
        var systemTypeToInstance = systemList.ToDictionary(s => s.GetType(), s => s);
        
        // Perform topological sort using Kahn's algorithm
        return TopologicalSort(dependencyGraph, systemTypeToInstance);
    }

    /// <summary>
    /// Builds a dependency graph from system RunAfter attributes.
    /// </summary>
    /// <param name="systems">Systems to analyze for dependencies.</param>
    /// <returns>Dictionary mapping system types to their dependencies.</returns>
    private static Dictionary<Type, HashSet<Type>> BuildDependencyGraph(List<ISystem> systems)
    {
        var graph = new Dictionary<Type, HashSet<Type>>();
        var systemTypes = systems.Select(s => s.GetType()).ToHashSet();

        // Initialize graph with all system types
        foreach (var system in systems)
        {
            graph[system.GetType()] = new HashSet<Type>();
        }

        // Build dependency relationships from RunAfter attributes
        foreach (var system in systems)
        {
            var systemType = system.GetType();
            var runAfterAttributes = systemType.GetCustomAttributes<RunAfterAttribute>();

            foreach (var attr in runAfterAttributes)
            {
                var dependencyType = attr.SystemType;
                
                // Only add dependency if the target system is actually registered
                if (systemTypes.Contains(dependencyType))
                {
                    graph[systemType].Add(dependencyType);
                }
            }
        }

        return graph;
    }

    /// <summary>
    /// Performs topological sorting using Kahn's algorithm.
    /// </summary>
    /// <param name="dependencyGraph">Graph of system dependencies.</param>
    /// <param name="systemInstances">Mapping from system types to instances.</param>
    /// <returns>Systems in dependency-resolved execution order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when circular dependencies are detected.</exception>
    private static List<ISystem> TopologicalSort(
        Dictionary<Type, HashSet<Type>> dependencyGraph,
        Dictionary<Type, ISystem> systemInstances)
    {
        var result = new List<ISystem>();
        var inDegree = new Dictionary<Type, int>();
        var queue = new Queue<Type>();

        // ───────────────────────────────────────────────────────────────────────
        // CALCULATE IN-DEGREES
        // ───────────────────────────────────────────────────────────────────────
        
        // Initialize in-degrees to zero
        foreach (var systemType in dependencyGraph.Keys)
        {
            inDegree[systemType] = 0;
        }

        // Count incoming edges (dependencies)
        foreach (var (systemType, dependencies) in dependencyGraph)
        {
            inDegree[systemType] = dependencies.Count;
        }

        // ───────────────────────────────────────────────────────────────────────
        // FIND SYSTEMS WITH NO DEPENDENCIES
        // ───────────────────────────────────────────────────────────────────────
        
        foreach (var (systemType, degree) in inDegree)
        {
            if (degree == 0)
            {
                queue.Enqueue(systemType);
            }
        }

        // ───────────────────────────────────────────────────────────────────────
        // PROCESS SYSTEMS IN DEPENDENCY ORDER
        // ───────────────────────────────────────────────────────────────────────
        
        while (queue.Count > 0)
        {
            var currentType = queue.Dequeue();
            result.Add(systemInstances[currentType]);

            // Reduce in-degree for systems that depend on current system
            foreach (var (systemType, dependencies) in dependencyGraph)
            {
                if (dependencies.Contains(currentType))
                {
                    inDegree[systemType]--;
                    if (inDegree[systemType] == 0)
                    {
                        queue.Enqueue(systemType);
                    }
                }
            }
        }

        // ───────────────────────────────────────────────────────────────────────
        // DETECT CIRCULAR DEPENDENCIES
        // ───────────────────────────────────────────────────────────────────────
        
        if (result.Count != systemInstances.Count)
        {
            var unprocessedSystems = systemInstances.Keys
                .Where(type => !result.Any(s => s.GetType() == type))
                .Select(type => type.Name);
            
            throw new InvalidOperationException(
                $"Circular dependency detected among systems: {string.Join(", ", unprocessedSystems)}. " +
                "Check RunAfter attributes for circular references.");
        }

        return result;
    }
}