using NUnit.Framework;
using Tilevault.Core;

namespace Tilevault.Tests
{
    /// <summary>
    /// The ad frequency rules from the design, as tests. These are the rules a
    /// later change is most likely to break silently, and the ones a player
    /// notices first when they go wrong.
    /// </summary>
    public class InterstitialPolicyTests
    {
        static InterstitialPolicy AfterGames(int count)
        {
            var policy = new InterstitialPolicy();
            for (int i = 0; i < count; i++) policy.NoteGameFinished();
            return policy;
        }

        [Test]
        public void NoAd_DuringTheOpeningGamesOfASession()
        {
            Assert.IsFalse(AfterGames(1).ShouldShow(0), "First game of a session.");
            Assert.IsFalse(AfterGames(2).ShouldShow(0), "Second game of a session.");
        }

        [Test]
        public void FirstAd_AllowedOnTheThirdGame()
        {
            Assert.IsTrue(AfterGames(3).ShouldShow(0));
        }

        [Test]
        public void NoSecondAd_UntilThreeMoreGames()
        {
            InterstitialPolicy policy = AfterGames(3);
            Assert.IsTrue(policy.ShouldShow(0));
            policy.NoteShown(0);

            // Far enough apart in time that only the game count can refuse it.
            double later = InterstitialPolicy.SecondsBetweenAds * 10;

            policy.NoteGameFinished();
            Assert.IsFalse(policy.ShouldShow(later), "1 game since the last ad.");
            policy.NoteGameFinished();
            Assert.IsFalse(policy.ShouldShow(later), "2 games since the last ad.");
            policy.NoteGameFinished();
            Assert.IsTrue(policy.ShouldShow(later), "3 games since the last ad.");
        }

        [Test]
        public void NoSecondAd_WithinTheCooldown()
        {
            InterstitialPolicy policy = AfterGames(3);
            policy.NoteShown(100);

            // Plenty of games, but not enough elapsed time.
            for (int i = 0; i < 10; i++) policy.NoteGameFinished();

            Assert.IsFalse(policy.ShouldShow(100 + InterstitialPolicy.SecondsBetweenAds - 1));
            Assert.IsTrue(policy.ShouldShow(100 + InterstitialPolicy.SecondsBetweenAds + 1));
        }

        [Test]
        public void BothConditionsMustHold()
        {
            InterstitialPolicy policy = AfterGames(3);
            policy.NoteShown(0);

            policy.NoteGameFinished();   // only 1 game since, but lots of time
            Assert.IsFalse(policy.ShouldShow(99999));

            policy.NoteGameFinished();
            policy.NoteGameFinished();   // 3 games since, but no time elapsed
            Assert.IsFalse(policy.ShouldShow(1));
        }

        [Test]
        public void NewSession_StartsFreeAgain()
        {
            InterstitialPolicy policy = AfterGames(5);
            policy.NoteShown(0);

            policy.ResetSession();

            Assert.IsFalse(policy.ShouldShow(99999), "A fresh session gets its free games back.");
            Assert.AreEqual(0, policy.GamesThisSession);
        }

        [Test]
        public void AdNeverPrecedesAFinishedGame()
        {
            var policy = new InterstitialPolicy();
            Assert.IsFalse(policy.ShouldShow(99999), "No game has ended yet.");
        }
    }
}
