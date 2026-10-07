using Tilevault.Core;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tilevault.Game.Presentation
{
    /// <summary>One tile or stone on the board. Owns only its own appearance.</summary>
    public sealed class TileView : MonoBehaviour
    {
        public RectTransform Rect { get; private set; }
        Image background;
        Text label;

        public int Value { get; private set; }
        public bool IsStone { get; private set; }

        float popTimer = -1f;
        const float PopDuration = 0.14f;
        float popStrength;

        public static TileView Create(Transform parent, float cellSize)
        {
            RectTransform rt = UIFactory.Rect("Tile", parent);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cellSize, cellSize);

            var view = rt.gameObject.AddComponent<TileView>();
            view.Rect = rt;

            view.background = rt.gameObject.AddComponent<Image>();
            view.background.sprite = SpriteFactory.RoundedRect(10);
            view.background.type = Image.Type.Sliced;
            view.background.raycastTarget = true;

            view.label = UIFactory.Label(rt, "", 40, Color.black);
            UIFactory.Fill(view.label.rectTransform, 2, 2, 2, 2);

            return view;
        }

        public void SetCellSize(float cellSize)
        {
            Rect.sizeDelta = new Vector2(cellSize, cellSize);
            // Long numbers have to shrink or they spill out of the tile.
            label.fontSize = Mathf.RoundToInt(cellSize * FontScaleFor(Value, IsStone));
        }

        static float FontScaleFor(int value, bool stone)
        {
            if (stone) return 0.34f;
            if (value >= 10000) return 0.26f;
            if (value >= 1000) return 0.31f;
            if (value >= 100) return 0.38f;
            return 0.44f;
        }

        public void SetTile(int value, Theme theme, float cellSize)
        {
            Value = value;
            IsStone = false;
            background.color = theme.TileFillFor(value);
            label.text = value.ToString();
            label.color = theme.TileTextFor(value);
            SetCellSize(cellSize);
        }

        public void SetStone(Cell cell, Theme theme, float cellSize)
        {
            Value = Cell.StoneValue;
            IsStone = true;
            background.color = theme.StoneColour;
            label.text = cell.StoneMovesLeft.ToString();
            label.color = theme.StoneText;
            SetCellSize(cellSize);
        }

        public void SetAnchoredPosition(Vector2 position) => Rect.anchoredPosition = position;

        /// <summary>A brief scale overshoot — used on spawn and on merge.</summary>
        public void Pop(float strength = 0.18f)
        {
            popTimer = 0f;
            popStrength = strength;
        }

        void Update()
        {
            if (popTimer < 0f) return;

            popTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(popTimer / PopDuration);

            // Up then back, so the tile lands at exactly 1.
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * popStrength;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (t >= 1f)
            {
                transform.localScale = Vector3.one;
                popTimer = -1f;
            }
        }
    }
}
