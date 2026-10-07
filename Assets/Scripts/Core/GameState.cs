using System;
using System.Collections.Generic;

namespace Tilevault.Core
{
    /// <summary>
    /// A playable session: board, score, undo history, power-up charges and the
    /// win/lose verdict. Engine-free, so a whole game can be played out in a
    /// unit test without a scene.
    /// </summary>
    public sealed class GameState
    {
        readonly List<Snapshot> history = new List<Snapshot>(GameConfig.UndoDepth + 1);
        readonly List<GridPos> scratchPositions = new List<GridPos>();
        readonly List<GridPos> scratchTiles = new List<GridPos>();
        readonly int[] charges = new int[3];

        public Board Board { get; private set; }
        public IRng Rng { get; private set; }
        public GameMode Mode { get; private set; }
        public int Size => Board.Size;
        public int Score { get; private set; }
        public int WinTarget { get; private set; }

        /// <summary>True once the target tile has been reached; stays true for the session.</summary>
        public bool HasWon { get; private set; }

        /// <summary>Set when the player dismisses the win overlay and keeps playing.</summary>
        public bool WinAcknowledged { get; set; }

        public bool IsGameOver => !Board.HasMoves();

        public int MovesMade { get; private set; }

        public GameState(int size = GameConfig.DefaultSize, GameMode mode = GameMode.Classic, ulong seed = 1)
        {
            NewGame(size, mode, seed);
        }

        public void NewGame(int size, GameMode mode, ulong seed)
        {
            GameConfig.RequireValidSize(size);

            Board = new Board(size);
            Rng = new SplitMix64Rng(seed);
            Mode = mode;
            WinTarget = GameConfig.WinTarget(size);
            Score = 0;
            MovesMade = 0;
            HasWon = false;
            WinAcknowledged = false;

            history.Clear();
            for (int i = 0; i < charges.Length; i++)
                charges[i] = GameConfig.PowerUpChargesPerGame;

            if (mode == GameMode.Stones)
                SeedStones();

            // The two opening tiles, drawn from the same seeded stream.
            SpawnTile();
            SpawnTile();
        }

        // ---- power-up charges -------------------------------------------------

        public int ChargesOf(PowerUp powerUp) => charges[(int)powerUp];

        public void AddCharges(PowerUp powerUp, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            charges[(int)powerUp] += count;
        }

        bool TrySpendCharge(PowerUp powerUp)
        {
            int i = (int)powerUp;
            if (charges[i] <= 0) return false;
            charges[i]--;
            return true;
        }

        // ---- history ----------------------------------------------------------

        public bool CanUndo => history.Count > 0 && ChargesOf(PowerUp.Undo) > 0;

        void PushSnapshot()
        {
            history.Add(new Snapshot(Board.ToArray(), Score, Rng.State, (int[])charges.Clone()));
            if (history.Count > GameConfig.UndoDepth)
                history.RemoveAt(0);
        }

        /// <summary>
        /// Restores the previous position. The undo charge is spent after the
        /// restore, so undoing a Delete hands back the Delete charge but still
        /// costs one undo.
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo) return false;

