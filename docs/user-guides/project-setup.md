---
title: "Project Setup Guide"
description: "Creating new game projects with RACEngine - step-by-step setup and configuration"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["setup", "project", "configuration", "new-project"]
---

# Project Setup Guide

## Overview

This guide walks you through creating a new game project using RACEngine. After completing this guide, you'll have a fully configured project ready for game development, complete with proper project structure, dependencies, and basic game framework.

## Prerequisites

- RACEngine successfully installed (see [Installation Guide](installation-guide.md))
- Basic understanding of C# and .NET project structure
- Familiarity with your chosen IDE (Visual Studio, VS Code, or Rider)
- Completed [Getting Started Tutorial](../educational-material/getting-started-tutorial.md) recommended

## Project Creation Methods

### Method 1: Manual Project Creation (Recommended for Learning)

This method teaches you the project structure and gives you full control over configuration.

#### Step 1: Create Project Directory Structure

```bash
# Create your game project directory
mkdir MyGame
cd MyGame

# Create standard directory structure
mkdir src
mkdir assets
mkdir assets/textures
mkdir assets/audio
mkdir assets/shaders
mkdir docs
mkdir scripts
```

#### Step 2: Create Project File

Create `MyGame.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>MyGame</AssemblyName>
    <RootNamespace>MyGame</RootNamespace>
  </PropertyGroup>

  <!-- RACEngine Dependencies -->
  <ItemGroup>
    <ProjectReference Include="..\RACEngine\src\Rac.Engine\Rac.Engine.csproj" />
    <ProjectReference Include="..\RACEngine\src\Rac.ECS\Rac.ECS.csproj" />
    <ProjectReference Include="..\RACEngine\src\Rac.Rendering\Rac.Rendering.csproj" />
    <ProjectReference Include="..\RACEngine\src\Rac.Audio\Rac.Audio.csproj" />
    <ProjectReference Include="..\RACEngine\src\Rac.Physics\Rac.Physics.csproj" />
    <ProjectReference Include="..\RACEngine\src\Rac.Core\Rac.Core.csproj" />
  </ItemGroup>

  <!-- Asset Management -->
  <ItemGroup>
    <None Include="assets/**/*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <!-- Development Tools (Optional) -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
  </ItemGroup>

</Project>
```

#### Step 3: Create Basic Game Structure

Create `src/Program.cs`:

```csharp
using MyGame.Core;
using System;

namespace MyGame;

/// <summary>
/// Main entry point for MyGame
/// Educational note: Clean separation between program entry and game logic
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🎮 Welcome to MyGame!");
        Console.WriteLine("Built with RACEngine - Educational Game Engine");
        Console.WriteLine();

        try
        {
            var game = new GameApplication();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Critical Error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
```

Create `src/Core/GameApplication.cs`:

