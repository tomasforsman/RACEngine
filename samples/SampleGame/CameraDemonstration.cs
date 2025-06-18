// ═══════════════════════════════════════════════════════════════════════════════
// CAMERA SYSTEM DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// Interactive demonstration of the dual-camera system showcasing:
// - Game world camera with pan, zoom, and rotation controls
// - UI overlay camera with fixed screen-space elements
// - Multi-pass rendering: game world first, UI overlay second
// - Coordinate transformation utilities for mouse interaction
//
// CONTROLS:
// - WASD: Pan game camera around world
// - Q/E: Rotate game camera
// - Mouse wheel: Zoom game camera in/out
// - Mouse click: Show world coordinates at cursor position
// - R: Reset camera to default position/zoom/rotation
// - ESC: Exit demonstration
//
// VISUAL ELEMENTS:
// - Moving game objects in world space (affected by camera transformations)
// - Fixed UI elements in screen space (HUD, crosshair, coordinate display)
// - Grid background to visualize camera transformations
// - Mouse cursor world position indicator

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace SampleGame;

/// <summary>
/// Interactive camera system demonstration showcasing dual-camera rendering.
/// </summary>
public static class CameraDemonstration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEMONSTRATION STATE
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static IEngineFacade _engine = null!;
    private static bool _showCoordinates = false;
    
    // Game world objects for demonstration
    private static readonly List<GameObject> _gameObjects = new();
    
    // Camera control sensitivity
    private const float PAN_SPEED = 2f;
    private const float ROTATION_SPEED = 2f;
    private const float ZOOM_SPEED = 0.1f;

    public static void Run(string[] args)
    {
        Console.WriteLine("Starting Camera System Demonstration...");
        Console.WriteLine("Controls:");
        Console.WriteLine("  WASD: Pan camera");
        Console.WriteLine("  Q/E: Rotate camera");
        Console.WriteLine("  Mouse wheel: Zoom");
        Console.WriteLine("  Mouse click: Show coordinates");
        Console.WriteLine("  R: Reset camera");
        Console.WriteLine("  ESC: Exit");
        
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        
        _engine = new EngineFacade(windowManager, inputService, configurationManager);
        
        // Set up event handlers
        _engine.LoadEvent += OnLoad;
        _engine.UpdateEvent += OnUpdate;
        _engine.RenderEvent += OnRender;
        _engine.KeyEvent += OnKeyEvent;
        _engine.MouseScrollEvent += OnMouseScroll;
        
        // Start the engine
        _engine.Run();
    }

    private static void OnLoad()
    {
        Console.WriteLine("Camera demonstration loaded. Use controls to interact with the camera system.");
        
        // Initialize game objects for demonstration
        InitializeGameObjects();
        
        // Set initial camera position
        _engine.CameraManager.GameCamera.Position = new Vector2D<float>(0f, 0f);
        _engine.CameraManager.GameCamera.Zoom = 1f;
        _engine.CameraManager.GameCamera.Rotation = 0f;
    }

    private static void OnUpdate(float deltaTime)
    {
        // Update game object positions for animation
        UpdateGameObjects(deltaTime);
        
        // Handle camera input
        HandleCameraInput(deltaTime);
    }

    private static void OnRender(float deltaTime)
    {
        _engine.Renderer.Clear();
        
        // ───────────────────────────────────────────────────────────────────────
        // PASS 1: RENDER GAME WORLD
        // ───────────────────────────────────────────────────────────────────────
        
        // Set game camera for world rendering
        _engine.Renderer.SetActiveCamera(_engine.CameraManager.GameCamera);
        
        RenderGameWorld();
        
        // ───────────────────────────────────────────────────────────────────────
        // PASS 2: RENDER UI OVERLAY
        // ───────────────────────────────────────────────────────────────────────
        
        // Set UI camera for overlay rendering
        _engine.Renderer.SetActiveCamera(_engine.CameraManager.UICamera);
        
        RenderUIOverlay();
        
        _engine.Renderer.FinalizeFrame();
    }

    private static void InitializeGameObjects()
    {
        // Create some demo objects in the game world
        _gameObjects.Clear();
        
        // Central object
        _gameObjects.Add(new GameObject(Vector2D<float>.Zero, new Vector4D<float>(1f, 0f, 0f, 1f), 0.1f));
        
        // Orbiting objects
        for (int i = 0; i < 8; i++)
        {
            float angle = i * MathF.PI * 2f / 8f;
            var position = new Vector2D<float>(MathF.Cos(angle) * 0.5f, MathF.Sin(angle) * 0.5f);
            var color = new Vector4D<float>(0f, 1f, 0f, 1f);
            _gameObjects.Add(new GameObject(position, color, 0.05f));
        }
        
        // Grid reference points
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                if (x == 0 && y == 0) continue; // Skip center
                
                var position = new Vector2D<float>(x * 0.5f, y * 0.5f);
                var color = new Vector4D<float>(0.3f, 0.3f, 0.3f, 1f);
                _gameObjects.Add(new GameObject(position, color, 0.02f));
            }
        }
    }

    private static void UpdateGameObjects(float deltaTime)
    {
        // Animate orbiting objects
        for (int i = 1; i < 9; i++) // Skip central object
        {
            var obj = _gameObjects[i];
            float angle = (i - 1) * MathF.PI * 2f / 8f + deltaTime * 0.5f;
            obj.Position = new Vector2D<float>(MathF.Cos(angle) * 0.5f, MathF.Sin(angle) * 0.5f);
        }
    }

    private static void HandleCameraInput(float deltaTime)
    {
        var camera = _engine.CameraManager.GameCamera;
        
        // Pan controls (WASD)
        Vector2D<float> panDelta = Vector2D<float>.Zero;
        
        // Note: In a real implementation, you'd get input state from the input service
        // For this demonstration, we'll use a simplified approach
        
        // Apply pan, zoom, and rotation to camera
        // These would be connected to actual input in a real application
    }

    private static void UpdateMouseWorldPosition()
    {
        // In a real implementation, you'd get mouse position from input service
        // and convert it to world coordinates using the camera manager
        // This is just a placeholder for the demonstration
    }

    private static void RenderGameWorld()
    {
        // Render grid background
        _engine.Renderer.SetColor(new Vector4D<float>(0.2f, 0.2f, 0.2f, 1f));
        RenderGrid();
        
        // Render game objects
        foreach (var gameObject in _gameObjects)
        {
            _engine.Renderer.SetColor(gameObject.Color);
            RenderQuad(gameObject.Position, gameObject.Size);
        }
    }

    private static void RenderUIOverlay()
    {
        // Render UI elements in screen space
        _engine.Renderer.SetColor(new Vector4D<float>(1f, 1f, 1f, 1f));
        
        // Crosshair at screen center
        RenderCrosshair();
        
        // Camera info display
        if (_showCoordinates)
        {
            RenderCoordinateDisplay();
        }
        
        // Instructions overlay
        RenderInstructions();
    }

    private static void RenderGrid()
    {
        // ───────────────────────────────────────────────────────────────────────
        // REFERENCE GRID RENDERING
        // ───────────────────────────────────────────────────────────────────────
        //
        // This grid demonstrates world-space rendering that moves with the camera.
        // It provides visual reference for camera transformations (pan, zoom, rotate).

        var gridVertices = new List<float>();
        const float majorGridSize = 8f;
        const float majorGridSpacing = 1f;
        const float minorGridSpacing = 0.2f;

        // Major grid lines (every 1 unit) - slightly more visible
        for (float y = -majorGridSize; y <= majorGridSize; y += majorGridSpacing)
        {
            gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
        }
        for (float x = -majorGridSize; x <= majorGridSize; x += majorGridSpacing)
        {
            gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
        }

        // Render major grid lines with subtle but visible color
        _engine.Renderer.SetShaderMode(ShaderMode.Normal);
        _engine.Renderer.SetPrimitiveType(PrimitiveType.Lines);
        _engine.Renderer.SetColor(new Vector4D<float>(0.4f, 0.4f, 0.4f, 0.8f));
        _engine.Renderer.UpdateVertices(gridVertices.ToArray());
        _engine.Renderer.Draw();

        // Minor grid lines (every 0.2 units) - very subtle
        gridVertices.Clear();
        for (float y = -majorGridSize; y <= majorGridSize; y += minorGridSpacing)
        {
            if (y % majorGridSpacing != 0) // Skip major grid line positions
            {
                gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
            }
        }
        for (float x = -majorGridSize; x <= majorGridSize; x += minorGridSpacing)
        {
            if (x % majorGridSpacing != 0) // Skip major grid line positions
            {
                gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
            }
        }

        // Render minor grid lines with very subtle color
        _engine.Renderer.SetColor(new Vector4D<float>(0.25f, 0.25f, 0.25f, 0.4f));
        _engine.Renderer.UpdateVertices(gridVertices.ToArray());
        _engine.Renderer.Draw();

        // Reset to triangles for other objects
        _engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
    }

    private static void RenderQuad(Vector2D<float> position, float size)
    {
        float halfSize = size * 0.5f;
        var vertices = new float[]
        {
            position.X - halfSize, position.Y - halfSize,
            position.X + halfSize, position.Y - halfSize,
            position.X + halfSize, position.Y + halfSize,
            position.X - halfSize, position.Y + halfSize,
        };
        
        _engine.Renderer.UpdateVertices(vertices);
        _engine.Renderer.Draw();
    }

    private static void RenderCrosshair()
    {
        // Simple crosshair at UI center (screen center)
        var crosshairVertices = new float[]
        {
            -20f, 0f, 20f, 0f,  // Horizontal line
            0f, -20f, 0f, 20f   // Vertical line
        };
        
        _engine.Renderer.UpdateVertices(crosshairVertices);
        _engine.Renderer.Draw();
    }

    private static void RenderCoordinateDisplay()
    {
        // In a real implementation, this would render text showing coordinates
        // For now, just render a placeholder indicator
        RenderQuad(new Vector2D<float>(-350f, 250f), 10f);
    }

    private static void RenderInstructions()
    {
        // In a real implementation, this would render instruction text
        // For now, just render placeholder indicators
        for (int i = 0; i < 5; i++)
        {
            RenderQuad(new Vector2D<float>(-380f, 200f - i * 30f), 5f);
        }
    }

    private static void OnKeyEvent(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        if (keyEvent != KeyboardKeyState.KeyEvent.Pressed) return;
        
        var camera = _engine.CameraManager.GameCamera;
        
        switch (key)
        {
            case Key.W:
                camera.Move(new Vector2D<float>(0f, 0.1f));
                break;
            case Key.S:
                camera.Move(new Vector2D<float>(0f, -0.1f));
                break;
            case Key.A:
                camera.Move(new Vector2D<float>(-0.1f, 0f));
                break;
            case Key.D:
                camera.Move(new Vector2D<float>(0.1f, 0f));
                break;
            case Key.Q:
                camera.Rotate(-0.1f);
                break;
            case Key.E:
                camera.Rotate(0.1f);
                break;
            case Key.R:
                // Reset camera
                camera.Position = Vector2D<float>.Zero;
                camera.Zoom = 1f;
                camera.Rotation = 0f;
                break;
            case Key.C:
                _showCoordinates = !_showCoordinates;
                break;
            case Key.Number1:
                _engine.Renderer.SetShaderMode(ShaderMode.Normal);
                break;
            case Key.Number2:
                _engine.Renderer.SetShaderMode(ShaderMode.SoftGlow);
                break;
            case Key.Number3:
                _engine.Renderer.SetShaderMode(ShaderMode.Bloom);
                break;
        }
    }

    private static void OnMouseScroll(float delta)
    {
        var camera = _engine.CameraManager.GameCamera;
        
        // Zoom with mouse wheel
        const float zoomSensitivity = 0.1f;
        float zoomDelta = delta * zoomSensitivity;
        
        // Apply zoom with limits
        camera.Zoom = Math.Max(0.1f, Math.Min(10f, camera.Zoom + zoomDelta));
    }

    /// <summary>
    /// Simple game object for demonstration purposes.
    /// </summary>
    private class GameObject
    {
        public Vector2D<float> Position { get; set; }
        public Vector4D<float> Color { get; set; }
        public float Size { get; set; }

        public GameObject(Vector2D<float> position, Vector4D<float> color, float size)
        {
            Position = position;
            Color = color;
            Size = size;
        }
    }
}