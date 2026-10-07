using System;
using System.Collections.Generic;

namespace Tilevault.Core
{
    /// <summary>
    /// The grid and the sliding rules. Mutates in place; <see cref="GameState"/>
    /// owns scoring, spawning and undo on top of it.
    /// </summary>
    public sealed class Board
    {
        readonly Cell[] cells;

        // Scratch buffers reused across moves so a swipe allocates nothing but its result.
        readonly int[] srcX;
        readonly int[] srcY;
        readonly Cell[] lineIn;
        readonly Cell[] lineOut;

        public int Size { get; }

        public Board(int size)
        {
            GameConfig.RequireValidSize(size);
            Size = size;
            cells = new Cell[size * size];
            srcX = new int[size];
            srcY = new int[size];
            lineIn = new Cell[size];
            lineOut = new Cell[size];
        }

        public Cell this[int x, int y]
        {
            get
            {
                RequireInBounds(x, y);
                return cells[y * Size + x];
            }
            set
            {
                RequireInBounds(x, y);
                cells[y * Size + x] = value;
            }
        }

        void RequireInBounds(int x, int y)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size)
                throw new ArgumentOutOfRangeException($"({x},{y}) is outside a {Size}x{Size} board.");
        }

        // ---- snapshot support -------------------------------------------------

        public Cell[] ToArray() => (Cell[])cells.Clone();

        public void LoadFrom(Cell[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Length != cells.Length)
                throw new ArgumentException($"Expected {cells.Length} cells, got {source.Length}.", nameof(source));
            Array.Copy(source, cells, cells.Length);
        }

        public void Clear()
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i] = Cell.Empty;
        }

        // ---- queries ----------------------------------------------------------

        public bool IsFull()
        {
            for (int i = 0; i < cells.Length; i++)
                if (cells[i].IsEmpty) return false;
            return true;
        }

        public int CountEmpty()
        {
            int n = 0;
            for (int i = 0; i < cells.Length; i++)
                if (cells[i].IsEmpty) n++;
            return n;
        }

        public void CollectEmpty(List<GridPos> into)
        {
            into.Clear();
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                if (this[x, y].IsEmpty)
                    into.Add(new GridPos(x, y));
        }

        public void CollectTiles(List<GridPos> into)
        {
            into.Clear();
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                if (this[x, y].IsTile)
                    into.Add(new GridPos(x, y));
        }

        public int MaxTileValue()
        {
            int max = 0;
            for (int i = 0; i < cells.Length; i++)
                if (cells[i].IsTile && cells[i].Value > max)
                    max = cells[i].Value;
            return max;
        }

        /// <summary>
        /// Game-over test. A move exists when a cell is empty, or when two
        /// orthogonally adjacent numbered tiles share a value. Stones occupy
        /// space and never pair, so they can only ever remove options.
        /// </summary>
        public bool HasMoves()
        {
            for (int i = 0; i < cells.Length; i++)
                if (cells[i].IsEmpty) return true;

            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                Cell c = this[x, y];
                if (!c.IsTile) continue;
                if (x + 1 < Size)
                {
                    Cell r = this[x + 1, y];
                    if (r.IsTile && r.Value == c.Value) return true;
                }
                if (y + 1 < Size)
                {
                    Cell u = this[x, y + 1];
                    if (u.IsTile && u.Value == c.Value) return true;
                }
            }
            return false;
        }

        // ---- the move ---------------------------------------------------------

        /// <summary>
        /// Slide and merge toward <paramref name="direction"/>, mutating the board.
        /// Does not spawn, score-keep or age stones — the caller does that only
        /// when <see cref="MoveResult.Changed"/> is true.
        /// </summary>
        public MoveResult Move(Direction direction)
        {
            var result = new MoveResult();

            for (int line = 0; line < Size; line++)
            {
                BuildLine(direction, line);
                ResolveLine(result);
                WriteLineBack();
            }

            return result;
        }

        /// <summary>
        /// Fills the scratch coordinates for one row or column, ordered from the
        /// leading edge inward — the edge tiles travel toward comes first, so the
        /// compaction below is direction-agnostic.
        /// </summary>
        void BuildLine(Direction direction, int line)
        {
            for (int i = 0; i < Size; i++)
            {
                int x, y;
                switch (direction)
                {
                    case Direction.Left:  x = i;            y = line;         break;
                    case Direction.Right: x = Size - 1 - i; y = line;         break;
                    case Direction.Down:  x = line;         y = i;            break;
                    case Direction.Up:    x = line;         y = Size - 1 - i; break;
                    default: throw new ArgumentOutOfRangeException(nameof(direction));
                }
                srcX[i] = x;
                srcY[i] = y;
                lineIn[i] = this[x, y];
                lineOut[i] = Cell.Empty;
            }
        }

        void ResolveLine(MoveResult result)
        {
            int write = 0;
            int lastMerged = -1;

            for (int i = 0; i < Size; i++)
            {
                Cell c = lineIn[i];
                if (c.IsEmpty) continue;

                if (c.IsStone)
                {
                    // Immovable: it keeps its own index, and nothing may slide past
                    // it or merge across it.
                    lineOut[i] = c;
                    write = i + 1;
                    lastMerged = -1;
                    continue;
                }

                bool canMerge = write > 0
                                && lineOut[write - 1].IsTile
                                && lineOut[write - 1].Value == c.Value
                                && write - 1 != lastMerged;

                if (canMerge)
                {
                    int merged = c.Value * 2;
                    lineOut[write - 1] = Cell.Tile(merged);
                    lastMerged = write - 1;

                    result.Changed = true;
                    result.ScoreGained += merged;
                    if (merged > result.HighestMerge) result.HighestMerge = merged;
                    result.Moves.Add(new TileMove(
                        srcX[i], srcY[i],
                        srcX[write - 1], srcY[write - 1],
                        c.Value, true, merged));
                }
                else
                {
                    lineOut[write] = c;
                    if (write != i)
                    {
                        result.Changed = true;
                        result.Moves.Add(new TileMove(
                            srcX[i], srcY[i],
                            srcX[write], srcY[write],
                            c.Value, false, c.Value));
                    }
                    write++;
                }
            }
        }

        void WriteLineBack()
        {
            for (int i = 0; i < Size; i++)
                this[srcX[i], srcY[i]] = lineOut[i];
        }

        // ---- stones -----------------------------------------------------------

        /// <summary>
        /// Ages every stone by one move, removing those that reach zero. Called
        /// once per <em>valid</em> move.
        /// </summary>
        public void AgeStones(List<GridPos> removed)
        {
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                Cell c = this[x, y];
                if (!c.IsStone) continue;
                Cell aged = c.Aged();
                this[x, y] = aged;
                if (aged.IsEmpty)
                    removed?.Add(new GridPos(x, y));
            }
        }
    }
}
