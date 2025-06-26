# Rac.ECS Project Documentation

## Project Overview

The `Rac.ECS` project implements a complete Entity-Component-System architecture that serves as the foundational data management and game object system for the Rac game engine. This implementation prioritizes data-oriented design, composition over inheritance, and high-performance component queries to support modern game development workflows.

### Key Design Principles

- **Data-Oriented Design**: Components store only data, systems contain only logic
- **Composition Over Inheritance**: Game objects are assembled from components rather than inherited from base classes
- **Performance-First**: Optimized component storage and query systems for frame-rate critical operations
- **Immutable Components**: Components are readonly record structs for thread safety and predictable behavior
- **Sparse Data Storage**: Only entities with specific components consume memory for those components

## Architecture Overview

The ECS implementation follows a three-pillar architecture where entities serve as lightweight identifiers, components represent pure data containers, and systems provide all behavioral logic. The `World` class acts as the central database, managing component storage and providing efficient query mechanisms for systems to operate on relevant entity sets.

### Core Architectural Decisions

- **Dictionary-based Component Storage**: Two-tier dictionary system (Type → EntityId → Component) enables O(1) component access
- **Query-driven System Design**: Systems discover relevant entities through type-safe component queries
- **Hierarchical Transform Support**: Built-in support for parent-child relationships with automatic world transform computation
- **Extension Method Pattern**: Core entity operations exposed through extension methods to maintain clean separation of concerns

## Namespace Organization

### Rac.ECS.Core

Contains the fundamental building blocks of the ECS architecture and primary interaction points for game code.

**Entity**: Lightweight identifier struct that represents game objects. Contains only an ID and alive status, following ECS principles where entities are pure identifiers without behavior or data.

**World**: Central component database and query engine. Manages all component storage, entity creation, and provides the query interface that systems use to discover relevant entities. Implements optimized multi-component queries with performance considerations for varying component pool sizes.

**EntityHierarchyExtensions**: Convenience layer that provides intuitive hierarchy management operations. Encapsulates complex parent-child relationship logic while maintaining the underlying ECS data model. Enables natural game object manipulation patterns while preserving ECS architectural integrity.

### Rac.ECS.Components

Houses all component definitions and the base component interface. Components in this namespace represent the data vocabulary of the game engine.

**IComponent**: Marker interface that defines the contract for all component types. Establishes the pure data principle where components contain no methods or behavior.

**TransformComponent**: Stores local transformation data (position, rotation, scale) relative to an entity's parent in the hierarchy. Provides the foundation for all spatial relationships in the game world.

**WorldTransformComponent**: Contains computed world-space transformation data. Generated automatically by the TransformSystem and represents the final transformation used by rendering, physics, and other world-space systems.

**ParentHierarchyComponent**: Manages parent-child relationships between entities. Stores parent references and child collections to enable scene graph traversal and hierarchical transform inheritance.

### Rac.ECS.Systems

Contains the behavioral logic layer of the ECS architecture and system management infrastructure.

**ISystem**: Defines the contract for all system implementations. Establishes the frame-based update pattern with delta time for frame-rate independent behavior.

**SystemScheduler**: Manages system registration, execution order, and batch updates. Provides the runtime infrastructure for coordinating system execution during the game loop.

**TransformSystem**: Specialized system that computes world transforms from local transforms and hierarchy relationships. Implements depth-first traversal algorithms to efficiently propagate transformations through the scene graph.

### Rac.ECS.Entities

Reserved namespace for future entity-related functionality. Currently contains placeholder interfaces for potential entity behavior extensions.

## Core Concepts and Workflows

### Component Lifecycle

Components follow an immutable data model where modifications create new component instances rather than mutating existing ones. This approach ensures predictable behavior, enables efficient caching strategies, and supports potential multithreading scenarios. Components are added to entities through the World's SetComponent method and queried through type-safe query operations.

### Hierarchy Management

The hierarchy system operates on two-way relationships where parent entities maintain child collections and child entities store parent references. This design enables efficient upward and downward traversal while supporting complex scene graph operations. Transform inheritance flows from parent to child through matrix composition, following standard graphics pipeline conventions.

### System Execution Model

Systems operate on entity sets discovered through component queries, processing relevant entities each frame. The SystemScheduler manages execution order and provides delta time for frame-rate independent updates. Systems remain stateless and operate exclusively through the World interface, ensuring clean separation between logic and data.

### Query Performance Characteristics

Multi-component queries optimize performance by iterating the smallest component pool first, reducing unnecessary entity checks. The query system supports single, dual, and triple component combinations with automatic performance optimization. Query results are lazy-evaluated and can be safely enumerated multiple times within a frame.

## Integration Points

### Engine Dependencies

- **Silk.NET.Maths**: Provides vector and matrix mathematics for spatial calculations
- **Rac.Core**: Foundation types and utilities (referenced but not detailed in provided files)

### Rendering System Integration

The ECS provides WorldTransformComponent data that rendering systems consume for vertex transformation and culling operations. The transform hierarchy ensures that rendering reflects the logical scene structure without requiring additional spatial calculations in rendering code.

### Physics System Integration

Physics systems operate on world transform data for collision detection and response calculations. The ECS component model allows physics properties to be attached to specific entities without affecting the transform system or other game logic.

### Input System Integration

Input systems can query for entities with input-related components and update their state based on user interaction. The hierarchy system supports interaction models where input on parent entities can affect child entities or vice versa.

## Usage Patterns

### Entity Creation and Setup

Typical entity creation involves calling `World.CreateEntity()` followed by component attachment through `World.SetComponent()`. Entities requiring hierarchical relationships use the extension methods to establish parent-child connections and set local transforms.

### System Implementation

Custom systems implement `ISystem` and use World queries to discover relevant entities. Systems typically iterate query results, read component data, perform calculations, and update components with new states. Frame-rate independence is achieved by scaling operations with the provided delta time.

### Hierarchy Operations

Parent-child relationships are established through `EntityHierarchyExtensions.SetParent()` or similar methods. Local transforms are set relative to parent coordinates, and world transforms are computed automatically by the TransformSystem. Hierarchy queries enable discovery of entity relationships for gameplay logic.

### Performance Optimization

Performance-critical code should minimize component queries per frame and cache query results when appropriate. Systems should process entities in batches when possible to improve cache locality. Component modifications should be batched when feasible to reduce World manipulation overhead.

## Extension Points

The ECS architecture supports extension through additional component types, custom systems, and enhanced query capabilities. New component types integrate seamlessly with existing query infrastructure. Custom systems can implement domain-specific logic while leveraging the established component and hierarchy systems.

Future enhancements may include component archetype optimization, multithreaded system execution, and enhanced query filtering capabilities. The current architecture provides a solid foundation for these advanced features without requiring fundamental design changes.