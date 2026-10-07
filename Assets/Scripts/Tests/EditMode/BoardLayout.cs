using System;
using Tilevault.Core;

namespace Tilevault.Tests
{
    /// <summary>
    /// Reads and writes boards as text so a test states its grid literally.
    /// Rows are written top-down; the grid's origin is bottom-left, so the first
    /// text row is the highest y.
    ///
    /// <c>"."</c> empty, a number is a tile, <c>"S"</c> or <c>"S5"</c> a stone
    /// with an optional remaining-move count.
    /// </summary>
    public static class BoardLayout
    {
        public static Board Parse(params string[] rows)
        {
            if (rows == null || rows.Length == 0)
                throw new ArgumentException("At least one row required.", nameof(rows));

            int size = rows.Length;
            var board = new Board(size);

            for (int r = 0; r < size; r++)
            {
                string[] tokens = rows[r].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != size)
                    throw new ArgumentException($"Row {r} has {tokens.Length} cells, expected {size}.");

                int y = size - 1 - r;
                for (int x = 0; x < size; x++)
                    board[x, y] = ParseCell(tokens[x]);
            }

            return board;
        }

        static Cell ParseCell(string token)
        {
            if (token == ".") return Cell.Empty;

            if (token[0] == 'S' || token[0] == 's')
            {
                string rest = token.Substring(1);
                int life = rest.Length == 0 ? GameConfig.StoneLifetimeMoves : int.Parse(rest);
                return Cell.Stone(life);
            }

            return Cell.Tile(int.Parse(token));
        }

        /// <summary>Renders values only — stones become <c>S</c>, timers omitted.</summary>
        public static string[] Render(Board board)
        {
            var rows = new string[board.Size];
            for (int r = 0; r < board.Size; r++)
            {
                int y = board.Size - 1 - r;
                var parts = new string[board.Size];
                for (int x = 0; x < board.Size; x++)
                {
                    Cell c = board[x, y];
                    parts[x] = c.IsEmpty ? "." : c.IsStone ? "S" : c.Value.ToString();
                }
                rows[r] = string.Join(" ", parts);
            }
            return rows;
        }

        public static string Flat(Board board) => string.Join(" / ", Render(board));
    }
}
