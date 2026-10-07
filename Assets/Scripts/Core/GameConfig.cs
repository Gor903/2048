using System;

namespace Tilevault.Core
{
    /// <summary>
    /// Tuning constants. Every number the rules depend on lives here, so balance
    /// changes never require hunting through the logic.
    /// </summary>
    public static class GameConfig
    {
        public const int MinSize = 3;
        public const int MaxSize = 6;
        public const int DefaultSize = 4;

        /// <summary>Chance a spawned tile is a 4 rather than a 2.</summary>
        public const double SpawnFourProbability = 0.1;

        public const int UndoDepth = 10;
        public const int PowerUpChargesPerGame = 3;
        public const int ChargesPerRewardedAd = 1;

        public const int StoneLifetimeMoves = 15;

        /// <summary>Bounded retries when a shuffle reproduces the layout it started from.</summary>
        public const int ShuffleRetries = 8;

        /// <summary>
        /// 2048 is not practically reachable on a 3x3 grid, so the target scales
        /// with the board.
        /// </summary>
        public static int WinTarget(int size)
        {
            switch (size)
            {
                case 3: return 256;
                case 4: return 2048;
                case 5: return 4096;
                case 6: return 8192;
                default: throw new ArgumentOutOfRangeException(nameof(size), size, "Unsupported board size.");
            }
        }

        /// <summary>Stones seeded at the start of a Stones game. Two on the default 4x4.</summary>
        public static int StoneCount(int size)
        {
            RequireValidSize(size);
            return size - 2;
        }

        public static bool IsValidSize(int size) => size >= MinSize && size <= MaxSize;

        public static void RequireValidSize(int size)
        {
            if (!IsValidSize(size))
                throw new ArgumentOutOfRangeException(nameof(size), size, "Board size must be 3..6.");
        }
    }
}
