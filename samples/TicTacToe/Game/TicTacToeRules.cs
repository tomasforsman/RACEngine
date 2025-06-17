// ════════════════════════════════════════════════════════════════════════════════
// TIC-TAC-TOE GAME RULES AND WIN DETECTION
// ════════════════════════════════════════════════════════════════════════════════
//
// Implements the core game logic for Tic-Tac-Toe, including win condition detection
// and move validation. This class demonstrates clean separation of game rules from
// ECS architecture, showing how domain logic can be encapsulated independently.
//
// ALGORITHM EDUCATION:
// The win detection algorithm demonstrates several computer science concepts:
// - Matrix analysis and pattern recognition
// - Efficient iteration strategies (rows, columns, diagonals)
// - Early termination optimization
// - Clean abstraction of complex logic
//
// DESIGN PATTERNS:
// - Static utility class for stateless game rules
// - Pure functions with no side effects
// - Clear input/output contracts for testability
// - Extensible design supporting different board sizes
//
// ════════════════════════════════════════════════════════════════════════════════

using TicTacToe.Components;

namespace TicTacToe.Game;

/// <summary>
/// Encapsulates the rules and logic for Tic-Tac-Toe game.
/// 
/// EDUCATIONAL NOTE:
/// This static class demonstrates how to separate game rules from the ECS architecture.
/// Game rules are domain logic that should be independent of how the game is represented
/// or stored, enabling clean testing and potential reuse in different contexts.
/// 
/// ALGORITHM FOCUS:
/// The win detection algorithms showcase efficient matrix analysis techniques,
/// demonstrating how to check winning patterns without redundant iterations.
/// </summary>
public static class TicTacToeRules
{
    // ═══════════════════════════════════════════════════════════════════════════
    // WIN CONDITION DETECTION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Checks if the current board state has a winner.
    /// 
    /// ALGORITHM EXPLANATION:
    /// Analyzes the board in three phases:
    /// 1. Horizontal rows: Check each row for consecutive matches
    /// 2. Vertical columns: Check each column for consecutive matches  
    /// 3. Diagonals: Check both main diagonal and anti-diagonal
    /// 
    /// OPTIMIZATION:
    /// Uses early termination - returns immediately when a winner is found,
    /// avoiding unnecessary checks of remaining patterns.
    /// </summary>
    /// <param name="board">2D array representing the current board state.</param>
    /// <param name="boardSize">Size of the square board.</param>
    /// <returns>The winning player, or null if no winner exists.</returns>
    public static Player? CheckForWinner(CellState[,] board, int boardSize)
    {
        if (board == null)
            throw new ArgumentNullException(nameof(board));
            
        if (boardSize < 3 || boardSize > board.GetLength(0) || boardSize > board.GetLength(1))
            throw new ArgumentException("Invalid board size", nameof(boardSize));
    
        // ─── Check Horizontal Rows ──────────────────────────────────────
        for (int row = 0; row < boardSize; row++)
        {
            var winner = CheckRowForWinner(board, row, boardSize);
            if (winner.HasValue) return winner;
        }
        
        // ─── Check Vertical Columns ─────────────────────────────────────
        for (int col = 0; col < boardSize; col++)
        {
            var winner = CheckColumnForWinner(board, col, boardSize);
            if (winner.HasValue) return winner;
        }
        
        // ─── Check Main Diagonal (top-left to bottom-right) ─────────────
        var diagonalWinner = CheckMainDiagonalForWinner(board, boardSize);
        if (diagonalWinner.HasValue) return diagonalWinner;
        
        // ─── Check Anti-Diagonal (top-right to bottom-left) ─────────────
        var antiDiagonalWinner = CheckAntiDiagonalForWinner(board, boardSize);
        if (antiDiagonalWinner.HasValue) return antiDiagonalWinner;
        
        return null; // No winner found
    }
    
