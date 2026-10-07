using System.Collections.Generic;
using NUnit.Framework;
using Tilevault.Core;

namespace Tilevault.Tests
{
    public class GameStateTests
    {
        static readonly Direction[] AllDirections =
            { Direction.Left, Direction.Right, Direction.Up, Direction.Down };

        static int CountTiles(Board b)
        {
            int n = 0;
            for (int y = 0; y < b.Size; y++)
            for (int x = 0; x < b.Size; x++)
                if (b[x, y].IsTile) n++;
            return n;
        }

        // ---- opening ----------------------------------------------------------

        [Test]
        public void NewGame_StartsWithTwoTiles()
        {
            var game = new GameState(4, GameMode.Classic, seed: 12345);

            Assert.AreEqual(2, CountTiles(game.Board));
            Assert.AreEqual(0, game.Score);
            Assert.IsFalse(game.HasWon);
            Assert.IsFalse(game.IsGameOver);
        }

        [Test]
        public void NewGame_SpawnsOnlyTwosAndFours()
        {
            for (ulong seed = 1; seed <= 50; seed++)
            {
                var game = new GameState(4, GameMode.Classic, seed);
                for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    Cell c = game.Board[x, y];
                    if (c.IsTile)
                        Assert.That(c.Value, Is.EqualTo(2).Or.EqualTo(4), $"seed {seed} spawned {c.Value}");
                }
            }
        }

        [Test]
        public void NewGame_StonesMode_SeedsTheExpectedStoneCount()
        {
            for (int size = GameConfig.MinSize; size <= GameConfig.MaxSize; size++)
            {
                var game = new GameState(size, GameMode.Stones, seed: 99);
                int stones = 0;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (game.Board[x, y].IsStone) stones++;

                Assert.AreEqual(GameConfig.StoneCount(size), stones, $"size {size}");
            }
        }

        // ---- spawning ---------------------------------------------------------

        [Test]
        public void ValidMove_SpawnsExactlyOneTile()
        {
            var game = new GameState(4, GameMode.Classic, seed: 7);
            int before = CountTiles(game.Board);

            MoveResult r = PlayFirstValidMove(game);

            Assert.IsTrue(r.Changed);
            Assert.IsNotNull(r.Spawn);
            // One spawn, minus one tile for each merge that happened.
            int merges = r.Moves.FindAll(m => m.Merged).Count;
            Assert.AreEqual(before + 1 - merges, CountTiles(game.Board));
        }

        [Test]
        public void InvalidMove_SpawnsNothingAndScoresNothing()
        {
            var game = new GameState(4, GameMode.Classic, seed: 3);

            // Pin a known position where Left is provably not a move.
            game.Board.Clear();
            game.Board[0, 0] = Cell.Tile(2);
            game.Board[0, 1] = Cell.Tile(4);

            int tilesBefore = CountTiles(game.Board);
            int scoreBefore = game.Score;

            MoveResult r = game.Move(Direction.Left);

            Assert.IsFalse(r.Changed);
            Assert.IsNull(r.Spawn);
            Assert.AreEqual(tilesBefore, CountTiles(game.Board));
            Assert.AreEqual(scoreBefore, game.Score);
        }

        static MoveResult PlayFirstValidMove(GameState game)
        {
            foreach (Direction d in AllDirections)
            {
                MoveResult r = game.Move(d);
                if (r.Changed) return r;
            }
            Assert.Fail("No valid move available from the opening position.");
            return null;
        }

        // ---- determinism ------------------------------------------------------

