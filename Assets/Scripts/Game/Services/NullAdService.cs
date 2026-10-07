using System;
using Tilevault.Core;
using UnityEngine;

namespace Tilevault.Game.Services
{
    /// <summary>
    /// The shipping-safe default: no SDK, no network, no ads. Rewarded calls
    /// fail politely rather than granting the reward, so the no-ads path is the
    /// same code path players with no connection take.
    ///
    /// It still runs the real <see cref="InterstitialPolicy"/>, so the frequency
    /// rules are exercised in every play session whether or not an SDK is present.
    /// </summary>
    public sealed class NullAdService : IAdService
    {
        readonly InterstitialPolicy policy = new InterstitialPolicy();

        public bool Initialised { get; private set; }
        public bool ConsentResolved => true;

        /// <summary>Set by the editor screenshot pass so banners never appear in store art.</summary>
        public static bool Silent;

        public void Init(Action onReady)
        {
            Initialised = true;
            onReady?.Invoke();
        }

        public void ShowBanner() { }
        public void HideBanner() { }

        public bool TryShowInterstitial()
        {
            if (!policy.ShouldShow(Time.realtimeSinceStartupAsDouble)) return false;

            // A real implementation would show here; the policy is still advanced
            // so the pacing matches what players would see with ads enabled.
            policy.NoteShown(Time.realtimeSinceStartupAsDouble);
            return false;
        }

        public void ShowRewarded(RewardedPlacement placement, Action<bool> onReward)
        {
            onReward?.Invoke(false);
        }

        public void ShowPrivacyOptions() { }

        public void NoteGameFinished() => policy.NoteGameFinished();
    }
}
