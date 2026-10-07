using Tilevault.Game.Localisation;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Screens
{
    public sealed class SettingsScreen : UiScreen
    {
        Text heading;
        UiButton sound;
        UiButton vibration;
        UiButton privacy;
        UiButton reset;
        UiButton back;

        void Awake()
        {
            InitRoot();
            Build();
        }

        void Build()
        {
            App app = App.I;
            Theme theme = app.Theme;

            heading = UIFactory.Label(Root, Strings.Get(Strings.Key.Settings), 64, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Heading");
            heading.rectTransform.anchorMin = new Vector2(0, 1);
            heading.rectTransform.anchorMax = new Vector2(1, 1);
            heading.rectTransform.pivot = new Vector2(0.5f, 1f);
            heading.rectTransform.anchoredPosition = new Vector2(0, -60);
            heading.rectTransform.sizeDelta = new Vector2(0, 90);

            VerticalLayoutGroup column = UIFactory.Column(Root, 24, new RectOffset(70, 70, 0, 0), "Options");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 1);
            columnRect.anchorMax = new Vector2(1, 1);
            columnRect.pivot = new Vector2(0.5f, 1f);
            columnRect.anchoredPosition = new Vector2(0, -200);
            columnRect.sizeDelta = new Vector2(0, 560);

            sound = Option(column.transform, () =>
            {
                app.Audio.Enabled = !app.Audio.Enabled;
                app.Audio.PlayButton();
                Refresh();
            });

            vibration = Option(column.transform, () =>
            {
                app.Haptics.Enabled = !app.Haptics.Enabled;
                if (app.Haptics.Enabled) app.Haptics.Medium();
                app.Audio.PlayButton();
                Refresh();
            });

            privacy = Option(column.transform, () =>
            {
                app.Audio.PlayButton();
                app.Ads.ShowPrivacyOptions();
            });

            reset = Option(column.transform, () =>
            {
                app.Audio.PlayButton();
                Dialog.Confirm(app.OverlayRoot, app.Theme,
                    Strings.Get(Strings.Key.ResetConfirm),
                    Strings.Get(Strings.Key.Reset),
                    Strings.Get(Strings.Key.Cancel),
                    () =>
                    {
                        app.Save.ResetAll();
                        app.Themes.Select(Theme.Classic);
                        app.ApplyTheme();
                        Refresh();
                    });
            });

            back = UIFactory.Button(Root, Strings.Get(Strings.Key.BackToHome),
                theme.ButtonFill, theme.ButtonText, () =>
                {
                    app.Audio.PlayButton();
                    app.Back();
                }, 36, 14);
            back.RectTransform.anchorMin = new Vector2(0.5f, 0f);
            back.RectTransform.anchorMax = new Vector2(0.5f, 0f);
            back.RectTransform.pivot = new Vector2(0.5f, 0f);
            back.RectTransform.anchoredPosition = new Vector2(0, 150);
            back.RectTransform.sizeDelta = new Vector2(560, 100);
        }

        UiButton Option(Transform parent, System.Action onClick)
        {
            UiButton button = UIFactory.Button(parent, "", App.I.Theme.ButtonFill,
                App.I.Theme.ButtonText, onClick, 34, 14);
            UIFactory.SizedAs(button.GameObject, 104);
            return button;
        }

        protected override void OnShow() => Refresh();

        void Refresh()
        {
            App app = App.I;
            string on = Strings.Get(Strings.Key.Yes);
            string off = Strings.Get(Strings.Key.No);

            sound.Text = $"{Strings.Get(Strings.Key.Sound)}: {(app.Audio.Enabled ? on : off)}";
            vibration.Text = $"{Strings.Get(Strings.Key.Vibration)}: {(app.Haptics.Enabled ? on : off)}";
            privacy.Text = Strings.Get(Strings.Key.PrivacySettings);
            reset.Text = Strings.Get(Strings.Key.ResetProgress);

            Theme theme = app.Theme;
            Color dim = new Color(theme.ButtonFill.r, theme.ButtonFill.g, theme.ButtonFill.b, 0.35f);

            sound.Tint(app.Audio.Enabled ? theme.ButtonFill : dim,
                app.Audio.Enabled ? theme.ButtonText : theme.HeadingText);
            vibration.Tint(app.Haptics.Enabled ? theme.ButtonFill : dim,
                app.Haptics.Enabled ? theme.ButtonText : theme.HeadingText);
            privacy.Tint(dim, theme.HeadingText);
            reset.Tint(dim, theme.HeadingText);
        }

        public override void ApplyTheme()
        {
            heading.color = App.I.Theme.HeadingText;
            back.Tint(App.I.Theme.ButtonFill, App.I.Theme.ButtonText);
            Refresh();
        }
    }
}
