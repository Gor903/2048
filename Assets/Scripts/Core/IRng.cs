namespace Tilevault.Core
{
    /// <summary>
    /// Every random decision in the game goes through this. The state is
    /// readable and writable so it can travel inside an undo snapshot — without
    /// that, undoing a move lets the player re-roll a spawn, and the daily
    /// challenge stops being the same board for everyone.
    /// </summary>
    public interface IRng
    {
        ulong State { get; set; }

        /// <summary>Uniform in [0, maxExclusive).</summary>
        int NextInt(int maxExclusive);

        /// <summary>Uniform in [0, 1).</summary>
        double NextDouble();
    }
}
