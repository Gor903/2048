using System;

namespace Tilevault.Game.Services
{
    public enum RewardedPlacement
    {
        ExtraCharges,
        ContinueAfterGameOver,
        UnlockTheme
    }

    /// <summary>
    /// Every ad in the game goes through this one seam, so ads can be disabled,
    /// swapped or stubbed from a single place — and so the game builds, runs and
    /// tests with no ad SDK present at all.
    /// </summary>
    public interface IAdService
    {
        bool Initialised { get; }
        bool ConsentResolved { get; }

        void Init(Action onReady);

        void ShowBanner();
        void HideBanner();

        /// <summary>Consults the frequency policy; returns false when it declined.</summary>
        bool TryShowInterstitial();

        /// <summary>
        /// <paramref name="onReward"/> runs only if the ad completed. Called with
        /// false when there was no fill, no network, or the player dismissed it.
        /// </summary>
        void ShowRewarded(RewardedPlacement placement, Action<bool> onReward);

        /// <summary>Re-opens the consent form from Settings.</summary>
        void ShowPrivacyOptions();

        void NoteGameFinished();
    }
}
