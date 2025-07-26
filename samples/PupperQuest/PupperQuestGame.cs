using Rac.Core.Manager;
using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.Engine;
using PupperQuest.Components;
using PupperQuest.Generation;
using PupperQuest.Systems;
using Silk.NET.Maths;

namespace PupperQuest;

/// <summary>
/// Main game class for PupperQuest - Grid-Based Roguelike Puppy Adventure Game.
/// Demonstrates RACEngine's ECS architecture through a complete turn-based game experience.
/// </summary>
/// <remarks>
/// Educational Value:
/// - Grid-based game mechanics using ECS architecture
/// - Procedural level generation algorithms  
/// - Turn-based vs real-time gameplay patterns in ECS
/// - Simple AI behaviors using component composition
/// - Game state management across multiple levels
/// 
/// This implementation showcases clean separation between game logic (components/systems)
/// and presentation (rendering), following modern game engine architecture principles.
/// </remarks>
public class PupperQuestGame
{
    private EngineFacade _engine = null!;
    private DungeonGenerator _dungeonGenerator = null!;
    private int _currentLevel = 1;
    private const int LevelWidth = 25;
    private const int LevelHeight = 20;

    /// <summary>
    /// Main entry point for running PupperQuest.
    /// </summary>
    public static void Run(string[] args)
    {
        var game = new PupperQuestGame();
        game.Start();
    }

    private void Start()
    {
        try
        {
            InitializeEngine();
            InitializeGame();
            RunGameLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running PupperQuest: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            CleanupEngine();
        }
    }

    private void InitializeEngine()
    {
        Console.WriteLine("🐶 Starting PupperQuest...");
        
        // Initialize engine components following the existing pattern
        var windowManager = new Rac.Core.Manager.WindowManager();
        var inputService = new Rac.Input.Service.SilkInputService();
        var configurationManager = new ConfigManager();
        
        _engine = new EngineFacade(windowManager, inputService, configurationManager);
        
        // Initialize game systems
        _engine.AddSystem(new PlayerInputSystem(inputService));
        _engine.AddSystem(new GridMovementSystem());
        
        Console.WriteLine("✅ Engine initialized");
    }

    private void InitializeGame()
    {
        _dungeonGenerator = new DungeonGenerator();
        
        // Load first level
        LoadLevel(_currentLevel);
        
        Console.WriteLine("🏠 Game initialized - Help the puppy find home!");
        Console.WriteLine("🎮 Controls: WASD to move");
        Console.WriteLine("🎯 Goal: Find the exit (stairs) to advance to the next level");
    }

    private void LoadLevel(int levelNumber)
    {
        Console.WriteLine($"🗺️ Generating level {levelNumber}...");
        
        // Clear existing entities (except camera and UI)
        ClearLevelEntities();
        
        // Generate new level
        var levelData = _dungeonGenerator.GenerateLevel(LevelWidth, LevelHeight, 4 + levelNumber);
        
        // Create level tiles
        CreateLevelTiles(levelData);
        
        // Spawn player
        SpawnPlayer(levelData.StartPosition);
        
        // Spawn enemies
        SpawnEnemies(levelData.EnemySpawns, levelNumber);
        
        // Spawn items
        SpawnItems(levelData.ItemSpawns);
        
        Console.WriteLine($"✅ Level {levelNumber} loaded");
    }

    private void ClearLevelEntities()
    {
        var entitiesToDestroy = new List<Entity>();
        
        // Find all game entities (those with GridPositionComponent)
        foreach (var (entity, _) in _engine.World.Query<GridPositionComponent>())
        {
            entitiesToDestroy.Add(entity);
        }
        
        // Destroy entities
        foreach (var entity in entitiesToDestroy)
        {
            _engine.World.DestroyEntity(entity);
        }
    }

