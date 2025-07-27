using PupperQuest.Components;

using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Engine;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace PupperQuest.Systems;

/// <summary>
/// Handles camera control input for zooming and movement.
/// Provides real-time camera manipulation with +/- for zoom and arrow keys for movement.
/// </summary>
/// <remarks>
/// Educational Note: Camera controls in games typically need real-time response,
/// unlike turn-based gameplay input. This system demonstrates immediate feedback
/// for camera manipulation while maintaining the ECS pattern.
/// </remarks>
public class CameraControlSystem : ISystem
{
    private readonly IInputService _inputService;
    private readonly EngineFacade _engine;
    private IWorld _world = null!;

    // Camera movement settings
    private const float CameraMoveSpeed = 2.0f;      // Units per second
    private const float ZoomSpeed = 0.5f;            // Zoom change per second
    private const float MinZoom = 0.1f;              // Minimum zoom level (zoomed out)
    private const float MaxZoom = 5.0f;              // Maximum zoom level (zoomed in)

    // Input state tracking
    private readonly HashSet<Key> _pressedKeys = new();

    public CameraControlSystem(IInputService inputService, EngineFacade engine)
    {
        _inputService = inputService;
        _engine = engine;
    }

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));

        // Subscribe to key events for real-time input
        _inputService.OnKeyEvent += OnKeyEvent;
    }

    public void Update(float deltaTime)
    {
        var camera = _engine.CameraManager.GameCamera;
        var currentPosition = camera.Position;
        var currentZoom = camera.Zoom;
        bool cameraChanged = false;

        // Get player position for camera following
        Vector2D<float>? playerWorldPos = null;
        foreach (var (entity, puppy, gridPos) in _world.Query<PuppyComponent, GridPositionComponent>())
        {
            playerWorldPos = gridPos.ToWorldPosition(1.0f);
            break;
        }

        // Follow player position
        if (playerWorldPos.HasValue)
        {
            var targetPosition = playerWorldPos.Value;
            var newPosition = Vector2D.Lerp(currentPosition, targetPosition, deltaTime * 5.0f);
            camera.Position = newPosition;
            cameraChanged = true;
        }

        // Handle manual camera movement (real-time override)
        var movement = Vector2D<float>.Zero;

        if (_pressedKeys.Contains(Key.Up))
            movement.Y += CameraMoveSpeed * deltaTime;  // Up arrow - move camera up to see content above
        if (_pressedKeys.Contains(Key.Down))
            movement.Y -= CameraMoveSpeed * deltaTime;  // Down arrow - move camera down to see content below
        if (_pressedKeys.Contains(Key.Left))
            movement.X -= CameraMoveSpeed * deltaTime;  // Left arrow - move camera left to see content to the left
        if (_pressedKeys.Contains(Key.Right))
            movement.X += CameraMoveSpeed * deltaTime;  // Right arrow - move camera right to see content to the right

        if (movement != Vector2D<float>.Zero)
        {
            camera.Position = currentPosition + movement;
            cameraChanged = true;
        }

        // Handle zoom (real-time)
        var zoomChange = 0f;

        // Test with Q/E keys to match working samples exactly
        if (_pressedKeys.Contains(Key.E) || _pressedKeys.Contains(Key.KeypadAdd) || _pressedKeys.Contains(Key.Equal))
            zoomChange += ZoomSpeed * deltaTime;  // E/+ zooms in (match working samples)
        if (_pressedKeys.Contains(Key.Q) || _pressedKeys.Contains(Key.KeypadSubtract) || _pressedKeys.Contains(Key.Minus))
            zoomChange -= ZoomSpeed * deltaTime;  // Q/- zooms out (match working samples)

        if (zoomChange != 0f)
        {
            var newZoom = Math.Clamp(currentZoom + zoomChange, MinZoom, MaxZoom);
            camera.Zoom = newZoom;
            cameraChanged = true;
        }

        // Log camera changes
        if (cameraChanged)
        {
            //Console.WriteLine($"📷 Camera: Position=({camera.Position.X:F2}, {camera.Position.Y:F2}), Zoom={camera.Zoom:F3}");
        }
    }

    public void Shutdown(IWorld world)
    {
        _inputService.OnKeyEvent -= OnKeyEvent;
    }

    private void OnKeyEvent(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        switch (keyEvent)
        {
            case KeyboardKeyState.KeyEvent.Pressed:
                _pressedKeys.Add(key);
                break;
            case KeyboardKeyState.KeyEvent.Released:
                _pressedKeys.Remove(key);
                break;
        }
    }
}