```csharp
using Rac.Engine;
using Rac.ECS;
using Rac.Core.Math;
using Rac.Rendering;
using MyGame.Systems;
using MyGame.Components;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyGame.Core;

/// <summary>
/// Main game application class
/// Educational note: Demonstrates proper game engine integration and lifecycle management
/// </summary>
public class GameApplication : GameWindow
{
    private World _world;
    private List<ISystem> _systems;
    private bool _isInitialized = false;

    public GameApplication() : base(
        GameWindowSettings.Default,
        new NativeWindowSettings()
        {
            Size = new OpenTK.Mathematics.Vector2i(1024, 768),
            Title = "MyGame - Powered by RACEngine",
            WindowBorder = WindowBorder.Fixed,
            StartVisible = false  // Show after initialization
        })
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        Console.WriteLine("🔧 Initializing game systems...");
        
        try
        {
            InitializeEngine();
            InitializeGame();
            
            _isInitialized = true;
            IsVisible = true;  // Show window after successful initialization
            
            Console.WriteLine("✅ Game initialization complete!");
            Console.WriteLine("🎮 Game is running. Close window to exit.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Initialization failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Initialize core engine systems
    /// Educational note: Proper initialization order prevents dependency issues
    /// </summary>
    private void InitializeEngine()
    {
        // Initialize ECS World
        _world = new World();
        
        // Initialize rendering system
        InitializeRenderer();
        
        // Initialize audio system (with fallback to null service)
        InitializeAudio();
        
        Console.WriteLine("   ✓ Engine systems initialized");
    }

    /// <summary>
    /// Initialize game-specific content and systems
    /// Educational note: Separate game logic from engine initialization
    /// </summary>
    private void InitializeGame()
    {
        // Create systems in dependency order
        _systems = new List<ISystem>
        {
            new InputSystem(KeyboardState),
            new GameLogicSystem(),
            new MovementSystem(),
            new RenderSystem()
        };

        // Create initial game entities
        CreateGameEntities();

        Console.WriteLine("   ✓ Game content initialized");
    }

    private void InitializeRenderer()
    {
        // Set clear color
        GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
        
        // Enable depth testing
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        
        // Configure viewport
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    private void InitializeAudio()
    {
        try
        {
            // Attempt to initialize audio system
            // Implementation depends on your audio service choice
            Console.WriteLine("   ✓ Audio system initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️  Audio initialization failed: {ex.Message}");
            Console.WriteLine("   ℹ️  Running in silent mode");
            // Continue without audio - use NullAudioService
        }
    }

    /// <summary>
    /// Create initial game entities
    /// Educational note: Demonstrates entity creation and component composition
    /// </summary>
    private void CreateGameEntities()
    {
        // ✅ MODERN: Create entity using fluent API for clean composition
        var testEntity = _world.CreateEntity()
            .WithName(_world, "TestEntity")
            .WithPosition(_world, new Vector2D<float>(Size.X / 2.0f, Size.Y / 2.0f))
            .WithComponent(_world, new VelocityComponent(new Vector2D<float>(50.0f, 30.0f))) // Slow movement
            .WithComponent(_world, new RenderComponent(
                new Vector2D<float>(50, 50),           // Size
                new Vector4D<float>(1.0f, 0.5f, 0.0f, 1.0f))); // Orange color

        Console.WriteLine($"   ✓ Created {1} game entities using fluent API");
        
        // 📚 TRADITIONAL COMPARISON: Old verbose approach
        /*
        // ⚠️ VERBOSE: Traditional approach - more lines, easier to miss components
        var testEntity = _world.CreateEntity();
        _world.SetComponent(testEntity, new PositionComponent(new Vector2D<float>(Size.X / 2.0f, Size.Y / 2.0f)));
        _world.SetComponent(testEntity, new VelocityComponent(new Vector2D<float>(50.0f, 30.0f)));
        _world.SetComponent(testEntity, new RenderComponent(new Vector2D<float>(50, 50), new Vector4D<float>(1.0f, 0.5f, 0.0f, 1.0f)));
        */
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!_isInitialized) return;

        base.OnUpdateFrame(args);
        
        float deltaTime = (float)args.Time;
        
        // Update all systems
        foreach (var system in _systems)
        {
            try
            {
                system.Update(_world, deltaTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ System update error: {ex.Message}");
                // Continue with other systems
            }
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        if (!_isInitialized) return;

        base.OnRenderFrame(args);
        
        // Clear the screen
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Rendering is handled by RenderSystem in UpdateFrame
        // This method just manages the OpenGL context
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        if (_isInitialized)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }

    protected override void OnUnload()
    {
        Console.WriteLine("🔧 Shutting down game systems...");
        
        // Cleanup systems
        _systems?.Clear();
        
        // Cleanup world
        _world = null;
        
        Console.WriteLine("✅ Game shutdown complete");
        
        base.OnUnload();
    }
}
```

#### Step 4: Create Game Components

Create `src/Components/GameComponents.cs`:

