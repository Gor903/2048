using System;

namespace Tilevault.Core
{
    /// <summary>
    /// One grid cell: empty, a numbered tile, or a stone with a countdown.
    /// </summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        /// <summary>Sentinel stored in <see cref="Value"/> for a stone.</summary>
        public const int StoneValue = -1;

        public readonly int Value;
        public readonly int StoneMovesLeft;

        Cell(int value, int stoneMovesLeft)
        {
            Value = value;
            StoneMovesLeft = stoneMovesLeft;
        }

        public static readonly Cell Empty = new Cell(0, 0);

        public static Cell Tile(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Tile value must be positive.");
            return new Cell(value, 0);
        }

        public static Cell Stone(int movesLeft)
        {
            if (movesLeft <= 0)
                throw new ArgumentOutOfRangeException(nameof(movesLeft), movesLeft, "Stone lifetime must be positive.");
            return new Cell(StoneValue, movesLeft);
        }

        public bool IsEmpty => Value == 0;
        public bool IsStone => Value == StoneValue;
        public bool IsTile => Value > 0;

        /// <summary>A stone one move closer to crumbling. Empty once the counter runs out.</summary>
        public Cell Aged()
        {
            if (!IsStone)
                return this;
            return StoneMovesLeft <= 1 ? Empty : new Cell(StoneValue, StoneMovesLeft - 1);
        }

        public bool Equals(Cell other) => Value == other.Value && StoneMovesLeft == other.StoneMovesLeft;
        public override bool Equals(object obj) => obj is Cell other && Equals(other);
        public override int GetHashCode() => (Value * 397) ^ StoneMovesLeft;
        public override string ToString() =>
            IsEmpty ? "." : IsStone ? $"S{StoneMovesLeft}" : Value.ToString();
    }
}
