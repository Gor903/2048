using System.Collections.Generic;
using Tilevault.Game.Localisation;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Screens
{
    /// <summary>
    /// Palette picker. Locked themes offer a rewarded ad; when no ad service is
    /// present the attempt fails politely and the theme stays locked.
    /// </summary>
    public sealed class ThemesScreen : UiScreen
    {
        sealed class Row
        {
            public Theme Theme;
            public Image Card;
            public Text Name;
            public UiButton Action;
            public Image[] Swatches;
        }

        Text heading;
        UiButton back;
        readonly List<Row> rows = new List<Row>();


        protected override void BuildContent()
        {
            Theme theme = App.I.Theme;

            heading = UIFactory.Label(Root, Strings.Get(Strings.Key.Themes), 64, theme.HeadingText,
                TextAnchor.MiddleCenter, FontStyle.Bold, "Heading");
            heading.rectTransform.anchorMin = new Vector2(0, 1);
            heading.rectTransform.anchorMax = new Vector2(1, 1);
            heading.rectTransform.pivot = new Vector2(0.5f, 1f);
            heading.rectTransform.anchoredPosition = new Vector2(0, -60);
            heading.rectTransform.sizeDelta = new Vector2(0, 90);

            VerticalLayoutGroup column = UIFactory.Column(Root, 20, new RectOffset(60, 60, 0, 0), "Themes");
            var columnRect = (RectTransform)column.transform;
            columnRect.anchorMin = new Vector2(0, 1);
            columnRect.anchorMax = new Vector2(1, 1);
            columnRect.pivot = new Vector2(0.5f, 1f);
            columnRect.anchoredPosition = new Vector2(0, -190);
            columnRect.sizeDelta = new Vector2(0, 900);

            foreach (Theme t in Theme.All)
                rows.Add(BuildRow(column.transform, t));

            back = UIFactory.Button(Root, Strings.Get(Strings.Key.BackToHome),
                theme.ButtonFill, theme.ButtonText, () =>
                {
                    App.I.Audio.PlayButton();
                    App.I.Back();
                }, 36, 14);
            back.RectTransform.anchorMin = new Vector2(0.5f, 0f);
            back.RectTransform.anchorMax = new Vector2(0.5f, 0f);
            back.RectTransform.pivot = new Vector2(0.5f, 0f);
            back.RectTransform.anchoredPosition = new Vector2(0, 150);
            back.RectTransform.sizeDelta = new Vector2(560, 100);
        }

        Row BuildRow(Transform parent, Theme t)
        {
            Image card = UIFactory.Panel(parent, t.GridBackground, 16, $"Theme_{t.Id}");
            UIFactory.SizedAs(card.gameObject, 170);

            Text name = UIFactory.Label(card.transform, Strings.Get(t.NameKey), 38, t.ButtonText,
                TextAnchor.MiddleLeft, FontStyle.Bold, "Name");
            name.rectTransform.anchorMin = new Vector2(0, 1);
            name.rectTransform.anchorMax = new Vector2(1, 1);
            name.rectTransform.pivot = new Vector2(0.5f, 1f);
            name.rectTransform.offsetMin = new Vector2(28, 0);
            name.rectTransform.offsetMax = new Vector2(-28, -16);
            name.rectTransform.sizeDelta = new Vector2(name.rectTransform.sizeDelta.x, 50);

            // A strip of the palette, so the choice is visible before committing.
            var swatches = new Image[5];
            for (int i = 0; i < swatches.Length; i++)
            {
                Image swatch = UIFactory.Panel(card.transform, t.TileFills[i * 2], 8, $"Swatch{i}");
                swatch.raycastTarget = false;
                var rect = (RectTransform)swatch.transform;
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(56, 56);
                rect.anchoredPosition = new Vector2(28 + i * 64, 26);
                swatches[i] = swatch;
            }

            UiButton action = UIFactory.Button(card.transform, "", t.ButtonFill, t.ButtonText,
                () => OnRowClicked(t), 30, 12);
            action.RectTransform.anchorMin = new Vector2(1, 0);
            action.RectTransform.anchorMax = new Vector2(1, 0);
            action.RectTransform.pivot = new Vector2(1, 0);
            action.RectTransform.anchoredPosition = new Vector2(-28, 26);
            action.RectTransform.sizeDelta = new Vector2(260, 68);

            return new Row { Theme = t, Card = card, Name = name, Action = action, Swatches = swatches };
        }

        void OnRowClicked(Theme t)
        {
            App app = App.I;
            app.Audio.PlayButton();

            if (app.Themes.IsUnlocked(t))
            {
                app.Themes.Select(t);
                Refresh();
                return;
            }

            app.Ads.ShowRewarded(RewardedPlacement.UnlockTheme, granted =>
            {
                if (granted)
                {
                    app.Themes.Unlock(t);
                    app.Themes.Select(t);
                }
                else
                {
                    Dialog.Toast(this, app.OverlayRoot, app.Theme, Strings.Get(Strings.Key.AdNotReady));
                }
                Refresh();
            });
        }

        protected override void OnShow() => Refresh();

        void Refresh()
        {
            App app = App.I;
            foreach (Row row in rows)
            {
                bool unlocked = app.Themes.IsUnlocked(row.Theme);
                bool active = app.Theme.Id == row.Theme.Id;

                row.Action.Text = active
                    ? Strings.Get(Strings.Key.Unlocked)
                    : unlocked
                        ? Strings.Get(Strings.Key.Play)
                        : Strings.Get(Strings.Key.WatchAdToUnlock);

                row.Action.Label.fontSize = unlocked ? 30 : 24;
                row.Card.color = row.Theme.GridBackground;
                row.Name.color = row.Theme.ButtonText;
                row.Action.Tint(row.Theme.ButtonFill, row.Theme.ButtonText);

                // The active theme gets a brighter card so it reads as selected.
                var outline = row.Card.GetComponent<Outline>();
                if (active && outline == null)
                {
                    outline = row.Card.gameObject.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(4, -4);
                }
                if (outline != null)
                {
                    outline.enabled = active;
                    outline.effectColor = row.Theme.Accent;
                }
            }
        }

        public override void ApplyTheme()
        {
            heading.color = App.I.Theme.HeadingText;
            back.Tint(App.I.Theme.ButtonFill, App.I.Theme.ButtonText);
            Refresh();
        }
    }
}
