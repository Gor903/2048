using UnityEngine;

namespace Tilevault.Game.UI
{
    /// <summary>
    /// Keeps its rect inside the device's safe area, so nothing interactive ends
    /// up under a camera cutout or the gesture bar. Re-applies when the area
    /// changes, which it does on rotation and on some foldables mid-session.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rt;
        Rect applied;
        ScreenOrientation appliedOrientation;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (applied != Screen.safeArea || appliedOrientation != Screen.orientation)
                Apply();
        }

        void Apply()
        {
            Rect area = Screen.safeArea;
            applied = area;
            appliedOrientation = Screen.orientation;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 min = area.position;
            Vector2 max = area.position + area.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
