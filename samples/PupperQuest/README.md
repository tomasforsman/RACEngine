# PupperQuest - Grid-Based Roguelike Puppy Adventure

> 🐶 A charming roguelike game where you help a lost puppy find their way home through procedurally generated levels.

PupperQuest demonstrates **RACEngine's ECS architecture** through a complete, playable game featuring grid-based movement, procedural level generation, AI systems, and turn-based gameplay.

## 🎮 Game Features

### Core Gameplay
- **Grid-Based Movement**: Turn-based WASD controls with smooth visual transitions
- **Procedural Generation**: Each level features unique room-and-corridor layouts  
- **Enemy AI**: Multiple enemy types with different behaviors (chase, flee, patrol, guard)
- **Item Collection**: Health treats, energy water, special bones, keys, and toys
- **Level Progression**: Find the exit to advance through increasingly challenging levels
- **Combat System**: Tactical encounters with damage and health management

### Technical Features
- **Clean ECS Architecture**: Proper separation of components, systems, and data
- **Rendering Pipeline**: Tile-based rendering with colored sprites for prototyping
- **Input Handling**: Responsive turn-based input with proper buffering
- **Game State Management**: Win/lose conditions, level progression, and collision detection
- **AI Systems**: Behavioral state machines for engaging enemy encounters

## 🎯 Educational Value

PupperQuest serves as an excellent learning example for:

- **Grid-based game mechanics** using ECS architecture
- **Procedural level generation** algorithms (room-and-corridor method)
- **Turn-based vs real-time gameplay** patterns in ECS
- **Simple AI behaviors** using component composition
- **Game state management** across multiple levels
- **Rendering system integration** with the engine pipeline

## 🚀 Running PupperQuest

### From Sample Launcher
```bash
cd samples/SampleGame
dotnet run -- pupperquest
```

### Direct Execution
```bash
cd samples/PupperQuest
dotnet run
```

## 🎮 Controls

| Key | Action |
|-----|--------|
| **W** | Move North |
| **A** | Move West |
| **S** | Move South |
| **D** | Move East |

## 🗺️ Game Elements

### Tiles
- **Gray Floor**: Walkable terrain
- **Dark Wall**: Impassable barriers
- **Green Exit**: Stairs to the next level
- **Blue Start**: Player spawn point

### Entities
- **Yellow Puppy**: The player character (you!)
- **Gray Rats**: Fast enemies that chase the player
- **Orange Cats**: Medium enemies that flee from the player
- **Blue Mailmen**: Large enemies that patrol and chase when close
- **Brown Guards**: Stationary enemies that defend passages

### Items
- **🦴 Treats**: Restore health points
- **💧 Water**: Restore energy/stamina
- **🦴 Bones**: Increase smell detection radius
- **🔑 Keys**: Unlock doors (future feature)
- **🧸 Toys**: Boost energy levels

## 🏗️ Architecture Overview

### Components (`Components/`)
- **GridPositionComponent**: Discrete grid coordinates for turn-based logic
- **MovementComponent**: Direction and timing for smooth animations
- **PuppyComponent**: Player stats (health, energy, smell radius)
- **EnemyComponent**: Enemy properties (type, damage, detection range)
- **AIComponent**: AI behavior state and targeting
- **TileComponent**: Level geometry and passability
- **SpriteComponent**: Visual representation data

### Systems (`Systems/`)
- **PlayerInputSystem**: WASD input handling and turn buffering
- **GridMovementSystem**: Grid-based movement with smooth interpolation
- **SimpleAISystem**: Enemy AI behaviors (chase, flee, patrol, guard, wander)
- **TileRenderingSystem**: Sprite rendering with proper layering
- **GameStateSystem**: Win/lose conditions, item collection, level progression

### Generation (`Generation/`)
- **DungeonGenerator**: Procedural room-and-corridor level generation
- **Room**: Rectangular room data structure with overlap detection
- **LevelData**: Complete level information including spawn points

## 🎨 Visual Design

PupperQuest uses **colored rectangles for sprites**, demonstrating that engaging gameplay doesn't require complex graphics. This approach:

- **Focuses on gameplay mechanics** rather than visual complexity
- **Enables rapid prototyping** and iteration
- **Demonstrates ECS architecture** clearly without visual distractions
- **Provides clear visual distinction** between different entity types

## 🧪 Technical Implementation

### ECS Patterns Demonstrated
```csharp
// Component: Pure data containers
public readonly record struct GridPositionComponent(int X, int Y) : IComponent;

// System: Stateless logic processors
public class GridMovementSystem : ISystem
{
    public void Update(float deltaTime)
    {
        foreach (var (entity, gridPos, movement) in _world.Query<GridPositionComponent, MovementComponent>())
        {
            // Process movement logic...
        }
    }
}
```

### Procedural Generation Example
```csharp
// Simple room-and-corridor algorithm
var level = dungeonGenerator.GenerateLevel(width: 25, height: 20, roomCount: 4);
```

### AI Behavior Implementation
```csharp
// State-based AI with different behaviors per enemy type
var direction = ai.Behavior switch
{
    AIBehavior.Hostile => CalculateChaseDirection(currentPos, playerPos),
    AIBehavior.Flee => CalculateFleeDirection(currentPos, playerPos),
    AIBehavior.Patrol => CalculatePatrolDirection(ai, currentPos),
    _ => Vector2D<int>.Zero
};
```

## 🔄 Game Loop Architecture

1. **Input Processing**: Buffer WASD commands for turn-based execution
2. **Movement Resolution**: Validate and apply grid movements with collision
3. **AI Execution**: Process enemy behaviors and targeting
4. **Game State Updates**: Handle collisions, item collection, win/lose
5. **Rendering**: Draw tiles, entities, and UI with proper layering
6. **Level Progression**: Generate new levels when exit is reached

## 🎓 Learning Outcomes

After studying PupperQuest, developers will understand:

- **ECS Component Design**: How to structure pure data containers
- **System Coordination**: Managing dependencies between game systems  
- **Procedural Generation**: Algorithms for creating interesting level layouts
- **Turn-Based Game Logic**: Handling discrete movement and timing
- **AI State Machines**: Simple but effective behavioral patterns
- **Game State Flow**: Managing progression, win/lose, and transitions

## 🚀 Future Enhancements

PupperQuest's modular architecture enables easy expansion:

### Immediate Additions
- **Audio Integration**: Barking, footsteps, ambient sounds
- **Visual Polish**: Replace rectangles with sprite graphics
- **More Actions**: Digging, fetching, rolling mechanics
- **Save System**: Basic save/load functionality

### Advanced Features
- **Complex AI**: A* pathfinding, group behaviors
- **Rich UI**: Inventory screen, settings menu, help system
- **Multiple Biomes**: Forest, suburbs, junkyard environments
- **Progression System**: Puppy grows with new abilities

## 🎯 Design Philosophy

PupperQuest embodies **RACEngine's educational mission**:

- **Educational Clarity**: Code teaches by example with comprehensive documentation
- **Professional Architecture**: Production-ready patterns and practices
- **Progressive Complexity**: Simple foundation with room for sophisticated features
- **Modular Design**: Each component demonstrates specific engine capabilities

The game proves that **compelling gameplay emerges from solid architecture** rather than complex graphics or features.

---

*PupperQuest - Where Every Puppy Finds Their Way Home!* 🐕🏠