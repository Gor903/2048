using System.IO;
using Tilevault.Game.Services;
using UnityEditor;
using UnityEngine;

namespace Tilevault.Editor
{
    /// <summary>
    /// Draws the icon and store art from the game's own palette, pixel by pixel
    /// on the CPU, so the whole set regenerates with one command and needs no
    /// graphics device.
    ///
    /// Everything here is deterministic: running it twice produces identical
    /// files, which keeps the art out of the "regenerated, now the diff is huge"
    /// category.
    /// </summary>
    public static class ArtGenerator
    {
        public const string OutputDirectory = "Assets/Art/Generated";
        public const string StoreDirectory = "Publishing/art";

        [MenuItem("Tilevault/Art/Generate All")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(StoreDirectory);

            // Application icon — full bleed, used for the legacy icon slots.
            WritePng(Path.Combine(OutputDirectory, "icon_1024.png"), BuildIcon(1024, bleed: true));

            // Adaptive icon: the foreground must stay inside the middle 66%,
            // because the launcher masks and animates the outer region away.
            WritePng(Path.Combine(OutputDirectory, "icon_fg.png"), BuildIcon(432, bleed: false, safeFraction: 0.62f));
            WritePng(Path.Combine(OutputDirectory, "icon_bg.png"), BuildFlat(432, Theme.Classic.GridBackground));

            // Store assets.
            WritePng(Path.Combine(StoreDirectory, "store-icon-512.png"), BuildIcon(512, bleed: true));
            WritePng(Path.Combine(StoreDirectory, "feature-graphic-1024x500.png"),
                BuildFeatureGraphic(1024, 500), 1024, 500);

            AssetDatabase.Refresh();
            ConfigureIconImportSettings();

            Debug.Log($"[Tilevault] Art written to {OutputDirectory} and {StoreDirectory}");
        }

        // ---- the mark ---------------------------------------------------------

        /// <summary>
        /// A 2x2 arrangement of tiles reading 2, 4, 8, 16 — the game's actual
        /// subject, in the game's actual colours, legible at 48px.
        /// </summary>
        static Color32[] BuildIcon(int size, bool bleed, float safeFraction = 1f)
        {
            Theme theme = Theme.Classic;
            var pixels = NewCanvas(size, bleed ? theme.GridBackground : new Color(0, 0, 0, 0));

            if (bleed)
            {
                // Rounded plate so the icon reads well on launchers that do not mask.
                FillRounded(pixels, size, 0, 0, size, size, size * 0.22f, theme.GridBackground);
            }

            float extent = size * safeFraction;
            float origin = (size - extent) * 0.5f;
            float gap = extent * 0.055f;
            float cell = (extent - gap * 3f) / 2f;

            int[] values = { 8, 16, 2, 4 };   // bottom-left, bottom-right, top-left, top-right

            for (int i = 0; i < 4; i++)
            {
                int cx = i % 2;
                int cy = i / 2;
                float x = origin + gap + cx * (cell + gap);
                float y = origin + gap + cy * (cell + gap);

                FillRounded(pixels, size, x, y, cell, cell, cell * 0.17f, theme.TileFillFor(values[i]));
                DrawNumber(pixels, size, size, values[i].ToString(), x, y, cell, cell,
                    theme.TileTextFor(values[i]));
            }

            return pixels;
        }

        static Color32[] BuildFlat(int size, Color colour) => NewCanvas(size, colour);

        /// <summary>
        /// 1024x500 feature graphic. Deliberately text-free: Play overlays the
        /// app title on this image in several surfaces, and baked-in text
        /// collides with it.
        /// </summary>
        static Color32[] BuildFeatureGraphic(int width, int height)
        {
            Theme theme = Theme.Classic;
            var pixels = new Color32[width * height];

            // Vertical wash from the grid colour into the accent.
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color row = Color.Lerp(theme.GridBackground, theme.Accent, t * 0.55f);
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = row;
            }

            // A descending run of tiles, the shape a real merge chain makes.
            int[] values = { 2, 4, 8, 16, 32, 64 };
            float tile = height * 0.30f;
            float gap = tile * 0.16f;
            float totalWidth = values.Length * tile + (values.Length - 1) * gap;
            float startX = (width - totalWidth) * 0.5f;
            float midY = height * 0.5f;

            for (int i = 0; i < values.Length; i++)
            {
                float x = startX + i * (tile + gap);
                // Gentle arc so the row does not read as a flat strip.
                float offset = Mathf.Sin((float)i / (values.Length - 1) * Mathf.PI) * height * 0.07f;
                float y = midY - tile * 0.5f + offset;

                FillRounded(pixels, width, height, x, y, tile, tile, tile * 0.17f,
                    theme.TileFillFor(values[i]));
                DrawNumber(pixels, width, height, values[i].ToString(), x, y, tile, tile,
                    theme.TileTextFor(values[i]));
            }

