using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tilevault.Core;

namespace Tilevault.Tests
{
    public class RngTests
    {
        [Test]
        public void SameSeed_SameSequence()
        {
            var a = new SplitMix64Rng(2024);
            var b = new SplitMix64Rng(2024);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(a.NextInt(1000), b.NextInt(1000), $"diverged at draw {i}");
        }

        [Test]
        public void RestoringState_ResumesTheSameSequence()
        {
            var rng = new SplitMix64Rng(7);
            for (int i = 0; i < 10; i++) rng.NextInt(100);

            ulong saved = rng.State;
            var expected = new List<int>();
            for (int i = 0; i < 20; i++) expected.Add(rng.NextInt(100));

            rng.State = saved;
            for (int i = 0; i < 20; i++)
                Assert.AreEqual(expected[i], rng.NextInt(100), $"diverged at draw {i} after restore");
        }

        [Test]
        public void NextInt_StaysInRange()
        {
            var rng = new SplitMix64Rng(99);
            for (int i = 0; i < 20000; i++)
            {
                int v = rng.NextInt(16);
                Assert.That(v, Is.InRange(0, 15));
            }
        }

        [Test]
        public void NextInt_CoversEveryValue()
        {
            var rng = new SplitMix64Rng(1234);
            var seen = new HashSet<int>();
            for (int i = 0; i < 5000; i++) seen.Add(rng.NextInt(16));

            Assert.AreEqual(16, seen.Count, "Every cell index must be reachable.");
        }

        [Test]
        public void NextInt_RejectsNonPositiveBounds()
        {
            var rng = new SplitMix64Rng(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(-5));
        }

        [Test]
        public void NextDouble_StaysInUnitInterval()
        {
            var rng = new SplitMix64Rng(31337);
            for (int i = 0; i < 20000; i++)
            {
                double d = rng.NextDouble();
                Assert.That(d, Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
            }
        }

        [Test]
        public void NextDouble_RoughlyMatchesTheSpawnRatio()
        {
            // Not a statistics test — a smoke check that 10% means roughly 10%.
            var rng = new SplitMix64Rng(5150);
            int fours = 0;
            const int draws = 100000;
            for (int i = 0; i < draws; i++)
                if (rng.NextDouble() < GameConfig.SpawnFourProbability) fours++;

            double ratio = (double)fours / draws;
            Assert.That(ratio, Is.EqualTo(GameConfig.SpawnFourProbability).Within(0.01));
        }
    }

    public class DailySeedTests
    {
        /// <summary>Local, because BitOperations is not in Unity's .NET Standard profile.</summary>
        static int PopCount(ulong v)
        {
            int n = 0;
            while (v != 0)
            {
                v &= v - 1;
                n++;
            }
            return n;
        }

        [Test]
        public void SameDate_SameSeed()
        {
            Assert.AreEqual(DailySeed.ForDate(2026, 10, 8), DailySeed.ForDate(2026, 10, 8));
        }

        [Test]
        public void AdjacentDates_UnrelatedSeeds()
        {
            ulong a = DailySeed.ForDate(2026, 10, 8);
            ulong b = DailySeed.ForDate(2026, 10, 9);

            Assert.AreNotEqual(a, b);

            // Consecutive days must not produce near-identical boards, so insist
            // the mixed values differ in a good share of their bits.
            Assert.Greater(PopCount(a ^ b), 16, "Day-to-day seeds should avalanche, not increment.");
        }

        [Test]
        public void SeedProducesAStableBoard()
        {
            ulong seed = DailySeed.ForDate(2026, 1, 1);
            var first = new GameState(4, GameMode.Classic, seed);
            var second = new GameState(4, GameMode.Classic, seed);

            Assert.AreEqual(BoardLayout.Flat(first.Board), BoardLayout.Flat(second.Board),
                "Everyone playing that date must get the same opening.");
        }

        [Test]
        public void DayNumber_IsOrderedAndUnique()
        {
            Assert.Less(DailySeed.DayNumber(new DateTime(2026, 10, 8)),
                        DailySeed.DayNumber(new DateTime(2026, 10, 9)));
            Assert.Less(DailySeed.DayNumber(new DateTime(2026, 12, 31)),
                        DailySeed.DayNumber(new DateTime(2027, 1, 1)));
        }
    }
}
