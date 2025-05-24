namespace TicTacToe.Components;

public enum Player
{
    X,
    O,
}

public record struct PlayerComponent(Player PlayerId, bool IsCurrent);