            return pixels;
        }

        // ---- drawing ----------------------------------------------------------

        // ---- numerals ---------------------------------------------------------

        /// <summary>
        /// Digits as stroked polylines on a unit box, drawn with round caps.
        /// A geometric stroke font suits the rounded tiles better than a segment
        /// display, and needs no font asset or graphics device.
        /// </summary>
        static readonly Vector2[][][] Digits =
        {
            // 0
            new[] { new[] { V(.5f,.9f), V(.65f,.85f), V(.75f,.72f), V(.78f,.5f), V(.75f,.28f),
                            V(.65f,.15f), V(.5f,.1f), V(.35f,.15f), V(.25f,.28f), V(.22f,.5f),
                            V(.25f,.72f), V(.35f,.85f), V(.5f,.9f) } },
            // 1
            new[] { new[] { V(.32f,.72f), V(.5f,.9f), V(.5f,.1f) },
                    new[] { V(.28f,.1f), V(.72f,.1f) } },
            // 2
            new[] { new[] { V(.24f,.74f), V(.33f,.86f), V(.55f,.9f), V(.74f,.8f),
                            V(.74f,.62f), V(.26f,.12f), V(.78f,.12f) } },
            // 3
            new[] { new[] { V(.26f,.84f), V(.5f,.9f), V(.72f,.82f), V(.72f,.66f), V(.5f,.54f),
                            V(.72f,.42f), V(.72f,.22f), V(.5f,.1f), V(.26f,.18f) } },
            // 4
            new[] { new[] { V(.68f,.1f), V(.68f,.9f), V(.18f,.36f), V(.84f,.36f) } },
            // 5
            new[] { new[] { V(.74f,.9f), V(.3f,.9f), V(.27f,.56f), V(.5f,.62f),
                            V(.72f,.5f), V(.72f,.26f), V(.5f,.1f), V(.27f,.18f) } },
            // 6
            new[] { new[] { V(.72f,.84f), V(.56f,.9f), V(.38f,.86f), V(.28f,.72f), V(.24f,.5f),
                            V(.25f,.32f), V(.33f,.17f), V(.5f,.1f), V(.66f,.14f), V(.75f,.27f),
                            V(.73f,.42f), V(.6f,.52f), V(.42f,.53f), V(.29f,.45f) } },
            // 7
            new[] { new[] { V(.24f,.9f), V(.78f,.9f), V(.44f,.1f) } },
            // 8
            new[] { new[] { V(.5f,.53f), V(.63f,.57f), V(.70f,.67f), V(.68f,.80f), V(.5f,.9f),
                            V(.32f,.80f), V(.30f,.67f), V(.37f,.57f), V(.5f,.53f) },
                    new[] { V(.5f,.53f), V(.66f,.46f), V(.74f,.33f), V(.72f,.19f), V(.5f,.1f),
                            V(.28f,.19f), V(.26f,.33f), V(.34f,.46f), V(.5f,.53f) } },
            // 9
            new[] { new[] { V(.28f,.16f), V(.44f,.1f), V(.62f,.14f), V(.72f,.28f), V(.76f,.5f),
                            V(.75f,.68f), V(.67f,.83f), V(.5f,.9f), V(.34f,.86f), V(.25f,.73f),
                            V(.27f,.58f), V(.4f,.48f), V(.58f,.47f), V(.71f,.55f) } }
        };

        static Vector2 V(float x, float y) => new Vector2(x, y);

        /// <summary>Draws a number centred in the given box.</summary>
        static void DrawNumber(Color32[] pixels, int stride, int height, string text,
            float boxX, float boxY, float boxW, float boxH, Color colour)
        {
            float glyphHeight = boxH * 0.46f;
            float glyphWidth = glyphHeight * 0.66f;
            float spacing = glyphWidth * 0.16f;
            float totalWidth = text.Length * glyphWidth + (text.Length - 1) * spacing;

            // Narrow the glyphs rather than overflow when the number is long.
            if (totalWidth > boxW * 0.82f)
            {
                float scale = boxW * 0.82f / totalWidth;
                glyphWidth *= scale;
                spacing *= scale;
                glyphHeight *= scale;
                totalWidth = boxW * 0.82f;
            }

            float penX = boxX + (boxW - totalWidth) * 0.5f;
            float baseY = boxY + (boxH - glyphHeight) * 0.5f;
            float strokeWidth = glyphHeight * 0.13f;

            foreach (char c in text)
            {
                int digit = c - '0';
                if (digit >= 0 && digit <= 9)
                    DrawGlyph(pixels, stride, height, Digits[digit],
                        penX, baseY, glyphWidth, glyphHeight, strokeWidth, colour);

                penX += glyphWidth + spacing;
            }
        }

