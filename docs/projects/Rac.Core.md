# Rac.Core Project Documentation

## Project Overview

The `Rac.Core` project provides the foundational infrastructure and cross-cutting concerns for the Rac game engine. This project encompasses essential utilities including mathematics operations, data structures, memory management, configuration systems, and platform abstraction layers that support all other engine components.

### Key Design Principles

- **Cross-Platform Foundation**: Abstracts platform-specific operations for consistent behavior across different operating systems
- **Immutable Configuration**: Thread-safe configuration management using immutable data structures and builder patterns
- **Service Abstraction**: Interface-driven design enabling dependency injection and testable architectures
- **Performance-First Utilities**: Optimized data structures and algorithms for frame-rate critical operations
- **Extensible Architecture**: Plugin-friendly design supporting custom implementations and engine extensions

### Performance Characteristics and Optimization Goals

The core infrastructure prioritizes minimal allocation in hot paths, efficient memory management through object pooling strategies, and optimized mathematical operations for real-time performance. Configuration systems use immutable structures to eliminate thread safety concerns while maintaining high-performance access patterns.

## Architecture Overview

Rac.Core follows a layered architecture providing foundation services that higher-level engine systems depend upon. The design emphasizes clear separation of concerns, with configuration management, windowing abstraction, logging infrastructure, and utility functions organized into focused namespaces that enable selective usage patterns.

### Core Architectural Decisions

- **Immutable Configuration Pattern**: Thread-safe configuration using immutable data structures with builder pattern construction
- **Service Interface Abstraction**: Clean dependency injection support through comprehensive interface definitions
- **Platform Abstraction Layer**: Unified API for platform-specific operations including windowing and file system access
- **Builder Pattern Implementation**: Fluent API for complex object construction with validation and defaults
- **Extension Method Pattern**: C# language extensions for engine-specific operations without modifying existing types

### Integration with ECS System and Other Engine Components

Rac.Core provides the foundational services that ECS and other engine systems depend upon, including configuration management for system initialization, logging infrastructure for diagnostics, and mathematical utilities for transform calculations. The windowing system integrates with rendering through platform abstraction layers.

## Namespace Organization

### Rac.Core.Configuration

Manages engine configuration and feature toggling through immutable data structures and builder patterns.

**EngineProfile**: Enumeration defining standard engine configurations including FullGame (complete feature set), Headless (server scenarios), and Custom (manual configuration). Provides predefined configuration templates for common deployment scenarios and development workflows.

**ImmutableEngineConfig**: Immutable configuration object managing engine subsystem feature flags and dependency injection services. Supports builder pattern construction for complex configuration scenarios while maintaining thread-safe access patterns throughout the engine lifecycle.

### Rac.Core.Builder

Provides fluent builder APIs for complex object construction with validation and default value management.

**EngineBuilder**: Immutable builder implementation for constructing engine configurations with fluent API patterns. Each method returns a new instance maintaining immutability while enabling readable configuration construction with method chaining and comprehensive validation.

### Rac.Core.Manager

Contains management abstractions for platform services and resource coordination.

**IWindowManager**: Interface abstracting cross-platform windowing operations including window creation, sizing, and event management. Supports various windowing platforms through Silk.NET integration while providing responsive layout handling and event-driven resize operations.

**WindowManager**: Concrete implementation providing cross-platform window management through Silk.NET. Handles window lifecycle, configuration, and provides comprehensive event management for window state changes and user interactions.

**ConfigManager**: Manages configuration loading, validation, and persistence across different engine profiles. Supports both file-based and programmatic configuration while maintaining compatibility with dependency injection systems and runtime configuration updates.

**WindowBuilder**: Builder pattern implementation for window creation with validation and default configuration management. Provides fluent API for window property configuration while ensuring platform compatibility and proper resource initialization.

### Rac.Core.Logger

Structured logging infrastructure supporting different severity levels and output targets.

**ILogger**: Interface defining logging service contracts with structured severity levels including Debug, Info, Warning, and Error messages. Enables consistent logging patterns across engine systems while supporting different logging backend implementations.

