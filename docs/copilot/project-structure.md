# RACEngine Project Structure - AI Reference

This document provides a compact, AI-friendly overview of RACEngine's project structure, designed to give context about the different components and their purposes.

## Core Projects (`src/`)

### Engine Foundation
- **`Rac.Core`** - Mathematics, data structures, memory management, and fundamental utilities
- **`Rac.Engine`** - Main engine orchestration, lifecycle management, and service coordination  
- **`Rac.GameEngine`** - Higher-level game-specific abstractions and utilities built on top of the core engine

### Architecture Systems
- **`Rac.ECS`** - Entity-Component-System implementation with World management and component storage
- **`Rac.Assets`** - Asset loading, management, and pipeline for textures, models, audio, and other resources

### Rendering and Graphics
- **`Rac.Rendering`** - OpenGL-based rendering pipeline with shader management, vertex buffers, and 2D/3D rendering
- **`Rac.Animation`** - Animation systems for sprites, skeletal animation, and tweening

### Input and Audio  
- **`Rac.Input`** - Cross-platform input handling for keyboard, mouse, and gamepad
- **`Rac.Audio`** - 3D spatial audio system using OpenAL with DSP effects and sound management

### Physics and AI
- **`Rac.Physics`** - 2D/3D physics simulation with collision detection and response
- **`Rac.AI`** - Artificial intelligence components including pathfinding, behavior trees, and steering behaviors

### Scripting and Tools
- **`Rac.Scripting`** - Scripting language integration for game logic
- **`Rac.Tools`** - Development tools and utilities for content creation
- **`Rac.ProjectTools`** - Project management, build system integration, and development workflow support

### Extended Features
- **`Rac.Networking`** - Multiplayer networking with client-server and peer-to-peer support

## Sample Projects (`samples/`)

### Demonstrations
- **`SampleGame`** - Basic game demonstrating core engine features and patterns
- **`TicTacToe`** - Simple complete game showing UI, game state management, and basic ECS usage

## Test Projects (`tests/`)

### Unit Testing
- **`Rac.Core.Tests`** - Tests for mathematical functions, data structures, and core utilities
- **`Rac.ECS.Tests`** - Entity-Component-System implementation verification
- **`Rac.Engine.Tests`** - Engine lifecycle and service coordination testing
- **`Rac.Audio.Tests`** - Audio system functionality and 3D audio positioning tests
- **`Rac.Rendering.Tests`** - Graphics pipeline, shader compilation, and rendering correctness tests
- **`Rac.ProjectTools.Tests`** - Development tools and project management functionality tests

## Important Configuration Files

### Build System
- **`RACEngine.sln`** - Main Visual Studio solution file containing all projects
- **`global.json`** - .NET SDK version specification for consistent builds
- **`.editorconfig`** - Code formatting and style configuration

### Project Structure
- **`LICENSE.md`** - MIT license terms
- **`README.md`** - Project overview and getting started information
- **`.gitignore`** - Git exclusion patterns for build artifacts and dependencies

### Documentation Hub
- **`docs/`** - Comprehensive documentation including architecture guides, tutorials, and API references

## Key Architecture Patterns

### Namespace Organization
All projects follow `Rac.{ModuleName}` namespace pattern for consistent organization.

### Dependency Structure
- `Rac.Core` is the foundation that all other projects depend on
- `Rac.ECS` provides the architectural foundation for game objects
- `Rac.Engine` orchestrates all systems and services
- `Rac.GameEngine` provides game-specific convenience APIs
- Feature modules (`Audio`, `Rendering`, `Physics`, etc.) are largely independent of each other

### Service Architecture
Each major system provides both low-level and high-level APIs:
- Interface-based services for dependency injection
- Null object implementations for optional subsystems
- Builder patterns for complex configuration
- Both immediate mode and deferred execution support

### Educational Focus
All code includes comprehensive documentation with:
- Academic references to established algorithms
- Performance optimization explanations
- Coordinate system and mathematical concept clarification
- Design pattern usage documentation

This structure supports both learning game engine development concepts and building production-quality games.