Rac.Core Project Documentation
Project Overview
The Rac.Core project implements the foundational infrastructure and services for the RACEngine. This implementation provides essential utilities including configuration management, window abstraction, logging services, dependency injection coordination, and mathematical extensions. The project serves as the backbone for all other engine components, establishing architectural patterns and providing cross-cutting concerns through well-designed abstractions.
Key Design Principles

Immutable Configuration: Thread-safe configuration objects using immutable design patterns
Fluent Builder APIs: Chainable method calls for readable and maintainable object construction
Dependency Injection Integration: Microsoft Extensions DI support for modular service architecture
Cross-Platform Abstraction: Platform-agnostic windowing and input through Silk.NET integration
Structured Configuration: INI file-based configuration with strongly-typed binding
Extension Method Patterns: Mathematical and utility extensions following .NET conventions

Performance Characteristics and Optimization Goals
The core infrastructure achieves optimal performance through several key strategies: immutable objects eliminating thread synchronization overhead, cached configuration binding reducing runtime parsing, efficient vector operations using optimized mathematical algorithms, and lazy initialization patterns minimizing startup overhead. The design prioritizes developer productivity while maintaining runtime efficiency.
Architecture Overview
The Rac.Core system implements foundational architecture patterns that establish the design principles for the entire engine. The project coordinates configuration management, service registration, window lifecycle, and utility services through clean abstractions that enable testability and extensibility.
Core Architectural Decisions

Immutable Object Pattern: Configuration objects are immutable after construction, ensuring thread safety and predictable behavior
Builder Pattern Implementation: Fluent APIs for complex object construction with validation and default handling
Service Registration Abstraction: Microsoft Extensions DI integration enabling modular service composition
Platform Abstraction Layer: Cross-platform windowing and input through Silk.NET with engine-specific abstractions
Configuration-Driven Architecture: INI file configuration with automatic binding to strongly-typed objects
Extension Method Strategy: Mathematical and utility extensions providing domain-specific functionality

Integration with Engine Infrastructure
The core infrastructure provides foundational services consumed by all other engine projects. Configuration management enables runtime customization without code changes. Window management abstracts platform-specific windowing systems while providing event-driven resize handling. Logging services offer structured debugging and monitoring capabilities. The dependency injection integration enables modular service composition and testing strategies.
Namespace Organization
Rac.Core.Builder
Contains fluent builder implementations for complex object construction with immutable results.
EngineBuilder: Immutable builder for constructing engine configurations with fluent API. Each method returns a new instance maintaining immutability while providing chainable configuration methods. Supports feature toggles for graphics, audio, input, ECS, and networking subsystems. Includes comprehensive service registration methods supporting different lifetimes (singleton, scoped, transient) and factory patterns.
Rac.Core.Configuration
Provides immutable configuration management with profile-based defaults and strongly-typed access.
EngineProfile: Enumeration defining engine profile types for different application scenarios. FullGame profile enables all features for complete game applications. Headless profile supports server applications with graphics and input disabled. Custom profile allows manual feature configuration for specialized use cases.
ImmutableEngineConfig: Immutable configuration class providing thread-safe engine settings with builder pattern support. Defines feature flags for major subsystems including graphics, audio, input, ECS, and networking. Includes service collection for dependency injection configuration. Supports profile-based defaults with selective property modification through 'With' pattern methods.
Rac.Core.Extension
Mathematical and utility extensions enhancing base library functionality.
Vector2DExtensions: Extension methods for Vector2D<float> adding common vector mathematics operations. Implements vector normalization creating unit vectors for direction calculations. Provides length clamping for velocity limiting and force constraints. Includes proper handling of edge cases such as zero-length vectors and negative maximum values.
Rac.Core.Logger
Structured logging infrastructure with multiple severity levels and implementation flexibility.
ILogger: Core logging interface defining the contract for logging services. Provides methods for debug, informational, warning, and error message logging with appropriate severity classification.
SerilogLogger: Concrete logger implementation providing structured logging capabilities while minimizing console output spam. Debug messages only appear in debug builds, informational messages use debug output, while warnings and errors use console output for visibility.
Rac.Core.Manager
Window management and configuration services providing cross-platform application lifecycle support.
ConfigManager: Manages application configuration loading from INI files with strongly-typed binding. Loads configuration from config.ini in the application directory with graceful handling of missing files. Provides automatic binding to WindowSettings for window-specific configuration.
WindowSettings: Strongly-typed configuration class for window properties loaded from INI files. Supports nullable properties distinguishing between explicit settings and defaults. Includes title, size, and VSync configuration with appropriate parsing requirements.
IWindowManager: Window management abstraction providing cross-platform windowing capabilities. Defines size, aspect ratio, and native window access. Includes resize event handling for responsive layout management.
WindowBuilder: Fluent builder pattern for constructing window configurations with chainable method calls. Supports title, size, VSync, window state, and resizability configuration with validation and reasonable defaults.
WindowManager: Concrete window management implementation using Silk.NET windowing system. Provides window creation, resize handling, and state management with automatic event propagation for responsive layouts.
Placeholder Namespaces
Rac.Core.Scheduler: Reserved for task scheduling implementation. Currently contains ITaskScheduler interface and TaskScheduler class placeholders for future frame-based and time-based task execution management.
Core Concepts and Workflows
Immutable Configuration Management
The configuration system implements immutable object patterns ensuring thread safety and predictable behavior across all engine components. Configuration loading occurs once during application startup with automatic binding to strongly-typed objects. The 'With' pattern enables selective property modification while maintaining immutability, supporting configuration composition and testing scenarios.
Fluent Builder Patterns
Object construction utilizes fluent builder APIs providing readable and maintainable configuration workflows. EngineBuilder enables service registration and feature configuration through method chaining. WindowBuilder provides window property configuration with validation and sensible defaults. All builders maintain immutability while supporting complex object construction scenarios.
Service Registration Architecture
The engine integrates Microsoft Extensions Dependency Injection enabling modular service composition and testing strategies. Service registration supports multiple lifetimes including singleton, scoped, and transient patterns. Factory method registration enables complex service initialization scenarios while maintaining dependency injection benefits.
Cross-Platform Window Management
Window management abstracts platform-specific windowing systems through Silk.NET integration while providing engine-specific abstractions. Window creation supports comprehensive configuration through builder patterns. Resize events enable responsive layout management across different display configurations.
Integration Points
Dependencies on External Libraries

