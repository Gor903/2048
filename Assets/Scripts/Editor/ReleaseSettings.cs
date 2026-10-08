namespace Tilevault.Editor
{
    /// <summary>
    /// The single source of truth for everything the release build depends on.
    /// These are the values Apply writes and Verify checks, so the two can never
    /// drift apart.
    /// </summary>
    public static class ReleaseSettings
    {
        // Irreversible once published — see docs/DECISIONS.md.
        public const string ApplicationId = "com.gor903.tilevault2048";
        public const string ProductName = "Tilevault";
        public const string CompanyName = "Gor903";

        /// <summary>Shown to users. The version code is the one the store orders by.</summary>
        public const string VersionName = "1.0.0";

        /// <summary>
        /// Android 8.0. Not a preference: Unity 6000.6's AndroidSdkVersions enum
        /// starts at 26, so a lower value is silently clamped rather than
        /// rejected. Adaptive icons are native from here, which is what this
        /// project ships.
        /// </summary>
        public const int MinSdk = 26;
        public const int TargetSdk = 36;     // Play rejects new submissions below this since 31 August 2026

        public const string ScenePath = "Assets/Scenes/Main.unity";

        /// <summary>
        /// Lives in Assets/Plugins/Android. Its absence is a build-blocking
        /// problem, not a warning: deleting it silently changes the permission
        /// set the store shows users.
        /// </summary>
        public const string ManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

        public const string IconPath = "Assets/Art/Generated/icon_1024.png";
        public const string AdaptiveForegroundPath = "Assets/Art/Generated/icon_fg.png";
        public const string AdaptiveBackgroundPath = "Assets/Art/Generated/icon_bg.png";

        public const string OutputDirectory = "Build";
        public const string BundleName = "tilevault.aab";

        /// <summary>
        /// False while the shipping build contains no ad SDK. The game opens no
        /// socket, so declaring INTERNET would contradict the privacy policy and
        /// the data-safety form, and users would see a permission the game never
        /// uses.
        ///
        /// Adding the AdMob SDK flips three things together, and they must move
        /// together or the build is dishonest: this constant, the
        /// <c>tools:node="remove"</c> entry in the custom manifest, and the
        /// data-safety answers in the submission pack.
        /// </summary>
        public const bool RequiresInternet = false;
    }
}
