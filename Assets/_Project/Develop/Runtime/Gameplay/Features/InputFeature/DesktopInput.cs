using UnityEngine;
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

        private float GetAxis(KeyControl negative, KeyControl positive)
            => (positive.isPressed ? 1 : 0) - (negative.isPressed ? 1 : 0);
    }
}