            Snapshot s = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);

            Board.LoadFrom(s.Cells);
            Score = s.Score;
            Rng.State = s.RngState;
            Array.Copy(s.Charges, charges, charges.Length);

            charges[(int)PowerUp.Undo]--;
            if (MovesMade > 0) MovesMade--;
            return true;
        }

        // ---- the move ---------------------------------------------------------

        public MoveResult Move(Direction direction)
        {
            if (IsGameOver) return MoveResult.NoChange;

            Snapshot before = new Snapshot(Board.ToArray(), Score, Rng.State, (int[])charges.Clone());

            MoveResult result = Board.Move(direction);
            if (!result.Changed)
                return result;   // invalid: nothing spawns, nothing ages, nothing scores

            history.Add(before);
            if (history.Count > GameConfig.UndoDepth)
                history.RemoveAt(0);

            Score += result.ScoreGained;
            MovesMade++;

            if (Mode == GameMode.Stones)
                Board.AgeStones(result.StonesRemoved);

            TileSpawn? spawned = SpawnTile();
            result.Spawn = spawned;

            if (!HasWon && Board.MaxTileValue() >= WinTarget)
                HasWon = true;

            return result;
        }

        // ---- power-ups --------------------------------------------------------

        /// <summary>Removes one numbered tile. Does not count as a move: nothing spawns.</summary>
        public bool UseDelete(int x, int y)
        {
            if (!Board[x, y].IsTile) return false;
            if (ChargesOf(PowerUp.Delete) <= 0) return false;

            PushSnapshot();
            TrySpendCharge(PowerUp.Delete);
            Board[x, y] = Cell.Empty;
            return true;
        }

        /// <summary>
        /// Redistributes the numbered tiles over the non-stone cells. Stones keep
        /// their positions and timers. Does not count as a move.
        /// </summary>
        public bool UseShuffle()
        {
            if (ChargesOf(PowerUp.Shuffle) <= 0) return false;

            Board.CollectTiles(scratchTiles);
            if (scratchTiles.Count == 0) return false;

            PushSnapshot();
            TrySpendCharge(PowerUp.Shuffle);

            var values = new int[scratchTiles.Count];
            for (int i = 0; i < scratchTiles.Count; i++)
                values[i] = Board[scratchTiles[i].X, scratchTiles[i].Y].Value;

            Cell[] original = Board.ToArray();

            for (int attempt = 0; attempt < GameConfig.ShuffleRetries; attempt++)
            {
                ApplyShuffle(values);
                if (!SameLayout(original, Board.ToArray()))
                    return true;
            }

            // Every retry reproduced the starting layout — accept it rather than loop.
            return true;
        }

        void ApplyShuffle(int[] values)
        {
            // Every cell that is not a stone is a candidate home.
            scratchPositions.Clear();
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                if (!Board[x, y].IsStone)
                    scratchPositions.Add(new GridPos(x, y));

            // Fisher-Yates over the candidates, drawn from the game's own stream.
            for (int i = scratchPositions.Count - 1; i > 0; i--)
            {
                int j = Rng.NextInt(i + 1);
                (scratchPositions[i], scratchPositions[j]) = (scratchPositions[j], scratchPositions[i]);
            }

            foreach (GridPos p in scratchPositions)
                Board[p.X, p.Y] = Cell.Empty;

            for (int i = 0; i < values.Length && i < scratchPositions.Count; i++)
                Board[scratchPositions[i].X, scratchPositions[i].Y] = Cell.Tile(values[i]);
        }

        static bool SameLayout(Cell[] a, Cell[] b)
        {
            for (int i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i])) return false;
            return true;
        }

        // ---- spawning ---------------------------------------------------------

        TileSpawn? SpawnTile()
        {
            Board.CollectEmpty(scratchPositions);
            if (scratchPositions.Count == 0) return null;

            GridPos p = scratchPositions[Rng.NextInt(scratchPositions.Count)];
            int value = Rng.NextDouble() < GameConfig.SpawnFourProbability ? 4 : 2;
            Board[p.X, p.Y] = Cell.Tile(value);
            return new TileSpawn(p.X, p.Y, value);
        }

        void SeedStones()
        {
            int wanted = GameConfig.StoneCount(Size);
            for (int i = 0; i < wanted; i++)
            {
                Board.CollectEmpty(scratchPositions);
                if (scratchPositions.Count == 0) return;
                GridPos p = scratchPositions[Rng.NextInt(scratchPositions.Count)];
                Board[p.X, p.Y] = Cell.Stone(GameConfig.StoneLifetimeMoves);
            }
        }

        // ---- persistence ------------------------------------------------------

        public Snapshot Capture() => new Snapshot(Board.ToArray(), Score, Rng.State, (int[])charges.Clone());

        public void Restore(Snapshot s, int size, GameMode mode, int movesMade, bool hasWon)
        {
            GameConfig.RequireValidSize(size);
            Board = new Board(size);
            Board.LoadFrom(s.Cells);
            Rng = new SplitMix64Rng(0) { State = s.RngState };
            Mode = mode;
            WinTarget = GameConfig.WinTarget(size);
            Score = s.Score;
            MovesMade = movesMade;
            HasWon = hasWon;
            WinAcknowledged = hasWon;
            Array.Copy(s.Charges, charges, charges.Length);
            history.Clear();
        }
    }
}