```csharp
using Rac.ECS;
using Rac.Core.Math;

namespace MyGame.Components;

/// <summary>
/// Position component for 2D games
/// Educational note: Simple 2D position suitable for most game types
/// </summary>
public readonly record struct PositionComponent(
    Vector2D<float> Position
) : IComponent;

/// <summary>
/// Velocity component for movement
/// Educational note: Separating velocity from position enables flexible movement systems
/// </summary>
public readonly record struct VelocityComponent(
    Vector2D<float> Velocity
) : IComponent;

/// <summary>
/// Basic rendering component
/// Educational note: Minimal data needed for simple sprite rendering
/// </summary>
public readonly record struct RenderComponent(
    Vector2D<float> Size,
    Vector4D<float> Color
) : IComponent;

/// <summary>
/// Player input component
/// Educational note: Tag component identifying player-controlled entities
/// </summary>
public readonly record struct PlayerInputComponent() : IComponent;

/// <summary>
/// Health component for gameplay mechanics
/// Educational note: Demonstrates how to add game-specific data
/// </summary>
public readonly record struct HealthComponent(
    int Current,
    int Maximum
) : IComponent;
```

#### Step 5: Create Game Systems

Create `src/Systems/MovementSystem.cs`:

```csharp
using Rac.ECS;
using MyGame.Components;

namespace MyGame.Systems;

/// <summary>
/// Handles entity movement based on velocity
/// Educational note: Pure system - no state, only logic
/// </summary>
public class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        foreach (var (entity, position, velocity) in world.Query<PositionComponent, VelocityComponent>())
        {
            var newPosition = position.Position + velocity.Velocity * deltaTime;
            world.SetComponent(entity, new PositionComponent(newPosition));
        }
    }
}
```

Create `src/Systems/InputSystem.cs`:

```csharp
using Rac.ECS;
using Rac.Core.Math;
using MyGame.Components;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MyGame.Systems;

/// <summary>
/// Handles player input and converts to game actions
/// Educational note: Input system translates user input to game state changes
/// </summary>
public class InputSystem : ISystem
{
    private readonly KeyboardState _keyboard;
    private const float PlayerSpeed = 200.0f;

    public InputSystem(KeyboardState keyboard)
    {
        _keyboard = keyboard;
    }

    public void Update(World world, float deltaTime)
    {
        foreach (var (entity, playerInput, velocity) in world.Query<PlayerInputComponent, VelocityComponent>())
        {
            var newVelocity = Vector2D<float>.Zero;

            if (_keyboard.IsKeyDown(Keys.W) || _keyboard.IsKeyDown(Keys.Up))
                newVelocity.Y += 1.0f;
            if (_keyboard.IsKeyDown(Keys.S) || _keyboard.IsKeyDown(Keys.Down))
                newVelocity.Y -= 1.0f;
            if (_keyboard.IsKeyDown(Keys.A) || _keyboard.IsKeyDown(Keys.Left))
                newVelocity.X -= 1.0f;
            if (_keyboard.IsKeyDown(Keys.D) || _keyboard.IsKeyDown(Keys.Right))
                newVelocity.X += 1.0f;

            // Normalize diagonal movement
            if (newVelocity.LengthSquared > 0)
            {
                newVelocity = newVelocity.Normalized() * PlayerSpeed;
            }

            world.SetComponent(entity, new VelocityComponent(newVelocity));
        }
    }
}
```

Create `src/Systems/GameLogicSystem.cs`:

