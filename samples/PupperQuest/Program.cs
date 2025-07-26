namespace PupperQuest;

/// <summary>
/// Entry point for PupperQuest - Grid-Based Roguelike Puppy Adventure Game.
/// </summary>
/// <remarks>
/// PupperQuest demonstrates RACEngine's capabilities through a complete roguelike game
/// featuring grid-based movement, procedural level generation, simple AI, and turn-based gameplay.
/// 
/// Educational Value:
/// - ECS architecture implementation
/// - Grid-based game mechanics
/// - Procedural content generation
/// - Turn-based game systems
/// - Component composition patterns
/// </remarks>
public static class Program
{
    public static void Main(string[] args)
    {
        PupperQuestGame.Run(args);
    }
}