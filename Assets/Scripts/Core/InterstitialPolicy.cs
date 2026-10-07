namespace Tilevault.Core
{
    /// <summary>
    /// Decides whether an interstitial may appear. Pure and engine-free so the
    /// frequency rules are unit-testable — these are the rules most likely to be
    /// quietly broken by a later change, and the ones players notice first.
    ///
    /// Interstitials appear after a game over and nowhere else; the caller
    /// enforces that by only consulting this at game over.
    /// </summary>
    public sealed class InterstitialPolicy
    {
        /// <summary>No ad during the opening games of a session.</summary>
        public const int FreeGamesAtSessionStart = 2;

        /// <summary>At most one ad per this many finished games.</summary>
        public const int GamesBetweenAds = 3;

        public const double SecondsBetweenAds = 120.0;

        int gamesThisSession;
        int gamesSinceLastAd;
        double lastAdTime;
        bool everShown;

        public int GamesThisSession => gamesThisSession;

        public void NoteGameFinished()
        {
            gamesThisSession++;
            gamesSinceLastAd++;
        }

        /// <param name="now">Seconds since session start, monotonic.</param>
        public bool ShouldShow(double now)
        {
            if (gamesThisSession <= FreeGamesAtSessionStart) return false;

            if (everShown)
            {
                if (gamesSinceLastAd < GamesBetweenAds) return false;
                if (now - lastAdTime < SecondsBetweenAds) return false;
            }

            return true;
        }

        public void NoteShown(double now)
        {
            lastAdTime = now;
            gamesSinceLastAd = 0;
            everShown = true;
        }

        public void ResetSession()
        {
            gamesThisSession = 0;
            gamesSinceLastAd = 0;
            lastAdTime = 0;
            everShown = false;
        }
    }
}
