using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using PupperQuest.Components;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace PupperQuest.Systems;

/// <summary>
/// Handles player input and translates it to grid movement commands.
/// Processes WASD keys for directional movement in a turn-based manner.
/// </summary>
/// <remarks>
/// Educational Note: Input handling in turn-based games differs from real-time games.
/// We buffer inputs and process them during turn resolution rather than immediately.
/// This allows for precise, predictable movement on the grid.
/// </remarks>
public class PlayerInputSystem : ISystem
{
    private readonly IInputService _inputService;
    private IWorld _world = null!;
    private bool _hasPendingMove;
    private Vector2D<int> _pendingDirection;

    public PlayerInputSystem(IInputService inputService)
    {
        _inputService = inputService;
    }

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        
        // Subscribe to key events for turn-based input
        _inputService.OnKeyEvent += OnKeyPressed;
    }

    public void Update(float deltaTime)
    {
        if (!_hasPendingMove) return;

        // Find the player entity and apply movement
        foreach (var (entity, puppy, gridPos, movement) in _world.Query<PuppyComponent, GridPositionComponent, MovementComponent>())
        {
            // Update movement component with new direction
            var newMovement = movement with { Direction = _pendingDirection, MoveTimer = 0.25f };
            _world.SetComponent(entity, newMovement);
            
            _hasPendingMove = false;
            break;
        }
    }

    public void Shutdown(IWorld world)
    {
        _inputService.OnKeyEvent -= OnKeyPressed;
    }

    private void OnKeyPressed(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        if (keyEvent != KeyboardKeyState.KeyEvent.Pressed || _hasPendingMove) return;

        _pendingDirection = key switch
        {
            Key.W => new Vector2D<int>(0, -1),    // North (move up: decrease grid Y since Y is flipped in rendering)
            Key.S => new Vector2D<int>(0, 1),     // South (move down: increase grid Y since Y is flipped in rendering)
            Key.A => new Vector2D<int>(-1, 0),    // West (move left: decrease X)
            Key.D => new Vector2D<int>(1, 0),     // East (move right: increase X)
            _ => Vector2D<int>.Zero
        };

        if (_pendingDirection != Vector2D<int>.Zero)
        {
            _hasPendingMove = true;
        }
    }
}