Microsoft.Extensions.Configuration: Configuration management with INI file support and strongly-typed binding
Microsoft.Extensions.DependencyInjection: Service registration and dependency injection infrastructure
Silk.NET.Windowing: Cross-platform windowing system abstraction and implementation
Silk.NET.Input: Input device abstraction for keyboard, mouse, and gamepad support

Service Provider Integration
The core infrastructure integrates with Microsoft Extensions DI enabling modular service composition across all engine projects. EngineBuilder provides service registration methods supporting different lifetimes and factory patterns. The configuration system automatically registers essential services while enabling custom service extension through builder patterns.
Configuration System Integration
Configuration management provides runtime customization capabilities consumed by all engine subsystems. INI file format enables user-friendly configuration editing without recompilation. Strongly-typed binding ensures type safety while enabling optional configuration parameters with graceful defaults.
Usage Patterns
Engine Configuration and Initialization
Standard engine setup involves creating an EngineBuilder with appropriate profile and configuring required services and features through fluent API methods.
csharp// Full game configuration with custom services
var config = EngineBuilder
.Create(EngineProfile.FullGame)
.WithNetworking(true)
.AddSingleton<IGameService, GameService>()
.AddTransient<IAudioManager, AudioManager>()
.Build();

var serviceProvider = config.Services.BuildServiceProvider();
Window Management and Configuration
Window creation utilizes the WindowBuilder pattern for comprehensive configuration with automatic platform abstraction through the WindowManager.
csharp// Configure and create application window
var windowManager = new WindowManager();
var window = WindowBuilder
.Configure(windowManager)
.WithTitle("My Game Application")
.WithSize(1920, 1080)
.WithVSync(true)
.WithState(WindowState.Maximized)
.Create();

// Handle resize events for responsive layouts
windowManager.OnResize += (newSize) => {
UpdateViewport(newSize.X, newSize.Y);
};
Configuration File Management
Configuration loading supports INI files with automatic binding to strongly-typed objects enabling runtime customization without code changes.
csharp// config.ini file content:
// [window]
// Title=My Game
// Size=1920,1080
// VSync=true

var configManager = new ConfigManager();
var windowTitle = configManager.Window.Title;    // "My Game"
var windowSize = configManager.Window.Size;      // "1920,1080"
var vSyncEnabled = configManager.Window.VSync;   // true
Mathematical Extensions
Vector operations utilize extension methods providing common mathematical operations for game development scenarios.
csharp// Vector normalization and length clamping
var velocity = new Vector2D<float>(6.0f, 8.0f);     // Length = 10
var direction = velocity.Normalize();                 // Result: (0.6, 0.8)
var limitedVelocity = velocity.ClampLength(5.0f);    // Result: (3.0, 4.0)

// Use in movement systems
var playerInput = GetPlayerInput().Normalize();
var movement = playerInput * movementSpeed * deltaTime;
Extension Points
Configuration System Extensions
The configuration architecture supports extension through additional configuration sections and strongly-typed binding classes. New configuration categories can be added by extending ConfigManager and adding corresponding settings classes. Custom configuration sources can be integrated through Microsoft Extensions Configuration providers.
Configuration validation and transformation can be implemented through custom binding logic and validation attributes. Runtime configuration updates can be supported through file watching and configuration reload patterns.
Service Registration Extensions
The service registration system supports extension through custom service registration methods and lifetime management patterns. Complex service initialization scenarios can be implemented through factory methods and service composition patterns.
Custom service scopes and specialized lifetime management can be implemented through Microsoft Extensions DI extension patterns. Service decoration and interception can be added through proxy patterns and aspect-oriented programming techniques.
Window Management Extensions
Window management supports extension through additional configuration options and platform-specific features. Custom window decorations and specialized display configurations can be implemented through Silk.NET extension points.
Advanced input handling and window event management can be extended through custom event handlers and input device abstractions. Multi-window applications can be supported through window manager extension patterns.
Future Enhancement Opportunities
The placeholder namespaces indicate planned expansions including comprehensive task scheduling for frame-based and time-based execution management. Performance monitoring and profiling capabilities can be integrated through the logging and configuration infrastructure.
Advanced configuration features may include hot-reload capabilities, configuration validation, and distributed configuration management. Service registration enhancements could include automatic service discovery, configuration-driven service registration, and advanced dependency resolution strategies.
The immutable architecture patterns provide a solid foundation for these enhancements while maintaining thread safety and predictable behavior across all engine components.