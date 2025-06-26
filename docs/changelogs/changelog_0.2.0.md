---
title: "Changelog 0.2.0"
description: "Core Engine Architecture - Major architectural refactoring with EngineFacade and modular project structure."
version: "0.2.0"
last_updated: "2025-05-15"
author: "Tomas Forsman"
---

# Changelog 0.2.0 - Core Engine Architecture

Released: May 6-15, 2025

This release represents a major architectural milestone, establishing the core engine structure that will serve as the foundation for all future development. The introduction of EngineFacade and proper project organization transforms RACEngine from a simple game prototype into a proper game engine.

## Added Features

### EngineFacade Integration System
* **Unified Interface**: Single entry point for ECS and rendering integration
* **Service Management**: Centralized service location and management
* **Lifecycle Management**: Proper initialization and cleanup of engine subsystems
* **Dependency Resolution**: Automatic dependency injection and service wiring

### Project Architecture
* **Rac.Engine Project**: Central engine project with unified entry point
* **Modular Structure**: Clear separation between different engine modules
* **ProjectTools**: Development utilities and component generation tools
* **Service Interfaces**: Well-defined interfaces for game object management and serialization

### Development Tools
* **Component Generation**: Automated tools for generating boilerplate component code
* **Main Window Application**: Initial application structure for development tools
* **Build System Integration**: Seamless integration with build and development workflows

## Major Refactoring

### Namespace Organization
* **Rac.* Pattern**: Consistent namespace pattern across all engine modules (Rac.Engine, Rac.Rendering, etc.)
* **Root Namespace**: Proper root namespace configuration for all projects
* **Using Directives**: Organized using directives for better code clarity
* **Consistency**: Uniform namespace usage across the entire codebase

### Architecture Improvements
* **GameEngine → Engine**: Renamed core engine class for clarity and consistency
* **Rendering Namespaces**: Refactored to Rac.Rendering for consistency with overall architecture
* **Component Separation**: Moved game-specific components out of engine core into sample applications
* **System Boundaries**: Clear boundaries between engine and application code

## Technical Details

### EngineFacade Architecture
The EngineFacade serves as the primary integration point:
- **ECS Integration**: Seamless integration between Entity-Component-System and rendering
- **Resource Management**: Centralized management of engine resources and services
- **Configuration**: Unified configuration and setup for engine subsystems
- **Extension Points**: Clear extension points for adding new engine functionality

### Project Structure
```
src/
├── Rac.Engine/           # Core engine with EngineFacade
├── Rac.Rendering/        # Rendering subsystem
├── Rac.ECS/             # Entity-Component-System
├── Rac.ProjectTools/    # Development utilities
└── Rac.*/              # Other engine modules
```

### Service Architecture
- **Interface-Based Design**: All services implement well-defined interfaces
- **Dependency Injection**: Services are injected where needed rather than directly instantiated
- **Lifecycle Management**: Proper initialization, update, and disposal patterns
- **Testing Support**: Service architecture supports easy mocking and testing

## Improvements

### Code Organization
* **Separation of Concerns**: Clear separation between engine and application code
* **Maintainability**: Improved code organization for easier maintenance and extension
* **Documentation**: Better code documentation and architectural clarity
* **Extensibility**: Framework for adding new engine features and capabilities

### Sample Application Updates
* **BoidSample Refactoring**: Updated to use EngineFacade for proper engine integration
* **Integration Examples**: Clear examples of how to integrate with the new architecture
* **Performance**: Improved performance through better resource management
* **Code Quality**: Higher code quality through architectural improvements

## Educational Impact

This release introduces crucial software architecture concepts:
- **Facade Pattern**: EngineFacade demonstrates proper use of facade pattern in complex systems
- **Dependency Injection**: Shows how to structure systems for testability and maintainability
- **Service-Oriented Architecture**: Illustrates service-based design in game engines
- **Modular Design**: Demonstrates how to organize complex systems into manageable modules

### Learning Opportunities
Developers can learn:
- Game engine architecture and design patterns
- Service-oriented design and dependency injection
- Proper namespace organization and project structure
- Integration patterns between different engine subsystems

## Breaking Changes

### API Changes
* **Namespace Updates**: All code using engine classes needs namespace updates
* **Integration Pattern**: Applications must use EngineFacade instead of direct component access
* **Project References**: Updated project reference structure requires build configuration updates

### Migration Path
Existing applications need to:
1. Update namespace references to new Rac.* pattern
2. Refactor to use EngineFacade for engine integration
3. Update project references to match new structure
4. Move game-specific components out of engine core

## Performance Improvements

### Resource Management
- More efficient resource allocation and deallocation
- Better memory management through centralized service management
- Reduced overhead through proper architectural patterns
- Improved startup and shutdown performance

### System Integration
- Faster communication between ECS and rendering systems
- Reduced coupling leading to better performance optimization opportunities
- More efficient service location and dependency resolution

## Future Foundation

This architectural foundation enables future enhancements:
- Plugin and extension systems
- Advanced service management and configuration
- Multi-threaded engine architecture
- Advanced debugging and profiling tools
- Cross-platform deployment systems

## Commits Included

- `6454377`: Add comprehensive README.md with project overview, features, structure, setup, sample game, contributing guidelines, license, and contact information
- `7a673c2`: Add Update method for custom system order and implement component removal functionality
- `9c10d16`: Add using directives for Rac.ECS.Components and update namespaces for consistency
- `1f9817a`: Add initial application structure with main window and component generation functionality for Rac.ProjectTools
- `ebd1a7a`: Moved game specific components out of the engine and into the sample game
- `855ed72`: Refactor BoidSample to use EngineFacade and improve code structure
- `564fbb8`: Refactor BoidSample to use EngineFacade for ECS and rendering integration
- `f8a94a4`: Add EngineFacade class to integrate ECS and rendering, and update BoidSample and project references
- `5af7b34`: Update namespaces and set root namespaces for Rac.Engine and Rac.Rendering projects
- `48f9389`: Rename GameEngine to Engine and update references for consistency
- `df14e70`: Refactor rendering namespaces to Rac.Rendering for consistency
- `938fc02`: Add Rac.Engine project with entry point and project references
- `e009106`: Add initial interfaces for game object management and serialization