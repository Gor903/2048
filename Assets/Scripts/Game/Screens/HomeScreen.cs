using Tilevault.Core;
using Tilevault.Game.Localisation;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Screens
{
    public sealed class HomeScreen : UiScreen
    {
        Text title;
        Text subtitle;
        Text bestLabel;
        Image card;
        UiButton play;
        UiButton daily;
        UiButton modeButton;
        UiButton themesButton;
        UiButton settingsButton;


        protected override void BuildContent()
        {
            App app = App.I;
            Theme theme = app.Theme;

            title = UIFactory.Label(Root, Strings.Get(Strings.Key.AppName), 96, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Title");
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.offsetMin = new Vector2(0, 0);
            title.rectTransform.offsetMax = new Vector2(0, -150);
            title.rectTransform.sizeDelta = new Vector2(0, 120);

            subtitle = UIFactory.Label(Root, Strings.Get(Strings.Key.AppSubtitle), 40, theme.Accent,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Subtitle");
            subtitle.rectTransform.anchorMin = new Vector2(0, 1);
            subtitle.rectTransform.anchorMax = new Vector2(1, 1);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.offsetMax = new Vector2(0, -268);
            subtitle.rectTransform.sizeDelta = new Vector2(0, 56);

            card = UIFactory.Panel(Root, theme.GridBackground, 16, "BestCard");
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 1f);
            cardRect.anchorMax = new Vector2(0.5f, 1f);
            cardRect.pivot = new Vector2(0.5f, 1f);
            cardRect.anchoredPosition = new Vector2(0, -360);
            cardRect.sizeDelta = new Vector2(620, 120);

            bestLabel = UIFactory.Label(card.transform, "", 38, theme.ButtonText);
            UIFactory.Fill(bestLabel.rectTransform, 16, 16, 0, 0);

            VerticalLayoutGroup column = UIFactory.Column(Root, 24,
                new RectOffset(80, 80, 0, 0), "Menu");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 0);
            columnRect.anchorMax = new Vector2(1, 0);
            columnRect.pivot = new Vector2(0.5f, 0f);
            columnRect.anchoredPosition = new Vector2(0, 150);
            columnRect.sizeDelta = new Vector2(0, 620);

            play = AddButton(column.transform, Strings.Get(Strings.Key.Play), 120, 44, () =>
            {
                app.Audio.PlayButton();
                if (app.Save.Data.hasSavedGame) app.ResumeSavedGame();
                else app.StartGame(app.Save.Data.preferredSize, (GameMode)app.Save.Data.preferredMode);
            });

            daily = AddButton(column.transform, Strings.Get(Strings.Key.DailyChallenge), 100, 36, () =>
            {
                app.Audio.PlayButton();
                app.StartDaily();
            });

            modeButton = AddButton(column.transform, Strings.Get(Strings.Key.Mode), 92, 34, () =>
            {
                app.Audio.PlayButton();
                app.OpenMode();
            });

            themesButton = AddButton(column.transform, Strings.Get(Strings.Key.Themes), 92, 34, () =>
            {
                app.Audio.PlayButton();
                app.OpenThemes();
            });

            settingsButton = AddButton(column.transform, Strings.Get(Strings.Key.Settings), 92, 34, () =>
            {
                app.Audio.PlayButton();
                app.OpenSettings();
            });
        }

        UiButton AddButton(Transform parent, string text, float height, int fontSize, System.Action onClick)
        {
            UiButton button = UIFactory.Button(parent, text, App.I.Theme.ButtonFill,
                App.I.Theme.ButtonText, onClick, fontSize, 14);
            UIFactory.SizedAs(button.GameObject, height);
            return button;
        }

        protected override void OnShow()
        {
            App app = App.I;
            int size = app.Save.Data.preferredSize;
            var gameMode = (GameMode)app.Save.Data.preferredMode;

            bestLabel.text = $"{Strings.Get(Strings.Key.Best)}  {app.Save.BestScore(size, gameMode)}";

            string modeName = Strings.Get(gameMode == GameMode.Classic
                ? Strings.Key.ModeClassic
                : Strings.Key.ModeStones);
            modeButton.Text = $"{Strings.Get(Strings.Key.Mode)}: {modeName} {Strings.SizeLabel(size)}";

            play.Text = app.Save.Data.hasSavedGame
                ? Strings.Get(Strings.Key.Resume)
                : Strings.Get(Strings.Key.Play);

            int streak = app.Save.Data.dailyStreak;
            daily.Text = streak > 0
                ? $"{Strings.Get(Strings.Key.DailyChallenge)} · {Strings.Get(Strings.Key.Streak, streak)}"
                : Strings.Get(Strings.Key.DailyChallenge);
        }

        public override void ApplyTheme()
        {
            Theme theme = App.I.Theme;
            title.color = theme.HeadingText;
            subtitle.color = theme.Accent;
            card.color = theme.GridBackground;
            bestLabel.color = theme.ButtonText;

            foreach (UiButton b in new[] { play, daily, modeButton, themesButton, settingsButton })
                b.Tint(theme.ButtonFill, theme.ButtonText);
        }

        /// <summary>Home is the root: back exits, which the stack handles.</summary>
        public override bool OnBack() => false;
    }
}
