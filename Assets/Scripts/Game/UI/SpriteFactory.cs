using System.Collections.Generic;
using UnityEngine;

namespace Tilevault.Game.UI
{
    /// <summary>
    /// Generates the few shapes the interface needs instead of shipping sprite
    /// files. Each shape is built once and cached; they are 9-sliced, so one
    /// small texture serves a tile and a full-width panel alike.
    /// </summary>
    public static class SpriteFactory
    {
        static readonly Dictionary<int, Sprite> RoundedCache = new Dictionary<int, Sprite>();
        static Sprite white;
        static Sprite circle;

        /// <summary>A plain opaque pixel, for flat fills.</summary>
        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    var tex = NewTexture(4, 4);
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
                    white.name = "white";
                }
                return white;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null) circle = BuildCircle(64);
                return circle;
            }
        }

        /// <param name="radius">Corner radius in pixels of the generated texture.</param>
        public static Sprite RoundedRect(int radius)
        {
            if (RoundedCache.TryGetValue(radius, out Sprite cached)) return cached;

            int size = radius * 2 + 4;
            var tex = NewTexture(size, size);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = RoundedAlpha(x, y, size, radius);

            tex.SetPixels32(pixels);
            tex.Apply();

            // The border makes it 9-sliced: corners keep their radius at any scale.
            var sprite = Sprite.Create(
                tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            sprite.name = $"rounded{radius}";

            RoundedCache[radius] = sprite;
            return sprite;
        }

        static Color32 RoundedAlpha(int x, int y, int size, int radius)
        {
            float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
            float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
            float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));

            // One pixel of feathering, so edges are not stair-stepped on screen.
            float alpha = Mathf.Clamp01(radius - distance + 0.5f);
            return new Color32(255, 255, 255, (byte)(alpha * 255f));
        }

        static Sprite BuildCircle(int size)
        {
            var tex = NewTexture(size, size);
            var pixels = new Color32[size * size];
            float r = size * 0.5f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float alpha = Mathf.Clamp01(r - d);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "circle";
            return sprite;
        }

        static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
