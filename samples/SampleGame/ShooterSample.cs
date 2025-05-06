using System;
using System.Collections.Generic;
using Silk.NET.Input;
using Silk.NET.Maths;
using Rac.Engine;
using Rac.Core.Manager;
using Rac.GameEngine;
using Rac.Input.Service;
using Rac.Input.State;

namespace SampleGame;

public class ShooterSample
{ 
    private static Engine _gameEngine;

    private enum Direction { Up, Right, Down, Left }
    private static Direction _direction = Direction.Up;
    private static float _angle = 0f;

    private static readonly Vector2D<float>[] _baseTriangle = new[]
    {
        new Vector2D<float>(-0.03f, -0.05f),
        new Vector2D<float>( 0.03f, -0.05f),
        new Vector2D<float>( 0.00f,  0.05f)
    };

    private static bool _autoFire;
    private static float _fireTimer;
    private const float FireInterval = 0.5f;
    private const float BulletSpeed = 0.75f;

    private class Bullet
    {
        public Vector2D<float> Position;
        public Vector2D<float> Direction;
    }
    private static readonly List<Bullet> _bullets = new();

    public static void Run(string[] args)
    {
        var windowManager = new WindowManager();
        var inputService  = new SilkInputService();
        var configurationManager = new ConfigManager();
        _gameEngine   = new Engine(windowManager, inputService, configurationManager);

        _gameEngine.OnKeyEvent    += OnKeyEvent;
        _gameEngine.OnEcsUpdate += HandleGameEcsUpdate;

        RedrawScene();

        _gameEngine.Run();
    }

    private static void OnKeyEvent(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        if (keyEvent == KeyboardKeyState.KeyEvent.Pressed && key == Key.X)
        {
            _autoFire = !_autoFire;
        }

        if (keyEvent != KeyboardKeyState.KeyEvent.Pressed)
            return;

        _direction = key switch
        {
            Key.W or Key.Up    => Direction.Up,
            Key.D or Key.Right => Direction.Right,
            Key.S or Key.Down  => Direction.Down,
            Key.A or Key.Left  => Direction.Left,
            _                  => _direction
        };

        _angle = _direction switch
        {
            Direction.Up    => 0f,
            Direction.Right => -MathF.PI / 2f,
            Direction.Down  =>  MathF.PI,
            Direction.Left  =>  MathF.PI / 2f,
            _               => 0f
        };
    }

    private static void HandleGameEcsUpdate(float frameTimeInSeconds)
    {
        if (_autoFire)
        {
            _fireTimer += frameTimeInSeconds;
            if (_fireTimer >= FireInterval)
            {
                _fireTimer -= FireInterval;
                var directionVector = _direction switch
                {
                    Direction.Up    => new Vector2D<float>(0,  1),
                    Direction.Right => new Vector2D<float>(1,  0),
                    Direction.Down  => new Vector2D<float>(0, -1),
                    Direction.Left  => new Vector2D<float>(-1, 0),
                    _               => new Vector2D<float>(0,  1)
                };
                _bullets.Add(new Bullet
                {
                    Position  = Vector2D<float>.Zero,
                    Direction = directionVector
                });
            }
        }

        for (int index = _bullets.Count - 1; index >= 0; index--)
        {
            var bullet = _bullets[index];
            bullet.Position += bullet.Direction * BulletSpeed * frameTimeInSeconds;
            if (Math.Abs(bullet.Position.X) > 1.1f ||
                Math.Abs(bullet.Position.Y) > 1.1f)
            {
                _bullets.RemoveAt(index);
            }
        }

        RedrawScene();
    }

    private static void RedrawScene()
    {
        var vertices = new List<float>();

        foreach (var vertex in _baseTriangle)
        {
            float x = vertex.X * MathF.Cos(_angle) - vertex.Y * MathF.Sin(_angle);
            float y = vertex.X * MathF.Sin(_angle) + vertex.Y * MathF.Cos(_angle);
            vertices.Add(x);
            vertices.Add(y);
        }

        foreach (var bullet in _bullets)
        {
            const float halfBulletSize = 0.02f;
            vertices.AddRange(new float[]
            {
                // Triangle 1
                bullet.Position.X - halfBulletSize, bullet.Position.Y - halfBulletSize,
                bullet.Position.X + halfBulletSize, bullet.Position.Y - halfBulletSize,
                bullet.Position.X + halfBulletSize, bullet.Position.Y + halfBulletSize,
                // Triangle 2
                bullet.Position.X - halfBulletSize, bullet.Position.Y - halfBulletSize,
                bullet.Position.X + halfBulletSize, bullet.Position.Y + halfBulletSize,
                bullet.Position.X - halfBulletSize, bullet.Position.Y + halfBulletSize,
            });
        }

        _gameEngine.UpdateVertices(vertices.ToArray());
    }
}