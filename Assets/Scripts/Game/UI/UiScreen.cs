using System.Collections.Generic;
using UnityEngine;

namespace Tilevault.Game.UI
{
    /// <summary>
    /// A full-screen page. Screens are built once and then shown and hidden, so
    /// navigation never costs an allocation or a layout rebuild.
    /// </summary>
    public abstract class UiScreen : MonoBehaviour
    {
        public RectTransform Root { get; private set; }

        bool initialised;

        /// <summary>
        /// Builds the screen once. Called from Awake at runtime and directly by
        /// the editor capture pass, where Awake never fires — idempotent so
        /// either path can run first.
        /// </summary>
        public void Initialise()
        {
            if (initialised) return;
            initialised = true;

            Root = (RectTransform)transform;
            UIFactory.Fill(Root);
            BuildContent();
        }

        void Awake() => Initialise();

        /// <summary>Lays out the screen. Runs exactly once.</summary>
        protected abstract void BuildContent();

        public bool Visible => gameObject.activeSelf;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        /// <summary>Restyle after a theme change.</summary>
        public virtual void ApplyTheme() { }

        /// <summary>
        /// Handle the Android back button. Return true when the screen consumed
        /// it; false lets the stack pop to the previous screen.
        /// </summary>
        public virtual bool OnBack() => false;
    }

    /// <summary>
    /// Navigation history. Nothing else may show or hide a screen, so the back
    /// button always has one consistent story to follow.
    /// </summary>
    public sealed class ScreenStack
    {
        readonly List<UiScreen> stack = new List<UiScreen>();

        public UiScreen Current => stack.Count > 0 ? stack[stack.Count - 1] : null;
        public int Depth => stack.Count;

        public void Replace(UiScreen screen)
        {
            foreach (UiScreen s in stack) s.Hide();
            stack.Clear();
            stack.Add(screen);
            screen.Show();
        }

        public void Push(UiScreen screen)
        {
            Current?.Hide();
            stack.Add(screen);
            screen.Show();
        }

        /// <summary>Returns false when there is nothing left to pop to.</summary>
        public bool Pop()
        {
            if (stack.Count <= 1) return false;

            Current.Hide();
            stack.RemoveAt(stack.Count - 1);
            Current.Show();
            return true;
        }

        public void ApplyTheme()
        {
            foreach (UiScreen s in stack) s.ApplyTheme();
        }
    }
}
