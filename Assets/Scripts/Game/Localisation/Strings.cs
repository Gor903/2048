using System.Collections.Generic;

namespace Tilevault.Game.Localisation
{
    /// <summary>
    /// Every player-visible string, one entry per key. Views and controllers
    /// call <see cref="Get"/> and never hold a literal, so adding a language is
    /// a new table rather than a search through the scene.
    /// </summary>
    public static class Strings
    {
        public const string Lang_English = "en";

        static string current = Lang_English;

        static readonly Dictionary<string, Dictionary<string, string>> Tables;

        /// <summary>
        /// Built in a static constructor rather than a field initialiser: those
        /// run in textual order, so a table declared below this point would still
        /// be null here and every lookup would throw.
        /// </summary>
        static Strings()
        {
            Tables = new Dictionary<string, Dictionary<string, string>>
            {
                [Lang_English] = English
            };
        }

        public static string CurrentLanguage => current;

        public static IEnumerable<string> AvailableLanguages => Tables.Keys;

        /// <summary>
        /// Picks the table once at startup from the system locale, falling back
        /// to English for anything not shipped.
        /// </summary>
        public static void SelectLanguage(string twoLetterIsoCode)
        {
            current = Tables.ContainsKey(twoLetterIsoCode) ? twoLetterIsoCode : Lang_English;
        }

        public static string Get(string key)
        {
            Dictionary<string, string> table = Tables[current];
            if (table.TryGetValue(key, out string value)) return value;

            // Missing in a translation is a content bug, not a crash: fall back
            // to English, then to the key so it is visible in a screenshot.
            if (current != Lang_English && English.TryGetValue(key, out value)) return value;
            return key;
        }

        public static string Get(string key, object arg0) => string.Format(Get(key), arg0);
        public static string Get(string key, object arg0, object arg1) => string.Format(Get(key), arg0, arg1);

        /// <summary>
        /// Board sizes are a translated entry rather than a formatted number: not
        /// every language writes dimensions with the same separator or digits.
        /// </summary>
        public static string SizeLabel(int size)
        {
            switch (size)
            {
                case 3: return Get(Key.Size3);
                case 4: return Get(Key.Size4);
                case 5: return Get(Key.Size5);
                case 6: return Get(Key.Size6);
                default: return size + "×" + size;
            }
        }

        /// <summary>Exposed so a test can assert every key resolves in every table.</summary>
        public static IReadOnlyDictionary<string, string> TableFor(string lang) => Tables[lang];

        static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            // Home
            [Key.AppName] = "Tilevault",
            [Key.AppSubtitle] = "2048",
            [Key.Play] = "Play",
            [Key.DailyChallenge] = "Daily Challenge",
            [Key.Mode] = "Mode",
            [Key.Themes] = "Themes",
            [Key.Settings] = "Settings",
            [Key.Quit] = "Quit",

            // HUD
            [Key.Score] = "SCORE",
            [Key.Best] = "BEST",
            [Key.Undo] = "Undo",
            [Key.Delete] = "Delete",
            [Key.Shuffle] = "Shuffle",
            [Key.MovesLeft] = "Moves left",

            // Game over
            [Key.GameOver] = "Game Over",
            [Key.ScoreValue] = "Score {0}",
            [Key.NewBest] = "New Best!",
            [Key.Retry] = "Retry",
            [Key.Home] = "Home",
            [Key.KeepPlaying] = "Keep Playing",
            [Key.NoThanks] = "No thanks",

            // Win
            [Key.YouReached] = "You reached {0}!",
            [Key.KeepGoing] = "Keep going",
            [Key.BackToHome] = "Back to Home",

            // Pause
            [Key.Pause] = "Pause",
            [Key.Paused] = "Paused",
            [Key.Resume] = "Resume",
            [Key.Restart] = "Restart",

            // Settings
            [Key.Sound] = "Sound",
            [Key.Vibration] = "Vibration",
            [Key.PrivacySettings] = "Privacy settings",
            [Key.ResetProgress] = "Reset progress",
            [Key.ResetConfirm] = "Reset progress? This deletes every score and unlock.",
            [Key.Cancel] = "Cancel",
            [Key.Reset] = "Reset",

