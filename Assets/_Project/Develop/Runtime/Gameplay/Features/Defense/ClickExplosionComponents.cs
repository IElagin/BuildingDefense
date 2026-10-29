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

    // The click damage of this run: the config value raised by the bought strike boost.
    public class ClickExplosionDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
