using System;

namespace Tilevault.Game.Services
{
    /// <summary>Holds the active palette and tells the views when it changes.</summary>
    public sealed class ThemeService
    {
        readonly SaveService save;

        public Theme Current { get; private set; }
        public event Action Changed;

        public ThemeService(SaveService save)
        {
            this.save = save;
            Current = Theme.ById(save.Data.themeId);
        }

        public bool IsUnlocked(Theme theme) => theme.UnlockedByDefault || save.IsThemeUnlocked(theme.Id);

        public void Unlock(Theme theme)
        {
            save.UnlockTheme(theme.Id);
        }

        public bool Select(Theme theme)
        {
            if (!IsUnlocked(theme)) return false;
            if (Current.Id == theme.Id) return true;

            Current = theme;
            save.Data.themeId = theme.Id;
            save.Save();
            Changed?.Invoke();
            return true;
        }
    }
}