```csharp
using Rac.ECS;
using MyGame.Components;

namespace MyGame.Systems;

/// <summary>
/// Handles game-specific logic and rules
/// Educational note: Centralized location for game rules and mechanics
/// </summary>
public class GameLogicSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        // Screen boundary checking
        CheckScreenBoundaries(world);
        
        // Game state updates
        UpdateGameState(world, deltaTime);
    }

    private void CheckScreenBoundaries(World world)
    {
        const float screenWidth = 1024.0f;
        const float screenHeight = 768.0f;
        const float margin = 25.0f; // Half of typical entity size

        foreach (var (entity, position, render) in world.Query<PositionComponent, RenderComponent>())
        {
            var pos = position.Position;
            var halfSize = render.Size * 0.5f;

            // Clamp to screen boundaries
            var clampedX = Math.Clamp(pos.X, halfSize.X + margin, screenWidth - halfSize.X - margin);
            var clampedY = Math.Clamp(pos.Y, halfSize.Y + margin, screenHeight - halfSize.Y - margin);

            if (clampedX != pos.X || clampedY != pos.Y)
            {
                world.SetComponent(entity, new PositionComponent(
                    new Vector2D<float>(clampedX, clampedY)));
            }
        }
    }

    private void UpdateGameState(World world, float deltaTime)
    {
        // Add game-specific logic here
        // Examples: score tracking, level progression, win/lose conditions
    }
}
```

Create `src/Systems/RenderSystem.cs`:

```csharp
using Rac.ECS;
using MyGame.Components;
using OpenTK.Graphics.OpenGL4;

namespace MyGame.Systems;

/// <summary>
/// Simple rendering system for 2D sprites
/// Educational note: Minimal renderer focusing on learning rather than performance
/// </summary>
public class RenderSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        // In a real implementation, this would:
        // 1. Collect all renderable entities
        // 2. Sort by depth/material
        // 3. Batch render calls
        // 4. Submit to GPU
        
        RenderEntities(world);
    }

    private void RenderEntities(World world)
    {
        // Simple immediate-mode rendering for educational purposes
        // Production code would use vertex buffers and batch rendering
        
        foreach (var (entity, position, render) in world.Query<PositionComponent, RenderComponent>())
        {
            RenderQuad(position.Position, render.Size, render.Color);
        }
    }

    private void RenderQuad(Vector2D<float> position, Vector2D<float> size, Vector4D<float> color)
    {
        // Convert screen coordinates to OpenGL normalized device coordinates
        float x = (position.X / 1024.0f) * 2.0f - 1.0f;
        float y = (position.Y / 768.0f) * 2.0f - 1.0f;
        float w = (size.X / 1024.0f);
        float h = (size.Y / 768.0f);

        // Simple quad rendering using immediate mode (educational only)
        GL.Color4(color.X, color.Y, color.Z, color.W);
        GL.Begin(PrimitiveType.Quads);
        {
            GL.Vertex2(x - w, y - h);
            GL.Vertex2(x + w, y - h);
            GL.Vertex2(x + w, y + h);
            GL.Vertex2(x - w, y + h);
        }
        GL.End();
    }
}
```

### Method 2: Using Project Templates (Advanced)

For experienced developers who want to quickly set up projects with standard configurations.

#### Step 1: Create Project Template

Create a template in `templates/game-project/`:

```bash
# From RACEngine root
mkdir -p templates/game-project
cd templates/game-project

# Copy template files
cp -r samples/SampleGame/* .

# Modify for template use
# Replace specific names with placeholders
# Add configuration options
```

#### Step 2: Install Template

```bash
# Install local template
dotnet new --install ./templates/game-project

# Use template
mkdir MyNewGame
cd MyNewGame
dotnet new racengine-game --name MyNewGame
```

## Project Configuration

### Configuration Files

Create `appsettings.json` for game configuration:

```json
{
  "GameSettings": {
    "WindowTitle": "MyGame",
    "WindowWidth": 1024,
    "WindowHeight": 768,
    "TargetFrameRate": 60,
    "EnableVSync": true
  },
  "Graphics": {
    "DefaultShaderMode": "Normal",
    "EnableBloom": false,
    "EnableAntiAliasing": true
  },
  "Audio": {
    "MasterVolume": 1.0,
    "EnableAudio": true,
    "AudioDevice": "default"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "MyGame": "Debug"
    }
  }
}
```

### Launch Settings

