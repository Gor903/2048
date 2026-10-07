using System.Globalization;
using Tilevault.Core;
using Tilevault.Game.Localisation;
using Tilevault.Game.Screens;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Tilevault.Game
{
    /// <summary>
    /// Composition root. Builds the camera, canvas, services and every screen in
    /// <see cref="Awake"/>, so the scene file holds a single object and the whole
    /// interface is reviewable as code.
    /// </summary>
    public sealed class App : MonoBehaviour
    {
        public static App I { get; private set; }

        public SaveService Save { get; private set; }
        public ThemeService Themes { get; private set; }
        public AudioService Audio { get; private set; }
        public HapticsService Haptics { get; private set; }
        public IAdService Ads { get; private set; }

        public ScreenStack Stack { get; private set; }

        /// <summary>Screens live here, inside the safe area.</summary>
        public RectTransform SafeRoot { get; private set; }

        /// <summary>Dialogs and toasts live here, above every screen.</summary>
        public RectTransform OverlayRoot { get; private set; }

        public Theme Theme => Themes.Current;

        HomeScreen home;
        GameScreen game;
        ModeScreen mode;
        ThemesScreen themesScreen;
        SettingsScreen settings;

        Image background;
        Camera cam;

        void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;

            SelectLanguage();
            BuildServices();
            BuildCamera();
            BuildCanvas();
            BuildScreens();

            Themes.Changed += ApplyTheme;
            ApplyTheme();

            Ads.Init(null);
            Stack.Replace(home);
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        /// <summary>
        /// Chosen once at startup from the system locale. Anything not shipped
        /// falls back to English inside <see cref="Strings"/>.
        /// </summary>
        static void SelectLanguage()
        {
            string code;
            try
            {
                code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            }
            catch
            {
                code = Strings.Lang_English;
            }
            Strings.SelectLanguage(code);
        }

        void BuildServices()
        {
            Save = new SaveService();
            Themes = new ThemeService(Save);
            Haptics = new HapticsService(Save);
            Ads = new NullAdService();

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            Audio = new AudioService(audioSource, Save);
        }

        void BuildCamera()
        {
            var go = new GameObject("Camera", typeof(Camera));
            go.transform.SetParent(transform, false);
            cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.Background;
            cam.orthographic = true;
            cam.cullingMask = 0;   // the canvas is screen-space overlay; nothing to render in 3D
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            // Match width: the game is portrait-locked, and a tall screen should
            // give more vertical room rather than shrink everything.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            background = UIFactory.Panel(canvasGo.transform, Theme.Background, 0, "Background");
            UIFactory.Fill(background.rectTransform);
            background.raycastTarget = false;

            SafeRoot = UIFactory.Rect("SafeArea", canvasGo.transform);
            UIFactory.Fill(SafeRoot);
            SafeRoot.gameObject.AddComponent<SafeAreaFitter>();

            OverlayRoot = UIFactory.Rect("Overlays", canvasGo.transform);
            UIFactory.Fill(OverlayRoot);

            EnsureEventSystem();
        }

        /// <summary>
        /// The project runs with the legacy input manager disabled, so the UI
        /// needs the Input System's module specifically — the standalone module
        /// would silently receive nothing.
        /// </summary>
        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(go);
        }

        void BuildScreens()
        {
            Stack = new ScreenStack();

            home = Make<HomeScreen>("HomeScreen");
            game = Make<GameScreen>("GameScreen");
            mode = Make<ModeScreen>("ModeScreen");
            themesScreen = Make<ThemesScreen>("ThemesScreen");
            settings = Make<SettingsScreen>("SettingsScreen");
        }

        T Make<T>(string name) where T : UiScreen
        {
            RectTransform rt = UIFactory.Rect(name, SafeRoot);
            var screen = rt.gameObject.AddComponent<T>();
            screen.gameObject.SetActive(false);
            return screen;
        }

        // ---- navigation -------------------------------------------------------

        public void GoHome() => Stack.Replace(home);
        public void OpenMode() => Stack.Push(mode);
        public void OpenThemes() => Stack.Push(themesScreen);
        public void OpenSettings() => Stack.Push(settings);
        public void Back() => Stack.Pop();

        public void StartGame(int size, GameMode gameMode)
        {
            game.StartNew(size, gameMode);
            Stack.Replace(game);
        }

        public void ResumeSavedGame()
        {
            if (!game.ResumeSaved())
            {
                StartGame(Save.Data.preferredSize, (GameMode)Save.Data.preferredMode);
                return;
            }
            Stack.Replace(game);
        }

        public void StartDaily()
        {
            game.StartDaily();
            Stack.Replace(game);
        }

        public void ApplyTheme()
        {
            background.color = Theme.Background;
            if (cam != null) cam.backgroundColor = Theme.Background;
            Stack.ApplyTheme();

            // Hidden screens restyle too, so switching back is never a flash of
            // the previous palette.
            home.ApplyTheme();
            game.ApplyTheme();
            mode.ApplyTheme();
            themesScreen.ApplyTheme();
            settings.ApplyTheme();
        }

        // ---- lifecycle --------------------------------------------------------

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            // Android routes its back button to Escape through the Input System.
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                HandleBack();
        }

        void HandleBack()
        {
            if (Stack.Current != null && Stack.Current.OnBack()) return;
            if (Stack.Pop()) return;

            Dialog.Confirm(OverlayRoot, Theme,
                Strings.Get(Strings.Key.ExitGame),
                Strings.Get(Strings.Key.Yes),
                Strings.Get(Strings.Key.No),
                Application.Quit);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) game.PersistIfPlaying();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) game.PersistIfPlaying();
        }

        void OnApplicationQuit()
        {
            game.PersistIfPlaying();
        }
    }
}
