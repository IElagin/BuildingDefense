using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
        private const int NoSlot = 0;
        private const int FirstSlot = 1;
        private const int SecondSlot = 2;
        private const int ThirdSlot = 3;

        public bool IsEnabled { get; set; } = true;

        public Vector2 PointerPosition
        {
            get
            {
                Mouse mouse = Mouse.current;

                if (mouse == null)
                    return Vector2.zero;

                return mouse.position.ReadValue();
            }
        }

        // A click on a HUD button must not also hit the ground under it.
        public bool IsPointerClicked
        {
            get
            {
                if (IsEnabled == false)
                    return false;

                Mouse mouse = Mouse.current;

                if (mouse == null || mouse.leftButton.wasPressedThisFrame == false)
                    return false;

                return IsPointerOverUI == false;
            }
        }

        public bool IsPointerOverUI
        {
            get
            {
                EventSystem eventSystem = EventSystem.current;

                return eventSystem != null && eventSystem.IsPointerOverGameObject();
            }
        }

        public bool IsStartWavePressed
        {
            get
            {
                if (IsEnabled == false)
                    return false;

                Keyboard keyboard = Keyboard.current;

                return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            }
        }

        public bool TryGetPressedSlot(out int slot)
        {
            slot = NoSlot;
            Keyboard keyboard = Keyboard.current;

            if (IsEnabled == false || keyboard == null)
                return false;

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
                slot = FirstSlot;
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
                slot = SecondSlot;
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
                slot = ThirdSlot;

            return slot != NoSlot;
        }
    }
}
