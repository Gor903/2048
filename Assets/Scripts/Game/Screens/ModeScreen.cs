using System.Collections.Generic;
using Tilevault.Core;
using Tilevault.Game.Localisation;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Screens
{
    /// <summary>Picks the rule set and the board size that Play will start.</summary>
    public sealed class ModeScreen : UiScreen
    {
        Text heading;
        Text modeHeading;
        Text sizeHeading;
        readonly List<(UiButton button, GameMode mode)> modeButtons = new List<(UiButton, GameMode)>();
        readonly List<(UiButton button, int size)> sizeButtons = new List<(UiButton, int)>();
        UiButton back;


        protected override void BuildContent()
        {
            Theme theme = App.I.Theme;

            heading = Heading(Strings.Get(Strings.Key.Mode), -60, 64);
            modeHeading = Heading(Strings.Get(Strings.Key.Mode), -230, 36);
            sizeHeading = Heading(Strings.Get(Strings.Key.BoardSize), -480, 36);

            HorizontalLayoutGroup modeRow = Row(-300, 150);
            foreach (GameMode m in new[] { GameMode.Classic, GameMode.Stones })
            {
                GameMode captured = m;
                string key = m == GameMode.Classic ? Strings.Key.ModeClassic : Strings.Key.ModeStones;
                UiButton button = UIFactory.Button(modeRow.transform, Strings.Get(key),
                    theme.ButtonFill, theme.ButtonText, () => SelectMode(captured), 36, 14);
                UIFactory.SizedAs(button.GameObject, 120);
                modeButtons.Add((button, captured));
            }

            HorizontalLayoutGroup sizeRow = Row(-550, 130);
            for (int s = GameConfig.MinSize; s <= GameConfig.MaxSize; s++)
            {
                int captured = s;
                UiButton button = UIFactory.Button(sizeRow.transform, Strings.SizeLabel(s),
                    theme.ButtonFill, theme.ButtonText, () => SelectSize(captured), 34, 14);
                UIFactory.SizedAs(button.GameObject, 110);
                sizeButtons.Add((button, captured));
            }

            back = UIFactory.Button(Root, Strings.Get(Strings.Key.BackToHome),
                theme.ButtonFill, theme.ButtonText, () =>
                {
                    App.I.Audio.PlayButton();
                    App.I.Back();
                }, 36, 14);
            var backRect = back.RectTransform;
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0, 150);
            backRect.sizeDelta = new Vector2(560, 100);
        }

        Text Heading(string text, float y, int fontSize)
        {
            Text label = UIFactory.Label(Root, text, fontSize, App.I.Theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Heading");
            label.rectTransform.anchorMin = new Vector2(0, 1);
            label.rectTransform.anchorMax = new Vector2(1, 1);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            label.rectTransform.anchoredPosition = new Vector2(0, y);
            label.rectTransform.sizeDelta = new Vector2(0, fontSize + 16);
            return label;
        }

        HorizontalLayoutGroup Row(float y, float height)
        {
            HorizontalLayoutGroup row = UIFactory.Row(Root, 20, new RectOffset(60, 60, 0, 0));
            var rect = (RectTransform)row.transform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(0, height);
            return row;
        }

        void SelectMode(GameMode m)
        {
            App.I.Audio.PlayButton();
            App.I.Save.Data.preferredMode = (int)m;
            App.I.Save.Save();
            Refresh();
        }

        void SelectSize(int size)
        {
            App.I.Audio.PlayButton();
            App.I.Save.Data.preferredSize = size;
            App.I.Save.Save();
            Refresh();
        }

        protected override void OnShow() => Refresh();

        /// <summary>The chosen option is filled, the others outlined.</summary>
        void Refresh()
        {
            Theme theme = App.I.Theme;
            var selectedMode = (GameMode)App.I.Save.Data.preferredMode;
            int selectedSize = App.I.Save.Data.preferredSize;

            Color idle = new Color(theme.ButtonFill.r, theme.ButtonFill.g, theme.ButtonFill.b, 0.3f);

            foreach ((UiButton button, GameMode m) in modeButtons)
                button.Tint(m == selectedMode ? theme.ButtonFill : idle,
                    m == selectedMode ? theme.ButtonText : theme.HeadingText);

            foreach ((UiButton button, int s) in sizeButtons)
                button.Tint(s == selectedSize ? theme.ButtonFill : idle,
                    s == selectedSize ? theme.ButtonText : theme.HeadingText);
        }

        public override void ApplyTheme()
        {
            Theme theme = App.I.Theme;
            heading.color = theme.HeadingText;
            modeHeading.color = theme.HeadingText;
            sizeHeading.color = theme.HeadingText;
            back.Tint(theme.ButtonFill, theme.ButtonText);
            Refresh();
        }
    }
}
