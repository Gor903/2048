using System;
using System.Collections.Generic;
using Tilevault.Core;
using Tilevault.Game.Localisation;
using Tilevault.Game.Presentation;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Screens
{
    /// <summary>
    /// The board, its HUD and the three overlays that can sit on top of it.
    /// Owns the <see cref="GameState"/>; everything here reads that state or
    /// calls into it, and never reimplements a rule.
    /// </summary>
    public sealed class GameScreen : UiScreen
    {
        GameState game;
        BoardView board;
        SwipeInput input;

        // Header
        Image scoreBox;
        Image bestBox;
        Text scoreCaption, scoreValue;
        Text bestCaption, bestValue;
        Text modeLabel;

        // Controls
        UiButton undo, delete, shuffle, pause;

        // Overlays
        GameObject pauseOverlay, overOverlay, winOverlay;
        Text overTitle, overScore, overBest;
        Text winTitle;
        UiButton overContinue;

        Text hint;

        bool deleteMode;
        bool isDaily;
        int dailyDay;
        bool scoreSubmitted;

        public GameState Game => game;

        void Awake()
        {
            InitRoot();
            Build();
        }

        // ---- construction -----------------------------------------------------

        void Build()
        {
            Theme theme = App.I.Theme;

            BuildHeader(theme);
            BuildControls(theme);
            BuildBoardArea(theme);
            BuildHint(theme);

            input = gameObject.AddComponent<SwipeInput>();
            input.Swiped += OnSwipe;

            pauseOverlay = BuildPauseOverlay(theme);
            overOverlay = BuildGameOverOverlay(theme);
            winOverlay = BuildWinOverlay(theme);
        }

        void BuildHeader(Theme theme)
        {
            modeLabel = UIFactory.Label(Root, "", 34, theme.HeadingText,
                TextAnchor.MiddleLeft, FontStyle.Bold, "ModeLabel");
            modeLabel.rectTransform.anchorMin = new Vector2(0, 1);
            modeLabel.rectTransform.anchorMax = new Vector2(0, 1);
            modeLabel.rectTransform.pivot = new Vector2(0, 1);
            modeLabel.rectTransform.anchoredPosition = new Vector2(40, -40);
            modeLabel.rectTransform.sizeDelta = new Vector2(420, 56);

            scoreBox = ScoreBox(theme, out scoreCaption, out scoreValue, Strings.Key.Score, -450);
            bestBox = ScoreBox(theme, out bestCaption, out bestValue, Strings.Key.Best, -230);
        }

        Image ScoreBox(Theme theme, out Text caption, out Text value, string captionKey, float xOffset)
        {
            Image box = UIFactory.Panel(Root, theme.GridBackground, 12, $"Box_{captionKey}");
            var rect = (RectTransform)box.transform;
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(xOffset, -28);
            rect.sizeDelta = new Vector2(200, 112);

            caption = UIFactory.Label(box.transform, Strings.Get(captionKey), 24, theme.ButtonText,
                TextAnchor.UpperCenter, FontStyle.Bold, "Caption");
            UIFactory.Fill(caption.rectTransform, 6, 6, 12, 60);

            value = UIFactory.Label(box.transform, "0", 40, theme.ButtonText,
                TextAnchor.LowerCenter, FontStyle.Bold, "Value");
            UIFactory.Fill(value.rectTransform, 6, 6, 44, 12);

            return box;
        }

        void BuildControls(Theme theme)
        {
            HorizontalLayoutGroup row = UIFactory.Row(Root, 16, new RectOffset(36, 36, 0, 0), "Controls");
            var rect = (RectTransform)row.transform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -170);
            rect.sizeDelta = new Vector2(0, 96);

            undo = Control(row.transform, theme, Strings.Key.Undo, OnUndo);
            delete = Control(row.transform, theme, Strings.Key.Delete, OnDelete);
            shuffle = Control(row.transform, theme, Strings.Key.Shuffle, OnShuffle);
            pause = Control(row.transform, theme, Strings.Key.Paused, OnPause);
        }

        UiButton Control(Transform parent, Theme theme, string key, Action onClick)
        {
            UiButton button = UIFactory.Button(parent, Strings.Get(key), theme.ButtonFill,
                theme.ButtonText, onClick, 26, 12);
            UIFactory.SizedAs(button.GameObject, 96);
            return button;
        }

        void BuildBoardArea(Theme theme)
        {
            RectTransform area = UIFactory.Rect("BoardArea", Root);
            area.anchorMin = new Vector2(0, 0);
            area.anchorMax = new Vector2(1, 1);
            // Leaves the header and controls above, and room for a banner below.
            area.offsetMin = new Vector2(36, 180);
            area.offsetMax = new Vector2(-36, -300);

            board = BoardView.Create(area, theme);
            UIFactory.Fill(board.GetComponent<RectTransform>());

            // Keeps the grid square on every aspect ratio without maths here.
            var fitter = board.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            board.TileTapped += OnTileTapped;
        }

        void BuildHint(Theme theme)
        {
            hint = UIFactory.Label(Root, Strings.Get(Strings.Key.SwipeHint), 30, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Normal, "Hint");
            hint.rectTransform.anchorMin = new Vector2(0, 0);
            hint.rectTransform.anchorMax = new Vector2(1, 0);
            hint.rectTransform.pivot = new Vector2(0.5f, 0f);
            hint.rectTransform.anchoredPosition = new Vector2(0, 110);
            hint.rectTransform.sizeDelta = new Vector2(0, 50);
            hint.gameObject.SetActive(false);
        }

        // ---- overlays ---------------------------------------------------------

        RectTransform OverlayShade(string name, out Image panel, float panelHeight)
        {
            RectTransform shade = UIFactory.Rect(name, Root);
            UIFactory.Fill(shade);

            var blocker = shade.gameObject.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0.6f);
            blocker.sprite = SpriteFactory.White;

            panel = UIFactory.Panel(shade, App.I.Theme.PanelBackground, 20, "Panel");
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(680, panelHeight);

            shade.gameObject.SetActive(false);
            return shade;
        }

        GameObject BuildPauseOverlay(Theme theme)
        {
            RectTransform shade = OverlayShade("PauseOverlay", out Image panel, 540);

            Text title = UIFactory.Label(panel.transform, Strings.Get(Strings.Key.Paused), 56,
                theme.HeadingText, TextAnchor.MiddleCenter, FontStyle.Bold, "Title");
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0, -36);
            title.rectTransform.sizeDelta = new Vector2(0, 80);

            VerticalLayoutGroup column = UIFactory.Column(panel.transform, 18,
                new RectOffset(48, 48, 0, 0), "Buttons");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 0);
            columnRect.anchorMax = new Vector2(1, 0);
            columnRect.pivot = new Vector2(0.5f, 0f);
            columnRect.anchoredPosition = new Vector2(0, 40);
            columnRect.sizeDelta = new Vector2(0, 370);

            PanelButton(column.transform, Strings.Key.Resume, () => SetPaused(false));
            PanelButton(column.transform, Strings.Key.Restart, RestartSame);
            PanelButton(column.transform, Strings.Key.Home, GoHome);

            return shade.gameObject;
        }

        GameObject BuildGameOverOverlay(Theme theme)
        {
            RectTransform shade = OverlayShade("GameOverOverlay", out Image panel, 640);

            overTitle = UIFactory.Label(panel.transform, Strings.Get(Strings.Key.GameOver), 56,
                theme.HeadingText, TextAnchor.MiddleCenter, FontStyle.Bold, "Title");
            overTitle.rectTransform.anchorMin = new Vector2(0, 1);
            overTitle.rectTransform.anchorMax = new Vector2(1, 1);
            overTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            overTitle.rectTransform.anchoredPosition = new Vector2(0, -32);
            overTitle.rectTransform.sizeDelta = new Vector2(0, 76);

            overScore = UIFactory.Label(panel.transform, "", 40, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Score");
            overScore.rectTransform.anchorMin = new Vector2(0, 1);
            overScore.rectTransform.anchorMax = new Vector2(1, 1);
            overScore.rectTransform.pivot = new Vector2(0.5f, 1f);
            overScore.rectTransform.anchoredPosition = new Vector2(0, -116);
            overScore.rectTransform.sizeDelta = new Vector2(0, 56);

            overBest = UIFactory.Label(panel.transform, "", 30, theme.Accent,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Best");
            overBest.rectTransform.anchorMin = new Vector2(0, 1);
            overBest.rectTransform.anchorMax = new Vector2(1, 1);
            overBest.rectTransform.pivot = new Vector2(0.5f, 1f);
            overBest.rectTransform.anchoredPosition = new Vector2(0, -174);
            overBest.rectTransform.sizeDelta = new Vector2(0, 44);

            VerticalLayoutGroup column = UIFactory.Column(panel.transform, 18,
                new RectOffset(48, 48, 0, 0), "Buttons");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 0);
            columnRect.anchorMax = new Vector2(1, 0);
            columnRect.pivot = new Vector2(0.5f, 0f);
            columnRect.anchoredPosition = new Vector2(0, 40);
            columnRect.sizeDelta = new Vector2(0, 370);

            overContinue = PanelButton(column.transform, Strings.Key.KeepPlaying, OnContinueWithAd);
            PanelButton(column.transform, Strings.Key.Retry, RestartSame);
            PanelButton(column.transform, Strings.Key.Home, GoHome);

            return shade.gameObject;
        }

        GameObject BuildWinOverlay(Theme theme)
        {
            RectTransform shade = OverlayShade("WinOverlay", out Image panel, 460);

            winTitle = UIFactory.Label(panel.transform, "", 48, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Title");
            winTitle.rectTransform.anchorMin = new Vector2(0, 1);
            winTitle.rectTransform.anchorMax = new Vector2(1, 1);
            winTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            winTitle.rectTransform.anchoredPosition = new Vector2(0, -44);
            winTitle.rectTransform.sizeDelta = new Vector2(0, 90);

            VerticalLayoutGroup column = UIFactory.Column(panel.transform, 18,
                new RectOffset(48, 48, 0, 0), "Buttons");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 0);
            columnRect.anchorMax = new Vector2(1, 0);
            columnRect.pivot = new Vector2(0.5f, 0f);
            columnRect.anchoredPosition = new Vector2(0, 40);
            columnRect.sizeDelta = new Vector2(0, 250);

            PanelButton(column.transform, Strings.Key.KeepGoing, () =>
            {
                game.WinAcknowledged = true;
                winOverlay.SetActive(false);
                input.Enabled = true;
            });
            PanelButton(column.transform, Strings.Key.Home, GoHome);

            return shade.gameObject;
        }

        UiButton PanelButton(Transform parent, string key, Action onClick)
        {
            UiButton button = UIFactory.Button(parent, Strings.Get(key), App.I.Theme.ButtonFill,
                App.I.Theme.ButtonText, () =>
                {
                    App.I.Audio.PlayButton();
                    onClick();
                }, 34, 14);
            UIFactory.SizedAs(button.GameObject, 100);
            return button;
        }

        // ---- starting ---------------------------------------------------------

        public void StartNew(int size, GameMode mode)
        {
            isDaily = false;
            dailyDay = 0;
            scoreSubmitted = false;

            ulong seed = (ulong)DateTime.UtcNow.Ticks;
            game = new GameState(size, mode, seed);
            AfterStart();
        }

        public void StartDaily()
        {
            isDaily = true;
            DateTime today = DateTime.UtcNow;
            dailyDay = DailySeed.DayNumber(today);
            scoreSubmitted = false;

            game = new GameState(GameConfig.DefaultSize, GameMode.Classic, DailySeed.ForDate(today));
            AfterStart();
        }

        public bool ResumeSaved()
        {
            if (!App.I.Save.TryRestoreGame(out GameState restored)) return false;

            isDaily = false;
            scoreSubmitted = false;
            game = restored;
            AfterStart();
            return true;
        }

        void AfterStart()
        {
            deleteMode = false;
            board.Build(game.Size);
            board.Track(game.Board);
            input.Enabled = true;

            pauseOverlay.SetActive(false);
            overOverlay.SetActive(false);
            winOverlay.SetActive(false);

            ShowHintIfFirstTime();
            Refresh();
            PersistIfPlaying();
        }

        void ShowHintIfFirstTime()
        {
            bool first = !App.I.Save.Data.hasSeenSwipeHint;
            hint.gameObject.SetActive(first);
            if (!first) return;

            App.I.Save.Data.hasSeenSwipeHint = true;
            App.I.Save.Save();
        }

        // ---- input ------------------------------------------------------------

        void OnSwipe(Direction direction)
        {
            if (game == null || board.Animating) return;
            if (pauseOverlay.activeSelf || overOverlay.activeSelf || winOverlay.activeSelf) return;

            if (deleteMode) { SetDeleteMode(false); return; }

            MoveResult result = game.Move(direction);
            if (!result.Changed) return;

            hint.gameObject.SetActive(false);
            input.Enabled = false;

            App.I.Audio.PlayMove();
            if (result.HighestMerge > 0)
            {
                App.I.Audio.PlayMerge(result.HighestMerge);
                App.I.Haptics.Light();
            }

            StartCoroutine(board.AnimateMove(result, game.Board, AfterMove));
            Refresh();
        }

        void AfterMove()
        {
            input.Enabled = true;
            Refresh();
            PersistIfPlaying();

            if (game.HasWon && !game.WinAcknowledged)
            {
                ShowWin();
                return;
            }

            if (game.IsGameOver) ShowGameOver();
        }

        void OnTileTapped(int x, int y)
        {
            if (!deleteMode || game == null) return;

            if (game.UseDelete(x, y))
            {
                App.I.Audio.PlayMerge(4);
                App.I.Haptics.Medium();
                board.Track(game.Board);
            }

            SetDeleteMode(false);
            Refresh();
            PersistIfPlaying();
        }

        // ---- power-ups --------------------------------------------------------

        void OnUndo()
        {
            if (game == null || board.Animating) return;
            App.I.Audio.PlayButton();

            if (!game.CanUndo)
            {
                OfferCharges(PowerUp.Undo);
                return;
            }

            game.Undo();
            board.Track(game.Board);
            Refresh();
            PersistIfPlaying();
        }

        void OnDelete()
        {
            if (game == null || board.Animating) return;
            App.I.Audio.PlayButton();

            if (game.ChargesOf(PowerUp.Delete) <= 0)
            {
                OfferCharges(PowerUp.Delete);
                return;
            }

            SetDeleteMode(!deleteMode);
        }

        void OnShuffle()
        {
            if (game == null || board.Animating) return;
            App.I.Audio.PlayButton();

            if (game.ChargesOf(PowerUp.Shuffle) <= 0)
            {
                OfferCharges(PowerUp.Shuffle);
                return;
            }

            if (game.UseShuffle())
            {
                App.I.Haptics.Medium();
                board.Track(game.Board);
                Refresh();
                PersistIfPlaying();
            }
        }

        void SetDeleteMode(bool on)
        {
            deleteMode = on;
            hint.gameObject.SetActive(on);
            if (on) hint.text = Strings.Get(Strings.Key.TapTileToRemove);
            Refresh();
        }

        /// <summary>Out of charges: offer a rewarded ad, and say so when none is available.</summary>
        void OfferCharges(PowerUp powerUp)
        {
            App app = App.I;
            app.Ads.ShowRewarded(RewardedPlacement.ExtraCharges, granted =>
            {
                if (granted)
                {
                    game.AddCharges(powerUp, GameConfig.ChargesPerRewardedAd);
                    Refresh();
                }
                else
                {
                    Dialog.Toast(this, app.OverlayRoot, app.Theme, Strings.Get(Strings.Key.OutOfCharges));
                }
            });
        }

        // ---- overlays ---------------------------------------------------------

        void OnPause() => SetPaused(true);

        void SetPaused(bool paused)
        {
            App.I.Audio.PlayButton();
            pauseOverlay.SetActive(paused);
            input.Enabled = !paused;
            if (paused) PersistIfPlaying();
        }

        void ShowWin()
        {
            input.Enabled = false;
            winTitle.text = Strings.Get(Strings.Key.YouReached, game.WinTarget);
            winOverlay.SetActive(true);
            App.I.Audio.PlayWin();
            App.I.Haptics.Heavy();
        }

        void ShowGameOver()
        {
            input.Enabled = false;
            App.I.Audio.PlayGameOver();
            App.I.Haptics.Heavy();

            SubmitScore();

            overScore.text = Strings.Get(Strings.Key.ScoreValue, game.Score);
            overBest.text = NewBest ? Strings.Get(Strings.Key.NewBest) : "";
            overOverlay.SetActive(true);

            App.I.Save.ClearGame();

            // The interstitial, if any, belongs here and nowhere else.
            App.I.Ads.NoteGameFinished();
            App.I.Ads.TryShowInterstitial();
        }

        bool NewBest;

        void SubmitScore()
        {
            if (scoreSubmitted) return;
            scoreSubmitted = true;

            if (isDaily)
            {
                App.I.Save.RecordDaily(dailyDay, game.Score);
                NewBest = false;
                return;
            }

            NewBest = App.I.Save.SubmitScore(game.Size, game.Mode, game.Score);
        }

        /// <summary>Clears room to keep playing, paid for with a rewarded ad.</summary>
        void OnContinueWithAd()
        {
            App app = App.I;
            app.Ads.ShowRewarded(RewardedPlacement.ContinueAfterGameOver, granted =>
            {
                if (!granted)
                {
                    Dialog.Toast(this, app.OverlayRoot, app.Theme, Strings.Get(Strings.Key.AdNotReady));
                    return;
                }

                ClearSmallestTiles(4);
                scoreSubmitted = false;
                overOverlay.SetActive(false);
                input.Enabled = true;
                board.Track(game.Board);
                Refresh();
            });
        }

        void ClearSmallestTiles(int count)
        {
            var found = new List<(int x, int y, int value)>();
            for (int y = 0; y < game.Size; y++)
            for (int x = 0; x < game.Size; x++)
                if (game.Board[x, y].IsTile)
                    found.Add((x, y, game.Board[x, y].Value));

            found.Sort((a, b) => a.value.CompareTo(b.value));
            for (int i = 0; i < count && i < found.Count; i++)
                game.Board[found[i].x, found[i].y] = Cell.Empty;
        }

        void RestartSame()
        {
            if (isDaily) StartDaily();
            else StartNew(game.Size, game.Mode);
        }

        void GoHome()
        {
            PersistIfPlaying();
            App.I.GoHome();
        }

        // ---- state ------------------------------------------------------------

        /// <summary>Called on pause, focus loss and quit, so a kill never loses a game.</summary>
        public void PersistIfPlaying()
        {
            if (game == null || isDaily) return;
            if (game.IsGameOver) return;
            App.I.Save.StoreGame(game);
        }

        void Refresh()
        {
            if (game == null) return;

            scoreValue.text = game.Score.ToString();
            bestValue.text = isDaily
                ? App.I.Save.Data.dailyBestScore.ToString()
                : App.I.Save.BestScore(game.Size, game.Mode).ToString();

            modeLabel.text = isDaily
                ? $"{Strings.Get(Strings.Key.DailyChallenge)}"
                : $"{Strings.Get(game.Mode == GameMode.Classic ? Strings.Key.ModeClassic : Strings.Key.ModeStones)} {Strings.SizeLabel(game.Size)}";

            undo.Text = $"{Strings.Get(Strings.Key.Undo)} {game.ChargesOf(PowerUp.Undo)}";
            delete.Text = $"{Strings.Get(Strings.Key.Delete)} {game.ChargesOf(PowerUp.Delete)}";
            shuffle.Text = $"{Strings.Get(Strings.Key.Shuffle)} {game.ChargesOf(PowerUp.Shuffle)}";

            Theme theme = App.I.Theme;
            Color dim = new Color(theme.ButtonFill.r, theme.ButtonFill.g, theme.ButtonFill.b, 0.35f);

            undo.Tint(game.CanUndo ? theme.ButtonFill : dim, theme.ButtonText);
            delete.Tint(deleteMode ? theme.Accent
                : game.ChargesOf(PowerUp.Delete) > 0 ? theme.ButtonFill : dim, theme.ButtonText);
            shuffle.Tint(game.ChargesOf(PowerUp.Shuffle) > 0 ? theme.ButtonFill : dim, theme.ButtonText);
        }

        protected override void OnShow()
        {
            if (game != null)
            {
                board.Track(game.Board);
                Refresh();
            }
        }

        protected override void OnHide() => PersistIfPlaying();

        public override bool OnBack()
        {
            if (deleteMode) { SetDeleteMode(false); return true; }
            if (winOverlay.activeSelf) return true;

            if (overOverlay.activeSelf)
            {
                GoHome();
                return true;
            }

            if (pauseOverlay.activeSelf)
            {
                SetPaused(false);
                return true;
            }

            SetPaused(true);
            return true;
        }

        public override void ApplyTheme()
        {
            Theme theme = App.I.Theme;
            if (board != null) board.SetTheme(theme);

            scoreBox.color = theme.GridBackground;
            bestBox.color = theme.GridBackground;
            scoreCaption.color = theme.ButtonText;
            scoreValue.color = theme.ButtonText;
            bestCaption.color = theme.ButtonText;
            bestValue.color = theme.ButtonText;
            modeLabel.color = theme.HeadingText;
            hint.color = theme.HeadingText;

            if (overTitle != null) overTitle.color = theme.HeadingText;
            if (overScore != null) overScore.color = theme.HeadingText;
            if (overBest != null) overBest.color = theme.Accent;
            if (winTitle != null) winTitle.color = theme.HeadingText;

            if (game != null)
            {
                board.Track(game.Board);
                Refresh();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Automation seams for the screenshot pass. Editor-only by compilation,
        /// so none of this can reach a player build.
        /// </summary>
        /// <summary>
        /// Starts a seeded game the capture script can then drive with real
        /// moves — store screenshots show positions the rules actually produce,
        /// not hand-placed boards that could never occur.
        /// </summary>
        public void CaptureStart(int size, GameMode mode, ulong seed)
        {
            isDaily = false;
            scoreSubmitted = false;
            game = new GameState(size, mode, seed);
            deleteMode = false;
            board.Build(size);
            board.Track(game.Board);
            CaptureHideOverlays();
            hint.gameObject.SetActive(false);
            Refresh();
        }

        /// <summary>Applies a move instantly, with no animation to wait on.</summary>
        public void CapturePlay(Direction direction)
        {
            if (game == null) return;
            game.Move(direction);
            board.Track(game.Board);
            Refresh();
        }

        public void CaptureShowGameOver(int score)
        {
            overScore.text = Strings.Get(Strings.Key.ScoreValue, score);
            overBest.text = Strings.Get(Strings.Key.NewBest);
            overOverlay.SetActive(true);
        }

        public void CaptureShowWin(int target)
        {
            winTitle.text = Strings.Get(Strings.Key.YouReached, target);
            winOverlay.SetActive(true);
        }

        public void CaptureHideOverlays()
        {
            overOverlay.SetActive(false);
            winOverlay.SetActive(false);
            pauseOverlay.SetActive(false);
        }
#endif
    }
}
