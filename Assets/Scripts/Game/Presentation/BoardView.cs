using System;
using System.Collections;
using System.Collections.Generic;
using Tilevault.Core;
using Tilevault.Game.Services;
using Tilevault.Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tilevault.Game.Presentation
{
    /// <summary>
    /// Draws the grid and animates moves. Animation is advisory: once it ends,
    /// the view resynchronises from the board itself, so a dropped frame or an
    /// interrupted move can never leave the display disagreeing with the rules.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        public const float MoveDuration = 0.105f;

        RectTransform root;
        RectTransform cellLayer;
        RectTransform tileLayer;
        Image backdrop;

        readonly List<Image> emptyCells = new List<Image>();
        readonly Dictionary<int, TileView> tiles = new Dictionary<int, TileView>();
        readonly List<TileView> recycled = new List<TileView>();

        Theme theme;
        int size;
        float cellSize;
        float gap;

        /// <summary>Raised when a tile is tapped, for the Delete power-up.</summary>
        public event Action<int, int> TileTapped;

        public bool Animating { get; private set; }

        public static BoardView Create(Transform parent, Theme theme)
        {
            RectTransform rt = UIFactory.Rect("BoardView", parent);
            var view = rt.gameObject.AddComponent<BoardView>();
            view.root = rt;
            view.theme = theme;

            view.backdrop = rt.gameObject.AddComponent<Image>();
            view.backdrop.sprite = SpriteFactory.RoundedRect(14);
            view.backdrop.type = Image.Type.Sliced;
            view.backdrop.color = theme.GridBackground;

            view.cellLayer = UIFactory.Fill(UIFactory.Rect("Cells", rt));
            view.tileLayer = UIFactory.Fill(UIFactory.Rect("Tiles", rt));

            return view;
        }

        public void SetTheme(Theme newTheme)
        {
            theme = newTheme;
            backdrop.color = theme.GridBackground;
            foreach (Image cell in emptyCells) cell.color = theme.EmptyCell;
        }

        /// <summary>Rebuilds the empty-cell lattice for a board size.</summary>
        public void Build(int boardSize)
        {
            size = boardSize;
            RecycleAll();

            foreach (Image cell in emptyCells)
                if (cell != null) Destroy(cell.gameObject);
            emptyCells.Clear();

            Measure();

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Image cell = UIFactory.Panel(cellLayer, theme.EmptyCell, 10, $"Cell_{x}_{y}");
                cell.raycastTarget = false;
                var rt = (RectTransform)cell.transform;
                rt.anchorMin = rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = PositionOf(x, y);
                emptyCells.Add(cell);
            }
        }

        void Measure()
        {
            float side = Mathf.Min(root.rect.width, root.rect.height);
            if (side <= 1f) side = 600f;   // before the first layout pass

            gap = side * 0.022f;
            cellSize = (side - gap * (size + 1)) / size;
        }

        Vector2 PositionOf(int x, int y)
        {
            float left = gap + x * (cellSize + gap) + cellSize * 0.5f;
            float bottom = gap + y * (cellSize + gap) + cellSize * 0.5f;
            return new Vector2(left, bottom);
        }

        int KeyOf(int x, int y) => y * size + x;

        // ---- syncing ----------------------------------------------------------

        /// <summary>Discards the animated state and redraws exactly what the board holds.</summary>
        public void SyncFrom(Board board)
        {
            if (board.Size != size) Build(board.Size);
            Measure();

            RecycleAll();

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Cell cell = board[x, y];
                if (cell.IsEmpty) continue;

                TileView tile = Take();
                if (cell.IsStone) tile.SetStone(cell, theme, cellSize);
                else tile.SetTile(cell.Value, theme, cellSize);

                tile.SetAnchoredPosition(PositionOf(x, y));
                tiles[KeyOf(x, y)] = tile;
                WireTap(tile, x, y);
            }

            // Cells may have resized if the board changed size.
            for (int i = 0; i < emptyCells.Count; i++)
            {
                int x = i % size, y = i / size;
                var rt = (RectTransform)emptyCells[i].transform;
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = PositionOf(x, y);
            }
        }

        void WireTap(TileView tile, int x, int y)
        {
            var trigger = tile.GetComponent<EventTrigger>();
            if (trigger == null) trigger = tile.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => TileTapped?.Invoke(x, y));
            trigger.triggers.Add(entry);
        }

        // ---- animation --------------------------------------------------------

        /// <summary>
        /// Slides the tiles described by <paramref name="result"/>, then resyncs.
        /// <paramref name="onComplete"/> runs after the resync.
        /// </summary>
        public IEnumerator AnimateMove(MoveResult result, Board board, Action onComplete)
        {
            Animating = true;

            var moving = new List<(TileView view, Vector2 from, Vector2 to, bool merged)>();

            foreach (TileMove m in result.Moves)
            {
                int key = KeyOf(m.FromX, m.FromY);
                if (!tiles.TryGetValue(key, out TileView view)) continue;
                tiles.Remove(key);

                moving.Add((view, PositionOf(m.FromX, m.FromY), PositionOf(m.ToX, m.ToY), m.Merged));

                // Merging tiles slide over the top of the tile they land on.
                if (m.Merged) view.transform.SetAsLastSibling();
            }

            float elapsed = 0f;
            while (elapsed < MoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / MoveDuration);
                float eased = 1f - (1f - t) * (1f - t);   // ease-out, so tiles arrive settled

                foreach (var m in moving)
                    if (m.view != null)
                        m.view.SetAnchoredPosition(Vector2.LerpUnclamped(m.from, m.to, eased));

                yield return null;
            }

            SyncFrom(board);

            // Pop whatever the move produced, now that positions are authoritative.
            foreach (TileMove m in result.Moves)
            {
                if (!m.Merged) continue;
                if (tiles.TryGetValue(KeyOf(m.ToX, m.ToY), out TileView grown))
                    grown.Pop(0.22f);
            }

            if (result.Spawn.HasValue)
            {
                TileSpawn s = result.Spawn.Value;
                if (tiles.TryGetValue(KeyOf(s.X, s.Y), out TileView spawned))
                {
                    spawned.transform.localScale = Vector3.zero;
                    spawned.Pop(0.0f);
                    StartCoroutine(GrowIn(spawned));
                }
            }

            Animating = false;
            onComplete?.Invoke();
        }

        static IEnumerator GrowIn(TileView tile)
        {
            const float duration = 0.11f;
            float elapsed = 0f;
            while (elapsed < duration && tile != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tile.transform.localScale = Vector3.one * t;
                yield return null;
            }
            if (tile != null) tile.transform.localScale = Vector3.one;
        }

        // ---- pooling ----------------------------------------------------------

        TileView Take()
        {
            if (recycled.Count > 0)
            {
                TileView reused = recycled[recycled.Count - 1];
                recycled.RemoveAt(recycled.Count - 1);
                reused.gameObject.SetActive(true);
                reused.transform.localScale = Vector3.one;
                return reused;
            }

            return TileView.Create(tileLayer, cellSize);
        }

        void RecycleAll()
        {
            foreach (TileView tile in tiles.Values)
            {
                if (tile == null) continue;
                tile.gameObject.SetActive(false);
                recycled.Add(tile);
            }
            tiles.Clear();
        }

        /// <summary>Re-measures after a layout change, e.g. a rotation or a banner appearing.</summary>
        public void Relayout(Board board)
        {
            Measure();
            SyncFrom(board);
        }

        Board tracked;
        float lastSide;

        /// <summary>
        /// The board is sized by a layout fitter, so its rect is not final until
        /// after the first layout pass and changes again on rotation. Watching the
        /// rect is cheaper and more reliable than trying to predict those moments.
        /// </summary>
        void LateUpdate()
        {
            if (tracked == null || Animating) return;

            float side = Mathf.Min(root.rect.width, root.rect.height);
            if (Mathf.Abs(side - lastSide) < 0.5f) return;

            lastSide = side;
            Relayout(tracked);
        }

        /// <summary>Remembers the board so layout changes can redraw it unprompted.</summary>
        public void Track(Board board)
        {
            tracked = board;
            SyncFrom(board);
            lastSide = Mathf.Min(root.rect.width, root.rect.height);
        }
    }
}