    /// <summary>
    /// Checks if the board is completely filled (all cells occupied).
    /// Used to detect draw conditions when no winner exists.
    /// </summary>
    /// <param name="board">2D array representing the current board state.</param>
    /// <param name="boardSize">Size of the square board.</param>
    /// <returns>True if all cells are occupied.</returns>
    public static bool IsBoardFull(CellState[,] board, int boardSize)
    {
        if (board == null)
            throw new ArgumentNullException(nameof(board));
            
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x, y] == CellState.Empty)
                    return false; // Found empty cell
            }
        }
        
        return true; // All cells are occupied
    }
    
    /// <summary>
    /// Validates if a move is legal on the current board.
    /// Checks both bounds and cell availability.
    /// </summary>
    /// <param name="move">The move to validate.</param>
    /// <param name="board">Current board state.</param>
    /// <param name="boardSize">Size of the square board.</param>
    /// <returns>True if the move is legal.</returns>
    public static bool IsValidMove(Move move, CellState[,] board, int boardSize)
    {
        if (board == null)
            throw new ArgumentNullException(nameof(board));
            
        // Check bounds
        if (!move.IsValidForBoard(boardSize))
            return false;
            
        // Check if cell is empty
        return board[move.X, move.Y] == CellState.Empty;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE WIN DETECTION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Checks a specific row for a winning pattern.
    /// Demonstrates linear pattern matching algorithm.
    /// </summary>
    private static Player? CheckRowForWinner(CellState[,] board, int row, int boardSize)
    {
        CellState firstCell = board[0, row];
        if (firstCell == CellState.Empty) return null;
        
        // Check if all cells in this row match the first cell
        for (int col = 1; col < boardSize; col++)
        {
            if (board[col, row] != firstCell)
                return null; // Pattern broken
        }
        
        // All cells match - convert to player
        return CellStateToPlayer(firstCell);
    }
    
    /// <summary>
    /// Checks a specific column for a winning pattern.
    /// Demonstrates linear pattern matching algorithm.
    /// </summary>
    private static Player? CheckColumnForWinner(CellState[,] board, int col, int boardSize)
    {
        CellState firstCell = board[col, 0];
        if (firstCell == CellState.Empty) return null;
        
        // Check if all cells in this column match the first cell
        for (int row = 1; row < boardSize; row++)
        {
            if (board[col, row] != firstCell)
                return null; // Pattern broken
        }
        
        // All cells match - convert to player
        return CellStateToPlayer(firstCell);
    }
    
    /// <summary>
    /// Checks the main diagonal (top-left to bottom-right) for a winning pattern.
    /// Demonstrates diagonal pattern matching algorithm.
    /// </summary>
    private static Player? CheckMainDiagonalForWinner(CellState[,] board, int boardSize)
    {
        CellState firstCell = board[0, 0];
        if (firstCell == CellState.Empty) return null;
        
        // Check diagonal from (0,0) to (boardSize-1, boardSize-1)
        for (int i = 1; i < boardSize; i++)
        {
            if (board[i, i] != firstCell)
                return null; // Pattern broken
        }
        
        // All cells match - convert to player
        return CellStateToPlayer(firstCell);
    }
    
    /// <summary>
    /// Checks the anti-diagonal (top-right to bottom-left) for a winning pattern.
    /// Demonstrates reverse diagonal pattern matching algorithm.
    /// </summary>
    private static Player? CheckAntiDiagonalForWinner(CellState[,] board, int boardSize)
    {
        CellState firstCell = board[boardSize - 1, 0];
        if (firstCell == CellState.Empty) return null;
        
        // Check diagonal from (boardSize-1, 0) to (0, boardSize-1)
        for (int i = 1; i < boardSize; i++)
        {
            if (board[boardSize - 1 - i, i] != firstCell)
                return null; // Pattern broken
        }
        
        // All cells match - convert to player
        return CellStateToPlayer(firstCell);
    }
    
    /// <summary>
    /// Converts a CellState to the corresponding Player enum.
    /// Provides clean mapping between board representation and player identity.
    /// </summary>
    private static Player? CellStateToPlayer(CellState cellState)
    {
        return cellState switch
        {
            CellState.X => Player.X,
            CellState.O => Player.O,
            CellState.Empty => null,
            _ => null
        };
    }
}
