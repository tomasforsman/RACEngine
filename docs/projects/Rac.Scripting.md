# Rac.Scripting Project Documentation

## Project Overview

The `Rac.Scripting` project provides comprehensive scripting capabilities for the Rac game engine, enabling dynamic game logic implementation through multiple scripting languages. The system supports both C# scripting for performance-critical scenarios and Lua scripting for rapid prototyping and designer-friendly game logic implementation.

### Key Design Principles

- **Multi-Language Support**: Unified scripting interface supporting both C# and Lua scripting environments
- **Hot-Reload Capability**: Dynamic script reloading for rapid iteration during development
- **Sandbox Security**: Isolated script execution preventing unauthorized system access
- **Performance Optimization**: Efficient script execution with compilation caching and optimized runtime
- **Designer-Friendly Workflow**: Accessible scripting patterns for non-programmer content creators

### Performance Characteristics and Optimization Goals

The scripting system prioritizes execution performance for frequent script calls while maintaining flexibility for rapid development iteration. C# scripting provides near-native performance through compilation, while Lua scripting offers fast interpretation with minimal startup overhead.

## Architecture Overview

The scripting system follows a plugin architecture where different scripting languages are implemented through a common interface, enabling consistent API patterns while leveraging the strengths of different scripting environments. Script engines manage compilation, execution, and resource management independently.

### Core Architectural Decisions

- **Interface-Driven Architecture**: Common scripting interface enabling multiple scripting language backends
- **Compilation Strategy**: C# script compilation for performance with Lua interpretation for flexibility
- **Sandbox Implementation**: Isolated script execution environments preventing system interference
- **Hot-Reload Infrastructure**: Dynamic script recompilation and state migration for development iteration
- **Engine API Exposure**: Controlled access to engine functionality through scripting bindings

### Integration with ECS System and Other Engine Components

Scripting integrates with ECS through script components that contain executable logic and state data. Scripts can query entities, modify components, and respond to game events while maintaining isolation from critical engine systems. The component-based approach enables script attachment to specific entities.

## Namespace Organization

### Rac.Scripting

The primary namespace contains scripting engine interfaces and implementations for different scripting language environments.

**IScriptEngine**: Interface defining scripting engine contracts including script compilation, execution, and lifecycle management. Provides foundation for different scripting language implementations while enabling testing scenarios and custom scripting systems.

**CSharpScriptEngine**: C# scripting implementation providing high-performance script execution through dynamic compilation. Supports full C# language features with engine API access while maintaining sandbox security and hot-reload capabilities for development scenarios.

**LuaScriptEngine**: Lua scripting implementation offering flexible script execution with minimal setup overhead. Provides designer-friendly scripting environment with simplified syntax while maintaining performance characteristics suitable for gameplay logic and rapid prototyping.

## Core Concepts and Workflows

### Script Compilation and Execution Pipeline

The scripting workflow encompasses script loading, compilation (for C#) or parsing (for Lua), execution context creation, and runtime execution. The system handles error management, debugging support, and performance monitoring throughout the script lifecycle.

### Hot-Reload Development Workflow

Hot-reload functionality enables dynamic script recompilation during development without application restart. The system manages script state preservation, dependency tracking, and execution context migration to maintain development iteration speed.

### Engine API Binding System

Script engines expose controlled access to engine functionality through carefully designed API bindings. Scripts can interact with ECS, rendering, audio, and input systems while maintaining security boundaries and preventing unauthorized system access.

### Integration with ECS

Script components store executable code references and execution state while scripting systems process entities to execute attached scripts. The component-based approach enables per-entity script logic with shared scripting resources and execution contexts.

## Integration Points

### Dependencies on Other Engine Projects

- **Rac.Core**: Configuration management for scripting settings and logging infrastructure
- **Rac.ECS**: Component-based script storage and entity scripting management
- **Microsoft.CodeAnalysis**: C# script compilation and analysis capabilities
- **Lua Runtime**: Lua script interpretation and execution environment

### How Other Systems Interact with Rac.Scripting

Game logic utilizes scripts for AI behavior, gameplay mechanics, and event handling. Asset systems may load script files as game content, while input systems can trigger script execution based on player actions. Debugging tools integrate with scripting for development support.

### Data Consumed from ECS

Script components contain executable code references and execution parameters. Entity queries enable scripts to interact with game state while component modifications allow scripts to affect game behavior. Event systems coordinate script execution with game events.

## Usage Patterns

### Common Setup Patterns

Scripting system initialization involves script engine configuration, API binding setup, and script loading infrastructure. The system supports both precompiled scripts for performance and dynamic script loading for development flexibility.

### How to Use the Project for Entities from ECS

Entities receive script components containing executable logic and execution parameters. Scripting systems process entities with script components, executing attached scripts with appropriate context and handling script lifecycle management.

### Resource Loading and Management Workflows

Script resources include source code files, compiled assemblies, and script configuration loaded through the asset system. The system manages script memory usage through compilation caching and execution context pooling.

### Performance Optimization Patterns

Optimal scripting performance requires strategic compilation caching, efficient API binding design, and appropriate script granularity. Performance-critical scenarios benefit from C# scripting while rapid iteration scenarios utilize Lua scripting flexibility.

## Extension Points

### How to Add New Scripting Features

New scripting capabilities can be added through custom script engines, specialized API bindings, or enhanced debugging tools. The interface-driven architecture enables integration of additional scripting languages while maintaining consistent development patterns.

### Extensibility Points

The script engine interface supports alternative scripting language implementations while API binding systems can be extended with domain-specific functionality. Script components can be enhanced with execution parameters and the system supports integration with external development tools.

### Future Enhancement Opportunities

The scripting architecture supports advanced features including visual scripting systems, collaborative script editing, performance profiling integration, and distributed script execution. Integration with version control systems can provide script asset management while debugging tools can enhance development workflows.