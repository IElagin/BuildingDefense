using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Explosions
{
    public class AreaDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class IsAreaDamageApplied : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    // A lasting area (a puddle) repeats its damage once per period.
    public class AreaDamagePeriod : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AreaDamageCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
