using System;
using Tilevault.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tilevault.Game.Presentation
{
    /// <summary>
    /// Turns pointer drags and arrow keys into directions. Uses the Input System
    /// package directly because the project ships with the legacy input manager
    /// disabled, where <c>UnityEngine.Input</c> throws rather than returning zero.
    ///
    /// Keyboard input is what makes the game drivable from a test and from the
    /// editor without a touchscreen.
    /// </summary>
    public sealed class SwipeInput : MonoBehaviour
    {
        /// <summary>Fraction of the shorter screen edge a drag must cover to count.</summary>
        const float SwipeFractionOfScreen = 0.045f;

        /// <summary>Below this, a drag is a tap and belongs to whatever was under it.</summary>
        const float MinimumPixels = 24f;

        public event Action<Direction> Swiped;

        public bool Enabled { get; set; } = true;

        bool tracking;
        Vector2 start;

        float Threshold => Mathf.Max(MinimumPixels, Mathf.Min(Screen.width, Screen.height) * SwipeFractionOfScreen);

        void Update()
        {
            if (!Enabled) return;

            ReadKeyboard();
            ReadPointer();
        }

        void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                Swiped?.Invoke(Direction.Left);
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                Swiped?.Invoke(Direction.Right);
            else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                Swiped?.Invoke(Direction.Up);
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                Swiped?.Invoke(Direction.Down);
        }

        void ReadPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                tracking = true;
                start = pointer.position.ReadValue();
                return;
            }

            if (!tracking) return;

            Vector2 delta = pointer.position.ReadValue() - start;

            // Fire as soon as the drag is long enough rather than waiting for
            // release — it makes fast repeated swipes feel responsive.
            if (delta.magnitude >= Threshold)
            {
                tracking = false;
                Swiped?.Invoke(DirectionOf(delta));
                return;
            }

            if (pointer.press.wasReleasedThisFrame)
                tracking = false;
        }

        static Direction DirectionOf(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Direction.Right : Direction.Left;
            return delta.y > 0 ? Direction.Up : Direction.Down;
        }

        /// <summary>Lets a test or the editor screenshot pass drive the board directly.</summary>
        public void Simulate(Direction direction) => Swiped?.Invoke(direction);
    }
}
