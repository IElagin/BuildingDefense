using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IInputService
    {
        bool IsEnabled { get; set; }

        Vector3 Direction { get; }

        Vector2 PointerPosition { get; }

        bool IsPointerPressed { get; }

        bool IsStartWavePressed { get; }
    }
}
