using System.Collections.Generic;

namespace Tilevault.Core
{
    /// <summary>One tile's journey during a move, for the view to animate.</summary>
    public readonly struct TileMove
    {
        public readonly int FromX;
        public readonly int FromY;
        public readonly int ToX;
        public readonly int ToY;

        /// <summary>The tile's value before the move.</summary>
        public readonly int Value;

        /// <summary>True when this tile was absorbed into the tile at the destination.</summary>
        public readonly bool Merged;

        /// <summary>Value standing at the destination once the move resolved.</summary>
        public readonly int ResultValue;

        public TileMove(int fromX, int fromY, int toX, int toY, int value, bool merged, int resultValue)
        {
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
            Value = value;
            Merged = merged;
            ResultValue = resultValue;
        }
    }

    public readonly struct TileSpawn
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Value;

        public TileSpawn(int x, int y, int value)
        {
            X = x;
            Y = y;
            Value = value;
        }
    }

    public readonly struct GridPos
    {
        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Everything that happened in one move. <see cref="Changed"/> is the
    /// validity test: a move that changed nothing scores nothing, spawns
    /// nothing, and does not age stones.
    /// </summary>
    public sealed class MoveResult
    {
        public bool Changed { get; internal set; }
        public int ScoreGained { get; internal set; }
        public List<TileMove> Moves { get; } = new List<TileMove>();
        public List<GridPos> StonesRemoved { get; } = new List<GridPos>();

        /// <summary>Null when the move was invalid, since no tile spawns then.</summary>
        public TileSpawn? Spawn { get; internal set; }

        /// <summary>Highest value produced by a merge this move; 0 if nothing merged.</summary>
        public int HighestMerge { get; internal set; }

        public static readonly MoveResult NoChange = new MoveResult();
    }
}
