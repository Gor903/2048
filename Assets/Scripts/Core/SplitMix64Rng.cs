using System;

namespace Tilevault.Core
{
    /// <summary>
    /// SplitMix64. Chosen over <see cref="System.Random"/> because that class's
    /// algorithm differs between .NET runtimes, so the same seed would produce
    /// different daily boards under Mono and IL2CPP. This is a fixed algorithm
    /// with one word of state, which also makes snapshotting trivial.
    /// </summary>
    public sealed class SplitMix64Rng : IRng
    {
        const ulong Gamma = 0x9E3779B97F4A7C15UL;

        ulong state;

        public SplitMix64Rng(ulong seed) => state = seed;

        public ulong State
        {
            get => state;
            set => state = value;
        }

        ulong NextULong()
        {
            state += Gamma;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Bound must be positive.");

            // Rejection sampling: drop the final partial band so every value is
            // equally likely. The loop almost never runs twice for board-sized bounds.
            ulong bound = (ulong)maxExclusive;
            ulong limit = ulong.MaxValue - (ulong.MaxValue % bound) - 1;
            ulong draw;
            do
            {
                draw = NextULong();
            } while (draw > limit);

            return (int)(draw % bound);
        }

        public double NextDouble()
        {
            // Top 53 bits scaled into [0,1) — the full mantissa of a double.
            return (NextULong() >> 11) * (1.0 / 9007199254740992.0);
        }
    }
}
