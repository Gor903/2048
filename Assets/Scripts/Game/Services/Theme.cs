using System.Collections.Generic;
using Tilevault.Game.Localisation;
using UnityEngine;

namespace Tilevault.Game.Services
{
    /// <summary>A colour palette. Tile colours are indexed by the exponent of the value.</summary>
    public sealed class Theme
    {
        public string Id;
        public string NameKey;
        public bool UnlockedByDefault;

        public Color Background;
        public Color GridBackground;
        public Color EmptyCell;
        public Color StoneColour;
        public Color StoneText;
        public Color Accent;
        public Color PanelBackground;
        public Color ButtonFill;
        public Color ButtonText;
        public Color HeadingText;

        /// <summary>Index 0 is the 2-tile, index 1 the 4-tile, and so on.</summary>
        public Color[] TileFills;
        public Color TileTextDark;
        public Color TileTextLight;

        /// <summary>Tiles past the end of the ramp all share the final colour.</summary>
        public Color TileFillFor(int value)
        {
            int index = Mathf.RoundToInt(Mathf.Log(value, 2f)) - 1;
            if (index < 0) index = 0;
            if (index >= TileFills.Length) index = TileFills.Length - 1;
            return TileFills[index];
        }

        /// <summary>The 2 and 4 tiles are pale, so they take dark text; the rest light.</summary>
        public Color TileTextFor(int value) => value <= 4 ? TileTextDark : TileTextLight;

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static readonly Theme Classic = new Theme
        {
            Id = "classic",
            NameKey = Strings.Key.ThemeClassic,
            UnlockedByDefault = true,
            Background = Hex("#FAF8EF"),
            GridBackground = Hex("#BBADA0"),
            EmptyCell = Hex("#CDC1B4"),
            StoneColour = Hex("#7A6F66"),
            StoneText = Hex("#EFE9E2"),
            Accent = Hex("#8F7A66"),
            PanelBackground = Hex("#FAF8EF"),
            ButtonFill = Hex("#8F7A66"),
            ButtonText = Hex("#F9F6F2"),
            HeadingText = Hex("#776E65"),
            TileTextDark = Hex("#776E65"),
            TileTextLight = Hex("#F9F6F2"),
            TileFills = new[]
            {
                Hex("#EEE4DA"), Hex("#EDE0C8"), Hex("#F2B179"), Hex("#F59563"),
                Hex("#F67C5F"), Hex("#F65E3B"), Hex("#EDCF72"), Hex("#EDCC61"),
                Hex("#EDC850"), Hex("#EDC53F"), Hex("#EDC22E"), Hex("#3C3A32")
            }
        };

        public static readonly Theme Dark = new Theme
        {
            Id = "dark",
            NameKey = Strings.Key.ThemeDark,
            UnlockedByDefault = true,
            Background = Hex("#15181D"),
            GridBackground = Hex("#262B33"),
            EmptyCell = Hex("#1D222A"),
            StoneColour = Hex("#4A525E"),
            StoneText = Hex("#C9D1DC"),
            Accent = Hex("#4F9DF7"),
            PanelBackground = Hex("#1A1E25"),
            ButtonFill = Hex("#2F6FD0"),
            ButtonText = Hex("#F2F6FC"),
            HeadingText = Hex("#E4EAF2"),
            TileTextDark = Hex("#1A1E25"),
            TileTextLight = Hex("#F2F6FC"),
            TileFills = new[]
            {
                Hex("#C9D1DC"), Hex("#A9B6C7"), Hex("#6FA8F5"), Hex("#4F8EE8"),
                Hex("#3B74D6"), Hex("#2F5EC0"), Hex("#6FD3C0"), Hex("#4BC2AB"),
                Hex("#36AE95"), Hex("#2A947E"), Hex("#1F7A68"), Hex("#0E4B40")
            }
        };

        public static readonly Theme Sunset = new Theme
        {
            Id = "sunset",
            NameKey = Strings.Key.ThemeSunset,
            UnlockedByDefault = false,
            Background = Hex("#2B1B33"),
            GridBackground = Hex("#43264B"),
            EmptyCell = Hex("#35203D"),
            StoneColour = Hex("#6B4A6E"),
            StoneText = Hex("#F7E3EC"),
            Accent = Hex("#FF8C6B"),
            PanelBackground = Hex("#33203B"),
            ButtonFill = Hex("#E26A54"),
            ButtonText = Hex("#FFF3EC"),
            HeadingText = Hex("#FFD9C6"),
            TileTextDark = Hex("#41202A"),
            TileTextLight = Hex("#FFF3EC"),
            TileFills = new[]
            {
                Hex("#FFE0C2"), Hex("#FFCBA0"), Hex("#FFA981"), Hex("#FF8C6B"),
                Hex("#F96F5E"), Hex("#E2544F"), Hex("#C94070"), Hex("#AE3184"),
                Hex("#8E2B8F"), Hex("#6E2A8C"), Hex("#512880"), Hex("#2E1A52")
            }
        };

        public static readonly Theme Forest = new Theme
        {
            Id = "forest",
            NameKey = Strings.Key.ThemeForest,
            UnlockedByDefault = false,
            Background = Hex("#F2F6EC"),
            GridBackground = Hex("#A8BC96"),
            EmptyCell = Hex("#C3D2B4"),
            StoneColour = Hex("#6E7A62"),
            StoneText = Hex("#F2F6EC"),
            Accent = Hex("#5B7F4B"),
            PanelBackground = Hex("#F2F6EC"),
            ButtonFill = Hex("#5B7F4B"),
            ButtonText = Hex("#F6FAF1"),
            HeadingText = Hex("#44543A"),
            TileTextDark = Hex("#44543A"),
            TileTextLight = Hex("#F6FAF1"),
            TileFills = new[]
            {
                Hex("#E4EDD8"), Hex("#D3E2C0"), Hex("#AFCE91"), Hex("#93BC72"),
                Hex("#79A95B"), Hex("#5F9447"), Hex("#E8C66B"), Hex("#DDB152"),
                Hex("#CE9A3E"), Hex("#B9822F"), Hex("#9C6823"), Hex("#3E4A33")
            }
        };

        public static readonly IReadOnlyList<Theme> All = new[] { Classic, Dark, Sunset, Forest };

        public static Theme ById(string id)
        {
            foreach (Theme t in All)
                if (t.Id == id) return t;
            return Classic;
        }
    }
}
