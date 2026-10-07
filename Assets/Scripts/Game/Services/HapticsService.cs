using UnityEngine;

namespace Tilevault.Game.Services
{
    /// <summary>
    /// Short vibrations on merges and game over. Uses Android's VibrationEffect
    /// where available so a merge is a tick rather than the half-second buzz
    /// <c>Handheld.Vibrate</c> produces, and degrades to nothing elsewhere.
    /// </summary>
    public sealed class HapticsService
    {
        readonly SaveService save;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject vibrator;
        bool supportsEffects;
#endif

        public HapticsService(SaveService save)
        {
            this.save = save;
            Initialise();
        }

        public bool Enabled
        {
            get => save.Data.vibrationOn;
            set
            {
                save.Data.vibrationOn = value;
                save.Save();
            }
        }

        void Initialise()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    supportsEffects = version.GetStatic<int>("SDK_INT") >= 26;
                }
            }
            catch
            {
                // A device without a vibrator is not an error — haptics just do nothing.
                vibrator = null;
            }
#endif
        }

        void Buzz(long milliseconds, int amplitude)
        {
            if (!Enabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (vibrator == null) return;
            try
            {
                if (supportsEffects)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>(
                               "createOneShot", milliseconds, amplitude))
                    {
                        vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", milliseconds);
                }
            }
            catch
            {
                // Ignore: haptics are decoration, never a failure path.
            }
#endif
        }

        public void Light() => Buzz(12, 60);
        public void Medium() => Buzz(22, 120);
        public void Heavy() => Buzz(45, 200);
    }
}
