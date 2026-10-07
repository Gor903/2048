using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.UI
{
    /// <summary>
    /// Builds the interface from code. Keeping the hierarchy here rather than in
    /// a scene file means layout changes are reviewable as a diff and the whole
    /// interface can be rebuilt headlessly for screenshots.
    /// </summary>
    public static class UIFactory
    {
        static Font cachedFont;

        /// <summary>
        /// Unity's built-in font. Legacy uGUI text is used throughout rather than
        /// TextMeshPro, whose font assets would have to be generated through the
        /// editor GUI and committed as binaries.
        /// </summary>
        public static Font Font
        {
            get
            {
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (cachedFont == null)
                        cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return cachedFont;
            }
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Stretches a transform to fill its parent, with optional insets.</summary>
        public static RectTransform Fill(RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Panel(Transform parent, Color colour, int cornerRadius = 0, string name = "Panel")
        {
            RectTransform rt = Rect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = colour;

            if (cornerRadius > 0)
            {
                image.sprite = SpriteFactory.RoundedRect(cornerRadius);
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.sprite = SpriteFactory.White;
            }

            return image;
        }

        public static Text Label(Transform parent, string text, int fontSize, Color colour,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Bold,
            string name = "Label")
        {
            RectTransform rt = Rect(name, parent);
            var label = rt.gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = colour;
            label.alignment = anchor;
            label.fontStyle = style;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>
        /// A filled rounded button with a centred label. The returned component
        /// carries both so callers can retint and relabel without a Find().
        /// </summary>
        public static UiButton Button(Transform parent, string text, Color fill, Color textColour,
            Action onClick, int fontSize = 34, int cornerRadius = 12, string name = null)
        {
            Image background = Panel(parent, fill, cornerRadius, name ?? $"Button_{text}");
            background.raycastTarget = true;

            Text label = Label(background.transform, text, fontSize, textColour);
            Fill(label.rectTransform, 12, 12, 4, 4);

            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            if (onClick != null) button.onClick.AddListener(() => onClick());

            var colours = button.colors;
            colours.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colours.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colours.fadeDuration = 0.06f;
            button.colors = colours;

            return new UiButton(button, background, label);
        }

        public static VerticalLayoutGroup Column(Transform parent, float spacing, RectOffset padding = null,
            string name = "Column")
        {
            RectTransform rt = Rect(name, parent);
            var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            return layout;
        }

        public static HorizontalLayoutGroup Row(Transform parent, float spacing, RectOffset padding = null,
            string name = "Row")
        {
            RectTransform rt = Rect(name, parent);
            var layout = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            return layout;
        }

        public static LayoutElement SizedAs(GameObject go, float height = -1, float width = -1, float flexibleWidth = -1)
        {
            var element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (height >= 0) element.preferredHeight = height;
            if (width >= 0) element.preferredWidth = width;
            if (flexibleWidth >= 0) element.flexibleWidth = flexibleWidth;
            return element;
        }
    }

    /// <summary>A button plus the pieces a screen needs to restyle it on a theme change.</summary>
    public sealed class UiButton
    {
        public readonly Button Button;
        public readonly Image Background;
        public readonly Text Label;

        public UiButton(Button button, Image background, Text label)
        {
            Button = button;
            Background = background;
            Label = label;
        }

        public GameObject GameObject => Button.gameObject;
        public RectTransform RectTransform => (RectTransform)Button.transform;

        public bool Interactable
        {
            get => Button.interactable;
            set => Button.interactable = value;
        }

        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }

        public void Tint(Color fill, Color text)
        {
            Background.color = fill;
            Label.color = text;
        }
    }
}
