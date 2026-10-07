using NUnit.Framework;
using Tilevault.Core;

namespace Tilevault.Tests
{
    public class BoardTests
    {
        static void AssertBoard(Board board, params string[] expected)
        {
            Assert.AreEqual(string.Join(" / ", expected), BoardLayout.Flat(board));
        }

        // ---- sliding ----------------------------------------------------------

        [Test]
        public void Slide_CompactsTilesTowardTheEdge()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                ". 2 . 4");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsTrue(r.Changed);
            Assert.AreEqual(0, r.ScoreGained, "Sliding without merging scores nothing.");
            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "2 4 . .");
        }

        [Test]
        public void Slide_InEveryDirection()
        {
            foreach (Direction d in new[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down })
            {
                var board = BoardLayout.Parse(
                    ". . . .",
                    ". 2 . .",
                    ". . . .",
                    ". . . .");

                MoveResult r = board.Move(d);
                Assert.IsTrue(r.Changed, $"A lone tile must be able to move {d}.");
                Assert.AreEqual(1, CountTiles(board), "Sliding must not create or destroy tiles.");
            }
        }

        static int CountTiles(Board b)
        {
            int n = 0;
            for (int y = 0; y < b.Size; y++)
            for (int x = 0; x < b.Size; x++)
                if (b[x, y].IsTile) n++;
            return n;
        }

        [Test]
        public void Move_ThatChangesNothing_IsInvalid()
        {
            var board = BoardLayout.Parse(
                "2 4 8 16",
                ". . . .",
                ". . . .",
                ". . . .");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsFalse(r.Changed, "Tiles already against the edge with no merges is not a move.");
            Assert.AreEqual(0, r.ScoreGained);
            Assert.IsEmpty(r.Moves);
        }

        // ---- merging ----------------------------------------------------------

        [Test]
        public void Merge_TwoEqualTiles_Double()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "2 2 . .");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsTrue(r.Changed);
            Assert.AreEqual(4, r.ScoreGained, "Score gains the value produced, not the values consumed.");
            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "4 . . .");
        }

        [Test]
        public void Merge_OnlyOncePerMove()
        {
            // The canonical case: 4 4 8 must give 8 8, never 16.
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "4 4 8 .");

            MoveResult r = board.Move(Direction.Left);

            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "8 8 . .");
            Assert.AreEqual(8, r.ScoreGained, "Only the one merge scores.");
        }

        [Test]
        public void Merge_FourEqualTiles_FormTwoPairs()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "2 2 2 2");

            MoveResult r = board.Move(Direction.Left);

            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "4 4 . .");
            Assert.AreEqual(8, r.ScoreGained);
        }

        [Test]
        public void Merge_PairsFromTheLeadingEdgeFirst()
        {
            // Swiping left, the leftmost pair merges: 2 2 2 -> 4 2, not 2 4.
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "2 2 2 .");

            board.Move(Direction.Left);

            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "4 2 . .");
        }

        [Test]
        public void Merge_RightwardPairsFromTheRightEdge()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                ". 2 2 2");

            board.Move(Direction.Right);

            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                ". . 2 4");
        }

        [Test]
        public void Merge_ReportsHighestMergeValue()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                "8 8 . .",
                "2 2 . .");

            MoveResult r = board.Move(Direction.Left);

            Assert.AreEqual(16, r.HighestMerge);
            Assert.AreEqual(20, r.ScoreGained, "4 from the twos plus 16 from the eights.");
        }

        // ---- stones -----------------------------------------------------------

        [Test]
        public void Stone_BlocksSlidingAndPreventsMergeAcrossIt()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "2 S 2 .");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsFalse(r.Changed, "Nothing can move: the stone pins both tiles.");
            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "2 S 2 .");
        }

        [Test]
        public void Stone_HoldsItsCellWhileTilesCompactAroundIt()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                ". 2 S 2");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsTrue(r.Changed);
            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "2 . S 2");
        }

        [Test]
        public void Stone_NeverMerges()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "S S . .");

            MoveResult r = board.Move(Direction.Left);

            Assert.IsFalse(r.Changed);
            Assert.AreEqual(0, r.ScoreGained);
            AssertBoard(board,
                ". . . .",
                ". . . .",
                ". . . .",
                "S S . .");
        }

        [Test]
        public void Stone_CrumblesWhenItsCounterRunsOut()
        {
            var board = BoardLayout.Parse(
                ". . . .",
                ". . . .",
                ". . . .",
                "S2 . . .");

            var removed = new System.Collections.Generic.List<GridPos>();

            board.AgeStones(removed);
            Assert.IsEmpty(removed, "One move left: still standing.");
            Assert.IsTrue(board[0, 0].IsStone);

            board.AgeStones(removed);
            Assert.AreEqual(1, removed.Count, "Counter exhausted: the stone is gone.");
            Assert.IsTrue(board[0, 0].IsEmpty);
        }

        // ---- end conditions ---------------------------------------------------

        [Test]
        public void GameOver_WhenFullWithNoAdjacentPair()
        {
            var board = BoardLayout.Parse(
                "2 4 2 4",
                "4 2 4 2",
                "2 4 2 4",
                "4 2 4 2");

            Assert.IsFalse(board.HasMoves());
        }

        [Test]
        public void NotGameOver_WhenFullButAPairIsAdjacent()
        {
            var board = BoardLayout.Parse(
                "2 4 2 4",
                "4 2 4 2",
                "2 4 2 4",
                "4 2 4 4");

            Assert.IsTrue(board.HasMoves());
        }

        [Test]
        public void NotGameOver_WhileACellIsEmpty()
        {
            var board = BoardLayout.Parse(
                "2 4 2 4",
                "4 2 4 2",
                "2 4 2 4",
                "4 2 4 .");

            Assert.IsTrue(board.HasMoves());
        }

        [Test]
        public void GameOver_StonesDoNotCountAsMergeablePairs()
        {
            var board = BoardLayout.Parse(
                "2 4 2 4",
                "4 2 4 2",
                "2 4 2 4",
                "4 2 S S");

            Assert.IsFalse(board.HasMoves(), "Two adjacent stones are not a pair.");
        }

        // ---- sizes ------------------------------------------------------------

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void Board_SupportsEverySupportedSize(int size)
        {
            var board = new Board(size);
            board[0, 0] = Cell.Tile(2);
            board[1, 0] = Cell.Tile(2);

            MoveResult r = board.Move(Direction.Left);

            Assert.IsTrue(r.Changed);
            Assert.AreEqual(4, board[0, 0].Value);
        }

        [TestCase(2)]
        [TestCase(7)]
        public void Board_RejectsUnsupportedSizes(int size)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Board(size));
        }
    }
}
