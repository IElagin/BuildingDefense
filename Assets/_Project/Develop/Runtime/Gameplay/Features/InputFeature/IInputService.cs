using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IInputService
    {
        bool IsEnabled { get; set; }

        Vector2 PointerPosition { get; }

        bool IsPointerClicked { get; }

        bool IsPointerOverUI { get; }

        bool IsStartWavePressed { get; }

        // HUD slot keys 1, 2, 3 (top row or numpad)
        bool TryGetPressedSlot(out int slot);
    }
}
