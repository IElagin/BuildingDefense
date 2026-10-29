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

    // The model that flies off as a corpse and the collider it uses on the Corpses layer.
    public class CorpseModel : IEntityComponent
    {
        public Transform Value;
    }

    public class CorpseCollider : IEntityComponent
    {
        public Collider Value;
    }

    public class IsCorpseFlying : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