            // Modes
            [Key.ModeClassic] = "Classic",
            [Key.ModeStones] = "Stones",
            [Key.BoardSize] = "Board size",
            [Key.Size3] = "3×3",
            [Key.Size4] = "4×4",
            [Key.Size5] = "5×5",
            [Key.Size6] = "6×6",

            // Themes
            [Key.ThemeClassic] = "Classic",
            [Key.ThemeDark] = "Dark",
            [Key.ThemeSunset] = "Sunset",
            [Key.ThemeForest] = "Forest",
            [Key.Unlock] = "Unlock",
            [Key.Unlocked] = "Unlocked",
            [Key.WatchAdToUnlock] = "Watch ad to unlock",

            // Power-ups
            [Key.OutOfCharges] = "Out of charges",
            [Key.WatchAdForMore] = "Watch an ad for {0} more",
            [Key.TapTileToRemove] = "Tap a tile to remove it",

            // System
            [Key.Loading] = "Loading…",
            [Key.NoConnection] = "No connection",
            [Key.AdNotReady] = "Ad not ready",
            [Key.ExitGame] = "Exit game?",
            [Key.Yes] = "Yes",
            [Key.No] = "No",

            // Daily
            [Key.DayNumber] = "Day {0}",
            [Key.Streak] = "Streak: {0}",
            [Key.ComeBackTomorrow] = "Come back tomorrow",
            [Key.TodaysBest] = "Today's best: {0}",

            // First-run hint
            [Key.SwipeHint] = "Swipe to move the tiles",
        };

        /// <summary>Key constants, so a typo is a compile error rather than a blank label.</summary>
        public static class Key
        {
            public const string AppName = "app.name";
            public const string AppSubtitle = "app.subtitle";
            public const string Play = "home.play";
            public const string DailyChallenge = "home.daily";
            public const string Mode = "home.mode";
            public const string Themes = "home.themes";
            public const string Settings = "home.settings";
            public const string Quit = "home.quit";

            public const string Score = "hud.score";
            public const string Best = "hud.best";
            public const string Undo = "hud.undo";
            public const string Delete = "hud.delete";
            public const string Shuffle = "hud.shuffle";
            public const string MovesLeft = "hud.movesleft";

            public const string GameOver = "over.title";
            public const string ScoreValue = "over.score";
            public const string NewBest = "over.newbest";
            public const string Retry = "over.retry";
            public const string Home = "over.home";
            public const string KeepPlaying = "over.keepplaying";
            public const string NoThanks = "over.nothanks";

            public const string YouReached = "win.reached";
            public const string KeepGoing = "win.keepgoing";
            public const string BackToHome = "win.home";

            public const string Pause = "pause.action";
            public const string Paused = "pause.title";
            public const string Resume = "pause.resume";
            public const string Restart = "pause.restart";

            public const string Sound = "settings.sound";
            public const string Vibration = "settings.vibration";
            public const string PrivacySettings = "settings.privacy";
            public const string ResetProgress = "settings.reset";
            public const string ResetConfirm = "settings.resetconfirm";
            public const string Cancel = "settings.cancel";
            public const string Reset = "settings.resetyes";

            public const string ModeClassic = "mode.classic";
            public const string ModeStones = "mode.stones";
            public const string BoardSize = "mode.size";
            public const string Size3 = "mode.size3";
            public const string Size4 = "mode.size4";
            public const string Size5 = "mode.size5";
            public const string Size6 = "mode.size6";

            public const string ThemeClassic = "theme.classic";
            public const string ThemeDark = "theme.dark";
            public const string ThemeSunset = "theme.sunset";
            public const string ThemeForest = "theme.forest";
            public const string Unlock = "theme.unlock";
            public const string Unlocked = "theme.unlocked";
            public const string WatchAdToUnlock = "theme.watchad";

            public const string OutOfCharges = "power.empty";
            public const string WatchAdForMore = "power.watchad";
            public const string TapTileToRemove = "power.taptile";

            public const string Loading = "sys.loading";
            public const string NoConnection = "sys.noconnection";
            public const string AdNotReady = "sys.adnotready";
            public const string ExitGame = "sys.exit";
            public const string Yes = "sys.yes";
            public const string No = "sys.no";

            public const string DayNumber = "daily.day";
            public const string Streak = "daily.streak";
            public const string ComeBackTomorrow = "daily.comeback";
            public const string TodaysBest = "daily.best";

            public const string SwipeHint = "hint.swipe";
        }
    }
}
