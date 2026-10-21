using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Knockback
{
    public class AreaImpulse : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class TakeImpulseEvent : IEntityComponent
    {
        public ReactiveEvent<Vector3> Value;
    }
}
