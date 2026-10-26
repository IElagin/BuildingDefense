using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    public class ClickExplosionRequest : IEntityComponent
    {
        public ReactiveEvent<Vector3> Value;
    }

    public class ClickExplosionEvent : IEntityComponent
    {
        public ReactiveEvent<Vector3> Value;
    }
}
