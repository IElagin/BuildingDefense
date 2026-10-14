using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
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
    }
}
