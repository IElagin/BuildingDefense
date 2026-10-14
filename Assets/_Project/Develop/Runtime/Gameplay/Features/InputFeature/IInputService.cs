using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IInputService
    {
        bool IsEnabled { get; set; }

        Vector2 PointerPosition { get; }

        bool IsPointerClicked { get; }

        bool IsStartWavePressed { get; }
    }
}
