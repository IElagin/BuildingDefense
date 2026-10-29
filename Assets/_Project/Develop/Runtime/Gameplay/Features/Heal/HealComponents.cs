using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Heal
{
    // The heal pair of the course TakeDamageRequest / TakeDamageEvent.
    public class HealRequest : IEntityComponent
    {
        public ReactiveEvent<float> Value;
    }

    public class HealEvent : IEntityComponent
    {
        public ReactiveEvent<float> Value;
    }

    public class CanHeal : IEntityComponent
    {
        public ICompositeCondition Value;
    }
}
