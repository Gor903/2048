namespace Tilevault.Core
{
    /// <summary>
    /// A restorable point in the game. The RNG state travels with it, so
    /// undo-then-replay reproduces the same spawns rather than letting the
    /// player re-roll a bad one.
    /// </summary>
    public sealed class Snapshot
    {
        public Cell[] Cells { get; }
        public int Score { get; }
        public ulong RngState { get; }
        public int[] Charges { get; }

        public Snapshot(Cell[] cells, int score, ulong rngState, int[] charges)
        {
            Cells = cells;
            Score = score;
            RngState = rngState;
            Charges = charges;
        }
    }
}
