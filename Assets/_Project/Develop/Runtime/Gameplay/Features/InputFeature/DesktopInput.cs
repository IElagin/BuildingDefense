using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
        public bool IsEnabled { get; set; } = true;

        public Vector3 Direction
        {
            get
            {
                if (IsEnabled == false)
                    return Vector3.zero;

                Keyboard keyboard = Keyboard.current;

                if (keyboard == null)
                    return Vector3.zero;

                return new Vector3(GetAxis(keyboard.aKey, keyboard.dKey), 0, GetAxis(keyboard.sKey, keyboard.wKey));
            }
        }

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
        public bool IsPointerPressed
        {
            get
            {
                if (IsEnabled == false)
                    return false;

                Mouse mouse = Mouse.current;

                if (mouse == null || mouse.leftButton.wasPressedThisFrame == false)
                    return false;

                EventSystem eventSystem = EventSystem.current;

                return eventSystem == null || eventSystem.IsPointerOverGameObject() == false;
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

        private float GetAxis(KeyControl negative, KeyControl positive)
            => (positive.isPressed ? 1 : 0) - (negative.isPressed ? 1 : 0);
    }
}
