namespace Rac.GameEngine.Pooling;

/// <summary>
/// Defines the contract for object pooling systems in the RACEngine.
/// Object pooling improves performance by reusing objects instead of constantly creating and destroying them.
/// This is especially important for frequently instantiated objects like bullets, particles, or temporary effects.
/// </summary>
public interface IPooling
{
    // Future: Consider adding methods like Get(), Return(), Clear(), etc.
}