        static void DrawGlyph(Color32[] pixels, int stride, int height, Vector2[][] polylines,
            float x, float y, float w, float h, float strokeWidth, Color colour)
        {
            foreach (Vector2[] line in polylines)
            for (int i = 0; i < line.Length - 1; i++)
            {
                Vector2 a = new Vector2(x + line[i].x * w, y + line[i].y * h);
                Vector2 b = new Vector2(x + line[i + 1].x * w, y + line[i + 1].y * h);
                DrawThickLine(pixels, stride, height, a, b, strokeWidth, colour);
            }
        }

        /// <summary>Round-capped line: fill every pixel within half a stroke of the segment.</summary>
        static void DrawThickLine(Color32[] pixels, int stride, int height,
            Vector2 a, Vector2 b, float strokeWidth, Color colour)
        {
            float radius = strokeWidth * 0.5f;

            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, b.x) - radius - 1));
            int x1 = Mathf.Min(stride - 1, Mathf.CeilToInt(Mathf.Max(a.x, b.x) + radius + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, b.y) - radius - 1));
            int y1 = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, b.y) + radius + 1));

            Vector2 ab = b - a;
            float lengthSquared = Mathf.Max(ab.sqrMagnitude, 1e-5f);

            for (int py = y0; py <= y1; py++)
            for (int px = x0; px <= x1; px++)
            {
                var p = new Vector2(px + 0.5f, py + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSquared);
                float distance = Vector2.Distance(p, a + ab * t);

                float coverage = Mathf.Clamp01(radius - distance + 0.5f);
                if (coverage <= 0f) continue;

                int index = py * stride + px;
                Color under = pixels[index];
                Color blended = Color.Lerp(under, colour, coverage);
                blended.a = Mathf.Max(under.a, colour.a * coverage);
                pixels[index] = blended;
            }
        }

        static Color32[] NewCanvas(int size, Color fill)
        {
            var pixels = new Color32[size * size];
            Color32 c = fill;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            return pixels;
        }

        static void FillRounded(Color32[] pixels, int size, float x, float y, float w, float h,
            float radius, Color colour)
            => FillRounded(pixels, size, size, x, y, w, h, radius, colour);

        /// <summary>Alpha-blended rounded rectangle with one pixel of feathering.</summary>
        static void FillRounded(Color32[] pixels, int stride, int height, float x, float y,
            float w, float h, float radius, Color colour)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(x));
            int x1 = Mathf.Min(stride - 1, Mathf.CeilToInt(x + w));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(y));
            int y1 = Mathf.Min(height - 1, Mathf.CeilToInt(y + h));

            for (int py = y0; py <= y1; py++)
            for (int px = x0; px <= x1; px++)
            {
                float fx = px + 0.5f;
                float fy = py + 0.5f;

                float cx = Mathf.Clamp(fx, x + radius, x + w - radius);
                float cy = Mathf.Clamp(fy, y + radius, y + h - radius);
                float distance = Mathf.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy));

                float coverage = Mathf.Clamp01(radius - distance + 0.5f);
                if (radius <= 0f)
                    coverage = fx >= x && fx <= x + w && fy >= y && fy <= y + h ? 1f : 0f;

                if (coverage <= 0f) continue;

                int index = py * stride + px;
                Color under = pixels[index];
                Color blended = Color.Lerp(under, colour, coverage);
                blended.a = Mathf.Max(under.a, colour.a * coverage);
                pixels[index] = blended;
            }
        }

        static void WritePng(string path, Color32[] pixels)
        {
            int size = Mathf.RoundToInt(Mathf.Sqrt(pixels.Length));
            WritePng(path, pixels, size, size);
        }

        static void WritePng(string path, Color32[] pixels, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        /// <summary>
        /// Icons must import uncompressed and readable, or Unity hands the build
        /// a block-compressed texture and the launcher icon turns to mush.
        /// </summary>
        static void ConfigureIconImportSettings()
        {
            foreach (string file in Directory.GetFiles(OutputDirectory, "*.png"))
            {
                var importer = AssetImporter.GetAtPath(file) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        public static void CI()
        {
            GenerateAll();
            EditorApplication.Exit(0);
        }
    }
}
