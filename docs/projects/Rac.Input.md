# Rac.Input Project Documentation

## Project Overview

The `Rac.Input` project provides comprehensive input management capabilities for the Rac game engine, handling keyboard, mouse, and other input devices through a unified interface. The system emphasizes responsive input handling with both polling and event-driven patterns to support different game interaction models while maintaining consistent behavior across platforms.

### Key Design Principles

- **Dual Input Models**: Support for both real-time polling and event-driven input handling patterns
- **Platform Independence**: Consistent input API across Windows, Linux, and macOS through Silk.NET abstraction
- **State Management**: Comprehensive input state tracking with frame-accurate input detection
- **Flexible Mapping**: Configurable input mapping system for customizable controls and accessibility
- **Performance Optimization**: Efficient input processing with minimal allocation in frame-critical paths

### Performance Characteristics and Optimization Goals

The input system prioritizes low-latency input response for real-time interactive applications while maintaining efficient memory usage through optimized state tracking. Event processing is designed for frame-rate independent operation, ensuring consistent input handling across different hardware configurations.

## Architecture Overview

The input system follows a service-oriented architecture with Silk.NET providing cross-platform input abstraction. The design separates input detection from input interpretation, enabling flexible control schemes while maintaining consistent underlying input handling across different game types and interaction patterns.

### Core Architectural Decisions

- **Service Interface Pattern**: Clean abstraction enabling testing and alternative input implementations
- **State Separation**: Distinct handling of continuous input states versus discrete input events
- **Event-Driven Architecture**: Asynchronous input event processing for responsive user interaction
- **Mapping Layer**: Configurable input mapping system separating physical inputs from logical actions
- **Frame-Accurate Timing**: Input state updates synchronized with game loop for consistent behavior

### Integration with ECS System and Other Engine Components

Input integrates with ECS through input components that store action states and control mappings. The input system can query ECS for input-enabled entities while providing input data for entity behavior systems. Integration with the camera system enables proper coordinate transformation for mouse input in world space.

## Namespace Organization

### Rac.Input.Service

Contains the primary input service interfaces and implementations for cross-platform input management.

**IInputService**: Defines the complete input service contract including keyboard state polling, mouse event handling, and lifecycle management. Provides both real-time state queries (KeyboardKeyState) and discrete event notifications (OnLeftClick, OnMouseScroll) to support different input handling patterns.

**SilkInputService**: Silk.NET-based implementation providing comprehensive input functionality across Windows, Linux, and macOS. Manages input device initialization, event processing, and state synchronization while maintaining thread-safe operation for concurrent input handling.

### Rac.Input.State

Manages input state representation and tracking for different input device types.

**KeyboardKeyState**: Manages keyboard input state tracking including both continuous key-down states and discrete key events (pressed/released). Provides efficient key state queries through hash-set-based storage while maintaining frame-accurate input detection for responsive gameplay.

**KeyEvent**: Enumeration defining discrete keyboard events including Pressed and Released states. Enables event-driven input handling patterns for menu navigation, text input, and other interaction scenarios requiring precise input timing.

### Rac.Input.Mapping

Provides configurable input mapping capabilities for customizable controls and accessibility support.

**InputMappings**: Interface for input mapping functionality that translates physical input events into logical game actions. Supports customizable control schemes, accessibility options, and context-sensitive input behaviors while maintaining separation between input detection and action interpretation.

## Core Concepts and Workflows

### Input Processing Pipeline

The input workflow encompasses device initialization, continuous state polling, event processing, and action mapping. The system handles raw input from Silk.NET through state management layers before delivering processed input data to game systems through both polling interfaces and event callbacks.

### State Management Patterns

Input state management distinguishes between continuous states (key currently pressed) and discrete events (key just pressed). The system maintains frame-accurate state tracking while providing both immediate polling access and historical event information for different input handling requirements.

### Event-Driven Input Handling

Event-based input processing enables responsive user interface interactions through callback mechanisms for mouse clicks, scroll events, and key presses. The event system maintains proper timing relationships and provides coordinate transformation for spatial input operations.

### Integration with ECS

Input components attached to entities store control mappings and action states while the input system updates these components based on player input. The system supports multiple input contexts, enabling different entities to respond to the same physical inputs in context-appropriate ways.

## Integration Points

### Dependencies on Other Engine Projects

- **Rac.Core**: Platform abstraction and configuration management for input device initialization
- **Rac.ECS**: Component-based input handling and entity input state management
- **Silk.NET.Input**: Cross-platform input device abstraction and event processing
- **Silk.NET.Windowing**: Window management integration for input focus and coordinate systems

### How Other Systems Interact with Rac.Input

Game logic systems query the input service for current input states and register for input events. The UI system consumes mouse and keyboard events for interface interaction, while the camera system uses input data for view control. Physics systems may use input for direct object manipulation.

### Data Consumed from ECS

Input components provide control mapping configuration and action state storage. Transform components enable world-space input coordinate transformation, while hierarchy relationships support context-sensitive input handling for nested UI elements and game objects.

## Usage Patterns

### Common Setup Patterns

Input service initialization involves window association, device enumeration, and event handler registration. The system supports both immediate input polling for real-time gameplay and event-driven input for user interface interactions and menu systems.

### How to Use the Project for Entities from ECS

Entities can have input components containing control mappings and action states. The input system updates entity input components based on configured mappings, enabling different entities to respond to input according to their specific control schemes and interaction requirements.

### Resource Loading and Management Workflows

Input management operates through device initialization and event handler setup during engine startup. The system maintains input device state throughout application lifecycle while providing proper cleanup and resource disposal during shutdown.

### Performance Optimization Patterns

Optimal input performance requires efficient state tracking through optimized data structures, minimal allocation during event processing, and strategic input mapping configuration. Frame-rate independent input timing ensures consistent behavior across different hardware configurations.

## Extension Points

### How to Add New Input Features

New input capabilities can be added through interface extension or specialized input components. The mapping system supports custom action definitions while the service interface can be extended for additional input device types including game controllers, touch input, and specialized hardware.

### Extensibility Points

The input mapping system enables custom control schemes and accessibility configurations. Input services can be extended with additional device support while maintaining compatibility with existing input handling patterns and game logic systems.

### Future Enhancement Opportunities

The input architecture supports advanced features including gesture recognition, haptic feedback integration, input prediction for network games, and accessibility enhancements. Integration with analytics systems can provide input behavior insights for game design optimization.