    private void CreateLevelTiles(LevelData levelData)
    {
        for (int x = 0; x < levelData.Width; x++)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                var tileType = levelData.Tiles[x, y];
                var entity = _engine.World.CreateEntity();
                
                // Grid position
                _engine.World.SetComponent(entity, new GridPositionComponent(x, y));
                
                // Tile component
                var isPassable = tileType != TileType.Wall;
                _engine.World.SetComponent(entity, new TileComponent(tileType, isPassable));
                
                // Visual representation
                var color = GetTileColor(tileType);
                _engine.World.SetComponent(entity, new SpriteComponent(
                    new Vector2D<float>(0.9f, 0.9f), color));
                
                // Transform for rendering
                var worldPos = new GridPositionComponent(x, y).ToWorldPosition(1.0f);
                _engine.World.SetComponent(entity, new TransformComponent(
                    worldPos, 0f, Vector2D<float>.One));
            }
        }
    }

    private void SpawnPlayer(Vector2D<int> position)
    {
        var player = _engine.World.CreateEntity();
        
        // Core components
        _engine.World.SetComponent(player, new GridPositionComponent(position.X, position.Y));
        _engine.World.SetComponent(player, new PuppyComponent(Health: 100, Energy: 100, SmellRadius: 3));
        _engine.World.SetComponent(player, new MovementComponent(Vector2D<int>.Zero, 0));
        
        // Visual representation - Bright yellow for the puppy
        _engine.World.SetComponent(player, new SpriteComponent(
            new Vector2D<float>(0.8f, 0.8f), 
            new Vector4D<float>(1.0f, 1.0f, 0.2f, 1.0f))); // Bright yellow
        
        // Transform for rendering
        var worldPos = new GridPositionComponent(position.X, position.Y).ToWorldPosition(1.0f);
        _engine.World.SetComponent(player, new TransformComponent(
            worldPos, 0f, Vector2D<float>.One));
        
        Console.WriteLine($"🐕 Puppy spawned at ({position.X}, {position.Y})");
    }

    private void SpawnEnemies(Vector2D<int>[] spawnPoints, int levelNumber)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var position = spawnPoints[i];
            var enemy = _engine.World.CreateEntity();
            
            // Determine enemy type based on level and spawn index
            var enemyType = (EnemyType)(i % Enum.GetValues<EnemyType>().Length);
            
            // Core components
            _engine.World.SetComponent(enemy, new GridPositionComponent(position.X, position.Y));
            _engine.World.SetComponent(enemy, new EnemyComponent(enemyType, 10, 3));
            _engine.World.SetComponent(enemy, new AIComponent(AIBehavior.Hostile, 0, Array.Empty<Vector2D<int>>()));
            
            // Visual representation
            var color = GetEnemyColor(enemyType);
            _engine.World.SetComponent(enemy, new SpriteComponent(
                new Vector2D<float>(0.7f, 0.7f), color));
            
            // Transform for rendering
            var worldPos = new GridPositionComponent(position.X, position.Y).ToWorldPosition(1.0f);
            _engine.World.SetComponent(enemy, new TransformComponent(
                worldPos, 0f, Vector2D<float>.One));
        }
        
        Console.WriteLine($"👹 Spawned {spawnPoints.Length} enemies");
    }

    private void SpawnItems(Vector2D<int>[] spawnPoints)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var position = spawnPoints[i];
            var item = _engine.World.CreateEntity();
            
            // Determine item type
            var itemType = (ItemType)(i % Enum.GetValues<ItemType>().Length);
            
            // Core components
            _engine.World.SetComponent(item, new GridPositionComponent(position.X, position.Y));
            _engine.World.SetComponent(item, new ItemComponent(itemType, 10));
            
            // Visual representation
            var color = GetItemColor(itemType);
            _engine.World.SetComponent(item, new SpriteComponent(
                new Vector2D<float>(0.5f, 0.5f), color));
            
            // Transform for rendering
            var worldPos = new GridPositionComponent(position.X, position.Y).ToWorldPosition(1.0f);
            _engine.World.SetComponent(item, new TransformComponent(
                worldPos, 0f, Vector2D<float>.One));
        }
        
        Console.WriteLine($"🎁 Spawned {spawnPoints.Length} items");
    }

    private void RunGameLoop()
    {
        Console.WriteLine("🎮 Starting game loop...");
        _engine.Run();
    }

    private void CleanupEngine()
    {
        Console.WriteLine("👋 Thanks for playing PupperQuest!");
    }

    private static Vector4D<float> GetTileColor(TileType tileType)
    {
        return tileType switch
        {
            TileType.Floor => new Vector4D<float>(0.8f, 0.8f, 0.8f, 1.0f),    // Light gray
            TileType.Wall => new Vector4D<float>(0.3f, 0.3f, 0.3f, 1.0f),     // Dark gray
            TileType.Door => new Vector4D<float>(0.6f, 0.3f, 0.0f, 1.0f),     // Brown
            TileType.Stairs => new Vector4D<float>(0.0f, 0.8f, 0.0f, 1.0f),   // Green
            TileType.Start => new Vector4D<float>(0.0f, 0.0f, 1.0f, 1.0f),    // Blue
            TileType.Exit => new Vector4D<float>(1.0f, 0.0f, 0.0f, 1.0f),     // Red
            _ => new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f)                  // White
        };
    }

    private static Vector4D<float> GetEnemyColor(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Rat => new Vector4D<float>(0.5f, 0.5f, 0.5f, 1.0f),     // Gray
            EnemyType.Cat => new Vector4D<float>(0.8f, 0.4f, 0.0f, 1.0f),     // Orange
            EnemyType.Mailman => new Vector4D<float>(0.0f, 0.0f, 0.8f, 1.0f), // Blue
            EnemyType.FenceGuard => new Vector4D<float>(0.4f, 0.2f, 0.0f, 1.0f), // Brown
            _ => new Vector4D<float>(1.0f, 0.0f, 1.0f, 1.0f)                  // Magenta
        };
    }

    private static Vector4D<float> GetItemColor(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Treat => new Vector4D<float>(1.0f, 0.8f, 0.6f, 1.0f),    // Light brown
            ItemType.Water => new Vector4D<float>(0.0f, 0.6f, 1.0f, 1.0f),    // Light blue
            ItemType.Bone => new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),     // White
            ItemType.Key => new Vector4D<float>(1.0f, 1.0f, 0.0f, 1.0f),      // Yellow
            ItemType.Toy => new Vector4D<float>(1.0f, 0.0f, 1.0f, 1.0f),      // Magenta
            _ => new Vector4D<float>(0.5f, 0.5f, 0.5f, 1.0f)                  // Gray
        };
    }
}