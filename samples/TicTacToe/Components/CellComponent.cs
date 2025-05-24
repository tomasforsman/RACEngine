namespace TicTacToe.Components;

public enum CellState
{
    Empty,
    X,
    O,
}

public record struct CellComponent(int X, int Y, CellState State);
