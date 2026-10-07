using System;
using Tilevault.Game.Localisation;
using Tilevault.Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.UI
{
    /// <summary>
    /// A modal panel over a dimmed backdrop. Built on demand and destroyed on
    /// dismissal — dialogs are rare enough that pooling them would be noise.
    /// </summary>
    public static class Dialog
    {
        public static GameObject Confirm(Transform parent, Theme theme, string message,
            string confirmText, string cancelText, Action onConfirm, Action onCancel = null)
        {
            RectTransform shade = UIFactory.Rect("Dialog", parent);
            UIFactory.Fill(shade);
            shade.SetAsLastSibling();

            var blocker = shade.gameObject.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0.55f);
            blocker.sprite = SpriteFactory.White;

            Image panel = UIFactory.Panel(shade, theme.PanelBackground, 18, "Panel");
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620, 340);

            Text label = UIFactory.Label(panel.transform, message, 34, theme.HeadingText);
            label.rectTransform.anchorMin = new Vector2(0, 1);
            label.rectTransform.anchorMax = new Vector2(1, 1);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            label.rectTransform.offsetMin = new Vector2(36, 0);
            label.rectTransform.offsetMax = new Vector2(-36, -40);
            label.rectTransform.sizeDelta = new Vector2(label.rectTransform.sizeDelta.x, 170);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            HorizontalLayoutGroup row = UIFactory.Row(panel.transform, 20,
                new RectOffset(32, 32, 0, 0), "Buttons");
            var rowRect = (RectTransform)row.transform;
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.offsetMin = new Vector2(0, 32);
            rowRect.offsetMax = new Vector2(0, 32 + 92);

            void Close()
            {
                UnityEngine.Object.Destroy(shade.gameObject);
            }

            UiButton cancel = UIFactory.Button(row.transform, cancelText,
                new Color(theme.ButtonFill.r, theme.ButtonFill.g, theme.ButtonFill.b, 0.35f),
                theme.HeadingText, () =>
                {
                    Close();
                    onCancel?.Invoke();
                });
            UIFactory.SizedAs(cancel.GameObject, 92);

            UiButton confirm = UIFactory.Button(row.transform, confirmText,
                theme.ButtonFill, theme.ButtonText, () =>
                {
                    Close();
                    onConfirm?.Invoke();
                });
            UIFactory.SizedAs(confirm.GameObject, 92);

            return shade.gameObject;
        }

        /// <summary>A short message that fades itself away. Used for "Out of charges".</summary>
        public static void Toast(MonoBehaviour host, Transform parent, Theme theme, string message)
        {
            Image panel = UIFactory.Panel(parent, theme.Accent, 14, "Toast");
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 190);
            rt.sizeDelta = new Vector2(560, 84);
            rt.SetAsLastSibling();

            Text label = UIFactory.Label(panel.transform, message, 30, theme.ButtonText);
            UIFactory.Fill(label.rectTransform, 16, 16, 0, 0);

            host.StartCoroutine(FadeAway(panel));
        }

        static System.Collections.IEnumerator FadeAway(Image panel)
        {
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            yield return new WaitForSeconds(1.3f);

            float t = 0f;
            while (t < 0.35f && panel != null)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = 1f - t / 0.35f;
                yield return null;
            }

            if (panel != null) UnityEngine.Object.Destroy(panel.gameObject);
        }

        public static string Text(string key) => Strings.Get(key);
    }
}
