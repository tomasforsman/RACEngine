namespace Rac.GameEngine.Serialization;

/// <summary>
/// Defines the contract for serialization services in the RACEngine.
/// Serialization enables saving and loading game state, including scenes, settings, and save data.
/// This abstraction allows for different serialization formats (JSON, binary, XML, etc.).
/// </summary>
public interface ISerialization
{
    // Future: Consider adding methods like Serialize<T>(), Deserialize<T>(), SaveToFile(), LoadFromFile(), etc.
}
