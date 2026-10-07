using System;

namespace Tilevault.Game.Services
{
    /// <summary>
    /// The on-disk shape. Flat arrays only: JsonUtility handles no dictionaries
    /// and no ulong, so the RNG state is carried as a long and cast back.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;
        public const int BestScoreSlots = 8;   // 4 sizes x 2 modes

        public int version = CurrentVersion;

        // Settings
        public bool soundOn = true;
        public bool vibrationOn = true;
        public string themeId = "classic";
        public string[] unlockedThemes = { "classic", "dark" };
        public bool hasSeenSwipeHint;

        // The mode and size Play starts with, remembered between sessions
        public int preferredSize = 4;
        public int preferredMode;

        // Best scores, indexed by SlotFor()
        public int[] bestScores = new int[BestScoreSlots];

        // Daily challenge
        public int dailyLastPlayedDay;
        public int dailyBestScore;
        public int dailyStreak;
        public int dailyLastCompletedDay;

        // A game in progress, so backgrounding or a kill never loses it
        public bool hasSavedGame;
        public int savedSize = 4;
        public int savedMode;
        public int savedScore;
        public int savedMoves;
        public bool savedHasWon;
        public long savedRngState;
        public int[] savedCellValues = Array.Empty<int>();
        public int[] savedStoneTimers = Array.Empty<int>();
        public int[] savedCharges = Array.Empty<int>();

        public static int SlotFor(int size, int mode) => (size - 3) * 2 + mode;
    }
}
