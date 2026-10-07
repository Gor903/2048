using System.Collections;
using System.IO;
using NUnit.Framework;
using Tilevault.Core;
using Tilevault.Game;
using Tilevault.Game.Presentation;
using Tilevault.Game.Screens;
using Tilevault.Game.Services;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tilevault.Tests.PlayMode
{
    /// <summary>
    /// Brings the real composition root up, drives it, and checks the things
    /// only a running player can show: that the interface is actually built,
    /// that input reaches the rules, and that a game survives being killed.
    /// </summary>
    public class AppPlayModeTests
    {
        App app;

        static string SavePath => Path.Combine(Application.persistentDataPath, "tilevault.save.json");

        static void WipeSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WipeSave();
            app = NewApp();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (app != null) Object.Destroy(app.gameObject);
            yield return null;
            WipeSave();
        }

        static App NewApp()
        {
            var go = new GameObject("App");
            return go.AddComponent<App>();
        }

        GameScreen Screen => app.GetComponentInChildren<GameScreen>(true);
        SwipeInput Input => Screen.GetComponent<SwipeInput>();

        static string Describe(Board board)
        {
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < board.Size; y++)
            for (int x = 0; x < board.Size; x++)
                sb.Append(board[x, y]).Append(',');
            return sb.ToString();
        }

        static int CountTiles(Board b)
        {
            int n = 0;
            for (int y = 0; y < b.Size; y++)
            for (int x = 0; x < b.Size; x++)
                if (b[x, y].IsTile) n++;
            return n;
        }

        // ---- the interface actually exists ------------------------------------

        [UnityTest]
        public IEnumerator App_BuildsItsCanvasAndLandsOnHome()
        {
            yield return null;

            var canvas = app.GetComponentInChildren<Canvas>();
            Assert.IsNotNull(canvas, "No canvas was built.");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);

            Assert.IsNotNull(app.SafeRoot);
            Assert.IsInstanceOf<HomeScreen>(app.Stack.Current);
        }

        [UnityTest]
        public IEnumerator App_RendersTextWithAUsableFont()
        {
            yield return null;

            Text[] labels = app.GetComponentsInChildren<Text>(true);
            Assert.Greater(labels.Length, 0, "No labels were built at all.");

            foreach (Text label in labels)
                Assert.IsNotNull(label.font, $"Label '{label.name}' has no font, so it would render blank.");
        }

        // ---- input reaches the rules ------------------------------------------

        [UnityTest]
        public IEnumerator StartingAGame_DealsTheOpeningPosition()
        {
            app.StartGame(4, GameMode.Classic);
            yield return null;

            Assert.IsInstanceOf<GameScreen>(app.Stack.Current);
            Assert.AreEqual(2, CountTiles(Screen.Game.Board));
            Assert.AreEqual(0, Screen.Game.Score);
        }

        [UnityTest]
        public IEnumerator Swiping_MovesTilesAndSpawnsAReplacement()
        {
            app.StartGame(4, GameMode.Classic);
            yield return null;

            string before = Describe(Screen.Game.Board);

            // Try each direction until one is a legal move from this opening.
            foreach (Direction d in new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down })
            {
                Input.Simulate(d);
                yield return new WaitForSeconds(BoardView.MoveDuration + 0.25f);

                if (Describe(Screen.Game.Board) != before)
                {
                    Assert.AreEqual(3, CountTiles(Screen.Game.Board),
                        "Two opening tiles plus one spawned.");
                    yield break;
                }
            }

            Assert.Fail("No direction produced a legal move from the opening position.");
        }

        [UnityTest]
        public IEnumerator BoardView_DrawsOneObjectPerOccupiedCell()
        {
            app.StartGame(4, GameMode.Classic);
            yield return null;
            yield return null;   // let the layout pass settle the grid

            var view = app.GetComponentInChildren<BoardView>(true);
            TileView[] drawn = view.GetComponentsInChildren<TileView>(false);

            Assert.AreEqual(CountTiles(Screen.Game.Board), drawn.Length,
                "The view and the rules disagree about how many tiles exist.");
        }

        // ---- the restart gate --------------------------------------------------

        [UnityTest]
        public IEnumerator AGameInProgress_SurvivesTheProcessBeingKilled()
        {
            app.StartGame(4, GameMode.Classic);
            yield return null;

            // Make a real move so the saved state is not just the opening.
            foreach (Direction d in new[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down })
            {
                Input.Simulate(d);
                yield return new WaitForSeconds(BoardView.MoveDuration + 0.25f);
                if (Screen.Game.MovesMade > 0) break;
            }

            Assert.Greater(Screen.Game.MovesMade, 0, "Could not make a move to save.");

            string expected = Describe(Screen.Game.Board);
            int expectedScore = Screen.Game.Score;

            Screen.PersistIfPlaying();

            // Kill it the way the OS would, then start over from the save file.
            Object.Destroy(app.gameObject);
            yield return null;

            app = NewApp();
            yield return null;

            Assert.IsTrue(app.Save.Data.hasSavedGame, "Nothing was written to disk.");

            app.ResumeSavedGame();
            yield return null;

            Assert.AreEqual(expected, Describe(Screen.Game.Board), "The restored board differs.");
            Assert.AreEqual(expectedScore, Screen.Game.Score, "The restored score differs.");
        }

        [UnityTest]
        public IEnumerator SettingsSurviveARestart()
        {
            app.Audio.Enabled = false;
            app.Haptics.Enabled = false;
            yield return null;

            Object.Destroy(app.gameObject);
            yield return null;

            app = NewApp();
            yield return null;

            Assert.IsFalse(app.Audio.Enabled, "Sound setting was not persisted.");
            Assert.IsFalse(app.Haptics.Enabled, "Vibration setting was not persisted.");
        }

        [UnityTest]
        public IEnumerator BestScoreIsKeptPerSizeAndMode()
        {
            app.Save.SubmitScore(4, GameMode.Classic, 5000);
            yield return null;

            Assert.AreEqual(5000, app.Save.BestScore(4, GameMode.Classic));
            Assert.AreEqual(0, app.Save.BestScore(4, GameMode.Stones), "Modes must not share a slot.");
            Assert.AreEqual(0, app.Save.BestScore(5, GameMode.Classic), "Sizes must not share a slot.");
        }

        // ---- themes ------------------------------------------------------------

        [UnityTest]
        public IEnumerator ChangingTheme_RepaintsAndPersists()
        {
            app.StartGame(4, GameMode.Classic);
            yield return null;

            Assert.AreEqual("classic", app.Theme.Id);

            app.Themes.Select(Theme.Dark);
            yield return null;

            Assert.AreEqual("dark", app.Theme.Id);

            Object.Destroy(app.gameObject);
            yield return null;
            app = NewApp();
            yield return null;

            Assert.AreEqual("dark", app.Theme.Id, "Theme choice was not persisted.");
        }

        [UnityTest]
        public IEnumerator LockedThemesCannotBeSelected()
        {
            yield return null;

            Assert.IsFalse(app.Themes.IsUnlocked(Theme.Sunset));
            Assert.IsFalse(app.Themes.Select(Theme.Sunset), "A locked theme must refuse selection.");
            Assert.AreEqual("classic", app.Theme.Id);
        }

        // ---- daily --------------------------------------------------------------

        [UnityTest]
        public IEnumerator DailyChallenge_IsTheSameBoardTwiceRunning()
        {
            app.StartDaily();
            yield return null;
            string first = Describe(Screen.Game.Board);

            app.StartDaily();
            yield return null;
            string second = Describe(Screen.Game.Board);

            Assert.AreEqual(first, second, "The daily board must not change between attempts.");
        }

        [UnityTest]
        public IEnumerator DailyChallenge_IsNotSavedAsAResumableGame()
        {
            app.StartDaily();
            yield return null;

            Screen.PersistIfPlaying();

            Assert.IsFalse(app.Save.Data.hasSavedGame,
                "Resuming the daily from a save would let a player retry a scored attempt.");
        }
    }
}