**SerilogLogger**: Serilog-based logging implementation providing comprehensive logging capabilities including structured logging, multiple output sinks, and configurable formatting patterns for development and production scenarios.

### Rac.Core.Scheduler

Task scheduling and execution management infrastructure for time-sensitive engine operations.

**ITaskScheduler**: Interface for task scheduling abstraction supporting frame-based execution, time-delayed operations, recurring task management, and priority-based queuing. Provides foundation for game engine update cycles, rendering coordination, and asynchronous operation management.

**TaskScheduler**: Implementation providing comprehensive task scheduling capabilities including coroutine-style async operations, priority-based execution, and frame-rate independent timing for consistent game behavior across different hardware configurations.

### Rac.Core.Extension

C# language extensions providing engine-specific operations and utility functions.

**Vector2DExtensions**: Extension methods for vector mathematics including distance calculations, normalization operations, angle computations, and geometric utility functions optimized for game development scenarios and frequent mathematical operations.

## Core Concepts and Workflows

### Configuration Management Workflow

Engine configuration follows an immutable pattern where base configurations are created through factory methods and modified configurations are generated through builder patterns. This approach ensures thread safety while enabling complex configuration scenarios through method chaining and validation.

### Platform Abstraction Patterns

The windowing system abstracts platform-specific operations through interface definitions that enable consistent behavior across Windows, Linux, and macOS. Platform-specific implementations are isolated behind interfaces to support future platform additions and testing scenarios.

### Service Registration and Discovery

The dependency injection infrastructure supports both constructor injection for required dependencies and service location for optional components. This pattern enables modular engine architecture where systems can function with reduced capability when optional services are unavailable.

### Integration with ECS

Core utilities integrate with ECS through mathematical functions for transform operations, configuration services for system initialization, and logging infrastructure for ECS operation diagnostics. The scheduling system coordinates ECS updates within the main game loop timing.

## Integration Points

### Dependencies on Other Engine Projects

- **Microsoft.Extensions.DependencyInjection**: Dependency injection infrastructure for service management
- **Silk.NET.Windowing**: Cross-platform windowing abstraction for window management
- **Silk.NET.Maths**: Mathematical utilities for vector and matrix operations
- **Serilog**: Structured logging implementation for comprehensive diagnostics

### How Other Systems Interact with Rac.Core

All engine systems depend on Rac.Core for foundational services including configuration management, logging infrastructure, and mathematical utilities. The ECS uses mathematical extensions for transform calculations, while rendering systems utilize windowing abstractions for platform compatibility.

### Data Consumed from ECS

Core systems primarily provide services to ECS rather than consuming ECS data. However, logging systems may capture ECS performance metrics, and configuration systems may use ECS entity counts for memory allocation optimization.

## Usage Patterns

### Common Setup Patterns

Engine initialization involves creating configurations through builder patterns, initializing logging infrastructure, and establishing windowing contexts. The system supports both simple default configurations and complex custom configurations for specialized deployment scenarios.

### How to Use the Project for Entities from ECS

While Rac.Core primarily provides foundational services, mathematical extensions can be used for entity transform calculations, and logging services enable entity operation diagnostics. Configuration systems determine which ECS features are available in different deployment scenarios.

### Resource Loading and Management Workflows

Resource management operates through service interfaces that abstract platform-specific operations. The configuration system determines which resources are available while logging provides diagnostics for resource operations and lifecycle management.

### Performance Optimization Patterns

Optimal performance requires appropriate configuration for target deployment scenarios, efficient mathematical operation usage through extension methods, and strategic logging level configuration to minimize performance impact during production operation.

## Extension Points

### How to Add New Core Features

New foundational services can be added through interface definition and dependency injection registration. The service interface pattern enables adding platform-specific implementations while maintaining compatibility with existing engine systems.

### Extensibility Points

The configuration system supports additional feature flags and service registrations for custom engine capabilities. Mathematical extensions can be added for domain-specific operations, while logging can be extended with custom output targets and formatting.

### Future Enhancement Opportunities

The core architecture supports advanced features including hot-reloadable configuration, dynamic service discovery, advanced mathematical operations for specialized game types, and enhanced platform abstraction for additional target platforms and deployment scenarios.