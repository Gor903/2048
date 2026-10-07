using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tilevault.Editor
{
    /// <summary>
    /// Apply, Verify and Build — each reachable from the menu and from the
    /// command line. Nothing the release depends on is a checkbox someone has to
    /// remember ticking; it is all set here by script.
    ///
    /// Build calls Verify first and refuses to run when Verify returns anything.
    /// </summary>
    public static class ReleaseTool
    {
        // ---- apply ------------------------------------------------------------

        [MenuItem("Tilevault/Release/Apply Settings")]
        public static void Apply()
        {
            PlayerSettings.companyName = ReleaseSettings.CompanyName;
            PlayerSettings.productName = ReleaseSettings.ProductName;
            PlayerSettings.SetApplicationIdentifier(
                UnityEditor.Build.NamedBuildTarget.Android, ReleaseSettings.ApplicationId);

            // Portrait, locked. A sliding-tile board has no landscape layout.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)ReleaseSettings.MinSdk;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)ReleaseSettings.TargetSdk;

            // 64-bit is mandatory on the store; 32-bit keeps older minSdk devices.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(
                UnityEditor.Build.NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetManagedStrippingLevel(
                UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.High);

            // Draw inside the cutout rather than letterboxing around it; the
            // interface keeps itself clear with SafeAreaFitter.
            PlayerSettings.Android.renderOutsideSafeArea = true;

            PlayerSettings.Android.forceInternetPermission = ReleaseSettings.RequiresInternet;
            PlayerSettings.Android.forceSDCardPermission = false;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan, UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ReleaseSettings.ScenePath, true)
            };

            PlayerSettings.bundleVersion = ReleaseSettings.VersionName;
            // Only ever climbs: the store rejects a code it has already seen.
            if (PlayerSettings.Android.bundleVersionCode < 1)
                PlayerSettings.Android.bundleVersionCode = 1;

            ApplyIcons();

            AssetDatabase.SaveAssets();
            Debug.Log("[Tilevault] Release settings applied.");
        }

        /// <summary>
        /// Adaptive is the only non-obsolete Android icon kind in Unity 6 —
        /// Legacy and Round both redirect to it — and it takes two layers rather
        /// than one flattened image.
        /// </summary>
        static void ApplyIcons()
        {
            var target = UnityEditor.Build.NamedBuildTarget.Android;

            var flat = AssetDatabase.LoadAssetAtPath<Texture2D>(ReleaseSettings.IconPath);
            if (flat != null)
            {
                int[] sizes = PlayerSettings.GetIconSizes(target, IconKind.Application);
                var icons = new Texture2D[sizes.Length];
                for (int i = 0; i < icons.Length; i++) icons[i] = flat;
                PlayerSettings.SetIcons(target, icons, IconKind.Application);
            }

            var foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(ReleaseSettings.AdaptiveForegroundPath);
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(ReleaseSettings.AdaptiveBackgroundPath);
            if (foreground == null || background == null) return;

            PlatformIconKind kind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
            PlatformIcon[] adaptive = PlayerSettings.GetPlatformIcons(target, kind);

            foreach (PlatformIcon icon in adaptive)
                icon.SetTextures(background, foreground);

            PlayerSettings.SetPlatformIcons(target, kind, adaptive);
        }

        // ---- verify -----------------------------------------------------------

        /// <summary>
        /// Returns a list of problems. Empty means the project is releasable.
        /// Checks the same things Apply sets, plus the ones that go wrong
        /// silently — a deleted manifest, a blank icon, a debug flag left on.
        /// </summary>
        public static List<string> Verify()
        {
            var problems = new List<string>();
            var target = UnityEditor.Build.NamedBuildTarget.Android;

            string id = PlayerSettings.GetApplicationIdentifier(target);
            if (id != ReleaseSettings.ApplicationId)
                problems.Add($"Application id is '{id}', expected '{ReleaseSettings.ApplicationId}'.");

            if (PlayerSettings.productName != ReleaseSettings.ProductName)
                problems.Add($"Product name is '{PlayerSettings.productName}', expected '{ReleaseSettings.ProductName}'.");

            if (PlayerSettings.companyName != ReleaseSettings.CompanyName)
                problems.Add($"Company name is '{PlayerSettings.companyName}', expected '{ReleaseSettings.CompanyName}'.");

            if ((int)PlayerSettings.Android.minSdkVersion != ReleaseSettings.MinSdk)
                problems.Add($"minSdk is {(int)PlayerSettings.Android.minSdkVersion}, expected {ReleaseSettings.MinSdk}.");

            if ((int)PlayerSettings.Android.targetSdkVersion != ReleaseSettings.TargetSdk)
                problems.Add($"targetSdk is {(int)PlayerSettings.Android.targetSdkVersion}, expected {ReleaseSettings.TargetSdk}.");

            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
                problems.Add("ARM64 is not in the target architectures; the store requires a 64-bit library.");

            if (PlayerSettings.GetScriptingBackend(target) != ScriptingImplementation.IL2CPP)
                problems.Add("Scripting backend is not IL2CPP.");

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
                problems.Add("Default orientation is not Portrait.");

            if (PlayerSettings.allowedAutorotateToLandscapeLeft || PlayerSettings.allowedAutorotateToLandscapeRight)
                problems.Add("Landscape autorotation is still allowed.");

            if (!EditorUserBuildSettings.buildAppBundle)
                problems.Add("Output format is APK; the store needs an app bundle.");

            if (EditorUserBuildSettings.development)
                problems.Add("Development build is on.");

            if (EditorUserBuildSettings.allowDebugging)
                problems.Add("Script debugging is on.");

            if (EditorUserBuildSettings.connectProfiler)
                problems.Add("Autoconnect profiler is on.");

            if (PlayerSettings.Android.forceInternetPermission != ReleaseSettings.RequiresInternet)
                problems.Add($"forceInternetPermission is {PlayerSettings.Android.forceInternetPermission}, " +
                             $"expected {ReleaseSettings.RequiresInternet}.");

            // The trap this check exists for: deleting the manifest restores the
            // network permission with no warning anywhere.
            if (!File.Exists(ReleaseSettings.ManifestPath))
                problems.Add($"Custom manifest is missing at {ReleaseSettings.ManifestPath}; " +
                             "without it Unity adds the network permission back.");
            else
            {
                string manifest = File.ReadAllText(ReleaseSettings.ManifestPath);
                if (!ReleaseSettings.RequiresInternet && !manifest.Contains("permission.INTERNET"))
                    problems.Add("Custom manifest no longer removes the INTERNET permission.");
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (scenes.Length != 1)
                problems.Add($"Build list holds {scenes.Length} enabled scenes, expected exactly 1.");
            else if (scenes[0].path != ReleaseSettings.ScenePath)
                problems.Add($"Build list holds '{scenes[0].path}', expected '{ReleaseSettings.ScenePath}'.");

            if (!File.Exists(ReleaseSettings.ScenePath))
                problems.Add($"Scene missing at {ReleaseSettings.ScenePath}.");

            problems.AddRange(VerifyIcons(target));

            if (PlayerSettings.Android.bundleVersionCode <= 0)
                problems.Add("Bundle version code must be a positive integer.");

            if (PlayerSettings.bundleVersion != ReleaseSettings.VersionName)
                problems.Add($"Version name is '{PlayerSettings.bundleVersion}', " +
                             $"expected '{ReleaseSettings.VersionName}'.");

            return problems;
        }

        static IEnumerable<string> VerifyIcons(UnityEditor.Build.NamedBuildTarget target)
        {
            PlatformIconKind kind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
            PlatformIcon[] adaptive = PlayerSettings.GetPlatformIcons(target, kind);

            if (adaptive == null || adaptive.Length == 0)
                yield return "No adaptive icon slots were returned; the Android module may be missing.";
            else if (adaptive.Any(i => i.GetTextures().All(t => t == null)))
                yield return "An adaptive icon layer is empty; the build would ship Unity's default icon.";

            foreach (string path in new[]
                     {
                         ReleaseSettings.IconPath,
                         ReleaseSettings.AdaptiveForegroundPath,
                         ReleaseSettings.AdaptiveBackgroundPath
                     })
            {
                if (!File.Exists(path))
                    yield return $"Icon source missing at {path}.";
                else if (new FileInfo(path).Length == 0)
                    yield return $"Icon at {path} is an empty file.";
            }
        }

        [MenuItem("Tilevault/Release/Verify")]
        public static void VerifyMenu()
        {
            List<string> problems = Verify();
            if (problems.Count == 0)
            {
                Debug.Log("[Tilevault] Verify passed: the project is releasable.");
                return;
            }

            Debug.LogError($"[Tilevault] Verify found {problems.Count} problem(s):\n  - " +
                           string.Join("\n  - ", problems));
        }

        // ---- build ------------------------------------------------------------

        [MenuItem("Tilevault/Release/Build Bundle")]
        public static void BuildMenu() => Build();

        public static bool Build()
        {
            List<string> problems = Verify();
            if (problems.Count > 0)
            {
                Debug.LogError($"[Tilevault] BUILD REFUSED — {problems.Count} preflight problem(s):\n  - " +
                               string.Join("\n  - ", problems));
                return false;
            }

            Directory.CreateDirectory(ReleaseSettings.OutputDirectory);

            bool signed = ApplySigningFromEnvironment(out string signingNote);
            string fileName = signed
                ? ReleaseSettings.BundleName
                : Path.GetFileNameWithoutExtension(ReleaseSettings.BundleName) + "-UNSIGNED.aab";
            string output = Path.Combine(ReleaseSettings.OutputDirectory, fileName);

            Debug.Log($"[Tilevault] Building {output} ({signingNote})");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ReleaseSettings.ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                // Passwords never linger in the project settings file.
                WipeSigningFromProjectSettings();
            }

            bool ok = report.summary.result == BuildResult.Succeeded;
            Debug.Log(ok
                ? $"[Tilevault] Build succeeded: {output}"
                : $"[Tilevault] Build FAILED: {report.summary.result}, {report.summary.totalErrors} error(s)");

            // The size in a build report measures the intermediate project, not
            // the artifact. Read the file itself instead.
            if (ok && File.Exists(output))
                Debug.Log($"[Tilevault] Artifact on disk: {new FileInfo(output).Length} bytes");

            return ok;
        }

        /// <summary>
        /// Signing credentials come from the environment, never from a file. An
        /// unsigned build is still produced, but named so nobody can mistake it
        /// for a releasable one.
        /// </summary>
        static bool ApplySigningFromEnvironment(out string note)
        {
            string keystore = Environment.GetEnvironmentVariable("TILEVAULT_KEYSTORE");
            string keystorePass = Environment.GetEnvironmentVariable("TILEVAULT_KEYSTORE_PASS");
            string alias = Environment.GetEnvironmentVariable("TILEVAULT_KEY_ALIAS");
            string aliasPass = Environment.GetEnvironmentVariable("TILEVAULT_KEY_PASS");

            if (string.IsNullOrEmpty(keystore) || string.IsNullOrEmpty(keystorePass) ||
                string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(aliasPass))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                note = "unsigned — no TILEVAULT_KEYSTORE* variables in the environment";
                return false;
            }

            if (!File.Exists(keystore))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                note = $"unsigned — keystore not found at {keystore}";
                return false;
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPass;

            note = $"signed with alias '{alias}'";
            return true;
        }

        static void WipeSigningFromProjectSettings()
        {
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasPass = "";
            AssetDatabase.SaveAssets();
        }

        // ---- command line -------------------------------------------------------

        public static void CIApply()
        {
            Apply();
            AssetDatabase.SaveAssets();
            EditorApplication.Exit(0);
        }

        public static void CIVerify()
        {
            List<string> problems = Verify();
            foreach (string p in problems) Debug.LogError($"[Tilevault] PREFLIGHT: {p}");

            Debug.Log(problems.Count == 0
                ? "[Tilevault] PREFLIGHT OK"
                : $"[Tilevault] PREFLIGHT FAILED with {problems.Count} problem(s)");

            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        public static void CIBuild()
        {
            bool ok = Build();
            EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>Every upload needs a higher code; bumped here, never by hand.</summary>
        public static void CIBumpVersionCode()
        {
            PlayerSettings.Android.bundleVersionCode++;
            AssetDatabase.SaveAssets();
            Debug.Log($"[Tilevault] Version code is now {PlayerSettings.Android.bundleVersionCode}");
            EditorApplication.Exit(0);
        }
    }
}