Create `.vscode/launch.json` for VS Code:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch MyGame",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/bin/Debug/net8.0/MyGame.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        },
        {
            "name": "Launch MyGame (Release)",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/bin/Release/net8.0/MyGame.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        }
    ]
}
```

Create `.vscode/tasks.json`:

```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/MyGame.csproj",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "group": "build",
            "presentation": {
                "reveal": "silent"
            },
            "problemMatcher": "$msCompile"
        }
    ]
}
```

## Asset Pipeline Setup

### Shader Assets

Create `assets/shaders/basic.vert`:

```glsl
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 uProjection;
uniform mat4 uView;
uniform mat4 uModel;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
    texCoord = aTexCoord;
}
```

Create `assets/shaders/basic.frag`:

```glsl
#version 330 core

in vec2 texCoord;
out vec4 FragColor;

uniform vec4 uColor;
uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, texCoord) * uColor;
}
```

### Audio Assets

```
assets/audio/
├── sfx/
│   ├── jump.ogg
│   ├── collect.ogg
│   └── hurt.ogg
├── music/
│   ├── menu.ogg
│   └── game.ogg
└── README.md
```

### Texture Assets

```
assets/textures/
├── sprites/
│   ├── player.png
│   ├── enemy.png
│   └── items/
│       ├── coin.png
│       └── powerup.png
├── ui/
│   ├── button.png
│   └── panel.png
└── README.md
```

## Testing Your Setup

### Build and Run

```bash
# Build the project
dotnet build

# Run in debug mode
dotnet run

# Run in release mode
dotnet run --configuration Release
```

### Verify Installation

Your game should:
1. ✅ Open a window with the specified title
2. ✅ Display a colored rectangle that can be moved with WASD/arrow keys
3. ✅ Show console output indicating successful initialization
4. ✅ Close cleanly when the window is closed

### Expected Output

```
🎮 Welcome to MyGame!
Built with RACEngine - Educational Game Engine

🔧 Initializing game systems...
   ✓ Engine systems initialized
   ✓ Audio system initialized
   ✓ Game content initialized
   ✓ Created 1 game entities
✅ Game initialization complete!
🎮 Game is running. Close window to exit.
```

## Next Steps

### Expand Your Game

1. **Add More Entities:**
   ```csharp
   // Create different entity types
   CreatePlayer();
   CreateEnemies();
   CreatePickups();
   ```

2. **Implement Game Mechanics:**
   ```csharp
   // Add collision detection
   // Implement health/damage system
   // Create scoring mechanics
   ```

3. **Enhance Visuals:**
   ```csharp
   // Load and use textures
   // Add particle effects
   // Implement animation system
   ```

4. **Add Audio:**
   ```csharp
   // Play sound effects
   // Add background music
   // Implement 3D audio
   ```

### Study Advanced Examples

- Examine the sample games in `samples/` directory
- Read the [Architecture Documentation](../architecture/index.md) for deeper understanding
- Explore the [Educational Materials](../educational-material/index.md) for game engine concepts

### Contributing Back

- Consider contributing improvements to RACEngine
- Share your game projects with the community
- Help improve documentation based on your experience

## Troubleshooting

### Common Setup Issues

1. **Build Errors:**
   - Check that all RACEngine project references are correct
   - Ensure .NET 8.0 SDK is installed
   - Clear NuGet cache: `dotnet nuget locals all --clear`

2. **Runtime Errors:**
   - Verify OpenGL drivers are up to date
   - Check console output for specific error messages
   - Ensure asset paths are correct

3. **Performance Issues:**
   - Use Release build for performance testing
   - Check graphics driver compatibility
   - Monitor entity count and system complexity

See [Common Issues](../faq/common-issues.md) for detailed troubleshooting.

## See Also

- [Installation Guide](installation-guide.md) - Setting up development environment
- [Getting Started Tutorial](../educational-material/getting-started-tutorial.md) - Step-by-step first game
- [Architecture Overview](../architecture/system-overview.md) - Understanding engine design

## Changelog

- 2025-06-26: Comprehensive project setup guide with complete game template and configuration examples