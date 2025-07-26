using PupperQuest.Components;
using Silk.NET.Maths;

namespace PupperQuest.Generation;

/// <summary>
/// Simple dungeon generation using room-and-corridor algorithm.
/// Creates rectangular rooms connected by straight corridors.
/// </summary>
/// <remarks>
/// Educational Note: Procedural content generation is a core technique in roguelike games.
/// This implementation demonstrates basic spatial algorithms for creating interesting,
/// playable level layouts.
/// 
/// Academic Reference: "Procedural Content Generation in Games" (Shaker, Togelius, Nelson, 2016)
/// Room-and-corridor generation is one of the oldest and most reliable PCG techniques.
/// </remarks>
public class DungeonGenerator
{
    private readonly Random _random;

    public DungeonGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Generates a complete level with rooms, corridors, and spawn points.
    /// </summary>
    /// <param name="width">Width of the level in tiles</param>
    /// <param name="height">Height of the level in tiles</param>
    /// <param name="roomCount">Number of rooms to generate</param>
    /// <returns>Generated level data</returns>
    public LevelData GenerateLevel(int width, int height, int roomCount)
    {
        var tiles = new TileType[width, height];
        
        // Initialize all tiles as walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = TileType.Wall;
            }
        }

        // Generate rooms
        var rooms = GenerateRooms(roomCount, width, height);
        
        // Carve out room spaces
        foreach (var room in rooms)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    tiles[x, y] = TileType.Floor;
                }
            }
        }

        // Connect rooms with corridors
        ConnectRooms(rooms, tiles, width, height);

        // Place special tiles
        var startPos = GetRandomRoomCenter(rooms[0]);
        var exitPos = GetRandomRoomCenter(rooms[^1]);
        
        tiles[startPos.X, startPos.Y] = TileType.Start;
        tiles[exitPos.X, exitPos.Y] = TileType.Exit;

        // Generate spawn points for enemies and items
        var enemySpawns = GenerateSpawnPoints(rooms, 3, 6);
        var itemSpawns = GenerateSpawnPoints(rooms, 2, 4);

        return new LevelData
        {
            Tiles = tiles,
            Width = width,
            Height = height,
            Rooms = rooms,
            StartPosition = startPos,
            ExitPosition = exitPos,
            EnemySpawns = enemySpawns,
            ItemSpawns = itemSpawns
        };
    }

    private Room[] GenerateRooms(int roomCount, int levelWidth, int levelHeight)
    {
        var rooms = new List<Room>();
        const int maxAttempts = 100;

        for (int i = 0; i < roomCount; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var width = _random.Next(4, 8);
                var height = _random.Next(4, 8);
                var x = _random.Next(1, levelWidth - width - 1);
                var y = _random.Next(1, levelHeight - height - 1);

                var newRoom = new Room(x, y, width, height);

                // Check for overlap with existing rooms
                bool overlaps = rooms.Any(room => newRoom.Overlaps(room));

                if (!overlaps)
                {
                    rooms.Add(newRoom);
                    break;
                }
            }
        }

        return rooms.ToArray();
    }

    private void ConnectRooms(Room[] rooms, TileType[,] tiles, int width, int height)
    {
        for (int i = 0; i < rooms.Length - 1; i++)
        {
            var roomA = rooms[i];
            var roomB = rooms[i + 1];

            var centerA = GetRandomRoomCenter(roomA);
            var centerB = GetRandomRoomCenter(roomB);

            // Create L-shaped corridor
            CreateHorizontalCorridor(tiles, centerA.X, centerB.X, centerA.Y, width, height);
            CreateVerticalCorridor(tiles, centerB.X, centerA.Y, centerB.Y, width, height);
        }
    }

    private void CreateHorizontalCorridor(TileType[,] tiles, int x1, int x2, int y, int width, int height)
    {
        int startX = Math.Min(x1, x2);
        int endX = Math.Max(x1, x2);

        for (int x = startX; x <= endX; x++)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                tiles[x, y] = TileType.Floor;
            }
        }
    }

    private void CreateVerticalCorridor(TileType[,] tiles, int x, int y1, int y2, int width, int height)
    {
        int startY = Math.Min(y1, y2);
        int endY = Math.Max(y1, y2);

        for (int y = startY; y <= endY; y++)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                tiles[x, y] = TileType.Floor;
            }
        }
    }

    private Vector2D<int> GetRandomRoomCenter(Room room)
    {
        var centerX = room.X + room.Width / 2;
        var centerY = room.Y + room.Height / 2;
        return new Vector2D<int>(centerX, centerY);
    }

    private Vector2D<int>[] GenerateSpawnPoints(Room[] rooms, int minCount, int maxCount)
    {
        var spawnPoints = new List<Vector2D<int>>();
        var spawnCount = _random.Next(minCount, maxCount + 1);

        for (int i = 0; i < spawnCount && rooms.Length > 0; i++)
        {
            var room = rooms[_random.Next(rooms.Length)];
            var x = _random.Next(room.X + 1, room.X + room.Width - 1);
            var y = _random.Next(room.Y + 1, room.Y + room.Height - 1);
            spawnPoints.Add(new Vector2D<int>(x, y));
        }

        return spawnPoints.ToArray();
    }
}

/// <summary>
/// Represents a rectangular room in the dungeon.
/// </summary>
public record struct Room(int X, int Y, int Width, int Height)
{
    /// <summary>
    /// Checks if this room overlaps with another room.
    /// </summary>
    public bool Overlaps(Room other)
    {
        return X < other.X + other.Width &&
               X + Width > other.X &&
               Y < other.Y + other.Height &&
               Y + Height > other.Y;
    }
}

/// <summary>
/// Contains all data for a generated level.
/// </summary>
public class LevelData
{
    public TileType[,] Tiles { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public Room[] Rooms { get; set; } = Array.Empty<Room>();
    public Vector2D<int> StartPosition { get; set; }
    public Vector2D<int> ExitPosition { get; set; }
    public Vector2D<int>[] EnemySpawns { get; set; } = Array.Empty<Vector2D<int>>();
    public Vector2D<int>[] ItemSpawns { get; set; } = Array.Empty<Vector2D<int>>();
}