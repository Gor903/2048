using System.Collections.Generic;
using System.IO;
using Tilevault.Core;
using Tilevault.Game;
using Tilevault.Game.Screens;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Editor
{
    /// <summary>
    /// Captures store screenshots by building the real interface and rendering
    /// it to a texture — the frames show positions the rules actually produce,
    /// and the whole set regenerates with one command when the art changes.
    ///
    /// No frame ticks in the editor, so every step forces its own layout pass
    /// rather than waiting for one.
    /// </summary>
    public static class ScreenshotTool
    {
        const int Width = 1080;
        const int Height = 1920;
        public const string OutputDirectory = "Publishing/art/screenshots";

        [MenuItem("Tilevault/Art/Capture Screenshots")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(OutputDirectory);

            var shots = new List<string>();

            shots.Add(Capture("01-game-4x4", app =>
            {
                GameScreen screen = Screen(app);
                app.Stack.Replace(screen);
                // A seeded game driven by real moves, so the board is a position
                // the rules can actually reach.
                app.Save.SubmitScore(4, GameMode.Classic, 4120);
                screen.CaptureStart(4, GameMode.Classic, 20481);
                PlayPattern(screen, 90);
            }));

            shots.Add(Capture("02-game-bigger-tiles", app =>
            {
                GameScreen screen = Screen(app);
                app.Stack.Replace(screen);
                app.Save.SubmitScore(4, GameMode.Classic, 21880);
                screen.CaptureStart(4, GameMode.Classic, 77003);
                PlayPattern(screen, 260);
            }));

            shots.Add(Capture("03-stones-mode", app =>
            {
                GameScreen screen = Screen(app);
                app.Stack.Replace(screen);
                app.Save.SubmitScore(5, GameMode.Stones, 9640);
                screen.CaptureStart(5, GameMode.Stones, 31337);
                // Under the stone lifetime, or the screenshot for Stones mode
                // shows a board with every stone already crumbled away.
                PlayPattern(screen, 9);
            }));

            shots.Add(Capture("04-dark-theme", app =>
            {
                app.Themes.Unlock(Theme.Dark);
                app.Themes.Select(Theme.Dark);
                GameScreen screen = Screen(app);
                app.Stack.Replace(screen);
                app.Save.SubmitScore(4, GameMode.Classic, 15300);
                screen.CaptureStart(4, GameMode.Classic, 909);
                PlayPattern(screen, 150);
            }));

            shots.Add(Capture("05-home", app =>
            {
                app.Save.SubmitScore(4, GameMode.Classic, 18640);
                app.GoHome();
            }));

            shots.Add(Capture("06-game-over", app =>
            {
                GameScreen screen = Screen(app);
                app.Stack.Replace(screen);
                app.Save.SubmitScore(4, GameMode.Classic, 12480);
                screen.CaptureStart(4, GameMode.Classic, 5150);
                PlayPattern(screen, 200);
                screen.CaptureShowGameOver(12480);
            }));

            AssetDatabase.Refresh();
            Debug.Log($"[Tilevault] Captured {shots.Count} screenshots into {OutputDirectory}");
        }

        static GameScreen Screen(App app) => app.GetComponentInChildren<GameScreen>(true);

        /// <summary>
        /// Drives the board with a repeating direction cycle. Deterministic, so
        /// re-running produces the same frames and the diff stays small.
        /// </summary>
        static void PlayPattern(GameScreen screen, int moves)
        {
            Direction[] cycle = { Direction.Left, Direction.Down, Direction.Right, Direction.Down };
            for (int i = 0; i < moves; i++)
            {
                if (screen.Game != null && screen.Game.IsGameOver) break;
                screen.CapturePlay(cycle[i % cycle.Length]);
            }
        }

        static string Capture(string name, System.Action<App> arrange)
        {
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4
            };

            GameObject appGo = null;
            GameObject camGo = null;

            try
            {
                appGo = new GameObject("CaptureApp");
                var app = appGo.AddComponent<App>();
                app.EditorBootstrap();

                camGo = new GameObject("CaptureCamera");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = app.Theme.Background;
                cam.targetTexture = rt;
                cam.cullingMask = ~0;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 100f;

                // Screen-space-camera lets the canvas render through a camera and
                // therefore into a texture; overlay canvases cannot be captured.
                Canvas canvas = app.Canvas;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 10f;

                // The device safe area belongs to the editor window, not to the
                // frame being captured, so it must not shrink the layout here.
                var fitter = app.SafeRoot.GetComponent<SafeAreaFitter>();
                if (fitter != null) Object.DestroyImmediate(fitter);
                UIFactory.Fill(app.SafeRoot);

                // Fix the canvas to the capture resolution rather than the editor's.
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
                var canvasRect = (RectTransform)canvas.transform;
                canvasRect.sizeDelta = new Vector2(Width, Height);

                // Captures share a save file, so without this a theme chosen for
                // one shot silently repaints every shot that follows it.
                app.Save.ResetAll();
                app.Themes.Select(Theme.Classic);
                app.ApplyTheme();

                Settle(app);
                arrange(app);
                Settle(app);

                GameScreen screen = Screen(app);
                if (screen != null && screen.Visible) screen.CaptureRelayout();
                Settle(app);

                cam.backgroundColor = app.Theme.Background;
                cam.Render();

                string path = Path.Combine(OutputDirectory, name + ".png");
                WriteTexture(rt, path);
                return path;
            }
            finally
            {
                if (camGo != null) Object.DestroyImmediate(camGo);
                if (appGo != null) Object.DestroyImmediate(appGo);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        /// <summary>Forces the layout the editor would otherwise never run.</summary>
        static void Settle(App app)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)app.Canvas.transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(app.SafeRoot);
            Canvas.ForceUpdateCanvases();
        }

        static void WriteTexture(RenderTexture rt, string path)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        public static void CI()
        {
            CaptureAll();
            EditorApplication.Exit(0);
        }
    }
}