        [Test]
        public void SameSeed_ProducesIdenticalGames()
        {
            string a = PlayScript(seed: 4242);
            string b = PlayScript(seed: 4242);

            Assert.AreEqual(a, b, "The daily challenge depends on this being byte-identical.");
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentGames()
        {
            Assert.AreNotEqual(PlayScript(seed: 1), PlayScript(seed: 2));
        }

        static string PlayScript(ulong seed)
        {
            var game = new GameState(4, GameMode.Classic, seed);
            // A fixed, repeating move pattern so only the RNG can vary the outcome.
            for (int i = 0; i < 40; i++)
                game.Move(AllDirections[i % AllDirections.Length]);
            return BoardLayout.Flat(game.Board) + " | " + game.Score;
        }

        // ---- undo -------------------------------------------------------------

        [Test]
        public void Undo_RestoresBoardAndScore()
        {
            var game = new GameState(4, GameMode.Classic, seed: 808);
            PlayFirstValidMove(game);

            string boardBefore = BoardLayout.Flat(game.Board);
            int scoreBefore = game.Score;

            MoveResult moved = null;
            foreach (Direction d in AllDirections)
            {
                moved = game.Move(d);
                if (moved.Changed) break;
            }
            Assert.IsTrue(moved.Changed);
            Assert.AreNotEqual(boardBefore, BoardLayout.Flat(game.Board));

            Assert.IsTrue(game.Undo());
            Assert.AreEqual(boardBefore, BoardLayout.Flat(game.Board));
            Assert.AreEqual(scoreBefore, game.Score);
        }

        [Test]
        public void Undo_RestoresRngSoTheReplayIsIdentical()
        {
            var game = new GameState(4, GameMode.Classic, seed: 55);
            PlayFirstValidMove(game);

            // Find a direction that is a real move, play it, remember the outcome.
            Direction chosen = default;
            string afterFirst = null;
            foreach (Direction d in AllDirections)
            {
                MoveResult r = game.Move(d);
                if (r.Changed)
                {
                    chosen = d;
                    afterFirst = BoardLayout.Flat(game.Board);
                    break;
                }
            }
            Assert.IsNotNull(afterFirst);

            Assert.IsTrue(game.Undo());
            MoveResult again = game.Move(chosen);

            Assert.IsTrue(again.Changed);
            Assert.AreEqual(afterFirst, BoardLayout.Flat(game.Board),
                "Replaying the undone move must spawn the same tile in the same cell.");
        }

        [Test]
        public void Undo_SpendsACharge()
        {
            var game = new GameState(4, GameMode.Classic, seed: 11);
            PlayFirstValidMove(game);

            int before = game.ChargesOf(PowerUp.Undo);
            Assert.IsTrue(game.Undo());
            Assert.AreEqual(before - 1, game.ChargesOf(PowerUp.Undo));
        }

        [Test]
        public void Undo_RefusedWithoutHistory()
        {
            var game = new GameState(4, GameMode.Classic, seed: 11);
            Assert.IsFalse(game.CanUndo);
            Assert.IsFalse(game.Undo());
        }

        [Test]
        public void Undo_RefusedWithoutCharges()
        {
            var game = new GameState(4, GameMode.Classic, seed: 11);
            PlayFirstValidMove(game);

            while (game.ChargesOf(PowerUp.Undo) > 0 && game.CanUndo)
                game.Undo();

            Assert.IsFalse(game.Undo(), "Out of undo charges.");
        }

        [Test]
        public void Undo_HistoryIsBounded()
        {
            var game = new GameState(4, GameMode.Classic, seed: 606);
            game.AddCharges(PowerUp.Undo, 1000);

            for (int i = 0; i < GameConfig.UndoDepth * 3; i++)
                game.Move(AllDirections[i % AllDirections.Length]);

            int undone = 0;
            while (game.Undo()) undone++;

            Assert.LessOrEqual(undone, GameConfig.UndoDepth,
                "History must not grow without bound.");
        }

        // ---- power-ups --------------------------------------------------------

        [Test]
        public void Delete_RemovesTheTileAndSpendsACharge()
        {
            var game = new GameState(4, GameMode.Classic, seed: 21);
            game.Board.Clear();
            game.Board[1, 1] = Cell.Tile(8);

            int before = game.ChargesOf(PowerUp.Delete);
            Assert.IsTrue(game.UseDelete(1, 1));

            Assert.IsTrue(game.Board[1, 1].IsEmpty);
            Assert.AreEqual(before - 1, game.ChargesOf(PowerUp.Delete));
        }

        [Test]
        public void Delete_DoesNotSpawnAReplacement()
        {
            var game = new GameState(4, GameMode.Classic, seed: 21);
            game.Board.Clear();
            game.Board[1, 1] = Cell.Tile(8);
            game.Board[2, 2] = Cell.Tile(4);

            game.UseDelete(1, 1);

            Assert.AreEqual(1, CountTiles(game.Board), "Deleting is not a move, so nothing spawns.");
        }

        [Test]
        public void Delete_RejectsEmptyCellsAndStones()
        {
            var game = new GameState(4, GameMode.Classic, seed: 21);
            game.Board.Clear();
            game.Board[0, 0] = Cell.Stone(5);

            Assert.IsFalse(game.UseDelete(3, 3), "Empty cell.");
            Assert.IsFalse(game.UseDelete(0, 0), "Stones are not removable by the power-up.");
        }

        [Test]
        public void Delete_IsUndoable()
        {
            var game = new GameState(4, GameMode.Classic, seed: 21);
            game.Board.Clear();
            game.Board[1, 1] = Cell.Tile(8);

            game.UseDelete(1, 1);
            Assert.IsTrue(game.Board[1, 1].IsEmpty);

            Assert.IsTrue(game.Undo());
            Assert.AreEqual(8, game.Board[1, 1].Value);
            Assert.AreEqual(GameConfig.PowerUpChargesPerGame, game.ChargesOf(PowerUp.Delete),
                "Undoing a delete hands the delete charge back.");
        }

        [Test]
        public void Shuffle_KeepsTheSameTileValuesAndLeavesStonesAlone()
        {
            var game = new GameState(4, GameMode.Stones, seed: 77);
            game.Board.Clear();
            game.Board[0, 0] = Cell.Tile(2);
            game.Board[1, 0] = Cell.Tile(4);
            game.Board[2, 0] = Cell.Tile(8);
            game.Board[3, 3] = Cell.Stone(9);

            Assert.IsTrue(game.UseShuffle());

            var values = new List<int>();
            for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                if (game.Board[x, y].IsTile) values.Add(game.Board[x, y].Value);

            values.Sort();
            CollectionAssert.AreEqual(new[] { 2, 4, 8 }, values, "Shuffle moves tiles, it does not change them.");

            Assert.IsTrue(game.Board[3, 3].IsStone, "Stones stay put.");
            Assert.AreEqual(9, game.Board[3, 3].StoneMovesLeft, "And keep their timer.");
        }

        [Test]
        public void Shuffle_RefusedWithoutCharges()
        {
            var game = new GameState(4, GameMode.Classic, seed: 77);
            while (game.ChargesOf(PowerUp.Shuffle) > 0)
                game.UseShuffle();

            Assert.IsFalse(game.UseShuffle());
        }

        // ---- win and lose -----------------------------------------------------

        [Test]
        public void Win_RaisedWhenTheTargetTileAppears()
        {
            var game = new GameState(4, GameMode.Classic, seed: 5);
            game.Board.Clear();
            game.Board[0, 0] = Cell.Tile(1024);
            game.Board[1, 0] = Cell.Tile(1024);

            Assert.IsFalse(game.HasWon);
            MoveResult r = game.Move(Direction.Left);

            Assert.IsTrue(r.Changed);
            Assert.IsTrue(game.HasWon);
            Assert.AreEqual(2048, r.HighestMerge);
        }

        [Test]
        public void Win_DoesNotEndTheGame()
        {
            var game = new GameState(4, GameMode.Classic, seed: 5);
            game.Board.Clear();
            game.Board[0, 0] = Cell.Tile(1024);
            game.Board[1, 0] = Cell.Tile(1024);
            game.Move(Direction.Left);

            Assert.IsTrue(game.HasWon);
            Assert.IsFalse(game.IsGameOver, "Keep going must remain possible.");
        }

        [TestCase(3, 256)]
        [TestCase(4, 2048)]
        [TestCase(5, 4096)]
        [TestCase(6, 8192)]
        public void WinTarget_ScalesWithBoardSize(int size, int expected)
        {
            var game = new GameState(size, GameMode.Classic, seed: 1);
            Assert.AreEqual(expected, game.WinTarget);
        }

        [Test]
        public void Move_RefusedOnceTheGameIsOver()
        {
            var game = new GameState(4, GameMode.Classic, seed: 1);
            game.Board.LoadFrom(BoardLayout.Parse(
                "2 4 2 4",
                "4 2 4 2",
                "2 4 2 4",
                "4 2 4 2").ToArray());

            Assert.IsTrue(game.IsGameOver);
            MoveResult r = game.Move(Direction.Left);
            Assert.IsFalse(r.Changed);
        }

        // ---- the gate: a whole session, no scene -------------------------------

        [Test]
        public void FullSession_PlaysFromOpeningToGameOver()
        {
            var game = new GameState(4, GameMode.Classic, seed: 20481);
            var rng = new SplitMix64Rng(999);

            int guard = 0;
            while (!game.IsGameOver)
            {
                Assert.Less(++guard, 20000, "A 4x4 game must terminate.");
                game.Move(AllDirections[rng.NextInt(AllDirections.Length)]);
            }

            Assert.Greater(game.Score, 0, "Any completed session scores something.");
            Assert.IsFalse(game.Board.HasMoves());
            Assert.AreEqual(16, CountTiles(game.Board), "A finished 4x4 Classic board is full of tiles.");
        }

        [Test]
        public void FullSession_StonesModeAlsoTerminates()
        {
            var game = new GameState(5, GameMode.Stones, seed: 31337);
            var rng = new SplitMix64Rng(2);

            int guard = 0;
            while (!game.IsGameOver)
            {
                Assert.Less(++guard, 20000, "A stones game must terminate.");
                game.Move(AllDirections[rng.NextInt(AllDirections.Length)]);
            }

            Assert.Greater(game.MovesMade, 0);
        }

        [Test]
        public void Stones_CrumbleOverTheCourseOfAGame()
        {
            var game = new GameState(4, GameMode.Stones, seed: 404);
            var rng = new SplitMix64Rng(8);

            int stonesAtStart = CountStones(game.Board);
            Assert.Greater(stonesAtStart, 0);

            for (int i = 0; i < GameConfig.StoneLifetimeMoves * 4 && !game.IsGameOver; i++)
                game.Move(AllDirections[rng.NextInt(AllDirections.Length)]);

            Assert.AreEqual(0, CountStones(game.Board),
                "Every seeded stone outlives its counter and goes.");
        }

        static int CountStones(Board b)
        {
            int n = 0;
            for (int y = 0; y < b.Size; y++)
            for (int x = 0; x < b.Size; x++)
                if (b[x, y].IsStone) n++;
            return n;
        }
    }
}
