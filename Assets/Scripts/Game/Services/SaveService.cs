using System;
using System.IO;
using Tilevault.Core;
using UnityEngine;

namespace Tilevault.Game.Services
{
    /// <summary>
    /// Reads and writes the single save file. Writes go to a temporary file that
    /// is then moved into place, so a kill mid-write leaves the previous save
    /// intact rather than a half-written one.
    /// </summary>
    public class SaveService
    {
        const string FileName = "tilevault.save.json";

        readonly string path;
        public SaveData Data { get; private set; }

        public SaveService()
        {
            path = Path.Combine(Application.persistentDataPath, FileName);
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                }
                else
                {
                    Data = new SaveData();
                }
            }
            catch (Exception e)
            {
                // A corrupt save must not brick the game — start clean and say so.
                Debug.LogWarning($"[Save] Unreadable save, starting fresh: {e.Message}");
                Data = new SaveData();
            }

            Normalise();
        }

        void Normalise()
        {
            if (Data.bestScores == null || Data.bestScores.Length != SaveData.BestScoreSlots)
                Data.bestScores = new int[SaveData.BestScoreSlots];
            if (Data.unlockedThemes == null || Data.unlockedThemes.Length == 0)
                Data.unlockedThemes = new[] { "classic", "dark" };
            if (string.IsNullOrEmpty(Data.themeId))
                Data.themeId = "classic";
            if (!GameConfig.IsValidSize(Data.savedSize))
                Data.savedSize = GameConfig.DefaultSize;
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data);
                string temp = path + ".tmp";
                File.WriteAllText(temp, json);

                if (File.Exists(path)) File.Delete(path);
                File.Move(temp, path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Could not write save: {e.Message}");
            }
        }

        // ---- scores -----------------------------------------------------------

        public int BestScore(int size, GameMode mode) =>
            Data.bestScores[SaveData.SlotFor(size, (int)mode)];

        /// <summary>Returns true when this run beat the stored best.</summary>
        public bool SubmitScore(int size, GameMode mode, int score)
        {
            int slot = SaveData.SlotFor(size, (int)mode);
            if (score <= Data.bestScores[slot]) return false;

            Data.bestScores[slot] = score;
            Save();
            return true;
        }

        // ---- game in progress -------------------------------------------------

        public void StoreGame(GameState game)
        {
            Cell[] cells = game.Board.ToArray();
            var values = new int[cells.Length];
            var timers = new int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                values[i] = cells[i].Value;
                timers[i] = cells[i].StoneMovesLeft;
            }

            Snapshot s = game.Capture();
            Data.hasSavedGame = true;
            Data.savedSize = game.Size;
            Data.savedMode = (int)game.Mode;
            Data.savedScore = game.Score;
            Data.savedMoves = game.MovesMade;
            Data.savedHasWon = game.HasWon;
            Data.savedRngState = unchecked((long)s.RngState);
            Data.savedCellValues = values;
            Data.savedStoneTimers = timers;
            Data.savedCharges = s.Charges;
            Save();
        }

        public bool TryRestoreGame(out GameState game)
        {
            game = null;
            if (!Data.hasSavedGame) return false;

            int size = Data.savedSize;
            int expected = size * size;
            if (Data.savedCellValues == null || Data.savedCellValues.Length != expected)
            {
                ClearGame();
                return false;
            }

            var cells = new Cell[expected];
            for (int i = 0; i < expected; i++)
            {
                int v = Data.savedCellValues[i];
                cells[i] = v == 0 ? Cell.Empty
                    : v == Cell.StoneValue ? Cell.Stone(Math.Max(1, Data.savedStoneTimers[i]))
                    : Cell.Tile(v);
            }

            int[] charges = Data.savedCharges != null && Data.savedCharges.Length == 3
                ? Data.savedCharges
                : new[] { GameConfig.PowerUpChargesPerGame, GameConfig.PowerUpChargesPerGame, GameConfig.PowerUpChargesPerGame };

            var snapshot = new Snapshot(cells, Data.savedScore, unchecked((ulong)Data.savedRngState), charges);

            game = new GameState(size, (GameMode)Data.savedMode, 1);
            game.Restore(snapshot, size, (GameMode)Data.savedMode, Data.savedMoves, Data.savedHasWon);
            return true;
        }

        public void ClearGame()
        {
            Data.hasSavedGame = false;
            Data.savedCellValues = Array.Empty<int>();
            Data.savedStoneTimers = Array.Empty<int>();
            Save();
        }

        // ---- daily ------------------------------------------------------------

        public void RecordDaily(int dayNumber, int score)
        {
            if (Data.dailyLastPlayedDay != dayNumber)
            {
                Data.dailyLastPlayedDay = dayNumber;
                Data.dailyBestScore = score;
            }
            else if (score > Data.dailyBestScore)
            {
                Data.dailyBestScore = score;
            }

            // A streak survives only when yesterday was also completed.
            if (Data.dailyLastCompletedDay != dayNumber)
            {
                Data.dailyStreak = IsPreviousDay(Data.dailyLastCompletedDay, dayNumber)
                    ? Data.dailyStreak + 1
                    : 1;
                Data.dailyLastCompletedDay = dayNumber;
            }

            Save();
        }

        static bool IsPreviousDay(int previous, int current)
        {
            if (previous == 0) return false;
            try
            {
                DateTime p = FromDayNumber(previous);
                DateTime c = FromDayNumber(current);
                return (c - p).Days == 1;
            }
            catch
            {
                return false;
            }
        }

        static DateTime FromDayNumber(int n) => new DateTime(n / 10000, n / 100 % 100, n % 100);

        public bool DailyPlayedToday(int dayNumber) => Data.dailyLastPlayedDay == dayNumber;

        // ---- themes -----------------------------------------------------------

        public bool IsThemeUnlocked(string id) => Array.IndexOf(Data.unlockedThemes, id) >= 0;

        public void UnlockTheme(string id)
        {
            if (IsThemeUnlocked(id)) return;
            Array.Resize(ref Data.unlockedThemes, Data.unlockedThemes.Length + 1);
            Data.unlockedThemes[Data.unlockedThemes.Length - 1] = id;
            Save();
        }

        // ---- reset ------------------------------------------------------------

        public void ResetAll()
        {
            Data = new SaveData();
            Save();
        }
    }
}
